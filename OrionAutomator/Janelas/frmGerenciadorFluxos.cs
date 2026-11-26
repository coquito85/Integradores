using ExecutorService.Model;
using OrionAutomator.Tema;
using ReferenciaFactory.Collections;
using ReferenciaFactory.Regras;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text;


namespace OrionAutomator.Janelas
{
    public enum AcaoGerenciador { NovoFluxo, AbrirArquivo, AbrirRecente, AbrirNoEditor, AbrirRecenteComPip, Cancelar }

    public partial class frmGerenciadorFluxos : Form
    {
        // ── Resultado (mesmo contrato do ManagerModeloForm original) ─────────
        public AcaoGerenciador Acao { get; private set; } = AcaoGerenciador.Cancelar;
        public string? CaminhoSelecionado { get; private set; }

        /// <summary>Disparado ao clicar no botao de chat (conecte aqui o assistente de IA).</summary>
        public event EventHandler? ChatSolicitado;

        private const string TituloMsg = "Orion Automator";
        private const string FiltroArquivos = "Fluxos (*.uax;*.json)|*.uax;*.json|Todos (*.*)|*.*";

        private UiConfig _ui = null!;
        private Dictionary<string, bool> _habilitadoXml = new(StringComparer.OrdinalIgnoreCase);
        private RegrasService _regras = null!;
        private ResultadoRegras _estado = null!;

        private List<string> _recentes = new();
        private Dictionary<string, ModeloSchedule> _schedules = new(StringComparer.OrdinalIgnoreCase);
        private string? _caminhoAtual;
        private int _ordemAtual;
        private bool _atualizandoLista;

        private int _hoverItem = -1;
        private int _hoverAcao;           // 0 = nenhuma, 1 = baixar, 2 = enviar

        // recursos de desenho (criados a partir do tema)
        private readonly List<IDisposable> _recursos = new();
        private SolidBrush _bFundo = null!, _bCard = null!, _bHover = null!, _bSel = null!, _bPrimaria = null!,
                           _bIconeFundo = null!, _bIconeHover = null!;
        private Pen _penBorda = null!, _penSel = null!;

        public frmGerenciadorFluxos()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            _ui = UiConfig.Carregar();
            _regras = RegrasService.Carregar();

            // 1) propriedades vindas do XML   2) regras de licenca/versao por cima (so podem desabilitar)
            // escala 1: o AutoScale do WinForms multiplica tudo (designer + XML) quando a janela abre.
            _habilitadoXml = UiConfigApplier.Aplicar(this, _ui, 1f, true, mnuExecutar, mnuAbrirItem, mnuPip, mnuRemover);
            PrepararRecursos();
            PrepararMenu();

            tmrRegras.Interval = _regras.IntervaloVerificacaoSegundos * 1000;
            tmrRegras.Tick += (_, _) => AplicarRegras();
            tmrRegras.Start();

            AgendamentoStore.Instance.Atualizado += OnAgendamentoAtualizado;
            TemaManager.TemaAlterado += OnTemaAlterado;
            _schedules = AgendamentoStore.Instance.Snapshot();

            AplicarRegras();
            CarregarRecentes();
            AtualizarPainelAgenda();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            tmrRegras.Stop();
            AgendamentoStore.Instance.Atualizado -= OnAgendamentoAtualizado;
            TemaManager.TemaAlterado -= OnTemaAlterado;
            foreach (var r in _recursos) r.Dispose();
            _recursos.Clear();
            base.OnFormClosed(e);
        }

        private void FrmGerenciadorFluxos_Load(object? sender, EventArgs e)
        {
            // o Load roda depois do AutoScale: agora o DPI real da tela e conhecido
            _ui.Escala = DeviceDpi / 96f;
            lstRecentes.ItemHeight = _ui.Escalar(_ui.Metrica("AlturaItem", 80));

            if (_ui.Erro != null)
                Debug.WriteLine("[UiConfig] " + _ui.Erro + " (usando padrões embutidos)");
            btnChat.BringToFront();
        }

