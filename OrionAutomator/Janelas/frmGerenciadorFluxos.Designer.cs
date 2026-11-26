using OrionAutomator.Tema;

namespace OrionAutomator.Janelas
{
    partial class frmGerenciadorFluxos
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        // Tudo aqui sao apenas valores iniciais: posicao, cor, fonte, visibilidade e habilitacao reais vem do UiConfig.xml.
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            pnlConteudo = new Panel();
            pnlEsquerda = new Panel();
            lstRecentes = new ListaFluxos();
            pnlRodape = new Panel();
            btnCancelar = new ModernButton();
            btnAbrirArquivo = new ModernButton();
            btnNovoFluxo = new ModernButton();
            pnlListaTopo = new Panel();
            lblModelosTitulo = new Label();
            lblModelosSub = new Label();
            btnNotificacao = new ModernButton();

            pnlAgendamento = new Panel();
            lblAgTitulo = new Label();
            lblData = new Label();
            txtData = new MaskedTextBox();
            lblHoraIni = new Label();
            txtHoraIni = new MaskedTextBox();
            lblHoraFim = new Label();
            txtHoraFim = new MaskedTextBox();
            chkAgendado = new ToggleSwitch();
            btnSalvarAgendamento = new ModernButton();
            lblUltimaTitulo = new Label();
            lblHistorico = new Label();

            pnlBanner = new Panel();
            lblBanner = new Label();

            pnlHeader = new Panel();
            pnlHeaderAccent = new Panel();
            lblTitulo = new Label();
            lblSubtitulo = new Label();
            lblVersao = new Label();

            btnChat = new ModernButton();

            tmrRegras = new System.Windows.Forms.Timer(components);

            ctxItem = new ContextMenuStrip(components);
            mnuExecutar = new ToolStripMenuItem();
            mnuAbrirItem = new ToolStripMenuItem();
            mnuPip = new ToolStripMenuItem();
            sepMenu = new ToolStripSeparator();
            mnuRemover = new ToolStripMenuItem();

            pnlConteudo.SuspendLayout();
            pnlEsquerda.SuspendLayout();
            pnlRodape.SuspendLayout();
            pnlListaTopo.SuspendLayout();
            pnlAgendamento.SuspendLayout();
            pnlBanner.SuspendLayout();
            pnlHeader.SuspendLayout();
            ctxItem.SuspendLayout();
            SuspendLayout();

