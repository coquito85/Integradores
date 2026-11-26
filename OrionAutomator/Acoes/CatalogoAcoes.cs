using OrionAutomator.Tema;
using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;
using ReferenciaStandard.Repository;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OrionAutomator.Acoes
{
    public enum TipoPropriedade { Texto, TextoLongo, Int, Numero, Bool, Lista, Arquivo }

    /// <summary>Uma propriedade editavel de uma acao (vem de &lt;Propriedade&gt; no Acoes.xml).</summary>
    public sealed class DefinicaoPropriedade
    {
        public string Nome { get; init; } = "";
        public string Rotulo { get; init; } = "";
        public TipoPropriedade Tipo { get; init; } = TipoPropriedade.Texto;
        public string Padrao { get; init; } = "";
        public string[] Opcoes { get; init; } = Array.Empty<string>();
        public double? Min { get; init; }
        public double? Max { get; init; }
        public string Categoria { get; init; } = "Geral";
        public string Descricao { get; init; } = "";
        public string Filtro { get; init; } = "Todos os arquivos (*.*)|*.*";
        public bool Obrigatorio { get; init; }

        /// <summary>Texto vazio = valido. Caso contrario, o motivo da invalidez (usado pelo painel e pelo executor).</summary>
        public string Validar(string? valor)
        {
            valor ??= "";
            if (Obrigatorio && string.IsNullOrWhiteSpace(valor)) return $"'{Rotulo}' é obrigatório.";
            if (valor.Length == 0) return "";

            switch (Tipo)
            {
                case TipoPropriedade.Int:
                    if (!int.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i))
                        return $"'{Rotulo}' deve ser um número inteiro.";
                    if (Min.HasValue && i < Min.Value) return $"'{Rotulo}' deve ser ≥ {Min.Value}.";
                    if (Max.HasValue && i > Max.Value) return $"'{Rotulo}' deve ser ≤ {Max.Value}.";
                    break;
                case TipoPropriedade.Numero:
                    if (!double.TryParse(valor, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                        return $"'{Rotulo}' deve ser um número.";
                    if (Min.HasValue && d < Min.Value) return $"'{Rotulo}' deve ser ≥ {Min.Value}.";
                    if (Max.HasValue && d > Max.Value) return $"'{Rotulo}' deve ser ≤ {Max.Value}.";
                    break;
                case TipoPropriedade.Lista:
                    if (Opcoes.Length > 0 && !Opcoes.Contains(valor, StringComparer.OrdinalIgnoreCase))
                        return $"'{Rotulo}': valor fora da lista ({string.Join(", ", Opcoes)}).";
                    break;
            }
            return "";
        }
    }

    /// <summary>Uma acao do catalogo: tudo o que o icone faz, lido do Acoes.xml.</summary>
    public sealed class DefinicaoAcao
    {
        private static readonly Regex Token = new(@"\{([A-Za-z_]\w*)\}", RegexOptions.Compiled);

        public string Id { get; init; } = "";
        public string Rotulo { get; init; } = "";
        public string Icone { get; init; } = "";
        public string Executor { get; init; } = "";
        public string Resumo { get; init; } = "";
        public string Descricao { get; init; } = "";
        public Color Cor { get; init; } = Color.Gray;
        public string Categoria { get; init; } = "";
        public List<DefinicaoPropriedade> Propriedades { get; } = new();

        public DefinicaoPropriedade? Propriedade(string nome) =>
            Propriedades.FirstOrDefault(p => string.Equals(p.Nome, nome, StringComparison.OrdinalIgnoreCase));

        /// <summary>Troca {Propriedade} pelo valor. Bool verdadeiro vira "Rotulo+", falso vira vazio (ex.: Ctrl+Enter).</summary>
        public string FormatarResumo(NoFluxo no)
        {
            if (string.IsNullOrEmpty(Resumo)) return "";
            return Token.Replace(Resumo, m =>
            {
                var p = Propriedade(m.Groups[1].Value);
                string v = no.Texto(m.Groups[1].Value);
                if (p?.Tipo == TipoPropriedade.Bool)
                    return v.Equals("true", StringComparison.OrdinalIgnoreCase) ? p.Rotulo + "+" : "";
                return v;
            }).Trim();
        }
    }

    /// <summary>Linha do catalogo: um subtitulo (Grupo) ou uma acao.</summary>
    public sealed class ItemCatalogo
    {
        public string? Grupo { get; init; }
        public DefinicaoAcao? Acao { get; init; }
    }

    public sealed class CategoriaAcao
    {
        public string Nome { get; init; } = "";
        public Color Cor { get; init; } = Color.Gray;
        public List<ItemCatalogo> Itens { get; } = new();
    }

    /// <summary>
    /// CENTRALIZADOR do catalogo de acoes. Camadas (a ultima vence, mesclando por "nome"):
    ///   1) Acoes.xml embutido na DLL ReferenciaStandard;
    ///   2) Config\Acoes.xml ao lado do .exe (opcional; so precisa conter o que muda).
    /// Recarregue (Carregar de novo) quando o tema mudar: as cores "@Nome" sao resolvidas na carga.
    /// </summary>
    public sealed class CatalogoAcoes
    {
        public const string NomeArquivo = "Acoes";

        public List<CategoriaAcao> Categorias { get; } = new();
        public string? Erro { get; private set; }

        private readonly Dictionary<string, DefinicaoAcao> _porId = new(StringComparer.OrdinalIgnoreCase);

        public DefinicaoAcao? Buscar(string? id) =>
            id != null && _porId.TryGetValue(id, out var a) ? a : null;

        public IEnumerable<DefinicaoAcao> Todas => _porId.Values;

        public static CatalogoAcoes Carregar(UiConfig ui)
        {
            var cat = new CatalogoAcoes();
            var erros = new List<string>();
            var dados = new Entid_AcoesDados();

            Mesclar(dados, FilesRepository.LerEmbutido(NomeArquivo), "Acoes.xml embutido", erros);
            Mesclar(dados, FilesRepository.LerExterno(NomeArquivo), "Config\\Acoes.xml", erros);

            foreach (var c in dados.Categorias)
            {
                var cor = ui.ResolverCor(c.Texto("cor")) ?? ui.Cor("Primaria");
                var categoria = new CategoriaAcao { Nome = c.Nome ?? "", Cor = cor };

                foreach (var i in c.Itens)
                {
                    if (i.Tag == "Grupo")
                    {
                        categoria.Itens.Add(new ItemCatalogo { Grupo = i.Texto("texto") ?? "" });
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(i.Nome)) continue;
                    var acao = Converter(i, categoria, ui);
                    if (cat._porId.ContainsKey(acao.Id))
                    {
                        erros.Add($"Acoes.xml: id duplicado '{acao.Id}' (ignorado).");
                        continue;
                    }
                    cat._porId[acao.Id] = acao;
                    categoria.Itens.Add(new ItemCatalogo { Acao = acao });
                }

                cat.Categorias.Add(categoria);
            }

            cat.Erro = erros.Count == 0 ? null : string.Join(" | ", erros);
            return cat;
        }

        // ── Conversao XML → definicao ───────────────────────────────────────
        private static DefinicaoAcao Converter(Entid_AcaoDados a, CategoriaAcao cat, UiConfig ui)
        {
            var def = new DefinicaoAcao
            {
                Id = a.Nome!,
                Rotulo = a.Texto("rotulo") ?? a.Nome!,
                Icone = a.Texto("icone") ?? "•",
                Executor = a.Texto("executor") ?? "",
                Resumo = a.Texto("resumo") ?? "",
                Descricao = a.Texto("descricao") ?? "",
                Cor = ui.ResolverCor(a.Texto("cor")) ?? cat.Cor,
                Categoria = cat.Nome
            };

            // propriedades comuns a toda acao (sobrescreviveis no XML com o mesmo nome)
            def.Propriedades.Add(new DefinicaoPropriedade
            {
                Nome = "titulo", Rotulo = "Nome do card", Tipo = TipoPropriedade.Texto,
                Padrao = def.Rotulo, Categoria = "Geral", Descricao = "Título mostrado no cartão do fluxo."
            });

            foreach (var p in a.Propriedades)
            {
                if (string.IsNullOrWhiteSpace(p.Nome)) continue;
                var nova = ConverterPropriedade(p);
                int existente = def.Propriedades.FindIndex(x => string.Equals(x.Nome, nova.Nome, StringComparison.OrdinalIgnoreCase));
                if (existente >= 0) def.Propriedades[existente] = nova; else def.Propriedades.Add(nova);
            }

            if (def.Propriedade("emErro") == null && def.Executor.Length > 0)
                def.Propriedades.Add(new DefinicaoPropriedade
                {
                    Nome = "emErro", Rotulo = "Em caso de erro", Tipo = TipoPropriedade.Lista,
                    Opcoes = new[] { "Stop", "Continue", "Retry" }, Padrao = "Stop", Categoria = "Geral",
                    Descricao = "Stop interrompe o fluxo; Continue segue para o próximo; Retry tenta até 3 vezes."
                });

            return def;
        }

        private static DefinicaoPropriedade ConverterPropriedade(Entid_Atributos p)
        {
            var tipo = (p.Texto("tipo") ?? "texto").Trim().ToLowerInvariant() switch
            {
                "textolongo" => TipoPropriedade.TextoLongo,
                "int" => TipoPropriedade.Int,
                "numero" or "número" => TipoPropriedade.Numero,
                "bool" => TipoPropriedade.Bool,
                "lista" => TipoPropriedade.Lista,
                "arquivo" => TipoPropriedade.Arquivo,
                _ => TipoPropriedade.Texto
            };

            return new DefinicaoPropriedade
            {
                Nome = p.Nome!,
                Rotulo = p.Texto("rotulo") ?? p.Nome!,
                Tipo = tipo,
                Padrao = p.Texto("padrao") ?? "",
                Opcoes = (p.Texto("opcoes") ?? "").Split('|', StringSplitOptions.RemoveEmptyEntries),
                Min = p.Numero("min"),
                Max = p.Numero("max"),
                Categoria = p.Texto("categoria") ?? "Geral",
                Descricao = p.Texto("descricao") ?? "",
                Filtro = p.Texto("filtro") ?? "Todos os arquivos (*.*)|*.*",
                Obrigatorio = p.Bool("obrigatorio") ?? false
            };
        }

        // ── Mesclagem de camadas ────────────────────────────────────────────
        private static void Mesclar(Entid_AcoesDados destino, string? xml, string origem, List<string> erros)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                // o embutido e obrigatorio; o externo e opcional (LerExterno devolve null se o arquivo nao existe)
                if (origem.StartsWith("Acoes.xml")) erros.Add($"{origem}: não encontrado ({FilesRepository.UltimoErro})");
                return;
            }

            Entid_AcoesDados novo;
            try { novo = XmlHelper.LerAcoes(xml); }
            catch (Exception ex) { erros.Add($"{origem}: {ex.Message}"); return; }

            foreach (var c in novo.Categorias)
            {
                var existente = destino.Categorias.FirstOrDefault(x => string.Equals(x.Nome, c.Nome, StringComparison.OrdinalIgnoreCase));
                if (existente == null) { destino.Categorias.Add(c); continue; }

                existente.Mesclar(c);
                foreach (var item in c.Itens)
                {
                    if (item.Tag == "Grupo")
                    {
                        if (!existente.Itens.Any(x => x.Tag == "Grupo" && x.Texto("texto") == item.Texto("texto")))
                            existente.Itens.Add(item);
                        continue;
                    }

                    var ja = existente.Itens.FirstOrDefault(x => x.Tag == "Acao" &&
                        string.Equals(x.Nome, item.Nome, StringComparison.OrdinalIgnoreCase));
                    if (ja == null) { existente.Itens.Add(item); continue; }

                    ja.Mesclar(item);
                    foreach (var p in item.Propriedades)   // propriedade existente e substituida; nova e acrescentada
                    {
                        int idx = ja.Propriedades.FindIndex(x => string.Equals(x.Nome, p.Nome, StringComparison.OrdinalIgnoreCase));
                        if (idx >= 0) ja.Propriedades[idx].Mesclar(p); else ja.Propriedades.Add(p);
                    }
                }
            }
        }
    }
}
