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
            chkPrevia = new CheckBox();
            cboGenerate = new ComboBox();
            lblGenerate = new Label();
            grpFonte = new GroupBox();
            pnlBanco = new Panel();
            lblConexaoAviso = new Label();
            btnTestarConexao = new Button();
            chkMostrarConexao = new CheckBox();
            txtConexao = new TextBox();
            lblConexao = new Label();
            pnlCsv = new Panel();
            chkCabecalho = new CheckBox();
            cboEncoding = new ComboBox();
            lblEncoding = new Label();
            cboDelimitador = new ComboBox();
            lblDelimitador = new Label();
            btnProcurar = new Button();
            txtArquivo = new TextBox();
            lblArquivo = new Label();
            cboFonte = new ComboBox();
            lblFonte = new Label();
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
            grpFonte.SuspendLayout();
            pnlBanco.SuspendLayout();
            pnlCsv.SuspendLayout();
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
            pnlConteudo.Controls.Add(grpFonte);
            pnlConteudo.Dock = DockStyle.Fill;
            pnlConteudo.Location = new Point(210, 64);
            pnlConteudo.Name = "pnlConteudo";
            pnlConteudo.Padding = new Padding(16);
            pnlConteudo.Size = new Size(890, 434);
            pnlConteudo.TabIndex = 0;
            //
            // grpFonte
            //
            grpFonte.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpFonte.Controls.Add(pnlBanco);
            grpFonte.Controls.Add(pnlCsv);
            grpFonte.Controls.Add(cboFonte);
            grpFonte.Controls.Add(lblFonte);
            grpFonte.Location = new Point(16, 16);
            grpFonte.Name = "grpFonte";
            grpFonte.Size = new Size(850, 170);
            grpFonte.TabIndex = 0;
            grpFonte.TabStop = false;
            grpFonte.Text = "Fonte de dados";
            //
            // lblFonte
            //
            lblFonte.AutoSize = true;
            lblFonte.Location = new Point(16, 31);
            lblFonte.Name = "lblFonte";
            lblFonte.Size = new Size(43, 15);
            lblFonte.TabIndex = 0;
            lblFonte.Text = "Origem:";
            //
            // cboFonte
            //
            cboFonte.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFonte.FormattingEnabled = true;
            cboFonte.Items.AddRange(new object[] { "Arquivo CSV", "MariaDB", "SQL Server" });
            cboFonte.Location = new Point(80, 28);
            cboFonte.Name = "cboFonte";
            cboFonte.Size = new Size(200, 23);
            cboFonte.TabIndex = 1;
            cboFonte.SelectedIndexChanged += cboFonte_SelectedIndexChanged;
            //
            // pnlCsv
            //
            pnlCsv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlCsv.Controls.Add(chkCabecalho);
            pnlCsv.Controls.Add(cboEncoding);
            pnlCsv.Controls.Add(lblEncoding);
            pnlCsv.Controls.Add(cboDelimitador);
            pnlCsv.Controls.Add(lblDelimitador);
            pnlCsv.Controls.Add(btnProcurar);
            pnlCsv.Controls.Add(txtArquivo);
            pnlCsv.Controls.Add(lblArquivo);
            pnlCsv.Location = new Point(10, 62);
            pnlCsv.Name = "pnlCsv";
            pnlCsv.Size = new Size(830, 96);
            pnlCsv.TabIndex = 2;
            //
            // lblArquivo
            //
            lblArquivo.AutoSize = true;
            lblArquivo.Location = new Point(6, 12);
            lblArquivo.Name = "lblArquivo";
            lblArquivo.Size = new Size(47, 15);
            lblArquivo.TabIndex = 0;
            lblArquivo.Text = "Arquivo:";
            //
            // txtArquivo
            //
            txtArquivo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtArquivo.Location = new Point(70, 9);
            txtArquivo.Name = "txtArquivo";
            txtArquivo.Size = new Size(690, 23);
            txtArquivo.TabIndex = 1;
            //
            // btnProcurar
            //
            btnProcurar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnProcurar.Location = new Point(766, 7);
            btnProcurar.Name = "btnProcurar";
            btnProcurar.Size = new Size(54, 27);
            btnProcurar.TabIndex = 2;
            btnProcurar.Text = "...";
            btnProcurar.UseVisualStyleBackColor = true;
            btnProcurar.Click += btnProcurar_Click;
            //
            // lblDelimitador
            //
            lblDelimitador.AutoSize = true;
            lblDelimitador.Location = new Point(6, 52);
            lblDelimitador.Name = "lblDelimitador";
            lblDelimitador.Size = new Size(66, 15);
            lblDelimitador.TabIndex = 3;
            lblDelimitador.Text = "Delimitador:";
            //
            // cboDelimitador
            //
            cboDelimitador.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDelimitador.FormattingEnabled = true;
            cboDelimitador.Items.AddRange(new object[] { "; (ponto e vírgula)", ", (vírgula)", "| (barra vertical)", "Tab" });
            cboDelimitador.Location = new Point(80, 49);
            cboDelimitador.Name = "cboDelimitador";
            cboDelimitador.Size = new Size(150, 23);
            cboDelimitador.TabIndex = 4;
            //
            // lblEncoding
            //
            lblEncoding.AutoSize = true;
            lblEncoding.Location = new Point(250, 52);
            lblEncoding.Name = "lblEncoding";
            lblEncoding.Size = new Size(61, 15);
            lblEncoding.TabIndex = 5;
            lblEncoding.Text = "Codificação:";
            //
            // cboEncoding
            //
            cboEncoding.DropDownStyle = ComboBoxStyle.DropDownList;
            cboEncoding.FormattingEnabled = true;
            cboEncoding.Items.AddRange(new object[] { "UTF-8", "Windows-1252 (ANSI)", "ISO-8859-1" });
            cboEncoding.Location = new Point(330, 49);
            cboEncoding.Name = "cboEncoding";
            cboEncoding.Size = new Size(170, 23);
            cboEncoding.TabIndex = 6;
            //
            // chkCabecalho
            //
            chkCabecalho.AutoSize = true;
            chkCabecalho.Checked = true;
            chkCabecalho.CheckState = CheckState.Checked;
            chkCabecalho.Location = new Point(530, 51);
            chkCabecalho.Name = "chkCabecalho";
            chkCabecalho.Size = new Size(200, 19);
            chkCabecalho.TabIndex = 7;
            chkCabecalho.Text = "Primeira linha é o cabeçalho";
            chkCabecalho.UseVisualStyleBackColor = true;
            //
            // pnlBanco
            //
            pnlBanco.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlBanco.Controls.Add(lblConexaoAviso);
            pnlBanco.Controls.Add(btnTestarConexao);
            pnlBanco.Controls.Add(chkMostrarConexao);
            pnlBanco.Controls.Add(txtConexao);
            pnlBanco.Controls.Add(lblConexao);
            pnlBanco.Location = new Point(10, 62);
            pnlBanco.Name = "pnlBanco";
            pnlBanco.Size = new Size(830, 96);
            pnlBanco.TabIndex = 3;
            pnlBanco.Visible = false;
            //
            // lblConexao
            //
            lblConexao.AutoSize = true;
            lblConexao.Location = new Point(6, 12);
            lblConexao.Name = "lblConexao";
            lblConexao.Size = new Size(98, 15);
            lblConexao.TabIndex = 0;
            lblConexao.Text = "String de conexão:";
            //
            // txtConexao
            //
            txtConexao.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtConexao.Location = new Point(110, 9);
            txtConexao.Name = "txtConexao";
            txtConexao.Size = new Size(710, 23);
            txtConexao.TabIndex = 1;
            txtConexao.UseSystemPasswordChar = true;
            //
            // chkMostrarConexao
            //
            chkMostrarConexao.AutoSize = true;
            chkMostrarConexao.Location = new Point(110, 44);
            chkMostrarConexao.Name = "chkMostrarConexao";
            chkMostrarConexao.Size = new Size(70, 19);
            chkMostrarConexao.TabIndex = 2;
            chkMostrarConexao.Text = "Mostrar";
            chkMostrarConexao.UseVisualStyleBackColor = true;
            chkMostrarConexao.CheckedChanged += chkMostrarConexao_CheckedChanged;
            //
            // btnTestarConexao
            //
            btnTestarConexao.Location = new Point(200, 40);
            btnTestarConexao.Name = "btnTestarConexao";
            btnTestarConexao.Size = new Size(140, 28);
            btnTestarConexao.TabIndex = 3;
            btnTestarConexao.Text = "Testar conexão";
            btnTestarConexao.UseVisualStyleBackColor = true;
            btnTestarConexao.Click += btnTestarConexao_Click;
            //
            // lblConexaoAviso
            //
            lblConexaoAviso.AutoSize = true;
            lblConexaoAviso.ForeColor = SystemColors.GrayText;
            lblConexaoAviso.Location = new Point(110, 74);
            lblConexaoAviso.Name = "lblConexaoAviso";
            lblConexaoAviso.Size = new Size(330, 15);
            lblConexaoAviso.TabIndex = 4;
            lblConexaoAviso.Text = "A string de conexão não é gravada em arquivo (somente em memória).";
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
            grpExecucao.Controls.Add(chkPrevia);
            grpExecucao.Controls.Add(cboGenerate);
            grpExecucao.Controls.Add(lblGenerate);
            grpExecucao.Location = new Point(16, 200);
            grpExecucao.Name = "grpExecucao";
            grpExecucao.Size = new Size(850, 190);
            grpExecucao.TabIndex = 1;
            grpExecucao.TabStop = false;
            grpExecucao.Text = "Geração";
            //
            // lblGenerate
            //
            lblGenerate.AutoSize = true;
            lblGenerate.Location = new Point(16, 31);
            lblGenerate.Name = "lblGenerate";
            lblGenerate.Size = new Size(64, 15);
            lblGenerate.TabIndex = 0;
            lblGenerate.Text = "Configuração:";
            //
            // cboGenerate
            //
            cboGenerate.DropDownStyle = ComboBoxStyle.DropDownList;
            cboGenerate.FormattingEnabled = true;
            cboGenerate.Location = new Point(100, 28);
            cboGenerate.Name = "cboGenerate";
            cboGenerate.Size = new Size(300, 23);
            cboGenerate.TabIndex = 1;
            //
            // chkPrevia
            //
            chkPrevia.AutoSize = true;
            chkPrevia.Location = new Point(100, 62);
            chkPrevia.Name = "chkPrevia";
            chkPrevia.Size = new Size(380, 19);
            chkPrevia.TabIndex = 2;
            chkPrevia.Text = "Gerar prévia mesmo com pendências (campos pendentes não são preenchidos)";
            chkPrevia.UseVisualStyleBackColor = true;
            //
            // btnGerar
            //
            btnGerar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGerar.Location = new Point(16, 98);
            btnGerar.Name = "btnGerar";
            btnGerar.Size = new Size(140, 34);
            btnGerar.TabIndex = 3;
            btnGerar.Text = "Gerar JSON";
            btnGerar.UseVisualStyleBackColor = true;
            btnGerar.Click += btnGerar_Click;
            //
            // btnVerJson
            //
            btnVerJson.Location = new Point(166, 98);
            btnVerJson.Name = "btnVerJson";
            btnVerJson.Size = new Size(130, 34);
            btnVerJson.TabIndex = 4;
            btnVerJson.Text = "Ver JSON";
            btnVerJson.UseVisualStyleBackColor = true;
            btnVerJson.Click += btnVerJson_Click;
            //
            // btnVerValidacao
            //
            btnVerValidacao.Location = new Point(306, 98);
            btnVerValidacao.Name = "btnVerValidacao";
            btnVerValidacao.Size = new Size(130, 34);
            btnVerValidacao.TabIndex = 5;
            btnVerValidacao.Text = "Ver validação";
            btnVerValidacao.UseVisualStyleBackColor = true;
            btnVerValidacao.Click += btnVerValidacao_Click;
            //
            // btnExportar
            //
            btnExportar.Location = new Point(446, 98);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(130, 34);
            btnExportar.TabIndex = 6;
            btnExportar.Text = "Exportar";
            btnExportar.UseVisualStyleBackColor = true;
            btnExportar.Click += btnExportar_Click;
            //
            // prgExecucao
            //
            prgExecucao.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            prgExecucao.Location = new Point(16, 146);
            prgExecucao.MarqueeAnimationSpeed = 30;
            prgExecucao.Name = "prgExecucao";
            prgExecucao.Size = new Size(818, 8);
            prgExecucao.Style = ProgressBarStyle.Marquee;
            prgExecucao.TabIndex = 7;
            prgExecucao.Visible = false;
            //
            // lblResumo
            //
            lblResumo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblResumo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblResumo.Location = new Point(16, 160);
            lblResumo.Name = "lblResumo";
            lblResumo.Size = new Size(818, 20);
            lblResumo.TabIndex = 8;
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
            grpFonte.ResumeLayout(false);
            grpFonte.PerformLayout();
            pnlBanco.ResumeLayout(false);
            pnlBanco.PerformLayout();
            pnlCsv.ResumeLayout(false);
            pnlCsv.PerformLayout();
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
        private CheckBox chkPrevia;
        private ComboBox cboGenerate;
        private Label lblGenerate;
        private GroupBox grpFonte;
        private Panel pnlBanco;
        private Label lblConexaoAviso;
        private Button btnTestarConexao;
        private CheckBox chkMostrarConexao;
        private TextBox txtConexao;
        private Label lblConexao;
        private Panel pnlCsv;
        private CheckBox chkCabecalho;
        private ComboBox cboEncoding;
        private Label lblEncoding;
        private ComboBox cboDelimitador;
        private Label lblDelimitador;
        private Button btnProcurar;
        private TextBox txtArquivo;
        private Label lblArquivo;
        private ComboBox cboFonte;
        private Label lblFonte;
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