            // ── pnlHeader ──
            pnlHeader.Controls.Add(lblVersao);
            pnlHeader.Controls.Add(lblSubtitulo);
            pnlHeader.Controls.Add(lblTitulo);
            pnlHeader.Controls.Add(pnlHeaderAccent);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1020, 76);
            pnlHeader.TabIndex = 0;

            pnlHeaderAccent.Dock = DockStyle.Bottom;
            pnlHeaderAccent.Name = "pnlHeaderAccent";
            pnlHeaderAccent.Size = new Size(1020, 2);

            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(24, 10);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Text = "ORION AUTOMATOR";

            lblSubtitulo.AutoSize = true;
            lblSubtitulo.Location = new Point(26, 46);
            lblSubtitulo.Name = "lblSubtitulo";
            lblSubtitulo.Text = "Editor de Automação Visual";

            lblVersao.AutoSize = true;
            lblVersao.Location = new Point(940, 28);
            lblVersao.Name = "lblVersao";
            lblVersao.Text = "v1.0.0";

            // ── pnlBanner ──
            pnlBanner.Controls.Add(lblBanner);
            pnlBanner.Dock = DockStyle.Top;
            pnlBanner.Name = "pnlBanner";
            pnlBanner.Size = new Size(1020, 40);
            pnlBanner.TabIndex = 1;
            pnlBanner.Visible = false;

            lblBanner.Dock = DockStyle.Fill;
            lblBanner.Name = "lblBanner";
            lblBanner.Padding = new Padding(24, 0, 24, 0);
            lblBanner.TextAlign = ContentAlignment.MiddleLeft;
            lblBanner.AutoEllipsis = true;

            // ── pnlConteudo ──
            pnlConteudo.Controls.Add(pnlEsquerda);
            pnlConteudo.Controls.Add(pnlAgendamento);
            pnlConteudo.Dock = DockStyle.Fill;
            pnlConteudo.Name = "pnlConteudo";
            pnlConteudo.TabIndex = 2;

            // ── pnlEsquerda ──
            pnlEsquerda.Controls.Add(lstRecentes);
            pnlEsquerda.Controls.Add(pnlRodape);
            pnlEsquerda.Controls.Add(pnlListaTopo);
            pnlEsquerda.Dock = DockStyle.Fill;
            pnlEsquerda.Name = "pnlEsquerda";
            pnlEsquerda.Padding = new Padding(24, 12, 24, 0);

            pnlListaTopo.Controls.Add(btnNotificacao);
            pnlListaTopo.Controls.Add(lblModelosSub);
            pnlListaTopo.Controls.Add(lblModelosTitulo);
            pnlListaTopo.Dock = DockStyle.Top;
            pnlListaTopo.Name = "pnlListaTopo";
            pnlListaTopo.Size = new Size(650, 68);

            lblModelosTitulo.AutoSize = true;
            lblModelosTitulo.Location = new Point(0, 6);
            lblModelosTitulo.Name = "lblModelosTitulo";
            lblModelosTitulo.Text = "Modelos Recentes";

            lblModelosSub.AutoSize = true;
            lblModelosSub.Location = new Point(0, 34);
            lblModelosSub.Name = "lblModelosSub";
            lblModelosSub.Text = "Selecione um modelo";

            btnNotificacao.Name = "btnNotificacao";
            btnNotificacao.Size = new Size(44, 44);
            btnNotificacao.Location = new Point(606, 6);
            btnNotificacao.TabIndex = 3;
            btnNotificacao.Text = "\uE7ED";
            btnNotificacao.Click += BtnNotificacao_Click;

            lstRecentes.ContextMenuStrip = ctxItem;
            lstRecentes.Dock = DockStyle.Fill;
            lstRecentes.Name = "lstRecentes";
            lstRecentes.TabIndex = 4;
            lstRecentes.DrawItem += LstRecentes_DrawItem;
            lstRecentes.SelectedIndexChanged += LstRecentes_SelectedIndexChanged;
            lstRecentes.MouseDown += LstRecentes_MouseDown;
            lstRecentes.MouseMove += LstRecentes_MouseMove;
            lstRecentes.MouseLeave += LstRecentes_MouseLeave;
            lstRecentes.DoubleClick += LstRecentes_DoubleClick;

            pnlRodape.Controls.Add(btnNovoFluxo);
            pnlRodape.Controls.Add(btnAbrirArquivo);
            pnlRodape.Controls.Add(btnCancelar);
            pnlRodape.Dock = DockStyle.Bottom;
            pnlRodape.Name = "pnlRodape";
            pnlRodape.Size = new Size(650, 72);

            btnCancelar.Name = "btnCancelar";
            btnCancelar.Location = new Point(0, 14);
            btnCancelar.Size = new Size(110, 42);
            btnCancelar.TabIndex = 5;
            btnCancelar.Text = "Cancelar";
            btnCancelar.Click += BtnCancelar_Click;

            btnAbrirArquivo.Name = "btnAbrirArquivo";
            btnAbrirArquivo.Location = new Point(122, 14);
            btnAbrirArquivo.Size = new Size(150, 42);
            btnAbrirArquivo.TabIndex = 6;
            btnAbrirArquivo.Text = "Abrir Arquivo";
            btnAbrirArquivo.Click += BtnAbrirArquivo_Click;

            btnNovoFluxo.Name = "btnNovoFluxo";
            btnNovoFluxo.Location = new Point(284, 14);
            btnNovoFluxo.Size = new Size(150, 42);
            btnNovoFluxo.TabIndex = 7;
            btnNovoFluxo.Text = "Novo Fluxo";
            btnNovoFluxo.Click += BtnNovoFluxo_Click;

            // ── pnlAgendamento ──
            pnlAgendamento.Controls.Add(lblHistorico);
            pnlAgendamento.Controls.Add(lblUltimaTitulo);
            pnlAgendamento.Controls.Add(btnSalvarAgendamento);
            pnlAgendamento.Controls.Add(chkAgendado);
            pnlAgendamento.Controls.Add(txtHoraFim);
            pnlAgendamento.Controls.Add(lblHoraFim);
            pnlAgendamento.Controls.Add(txtHoraIni);
            pnlAgendamento.Controls.Add(lblHoraIni);
            pnlAgendamento.Controls.Add(txtData);
            pnlAgendamento.Controls.Add(lblData);
            pnlAgendamento.Controls.Add(lblAgTitulo);
            pnlAgendamento.Dock = DockStyle.Right;
            pnlAgendamento.Name = "pnlAgendamento";
            pnlAgendamento.Size = new Size(320, 500);
            pnlAgendamento.Paint += PnlAgendamento_Paint;

            lblAgTitulo.AutoSize = true;
            lblAgTitulo.Location = new Point(24, 22);
            lblAgTitulo.Name = "lblAgTitulo";
            lblAgTitulo.Text = "Agendamento";

            lblData.AutoSize = true;
            lblData.Location = new Point(24, 64);
            lblData.Name = "lblData";
            lblData.Text = "Data de execução";

            ConfigurarMascara(txtData, "txtData", "00/00/0000", new Point(24, 86), 8);
            lblHoraIni.AutoSize = true;
            lblHoraIni.Location = new Point(24, 128);
            lblHoraIni.Name = "lblHoraIni";
            lblHoraIni.Text = "Hora inicial";

            ConfigurarMascara(txtHoraIni, "txtHoraIni", "00:00:00", new Point(24, 150), 9);
            lblHoraFim.AutoSize = true;
            lblHoraFim.Location = new Point(24, 192);
            lblHoraFim.Name = "lblHoraFim";
            lblHoraFim.Text = "Hora final";

            ConfigurarMascara(txtHoraFim, "txtHoraFim", "00:00:00", new Point(24, 214), 10);

            chkAgendado.Name = "chkAgendado";
            chkAgendado.Location = new Point(24, 260);
            chkAgendado.Size = new Size(160, 28);
            chkAgendado.TabIndex = 11;
            chkAgendado.Text = "Agendado";

            btnSalvarAgendamento.Name = "btnSalvarAgendamento";
            btnSalvarAgendamento.Location = new Point(24, 304);
            btnSalvarAgendamento.Size = new Size(272, 42);
            btnSalvarAgendamento.TabIndex = 12;
            btnSalvarAgendamento.Text = "Salvar agendamento";
            btnSalvarAgendamento.Click += BtnSalvarAgendamento_Click;

            lblUltimaTitulo.AutoSize = true;
            lblUltimaTitulo.Location = new Point(24, 372);
            lblUltimaTitulo.Name = "lblUltimaTitulo";
            lblUltimaTitulo.Text = "Última execução";

            lblHistorico.Location = new Point(24, 396);
            lblHistorico.Name = "lblHistorico";
            lblHistorico.Size = new Size(272, 130);
            lblHistorico.Text = "Selecione um modelo para agendar.";

            // ── btnChat (flutuante) ──
            btnChat.Name = "btnChat";
            btnChat.Size = new Size(52, 52);
            btnChat.Location = new Point(944, 580);
            btnChat.TabIndex = 13;
            btnChat.Text = "\uE8BD";
            btnChat.Click += BtnChat_Click;

            // ── ctxItem ──
            ctxItem.Items.AddRange(new ToolStripItem[] { mnuExecutar, mnuAbrirItem, mnuPip, sepMenu, mnuRemover });
            ctxItem.Name = "ctxItem";
            ctxItem.ShowImageMargin = false;
            ctxItem.Opening += CtxItem_Opening;

            mnuExecutar.Name = "mnuExecutar";
            mnuExecutar.Text = "Executar";
            mnuExecutar.Click += MnuExecutar_Click;

            mnuAbrirItem.Name = "mnuAbrirItem";
            mnuAbrirItem.Text = "Abrir Item";
            mnuAbrirItem.Click += MnuAbrirItem_Click;

            mnuPip.Name = "mnuPip";
            mnuPip.Text = "Picture in Picture";
            mnuPip.Click += MnuPip_Click;

            sepMenu.Name = "sepMenu";

            mnuRemover.Name = "mnuRemover";
            mnuRemover.Text = "Remover da lista";
            mnuRemover.Click += MnuRemover_Click;

            // ── Form ──
            // Ordem importa para o Dock: Fill primeiro, depois Top (o ultimo adicionado fica por cima).
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1020, 660);
            Controls.Add(pnlConteudo);
            Controls.Add(pnlBanner);
            Controls.Add(pnlHeader);
            Controls.Add(btnChat);
            DoubleBuffered = true;
            Name = "frmGerenciadorFluxos";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Orion Automator";
            Load += FrmGerenciadorFluxos_Load;
            Resize += (_, _) => btnChat.BringToFront();

            pnlConteudo.ResumeLayout(false);
            pnlEsquerda.ResumeLayout(false);
            pnlRodape.ResumeLayout(false);
            pnlListaTopo.ResumeLayout(false);
            pnlListaTopo.PerformLayout();
            pnlAgendamento.ResumeLayout(false);
            pnlAgendamento.PerformLayout();
            pnlBanner.ResumeLayout(false);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ctxItem.ResumeLayout(false);
            ResumeLayout(false);
        }

        private static void ConfigurarMascara(MaskedTextBox t, string nome, string mascara, Point local, int tab)
        {
            t.Name = nome;
            t.Mask = mascara;
            t.PromptChar = '_';
            t.Culture = System.Globalization.CultureInfo.InvariantCulture;
            t.Location = local;
            t.Size = new Size(272, 30);
            t.TabIndex = tab;
            t.TextMaskFormat = MaskFormat.IncludeLiterals;
            t.InsertKeyMode = InsertKeyMode.Overwrite;
        }

        #endregion

        private Panel pnlConteudo;
        private Panel pnlEsquerda;
        private ListaFluxos lstRecentes;
        private Panel pnlRodape;
        private ModernButton btnCancelar;
        private ModernButton btnAbrirArquivo;
        private ModernButton btnNovoFluxo;
        private Panel pnlListaTopo;
        private Label lblModelosTitulo;
        private Label lblModelosSub;
        private ModernButton btnNotificacao;

        private Panel pnlAgendamento;
        private Label lblAgTitulo;
        private Label lblData;
        private MaskedTextBox txtData;
        private Label lblHoraIni;
        private MaskedTextBox txtHoraIni;
        private Label lblHoraFim;
        private MaskedTextBox txtHoraFim;
        private ToggleSwitch chkAgendado;
        private ModernButton btnSalvarAgendamento;
        private Label lblUltimaTitulo;
        private Label lblHistorico;

        private Panel pnlBanner;
        private Label lblBanner;
        private Panel pnlHeader;
        private Panel pnlHeaderAccent;
        private Label lblTitulo;
        private Label lblSubtitulo;
        private Label lblVersao;

        private ModernButton btnChat;

        private System.Windows.Forms.Timer tmrRegras;

        private ContextMenuStrip ctxItem;
        private ToolStripMenuItem mnuExecutar;
        private ToolStripMenuItem mnuAbrirItem;
        private ToolStripMenuItem mnuPip;
        private ToolStripSeparator sepMenu;
        private ToolStripMenuItem mnuRemover;
    }
}
