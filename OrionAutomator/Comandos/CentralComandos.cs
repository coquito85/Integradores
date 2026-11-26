using OrionAutomator.Tema;
using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;
using ReferenciaStandard.Repository;

namespace OrionAutomator.Comandos
{
    /// <summary>
    /// CENTRALIZADOR de comandos do editor (equivalente ao UiConfig.xml para a barra padrao).
    ///
    /// O que cada comando FAZ e registrado uma unica vez (Registrar). O Comandos.xml decide ONDE ele aparece
    /// (menu, barra, atalho). O mesmo comando em dois lugares compartilha o handler e o estado habilitado:
    /// habilitar/desabilitar "fluxo.pausar" muda o item do menu e o botao da barra juntos.
    ///
    /// Camadas do XML (mescladas por "nome"): Comandos.xml embutido → Config\Comandos.xml ao lado do .exe.
    /// </summary>
    public sealed class CentralComandos
    {
        public const string NomeArquivo = "Comandos";

        private sealed class Comando
        {
            public Action? Executar;
            public Func<bool>? Habilitado;       // regra dinamica (null = sempre habilitado)
            public bool? ForcadoHabilitado;      // Definir(): sobrepoe a regra enquanto nao for limpo
            public readonly List<ToolStripItem> Itens = new();
        }

        private readonly Dictionary<string, Comando> _comandos = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ToolStripItem> _itensPorNome = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<Keys, string> _atalhosExtras = new();

        /// <summary>Problemas de leitura do XML (null = tudo ok).</summary>
        public string? Erro { get; private set; }

        /// <summary>Ids usados no XML para os quais ninguem registrou handler (aparecem desabilitados).</summary>
        public IReadOnlyCollection<string> SemHandler =>
            _comandos.Where(c => c.Value.Executar == null).Select(c => c.Key).OrderBy(x => x).ToList();

        /// <summary>Todos os itens de menu/barra criados (para o UiConfigApplier sobrepor texto/visivel/habilitado).</summary>
        public IEnumerable<ToolStripItem> Itens => _itensPorNome.Values;

        public ToolStripItem? Item(string nome) => _itensPorNome.TryGetValue(nome, out var i) ? i : null;

        // ── Registro ────────────────────────────────────────────────────────
        public CentralComandos Registrar(string id, Action executar, Func<bool>? habilitado = null)
        {
            var c = Obter(id);
            c.Executar = executar;
            c.Habilitado = habilitado;
            return this;
        }

        private Comando Obter(string id)
        {
            if (!_comandos.TryGetValue(id, out var c)) _comandos[id] = c = new Comando();
            return c;
        }

        public bool Executar(string id)
        {
            if (!_comandos.TryGetValue(id, out var c) || c.Executar == null || !EstaHabilitado(id)) return false;
            c.Executar();
            return true;
        }

        /// <summary>Forca habilitado/desabilitado (ex.: Pausar/Parar so durante a execucao). null devolve o controle a regra.</summary>
        public void Definir(string id, bool? habilitado)
        {
            Obter(id).ForcadoHabilitado = habilitado;
            Atualizar(id);
        }

        public bool EstaHabilitado(string id)
        {
            if (!_comandos.TryGetValue(id, out var c) || c.Executar == null) return false;
            if (c.ForcadoHabilitado.HasValue) return c.ForcadoHabilitado.Value;
            return c.Habilitado?.Invoke() ?? true;
        }

        /// <summary>Reavalia o estado de todos os itens (chame ao mudar a selecao, a aba, a execucao...).</summary>
        public void Atualizar()
        {
            foreach (var id in _comandos.Keys.ToList()) Atualizar(id);
        }

        private void Atualizar(string id)
        {
            if (!_comandos.TryGetValue(id, out var c)) return;
            bool hab = EstaHabilitado(id);
            foreach (var item in c.Itens)
            {
                // o habilitado="false" do XML e o estado de partida; a regra registrada do comando assume em seguida
                item.Enabled = hab;
            }
        }

        /// <summary>Atalhos de teclado de botoes da barra sem item de menu equivalente. Chame no ProcessCmdKey do formulario.</summary>
        public bool TratarTecla(Keys tecla) =>
            _atalhosExtras.TryGetValue(tecla, out var id) && Executar(id);

        public void DefinirTexto(string nomeItem, string texto)
        {
            if (_itensPorNome.TryGetValue(nomeItem, out var i)) i.Text = texto;
        }

        // ── Construcao a partir do XML ──────────────────────────────────────
        public void Construir(MenuStrip menu, ToolStrip barra, UiConfig ui)
        {
            foreach (var c in _comandos.Values) c.Itens.Clear();
            _itensPorNome.Clear();
            _atalhosExtras.Clear();

            var erros = new List<string>();
            var dados = new Entid_ComandosDados();
            Mesclar(dados, FilesRepository.LerEmbutido(NomeArquivo), obrigatorio: true, "Comandos.xml embutido", erros);
            Mesclar(dados, FilesRepository.LerExterno(NomeArquivo), obrigatorio: false, "Config\\Comandos.xml", erros);
            Erro = erros.Count == 0 ? null : string.Join(" | ", erros);

            menu.Items.Clear();
            barra.Items.Clear();

            var atalhosMenu = new HashSet<Keys>();

            foreach (var m in dados.Menus)
            {
                var topo = CriarItemMenu(m, ui, atalhosMenu);
                if (topo != null) menu.Items.Add(topo);
            }

            foreach (var b in dados.Barra)
            {
                var item = CriarItemBarra(b, ui);
                if (item == null) continue;
                barra.Items.Add(item);

                // atalho declarado so na barra (sem equivalente no menu): tratado por TratarTecla
                if (b.Tag == "Botao" && TentarAtalho(b.Texto("atalho"), out var k) && !atalhosMenu.Contains(k) && b.Texto("comando") is { Length: > 0 } cmd)
                    _atalhosExtras[k] = cmd;
            }

            Atualizar();
        }

