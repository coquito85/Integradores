#nullable disable
namespace GenerateJS.Forms
{
    partial class frmPrincipal
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

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            pnlConteudo = new Panel();
            grpExecucao = new GroupBox();
            lblResumo = new Label();
            prgExecucao = new ProgressBar();
            btnExportar = new Button();
            btnVerValidacao = new Button();
            btnVerJson = new Button();
            btnGerar = new Button();
            lblConfigResumo = new Label();
            btnConfigurar = new Button();
            pnlLayoutAtivo = new Panel();
            lblLayoutDetalhe = new Label();
            lblLayoutAtivo = new Label();
            pnlMensagens = new Panel();
            lstMensagens = new ListBox();
            lblMensagensTitulo = new Label();
            pnlNav = new Panel();
            btnNavValidacao = new Button();
            btnNavJson = new Button();
            btnNavConfig = new Button();
            btnNavLayouts = new Button();
            lblMarca = new Label();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            pnlConteudo.SuspendLayout();
            grpExecucao.SuspendLayout();
            pnlLayoutAtivo.SuspendLayout();
            pnlMensagens.SuspendLayout();
            pnlNav.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // pnlConteudo
            //
            pnlConteudo.AutoScroll = true;
            pnlConteudo.Controls.Add(grpExecucao);
            pnlConteudo.Dock = DockStyle.Fill;
            pnlConteudo.Location = new Point(210, 64);
            pnlConteudo.Name = "pnlConteudo";
            pnlConteudo.Padding = new Padding(16);
            pnlConteudo.Size = new Size(890, 434);
            pnlConteudo.TabIndex = 0;
            //
            // grpExecucao
            //
            grpExecucao.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpExecucao.Controls.Add(lblResumo);
            grpExecucao.Controls.Add(prgExecucao);
            grpExecucao.Controls.Add(btnExportar);
            grpExecucao.Controls.Add(btnVerValidacao);
            grpExecucao.Controls.Add(btnVerJson);
            grpExecucao.Controls.Add(btnGerar);
            grpExecucao.Controls.Add(lblConfigResumo);
            grpExecucao.Controls.Add(btnConfigurar);
            grpExecucao.Location = new Point(16, 16);
            grpExecucao.Name = "grpExecucao";
            grpExecucao.Size = new Size(850, 190);
            grpExecucao.TabIndex = 0;
            grpExecucao.TabStop = false;
            grpExecucao.Text = "Execução";
            //
            // btnConfigurar
            //
            btnConfigurar.Location = new Point(16, 26);
            btnConfigurar.Name = "btnConfigurar";
            btnConfigurar.Size = new Size(170, 32);
            btnConfigurar.TabIndex = 0;
            btnConfigurar.Text = "Configurações...";
            btnConfigurar.UseVisualStyleBackColor = true;
            btnConfigurar.Click += btnConfigurar_Click;
            //
            // lblConfigResumo
            //
            lblConfigResumo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblConfigResumo.AutoEllipsis = true;
            lblConfigResumo.ForeColor = SystemColors.GrayText;
            lblConfigResumo.Location = new Point(196, 26);
            lblConfigResumo.Name = "lblConfigResumo";
            lblConfigResumo.Size = new Size(638, 32);
            lblConfigResumo.TabIndex = 1;
            lblConfigResumo.Text = "Configure a origem dos dados e a geração.";
            lblConfigResumo.TextAlign = ContentAlignment.MiddleLeft;
            //
            // btnGerar
            //
            btnGerar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGerar.Location = new Point(16, 78);
            btnGerar.Name = "btnGerar";
            btnGerar.Size = new Size(140, 34);
            btnGerar.TabIndex = 2;
            btnGerar.Text = "Gerar JSON";
            btnGerar.UseVisualStyleBackColor = true;
            btnGerar.Click += btnGerar_Click;
            //
            // btnVerJson
            //
            btnVerJson.Location = new Point(166, 78);
            btnVerJson.Name = "btnVerJson";
            btnVerJson.Size = new Size(130, 34);
            btnVerJson.TabIndex = 3;
            btnVerJson.Text = "Ver JSON";
            btnVerJson.UseVisualStyleBackColor = true;
            btnVerJson.Click += btnVerJson_Click;
            //
            // btnVerValidacao
            //
            btnVerValidacao.Location = new Point(306, 78);
            btnVerValidacao.Name = "btnVerValidacao";
            btnVerValidacao.Size = new Size(130, 34);
            btnVerValidacao.TabIndex = 4;
            btnVerValidacao.Text = "Ver validação";
            btnVerValidacao.UseVisualStyleBackColor = true;
            btnVerValidacao.Click += btnVerValidacao_Click;
            //
            // btnExportar
            //
            btnExportar.Location = new Point(446, 78);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(130, 34);
            btnExportar.TabIndex = 5;
            btnExportar.Text = "Exportar";
            btnExportar.UseVisualStyleBackColor = true;
            btnExportar.Click += btnExportar_Click;
            //
            // prgExecucao
            //
            prgExecucao.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            prgExecucao.Location = new Point(16, 128);
            prgExecucao.MarqueeAnimationSpeed = 30;
            prgExecucao.Name = "prgExecucao";
            prgExecucao.Size = new Size(818, 8);
            prgExecucao.Style = ProgressBarStyle.Marquee;
            prgExecucao.TabIndex = 6;
            prgExecucao.Visible = false;
            //
            // lblResumo
            //
            lblResumo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblResumo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblResumo.Location = new Point(16, 146);
            lblResumo.Name = "lblResumo";
            lblResumo.Size = new Size(818, 20);
            lblResumo.TabIndex = 7;
            //
            // pnlLayoutAtivo
            //
            pnlLayoutAtivo.BackColor = Color.FromArgb(241, 245, 249);
            pnlLayoutAtivo.Controls.Add(lblLayoutDetalhe);
            pnlLayoutAtivo.Controls.Add(lblLayoutAtivo);
            pnlLayoutAtivo.Dock = DockStyle.Top;
            pnlLayoutAtivo.Location = new Point(210, 0);
            pnlLayoutAtivo.Name = "pnlLayoutAtivo";
            pnlLayoutAtivo.Size = new Size(890, 64);
            pnlLayoutAtivo.TabIndex = 1;
            //
            // lblLayoutAtivo
            //
            lblLayoutAtivo.AutoSize = true;
            lblLayoutAtivo.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblLayoutAtivo.Location = new Point(16, 8);
            lblLayoutAtivo.Name = "lblLayoutAtivo";
            lblLayoutAtivo.Size = new Size(240, 25);
            lblLayoutAtivo.TabIndex = 0;
            lblLayoutAtivo.Text = "Nenhum layout selecionado";
            //
            // lblLayoutDetalhe
            //
            lblLayoutDetalhe.AutoSize = true;
            lblLayoutDetalhe.ForeColor = SystemColors.GrayText;
            lblLayoutDetalhe.Location = new Point(18, 38);
            lblLayoutDetalhe.Name = "lblLayoutDetalhe";
            lblLayoutDetalhe.Size = new Size(0, 15);
            lblLayoutDetalhe.TabIndex = 1;
            //
            // pnlMensagens
            //
            pnlMensagens.Controls.Add(lstMensagens);
            pnlMensagens.Controls.Add(lblMensagensTitulo);
            pnlMensagens.Dock = DockStyle.Bottom;
            pnlMensagens.Location = new Point(210, 498);
            pnlMensagens.Name = "pnlMensagens";
            pnlMensagens.Size = new Size(890, 160);
            pnlMensagens.TabIndex = 2;
            //
            // lblMensagensTitulo
            //
            lblMensagensTitulo.BackColor = Color.FromArgb(226, 232, 240);
            lblMensagensTitulo.Dock = DockStyle.Top;
            lblMensagensTitulo.Location = new Point(0, 0);
            lblMensagensTitulo.Name = "lblMensagensTitulo";
            lblMensagensTitulo.Padding = new Padding(8, 0, 0, 0);
            lblMensagensTitulo.Size = new Size(890, 22);
            lblMensagensTitulo.TabIndex = 0;
            lblMensagensTitulo.Text = "Mensagens";
            lblMensagensTitulo.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lstMensagens
            //
            lstMensagens.Dock = DockStyle.Fill;
            lstMensagens.FormattingEnabled = true;
            lstMensagens.HorizontalScrollbar = true;
            lstMensagens.ItemHeight = 15;
            lstMensagens.Location = new Point(0, 22);
            lstMensagens.Name = "lstMensagens";
            lstMensagens.Size = new Size(890, 138);
            lstMensagens.TabIndex = 1;
            //
            // pnlNav
            //
            pnlNav.BackColor = Color.FromArgb(30, 41, 59);
            pnlNav.Controls.Add(btnNavValidacao);
            pnlNav.Controls.Add(btnNavJson);
            pnlNav.Controls.Add(btnNavConfig);
            pnlNav.Controls.Add(btnNavLayouts);
            pnlNav.Controls.Add(lblMarca);
            pnlNav.Dock = DockStyle.Left;
            pnlNav.Location = new Point(0, 0);
            pnlNav.Name = "pnlNav";
            pnlNav.Size = new Size(210, 658);
            pnlNav.TabIndex = 3;
            //
            // lblMarca
            //
            lblMarca.Dock = DockStyle.Top;
            lblMarca.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblMarca.ForeColor = Color.White;
            lblMarca.Location = new Point(0, 0);
            lblMarca.Name = "lblMarca";
            lblMarca.Padding = new Padding(16, 0, 0, 0);
            lblMarca.Size = new Size(210, 80);
            lblMarca.TabIndex = 0;
            lblMarca.Text = "Beta JSON\r\nIntegrator";
            lblMarca.TextAlign = ContentAlignment.MiddleLeft;
            //
            // btnNavLayouts
            //
            btnNavLayouts.Dock = DockStyle.Top;
            btnNavLayouts.FlatAppearance.BorderSize = 0;
            btnNavLayouts.FlatStyle = FlatStyle.Flat;
            btnNavLayouts.ForeColor = Color.White;
            btnNavLayouts.Location = new Point(0, 80);
            btnNavLayouts.Name = "btnNavLayouts";
            btnNavLayouts.Padding = new Padding(16, 0, 0, 0);
            btnNavLayouts.Size = new Size(210, 44);
            btnNavLayouts.TabIndex = 1;
            btnNavLayouts.Text = "Layouts";
            btnNavLayouts.TextAlign = ContentAlignment.MiddleLeft;
            btnNavLayouts.UseVisualStyleBackColor = true;
            btnNavLayouts.Click += btnNavLayouts_Click;
            //
            // btnNavConfig
            //
            btnNavConfig.Dock = DockStyle.Top;
            btnNavConfig.FlatAppearance.BorderSize = 0;
            btnNavConfig.FlatStyle = FlatStyle.Flat;
            btnNavConfig.ForeColor = Color.White;
            btnNavConfig.Location = new Point(0, 124);
            btnNavConfig.Name = "btnNavConfig";
            btnNavConfig.Padding = new Padding(16, 0, 0, 0);
            btnNavConfig.Size = new Size(210, 44);
            btnNavConfig.TabIndex = 2;
            btnNavConfig.Text = "Configuração XML";
            btnNavConfig.TextAlign = ContentAlignment.MiddleLeft;
            btnNavConfig.UseVisualStyleBackColor = true;
            btnNavConfig.Click += btnNavConfig_Click;
            //
            // btnNavJson
            //
            btnNavJson.Dock = DockStyle.Top;
            btnNavJson.FlatAppearance.BorderSize = 0;
            btnNavJson.FlatStyle = FlatStyle.Flat;
            btnNavJson.ForeColor = Color.White;
            btnNavJson.Location = new Point(0, 168);
            btnNavJson.Name = "btnNavJson";
            btnNavJson.Padding = new Padding(16, 0, 0, 0);
            btnNavJson.Size = new Size(210, 44);
            btnNavJson.TabIndex = 3;
            btnNavJson.Text = "Visualizador JSON";
            btnNavJson.TextAlign = ContentAlignment.MiddleLeft;
            btnNavJson.UseVisualStyleBackColor = true;
            btnNavJson.Click += btnNavJson_Click;
            //
            // btnNavValidacao
            //
            btnNavValidacao.Dock = DockStyle.Top;
            btnNavValidacao.FlatAppearance.BorderSize = 0;
            btnNavValidacao.FlatStyle = FlatStyle.Flat;
            btnNavValidacao.ForeColor = Color.White;
            btnNavValidacao.Location = new Point(0, 212);
            btnNavValidacao.Name = "btnNavValidacao";
            btnNavValidacao.Padding = new Padding(16, 0, 0, 0);
            btnNavValidacao.Size = new Size(210, 44);
            btnNavValidacao.TabIndex = 4;
            btnNavValidacao.Text = "Validação";
            btnNavValidacao.TextAlign = ContentAlignment.MiddleLeft;
            btnNavValidacao.UseVisualStyleBackColor = true;
            btnNavValidacao.Click += btnNavValidacao_Click;
            //
            // statusStrip
            //
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStrip.Location = new Point(0, 658);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1100, 22);
            statusStrip.TabIndex = 4;
            //
            // lblStatus
            //
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(38, 17);
            lblStatus.Text = "Pronto";
            //
            // frmPrincipal
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 680);
            Controls.Add(pnlConteudo);
            Controls.Add(pnlLayoutAtivo);
            Controls.Add(pnlMensagens);
            Controls.Add(pnlNav);
            Controls.Add(statusStrip);
            MinimumSize = new Size(980, 640);
            Name = "frmPrincipal";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Beta JSON Integrator";
            Load += frmPrincipal_Load;
            pnlConteudo.ResumeLayout(false);
            grpExecucao.ResumeLayout(false);
            grpExecucao.PerformLayout();
            pnlLayoutAtivo.ResumeLayout(false);
            pnlLayoutAtivo.PerformLayout();
            pnlMensagens.ResumeLayout(false);
            pnlNav.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlConteudo;
        private GroupBox grpExecucao;
        private Label lblResumo;
        private ProgressBar prgExecucao;
        private Button btnExportar;
        private Button btnVerValidacao;
        private Button btnVerJson;
        private Button btnGerar;
        private Label lblConfigResumo;
        private Button btnConfigurar;
        private Panel pnlLayoutAtivo;
        private Label lblLayoutDetalhe;
        private Label lblLayoutAtivo;
        private Panel pnlMensagens;
        private ListBox lstMensagens;
        private Label lblMensagensTitulo;
        private Panel pnlNav;
        private Button btnNavValidacao;
        private Button btnNavJson;
        private Button btnNavConfig;
        private Button btnNavLayouts;
        private Label lblMarca;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
    }
}
