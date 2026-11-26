using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace OrionAutomator.Acoes
{
    public enum EstadoNo { Normal, Executando, Sucesso, Erro }

    /// <summary>
    /// Instancia de uma acao no fluxo. Generico: nao existe uma classe por tipo de no; o que o no "e" vem da
    /// DefinicaoAcao (Acoes.xml) e os valores ficam num dicionario (nome da propriedade → texto).
    /// </summary>
    public sealed class NoFluxo
    {
        public string Id { get; init; } = Guid.NewGuid().ToString("N");
        public string AcaoId { get; init; } = "";

        /// <summary>Posicao em pixels logicos (96 dpi).</summary>
        public Point Posicao { get; set; }

        public Dictionary<string, string> Valores { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Id do no seguinte (conexao de saida). Null = fim da cadeia.</summary>
        public string? ProximoId { get; set; }

        public EstadoNo Estado { get; set; }

        public static NoFluxo Criar(DefinicaoAcao def, Point posicao)
        {
            var no = new NoFluxo { AcaoId = def.Id, Posicao = posicao };
            foreach (var p in def.Propriedades) no.Valores[p.Nome] = p.Padrao;
            return no;
        }

        public string Texto(string chave, string padrao = "") =>
            Valores.TryGetValue(chave, out var v) && v != null ? v : padrao;

        public int Int(string chave, int padrao = 0) =>
            int.TryParse(Texto(chave).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : padrao;

        public double Numero(string chave, double padrao = 0) =>
            double.TryParse(Texto(chave).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double v) ? v : padrao;

        public bool Bool(string chave, bool padrao = false) =>
            Valores.TryGetValue(chave, out var v) ? v.Equals("true", StringComparison.OrdinalIgnoreCase) || v == "1" : padrao;

        public string Titulo(DefinicaoAcao? def)
        {
            string t = Texto("titulo");
            return t.Length > 0 ? t : def?.Rotulo ?? AcaoId;
        }
    }

    /// <summary>Fluxo em memoria: nos + id do no inicial. Salvo como XML (extensao .uax, a mesma do gerenciador).</summary>
    public sealed class ModeloFluxo
    {
        public const int VersaoArquivo = 1;

        public string Nome { get; set; } = "Novo fluxo";
        public List<NoFluxo> Nos { get; } = new();

        public NoFluxo? Buscar(string? id) => id == null ? null : Nos.FirstOrDefault(n => n.Id == id);

        public int TotalConexoes => Nos.Count(n => n.ProximoId != null && Buscar(n.ProximoId) != null);

        // ── Arquivo ─────────────────────────────────────────────────────────
        public void Salvar(string caminho)
        {
            var raiz = new XElement("Fluxo",
                new XAttribute("versao", VersaoArquivo),
                new XAttribute("nome", Nome),
                Nos.Select(n => new XElement("No",
                    new XAttribute("id", n.Id),
                    new XAttribute("acao", n.AcaoId),
                    new XAttribute("x", n.Posicao.X),
                    new XAttribute("y", n.Posicao.Y),
                    n.ProximoId != null ? new XAttribute("proximo", n.ProximoId) : null,
                    n.Valores.Select(v => new XElement("Valor", new XAttribute("nome", v.Key), v.Value)))));

            new XDocument(new XDeclaration("1.0", "utf-8", null), raiz).Save(caminho);
        }

        public static ModeloFluxo Abrir(string caminho)
        {
            string texto = File.ReadAllText(caminho);
            if (texto.TrimStart('\uFEFF', ' ', '\t', '\r', '\n').StartsWith("{"))
                throw new InvalidDataException(
                    "Este arquivo está no formato JSON do Studio original. O editor novo grava fluxos em XML; " +
                    "a conversão dos fluxos antigos ainda não foi implementada.");

            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
            using var leitor = XmlReader.Create(new StringReader(texto.TrimStart('\uFEFF')), settings);
            var raiz = XDocument.Load(leitor).Root ?? throw new InvalidDataException("Arquivo sem elemento raiz.");

            if (raiz.Name.LocalName != "Fluxo") throw new InvalidDataException("Arquivo não é um fluxo do Orion Automator.");
            int versao = (int?)raiz.Attribute("versao") ?? 1;
            if (versao > VersaoArquivo)
                throw new InvalidDataException($"Fluxo gravado numa versão mais nova ({versao}). Atualize o programa.");

            var modelo = new ModeloFluxo { Nome = (string?)raiz.Attribute("nome") ?? Path.GetFileNameWithoutExtension(caminho) };
            foreach (var e in raiz.Elements("No"))
            {
                var no = new NoFluxo
                {
                    Id = (string?)e.Attribute("id") ?? Guid.NewGuid().ToString("N"),
                    AcaoId = (string?)e.Attribute("acao") ?? "",
                    Posicao = new Point((int?)e.Attribute("x") ?? 0, (int?)e.Attribute("y") ?? 0),
                    ProximoId = (string?)e.Attribute("proximo")
                };
                foreach (var v in e.Elements("Valor"))
                    if ((string?)v.Attribute("nome") is { Length: > 0 } nome) no.Valores[nome] = v.Value;
                modelo.Nos.Add(no);
            }
            return modelo;
        }
    }
}