        private ToolStripItem? CriarItemMenu(Entid_No n, UiConfig ui, HashSet<Keys> atalhosMenu)
        {
            if (n.Tag == "Separador") return new ToolStripSeparator();
            if (n.Tag != "Menu" && n.Tag != "Item" && n.Tag != "Submenu") return null;

            var item = new ToolStripMenuItem(n.Texto("texto") ?? n.Nome ?? "") { Name = n.Nome ?? "" };
            item.Visible = n.Bool("visivel") ?? true;

            foreach (var f in n.Filhos)
            {
                var filho = CriarItemMenu(f, ui, atalhosMenu);
                if (filho != null) item.DropDownItems.Add(filho);
            }

            if (n.Texto("especial") == "tema")
            {
                // item provisorio: sem ele o submenu nao abre; TemaMenu.Preencher o substitui ao abrir
                item.DropDownItems.Add(new ToolStripMenuItem("..."));
                item.DropDownOpening += (_, _) => TemaMenu.Preencher(item.DropDownItems);
            }

            string? cmd = n.Texto("comando");
            if (!string.IsNullOrWhiteSpace(cmd))
            {
                Vincular(item, cmd);
                if (n.Bool("habilitado") == false) item.Enabled = false;

                if (TentarAtalho(n.Texto("atalho"), out var keys) && atalhosMenu.Add(keys))
                    item.ShortcutKeys = keys;
            }

            Indexar(item);
            return item;
        }

        private ToolStripItem? CriarItemBarra(Entid_No n, UiConfig ui)
        {
            ToolStripItem item;
            switch (n.Tag)
            {
                case "Separador":
                    return new ToolStripSeparator();

                case "Rotulo":
                    item = new ToolStripLabel(n.Texto("texto") ?? "") { Name = n.Nome ?? "" };
                    break;

                case "Botao":
                    var b = new ToolStripButton(n.Texto("texto") ?? "")
                    {
                        Name = n.Nome ?? "",
                        DisplayStyle = ToolStripItemDisplayStyle.Text
                    };
                    item = b;
                    break;

                default:
                    return null;
            }

            item.Visible = n.Bool("visivel") ?? true;

            // o MenuRenderer le Tag (Color) como cor do texto: assim a cor segue o tema
            var cor = ui.ResolverCor(n.Texto("cor"));
            if (cor.HasValue) item.Tag = cor.Value;

            string? cmd = n.Texto("comando");
            if (!string.IsNullOrWhiteSpace(cmd))
            {
                Vincular(item, cmd);
                string dica = n.Texto("dica") ?? "";
                string atalho = n.Texto("atalho") ?? "";
                item.ToolTipText = atalho.Length > 0 ? $"{dica} ({atalho})" : dica;
                if (n.Bool("habilitado") == false) item.Enabled = false;
            }

            Indexar(item);
            return item;
        }

        private void Vincular(ToolStripItem item, string comando)
        {
            var c = Obter(comando);
            c.Itens.Add(item);
            item.Click += (_, _) => Executar(comando);
        }

        private void Indexar(ToolStripItem item)
        {
            if (!string.IsNullOrEmpty(item.Name)) _itensPorNome[item.Name] = item;
        }

        // ── Atalhos ─────────────────────────────────────────────────────────
        private static bool TentarAtalho(string? texto, out Keys keys)
        {
            keys = Keys.None;
            if (string.IsNullOrWhiteSpace(texto)) return false;

            foreach (var parte in texto.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                switch (parte.ToLowerInvariant())
                {
                    case "ctrl": case "control": keys |= Keys.Control; break;
                    case "shift": keys |= Keys.Shift; break;
                    case "alt": keys |= Keys.Alt; break;
                    default:
                        if (!Enum.TryParse<Keys>(parte, true, out var k)) return false;
                        keys |= k;
                        break;
                }
            }
            return (keys & Keys.KeyCode) != Keys.None;
        }

        // ── Mesclagem (menu/botao pelo "nome"; item novo vai para o fim) ────
        private static void Mesclar(Entid_ComandosDados destino, string? xml, bool obrigatorio, string origem, List<string> erros)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                if (obrigatorio) erros.Add($"{origem}: não encontrado ({FilesRepository.UltimoErro})");
                return;
            }

            Entid_ComandosDados novo;
            try { novo = XmlHelper.LerComandos(xml); }
            catch (Exception ex) { erros.Add($"{origem}: {ex.Message}"); return; }

            MesclarLista(destino.Menus, novo.Menus);
            MesclarLista(destino.Barra, novo.Barra);
        }

        private static void MesclarLista(List<Entid_No> destino, List<Entid_No> novos)
        {
            foreach (var n in novos)
            {
                var existente = string.IsNullOrEmpty(n.Nome) || n.Tag == "Separador"
                    ? null
                    : destino.FirstOrDefault(x => x.Tag == n.Tag && string.Equals(x.Nome, n.Nome, StringComparison.OrdinalIgnoreCase));

                if (existente == null) { destino.Add(n); continue; }

                existente.Mesclar(n);
                MesclarLista(existente.Filhos, n.Filhos);
            }
        }
    }
}