        /// <summary>F5: relê o UiConfig.xml (embutido + Config\UiConfig.xml) e reaplica cores, fontes e propriedades.</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5)
            {
                RecarregarTema();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>Tema trocado no menu (ou "Recarregar tema"): relê tudo e reaplica com a janela aberta.</summary>
        private void OnTemaAlterado(object? sender, EventArgs e)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke(new Action(RecarregarTema));
            else RecarregarTema();
        }

        private void BtnTema_Click(object? sender, EventArgs e)
        {
            TemaMenu.Preencher(ctxTema.Items);
            ctxTema.Show(btnTema, new Point(0, btnTema.Height + 2));
        }

        private void RecarregarTema()
        {
            var novo = UiConfig.Carregar();
            novo.Escala = DeviceDpi / 96f;
            _ui = novo;

            _habilitadoXml = UiConfigApplier.Aplicar(this, _ui, _ui.Escala, false, mnuExecutar, mnuAbrirItem, mnuPip, mnuRemover);

            foreach (var r in _recursos) r.Dispose();
            _recursos.Clear();
            PrepararRecursos();
            PrepararMenu();

            lstRecentes.ItemHeight = _ui.Escalar(_ui.Metrica("AlturaItem", 80));
            AplicarRegras();
            btnChat.BringToFront();
            Invalidate(true);

            if (_ui.Erro != null)
                MessageBox.Show(this, "O tema foi recarregado com avisos:\n\n" + _ui.Erro, TituloMsg,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // ══ Tema ═════════════════════════════════════════════════════════════
        private SolidBrush CriarBrush(string cor) { var b = new SolidBrush(_ui.Cor(cor)); _recursos.Add(b); return b; }
        private Pen CriarCaneta(string cor) { var p = new Pen(_ui.Cor(cor)); _recursos.Add(p); return p; }

        private void PrepararRecursos()
        {
            _bFundo = CriarBrush("Fundo");
            _bCard = CriarBrush("Superficie");
            _bHover = CriarBrush("SuperficieAlt");
            _bSel = CriarBrush("PrimariaSuave");
            _bPrimaria = CriarBrush("Primaria");
            _bIconeFundo = CriarBrush("SuperficieAlt");
            _bIconeHover = CriarBrush("Primaria");
            _penBorda = CriarCaneta("Borda");
            _penSel = CriarCaneta("Primaria");
        }

        private void PrepararMenu()
        {
            EstilizarMenu(ctxItem);
            EstilizarMenu(ctxTema);
            mnuRemover.Tag = _ui.Cor("Perigo");
        }

        private void EstilizarMenu(ContextMenuStrip menu)
        {
            menu.Renderer = new MenuRenderer(_ui);
            menu.BackColor = _ui.Cor("Superficie");
            menu.ForeColor = _ui.Cor("Texto");
            menu.Font = _ui.Fonte("Normal");
            menu.Padding = new Padding(4);
        }

        // ══ Regras de licenca / versao ═══════════════════════════════════════
        private void AplicarRegras()
        {
            _estado = _regras.Avaliar();

            lblVersao.Text = string.IsNullOrWhiteSpace(_estado.Titular)
                ? $"v{_estado.VersaoAtual.ToString(3)}"
                : $"v{_estado.VersaoAtual.ToString(3)}  ·  {_estado.Titular}";

            string? msg;
            Color fundo, texto;

            if (!_estado.LicencaValida || _estado.VersaoObrigatoriaPendente)
            { msg = _estado.MensagemBloqueio; fundo = _ui.Cor("Perigo"); texto = _ui.Cor("TextoSobrePerigo"); }
            else if (_estado.AvisoLicenca)
            { msg = _estado.MensagemAviso; fundo = _ui.Cor("Aviso"); texto = _ui.Cor("TextoSobreAviso"); }
            else if (_estado.VersaoNovaDisponivel)
            { msg = _estado.MensagemVersaoNova; fundo = _ui.Cor("PrimariaSuave"); texto = _ui.Cor("Primaria"); }
            else
            { msg = null; fundo = texto = Color.Empty; }

            pnlBanner.Visible = msg != null;
            if (msg != null)
            {
                pnlBanner.BackColor = fundo;
                lblBanner.BackColor = fundo;
                lblBanner.ForeColor = texto;
                lblBanner.Text = msg;
            }

            // as regras so desabilitam; nunca habilitam algo que o XML deixou desabilitado
            Bloquear("NovoFluxo", btnNovoFluxo);
            Bloquear("AbrirArquivo", btnAbrirArquivo);
            Bloquear("Chat", btnChat);

            btnNotificacao.Emblema = (msg != null) ? "!" : null;
            AtualizarPainelAgenda();
        }

        /// <summary>habilitado = (o que o XML definiu) E (o que as regras permitem). Reavaliavel a qualquer momento.</summary>
        private void Bloquear(string funcionalidade, Control c) =>
            c.Enabled = HabilitadoNoXml(c) && _regras.Permite(_estado, funcionalidade, out _);

        private bool HabilitadoNoXml(Control c) => !_habilitadoXml.TryGetValue(c.Name, out var h) || h;

        /// <summary>Verificacao no momento da acao (cobre prazo vencendo com o app aberto).</summary>
        private bool Permitir(string funcionalidade)
        {
            _estado = _regras.Avaliar();
            if (_regras.Permite(_estado, funcionalidade, out var motivo)) return true;
            AplicarRegras();
            MessageBox.Show(this, motivo, TituloMsg, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private void BtnNotificacao_Click(object? sender, EventArgs e)
        {
            var r = _regras.Avaliar();
            var sb = new StringBuilder();

            sb.AppendLine($"Versão instalada: {r.VersaoAtual.ToString(3)}");
            if (r.VersaoUltima != null) sb.AppendLine($"Última versão: {r.VersaoUltima.ToString(3)}");
            if (r.Validade.HasValue)
                sb.AppendLine($"Licença válida até: {r.Validade:dd/MM/yyyy}" +
                              (r.DiasRestantes is >= 0 ? $" ({r.DiasRestantes} dia(s))" : ""));

            foreach (var m in new[] { r.MensagemBloqueio, r.MensagemAviso, r.MensagemVersaoNova })
                if (!string.IsNullOrWhiteSpace(m)) { sb.AppendLine(); sb.AppendLine(m); }

            bool abrirUrl = r.VersaoNovaDisponivel &&
                            Uri.TryCreate(r.UrlDownload, UriKind.Absolute, out var uri) &&
                            (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);

            if (abrirUrl)
            {
                sb.AppendLine();
                sb.Append("Deseja abrir a página de download?");
                if (MessageBox.Show(this, sb.ToString(), TituloMsg, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    try { Process.Start(new ProcessStartInfo(r.UrlDownload!) { UseShellExecute = true }); } catch { }
                }
            }
            else
            {
                MessageBox.Show(this, sb.ToString().TrimEnd(), TituloMsg, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnChat_Click(object? sender, EventArgs e)
        {
            if (!Permitir("Chat")) return;
            if (ChatSolicitado == null)
            {
                MessageBox.Show(this, "O assistente de IA ainda não foi conectado nesta versão.", TituloMsg,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            ChatSolicitado.Invoke(this, EventArgs.Empty);
        }

        // ══ Recentes ═════════════════════════════════════════════════════════
        private void CarregarRecentes()
        {
            _recentes = RecentesStore.Ler();
            AtualizarLista();
        }

        public static void AdicionarRecente(string path) => RecentesStore.Adicionar(path);

        private bool IndiceValido(int i) => i >= 0 && i < _recentes.Count;

        private void AtualizarLista()
        {
            string? manter = _caminhoAtual;

            _recentes = _recentes
                .OrderBy(p => _schedules.TryGetValue(p, out var s) ? s.Ordem : int.MaxValue)
                .ThenBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();

            _atualizandoLista = true;
            lstRecentes.BeginUpdate();
            lstRecentes.Items.Clear();
            foreach (var p in _recentes) lstRecentes.Items.Add(p);
            if (lstRecentes.Items.Count == 0) lstRecentes.Items.Add("");
            lstRecentes.EndUpdate();
            _atualizandoLista = false;

            _caminhoAtual = null;
            int idx = manter == null ? -1 : _recentes.FindIndex(p => string.Equals(p, manter, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0) lstRecentes.SelectedIndex = idx;
            else AtualizarPainelAgenda();
        }

        private void LstRecentes_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_atualizandoLista) return;
            lstRecentes.Invalidate();

            int idx = lstRecentes.SelectedIndex;
            _caminhoAtual = IndiceValido(idx) ? _recentes[idx] : null;
            CarregarAgendaNoPainel(_caminhoAtual);
            AtualizarPainelAgenda();
        }

        private void LstRecentes_DoubleClick(object? sender, EventArgs e)
        {
            if (TentarObterSelecionado("AbrirRecente", out var path))
                Finalizar(AcaoGerenciador.AbrirRecente, path);
        }

        // ── Desenho do item ──────────────────────────────────────────────────
        private (Rectangle baixar, Rectangle enviar) RetsAcao(Rectangle card)
        {
            int t = E(_ui.Metrica("TamanhoIconeAcao", 28));
            int gap = E(_ui.Metrica("EspacoIconeAcao", 8));
            int mr = E(_ui.Metrica("MargemIconeAcao", 14));
            int y = card.Y + (card.Height - t) / 2;
            var enviar = new Rectangle(card.Right - mr - t, y, t, t);
            var baixar = new Rectangle(enviar.X - gap - t, y, t, t);
            return (baixar, enviar);
        }

        private int E(int valor) => _ui.Escalar(valor);

        private Rectangle CardDe(Rectangle b) => new(b.X + 1, b.Y + E(4), b.Width - 3, b.Height - E(8));

        private void LstRecentes_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || _ui == null) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillRectangle(_bFundo, e.Bounds);

            if (e.Index >= _recentes.Count)
            {
                TextRenderer.DrawText(g, "Nenhum projeto recente encontrado.", _ui.Fonte("ItemVazio"),
                    new Point(e.Bounds.X + E(12), e.Bounds.Y + E(20)), _ui.Cor("TextoDesabilitado"), TextFormatFlags.NoPadding);
                return;
            }

            bool sel = (e.State & DrawItemState.Selected) != 0;
            bool hover = _hoverItem == e.Index;
            string path = _recentes[e.Index];
            string nome = Path.GetFileNameWithoutExtension(path);
            string dir = Path.GetDirectoryName(path) ?? "";
            bool existe = File.Exists(path);
            bool temSched = _schedules.TryGetValue(path, out var sched);

            var card = CardDe(e.Bounds);
            using (var gp = Desenho.Arredondado(card, E(_ui.Metrica("RaioCard", 10))))
            {
                g.FillPath(sel ? _bSel : hover ? _bHover : _bCard, gp);
                g.DrawPath(sel ? _penSel : _penBorda, gp);
            }

            // icone do fluxo
            int tamIcone = E(36);
            var caixaIcone = new Rectangle(card.X + E(14), card.Y + (card.Height - tamIcone) / 2, tamIcone, tamIcone);
            using (var gp = Desenho.Arredondado(caixaIcone, E(9)))
                g.FillPath(sel ? _bPrimaria : _bIconeFundo, gp);
            TextRenderer.DrawText(g, "\uE713", _ui.Fonte("Icone"), caixaIcone,
                sel ? _ui.Cor("TextoSobrePrimaria") : _ui.Cor("Primaria"),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            // textos
            var (rBaixar, _) = RetsAcao(card);
            int tx = caixaIcone.Right + E(14);
            int largura = Math.Max(E(40), rBaixar.X - E(10) - tx);
            const TextFormatFlags fl = TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

            void Linha(string texto, string fonte, Color cor, int dy) =>
                TextRenderer.DrawText(g, texto, _ui.Fonte(fonte), new Rectangle(tx, card.Y + E(dy), largura, E(18)), cor, fl);

            Linha(nome, "ItemNome", !existe ? _ui.Cor("Perigo") : sel ? _ui.Cor("Primaria") : _ui.Cor("Texto"), 8);
            Linha(dir, "ItemPath", _ui.Cor("TextoSecundario"), 28);

            if (!existe)
            {
                Linha("(arquivo não encontrado)", "ItemMeta", _ui.Cor("Perigo"), 44);
            }
            else if (temSched)
            {
                Linha(TextoAgenda(sched!), "ItemMeta", _ui.Cor("TextoSecundario"), 44);
                var (texto, erro) = TextoUltima(sched!);
                Linha(texto, "ItemMeta", erro ? _ui.Cor("Perigo") : _ui.Cor("TextoSecundario"), 58);
            }
            else
            {
                Linha("Sem agendamento", "ItemMeta", _ui.Cor("TextoDesabilitado"), 44);
            }

            // icones de acao (nuvem)
            var (rb, re) = RetsAcao(card);
            DesenharIconeAcao(g, rb, "\uE896", hover && _hoverAcao == 1);
            DesenharIconeAcao(g, re, "\uE898", hover && _hoverAcao == 2);
        }

        private void DesenharIconeAcao(Graphics g, Rectangle r, string glifo, bool hover)
        {
            using (var gp = Desenho.Arredondado(r, E(7)))
                g.FillPath(hover ? _bIconeHover : _bIconeFundo, gp);
            TextRenderer.DrawText(g, glifo, _ui.Fonte("IconeAcao"), r,
                hover ? _ui.Cor("TextoSobrePrimaria") : _ui.Cor("Primaria"),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        private static string TextoAgenda(ModeloSchedule s)
        {
            string info = $"Ordem: {s.Ordem}";
            if (s.Agendado && s.DataExecucao.HasValue)
            {
                info += $"  ·  Agendado: {s.DataExecucao:dd/MM/yyyy HH:mm}";
                if (s.DataFim.HasValue) info += $" → {s.DataFim:HH:mm}";
                if (s.UltimoInicio.HasValue && s.UltimoInicio >= s.DataExecucao) info += "  (processado)";
            }
            return info;
        }

        private static (string texto, bool erro) TextoUltima(ModeloSchedule s)
        {
            bool ignoradoMaisRecente = s.IgnoradoEm.HasValue && (!s.UltimoInicio.HasValue || s.IgnoradoEm > s.UltimoInicio);
            if (ignoradoMaisRecente)
                return ($"Ignorado em {s.IgnoradoEm:dd/MM/yyyy HH:mm} (horário passou sem executar)", true);

            if (s.UltimoInicio.HasValue)
                return ($"Última execução: {s.UltimoInicio:dd/MM/yyyy HH:mm:ss}  ·  {FormatarDuracao(s.DuracaoUltimaSegundos)}  ·  {s.UltimoStatus}",
                        s.UltimoStatus == IsStatusExecute.Erro);

            return ("Nunca executado", false);
        }

        private static string FormatarDuracao(int segundos)
        {
            var t = TimeSpan.FromSeconds(Math.Max(0, segundos));
            if (t.TotalHours >= 1) return $"{(int)t.TotalHours}h {t.Minutes}min {t.Seconds}s";
            if (t.TotalMinutes >= 1) return $"{t.Minutes}min {t.Seconds}s";
            return $"{t.Seconds}s";
        }

        // ── Mouse na lista ───────────────────────────────────────────────────
        private void InvalidarItem(int idx)
        {
            if (idx >= 0 && idx < lstRecentes.Items.Count)
                lstRecentes.Invalidate(lstRecentes.GetItemRectangle(idx));
        }

        private void LstRecentes_MouseDown(object? sender, MouseEventArgs e)
        {
            int idx = lstRecentes.IndexFromPoint(e.Location);
            if (!IndiceValido(idx)) { lstRecentes.SelectedIndex = -1; return; }

            if (e.Button == MouseButtons.Left)
            {
                var (rb, re) = RetsAcao(CardDe(lstRecentes.GetItemRectangle(idx)));
                if (rb.Contains(e.Location)) { BaixarItem(idx); return; }
                if (re.Contains(e.Location)) { EnviarItem(idx); return; }
            }
            lstRecentes.SelectedIndex = idx;
        }

        private void LstRecentes_MouseMove(object? sender, MouseEventArgs e)
        {
            int idx = lstRecentes.IndexFromPoint(e.Location);
            int acao = 0;

            if (IndiceValido(idx))
            {
                var (rb, re) = RetsAcao(CardDe(lstRecentes.GetItemRectangle(idx)));
                if (rb.Contains(e.Location)) acao = 1;
                else if (re.Contains(e.Location)) acao = 2;
            }
            else idx = -1;

            if (idx != _hoverItem || acao != _hoverAcao)
            {
                int antigo = _hoverItem;
                _hoverItem = idx;
                _hoverAcao = acao;
                InvalidarItem(antigo);
                InvalidarItem(idx);
            }
            lstRecentes.Cursor = acao != 0 ? Cursors.Hand : Cursors.Default;
        }

        private void LstRecentes_MouseLeave(object? sender, EventArgs e)
        {
            int antigo = _hoverItem;
            _hoverItem = -1;
            _hoverAcao = 0;
            lstRecentes.Cursor = Cursors.Default;
            InvalidarItem(antigo);
        }

        // ── Nuvem (ganchos para a integracao futura) ─────────────────────────
        private void BaixarItem(int idx)
        {
            if (!Permitir("Nuvem")) return;
            MessageBox.Show(this, "Integração de nuvem ainda não configurada neste projeto.", TituloMsg,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void EnviarItem(int idx)
        {
            if (!Permitir("Nuvem")) return;
            MessageBox.Show(this, "Integração de nuvem ainda não configurada neste projeto.", TituloMsg,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ══ Menu de contexto ═════════════════════════════════════════════════
        private void CtxItem_Opening(object? sender, CancelEventArgs e)
        {
            int idx = lstRecentes.SelectedIndex;
            bool valido = IndiceValido(idx);
            if (!valido) { e.Cancel = true; return; }

            bool existe = File.Exists(_recentes[idx]);
            var r = _regras.Avaliar();
            mnuExecutar.Enabled = existe && _regras.Permite(r, "AbrirRecente", out _);
            mnuAbrirItem.Enabled = existe && _regras.Permite(r, "AbrirNoEditor", out _);
            mnuPip.Enabled = existe && _regras.Permite(r, "Pip", out _);
            mnuRemover.Enabled = _regras.Permite(r, "Remover", out _);
        }

        private bool TentarObterSelecionado(string funcionalidade, out string caminho)
        {
            caminho = "";
            int idx = lstRecentes.SelectedIndex;
            if (!IndiceValido(idx)) return false;
            if (!Permitir(funcionalidade)) return false;

            var p = _recentes[idx];
            if (!File.Exists(p))
            {
                MessageBox.Show(this, "Arquivo não encontrado:\n" + p, TituloMsg, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            caminho = p;
            return true;
        }

        private void MnuExecutar_Click(object? sender, EventArgs e)
        {
            if (TentarObterSelecionado("AbrirRecente", out var p)) Finalizar(AcaoGerenciador.AbrirRecente, p);
        }

        private void MnuAbrirItem_Click(object? sender, EventArgs e)
        {
            if (TentarObterSelecionado("AbrirNoEditor", out var p)) Finalizar(AcaoGerenciador.AbrirNoEditor, p);
        }

        private void MnuPip_Click(object? sender, EventArgs e)
        {
            if (TentarObterSelecionado("Pip", out var p)) Finalizar(AcaoGerenciador.AbrirRecenteComPip, p);
        }

        private void MnuRemover_Click(object? sender, EventArgs e)
        {
            int idx = lstRecentes.SelectedIndex;
            if (!IndiceValido(idx)) return;
            _recentes.RemoveAt(idx);
            RecentesStore.Gravar(_recentes);
            _caminhoAtual = null;
            AtualizarLista();
        }

        // ══ Botoes inferiores ════════════════════════════════════════════════
        private void Finalizar(AcaoGerenciador acao, string? caminho)
        {
            Acao = acao;
            CaminhoSelecionado = caminho;
            DialogResult = acao == AcaoGerenciador.Cancelar ? DialogResult.Cancel : DialogResult.OK;
        }

        private void BtnNovoFluxo_Click(object? sender, EventArgs e)
        {
            if (Permitir("NovoFluxo")) Finalizar(AcaoGerenciador.NovoFluxo, null);
        }

        private void BtnAbrirArquivo_Click(object? sender, EventArgs e)
        {
            if (!Permitir("AbrirArquivo")) return;

            using var dlg = new OpenFileDialog { Title = "Abrir fluxo", Filter = FiltroArquivos };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            Finalizar(AcaoGerenciador.AbrirArquivo, dlg.FileName);
        }

        private void BtnCancelar_Click(object? sender, EventArgs e) => Finalizar(AcaoGerenciador.Cancelar, null);

        // ══ Agendamento ══════════════════════════════════════════════════════
        private void OnAgendamentoAtualizado(string path)
        {
            try
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(new Action(() =>
                {
                    _schedules = AgendamentoStore.Instance.Snapshot();
                    lstRecentes.Invalidate();
                    if (string.Equals(_caminhoAtual, path, StringComparison.OrdinalIgnoreCase))
                        AtualizarHistorico(path);
                }));
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        private static DateTime PadraoInicio()
        {
            var d = DateTime.Now.AddMinutes(5);
            return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0);
        }

        private void DefinirCampos(DateTime ini, DateTime fim, bool agendado)
        {
            txtData.Text = ini.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            txtHoraIni.Text = ini.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            txtHoraFim.Text = fim.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            chkAgendado.Checked = agendado;
        }

        private void CarregarAgendaNoPainel(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                var p = PadraoInicio();
                _ordemAtual = 0;
                DefinirCampos(p, p.AddMinutes(10), false);
                AtualizarHistorico(null);
                return;
            }

            if (_schedules.TryGetValue(path, out var s))
            {
                var ini = s.DataExecucao ?? PadraoInicio();
                var fim = s.DataFim ?? ini.AddMinutes(10);
                _ordemAtual = s.Ordem;
                DefinirCampos(ini, ini.Date + fim.TimeOfDay, s.Agendado);
            }
            else
            {
                var p = PadraoInicio();
                _ordemAtual = AgendamentoStore.Instance.ProximaOrdem();
                DefinirCampos(p, p.AddMinutes(10), false);
            }
            AtualizarHistorico(path);
        }

        private void AtualizarPainelAgenda()
        {
            bool permitido = _regras.Permite(_estado, "Agendar", out var motivo);
            bool liberado = _caminhoAtual != null && permitido;

            foreach (Control c in new Control[] { txtData, txtHoraIni, txtHoraFim, chkAgendado, btnSalvarAgendamento })
                c.Enabled = liberado && HabilitadoNoXml(c);

            if (!permitido)
                lblHistorico.Text = "Agendamento bloqueado.\n" + motivo;
            else if (_caminhoAtual == null)
                lblHistorico.Text = "Selecione um modelo para agendar.";
            else
                AtualizarHistorico(_caminhoAtual);
        }

        private void AtualizarHistorico(string? path)
        {
            if (string.IsNullOrEmpty(path) || !_schedules.TryGetValue(path, out var s))
            {
                lblHistorico.Text = path == null ? "Selecione um modelo para agendar." : "Nunca executado.";
                return;
            }

            var sb = new StringBuilder();
            if (s.UltimoInicio.HasValue)
            {
                sb.AppendLine($"Início: {s.UltimoInicio:dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine($"Fim: {s.UltimoFim:dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine($"Duração: {FormatarDuracao(s.DuracaoUltimaSegundos)}");
                sb.AppendLine($"Resultado: {s.UltimoStatus}");
                if (!string.IsNullOrWhiteSpace(s.UltimoDetalhe)) sb.AppendLine($"Detalhe: {s.UltimoDetalhe}");
            }
            else sb.AppendLine("Nunca executado.");

            if (s.IgnoradoEm.HasValue)
            {
                sb.AppendLine();
                sb.AppendLine($"Ignorado: {s.IgnoradoEm:dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine("(o horário passou sem executar)");
            }
            lblHistorico.Text = sb.ToString().TrimEnd();
        }

        private bool TentarLerAgenda(out DateTime ini, out DateTime fim, out string? erro)
        {
            ini = fim = default;
            erro = null;

            if (!DateTime.TryParseExact(txtData.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
            { erro = "Data inválida. Use o formato dd/mm/aaaa."; return false; }
            if (!DateTime.TryParseExact(txtHoraIni.Text, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var hi))
            { erro = "Hora inicial inválida. Use hh:mm:ss."; return false; }
            if (!DateTime.TryParseExact(txtHoraFim.Text, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var hf))
            { erro = "Hora final inválida. Use hh:mm:ss."; return false; }

            ini = data.Date + hi.TimeOfDay;
            fim = data.Date + hf.TimeOfDay;

            if (fim <= ini) { erro = "A hora final deve ser maior que a hora inicial."; return false; }
            return true;
        }

        private void BtnSalvarAgendamento_Click(object? sender, EventArgs e)
        {
            if (!Permitir("Agendar")) return;
            var caminho = _caminhoAtual;
            if (string.IsNullOrEmpty(caminho)) return;

            if (!TentarLerAgenda(out var ini, out var fim, out var erro))
            {
                MessageBox.Show(this, erro, TituloMsg, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (chkAgendado.Checked && ini < DateTime.Now)
            {
                MessageBox.Show(this, "O horário inicial já passou. Escolha um horário futuro.", TituloMsg,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // trabalha numa copia: se a validacao falhar, o agendamento salvo nao e alterado
            var sched = AgendamentoStore.Instance.Obter(caminho)?.Clone() ?? new ModeloSchedule { FilePath = caminho };
            sched.Ordem = _ordemAtual;
            sched.DataExecucao = ini;
            sched.DataFim = fim;
            sched.Agendado = chkAgendado.Checked;

            var (ok, msgErro) = AgendamentoStore.Instance.Salvar(sched);
            if (!ok)
            {
                MessageBox.Show(this, msgErro, TituloMsg, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _schedules = AgendamentoStore.Instance.Snapshot();
            AtualizarLista();

            string msg = sched.Agendado
                ? $"Agendamento salvo.\n\nInício: {ini:dd/MM/yyyy HH:mm:ss}\nFim: {fim:dd/MM/yyyy HH:mm:ss}"
                : "Agendamento salvo (desativado).";
            MessageBox.Show(this, msg, TituloMsg, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PnlAgendamento_Paint(object? sender, PaintEventArgs e)
        {
            if (_ui == null) return;
            using var pen = new Pen(_ui.Cor("Borda"));
            e.Graphics.DrawLine(pen, 0, 0, 0, pnlAgendamento.Height);
        }
    }
}
