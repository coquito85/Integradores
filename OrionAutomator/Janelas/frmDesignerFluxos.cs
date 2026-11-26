using OrionAutomator.Tema;
using System.ComponentModel;

namespace OrionAutomator.Janelas
{
    /// <summary>
    /// Editor de fluxos (substitui o antigo StudioModeloForm). Esta versao e o AMBIENTE preparado: janela, zonas
    /// (barra, catalogo, canvas, propriedades, log, status), tema trocavel em tempo real e layout configuravel
    /// pelo UiConfig.xml. Os nos, conexoes e a execucao entram nas zonas depois.
    /// </summary>
    public partial class frmDesignerFluxos : Form
    {
        private UiConfig _ui = null!;

        /// <summary>Fluxo a abrir (null = novo fluxo). Defina antes de ShowDialog.</summary>
        public string? CaminhoArquivo { get; set; }

        public frmDesignerFluxos()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            _ui = UiConfig.Carregar();
            // escala 1: o AutoScale do WinForms multiplica tudo (designer + XML) quando a janela abre
            AplicarTema(inicial: true);
            TemaManager.TemaAlterado += OnTemaAlterado;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (_ui == null) return;

            // o Load roda depois do AutoScale: agora o DPI real da tela e conhecido
            _ui.Escala = DeviceDpi / 96f;
            AtualizarCabecalho();
            Registrar($"Ambiente do editor pronto. Tema: {TemaManager.Atual.Nome}.");
            if (_ui.Erro != null) Registrar("Aviso de tema: " + _ui.Erro);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            TemaManager.TemaAlterado -= OnTemaAlterado;
            base.OnFormClosed(e);
        }

        // ══ Tema ═════════════════════════════════════════════════════════════
        private void AplicarTema(bool inicial)
        {
            UiConfigApplier.Aplicar(this, _ui, inicial ? 1f : _ui.Escala, inicial,
                mnuArquivo, mnuFechar, mnuExibir, mnuTema);

            menuPrincipal.Renderer = new MenuRenderer(_ui);
            menuPrincipal.BackColor = _ui.Cor("Superficie");
            menuPrincipal.ForeColor = _ui.Cor("Texto");
            menuPrincipal.Font = _ui.Fonte("Normal");

            AtualizarCabecalho();
        }

        private void OnTemaAlterado(object? sender, EventArgs e)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke(new Action(RecarregarTema));
            else RecarregarTema();
        }

        /// <summary>Relê tema + UiConfig.xml (embutido e Config\UiConfig.xml) e reaplica sem fechar a janela.</summary>
        private void RecarregarTema()
        {
            var novo = UiConfig.Carregar();
            novo.Escala = DeviceDpi / 96f;
            _ui = novo;

            AplicarTema(inicial: false);
            Invalidate(true);

            Registrar($"Tema aplicado: {TemaManager.Atual.Nome}.");
            if (_ui.Erro != null) Registrar("Aviso de tema: " + _ui.Erro);
        }

        /// <summary>F5: recarrega o tema e o UiConfig.xml (util ao editar o XML com a janela aberta).</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5)
            {
                RecarregarTema();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void MnuTema_DropDownOpening(object? sender, EventArgs e) => TemaMenu.Preencher(mnuTema.DropDownItems);

        private void MnuFechar_Click(object? sender, EventArgs e) => Close();

        // ══ Cabecalho / log ══════════════════════════════════════════════════
        private void AtualizarCabecalho()
        {
            string nome = CaminhoArquivo == null ? "Novo fluxo" : Path.GetFileNameWithoutExtension(CaminhoArquivo);
            lblFluxo.Text = nome;
            Text = $"Orion Automator — {nome}";
        }

        private void Registrar(string mensagem)
        {
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {mensagem}{Environment.NewLine}");
        }
    }
}
