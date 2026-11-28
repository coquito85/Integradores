namespace EmissorLicencas
{
    partial class EmissorForm
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

        #region Código gerado pelo Designer

        private void InitializeComponent()
        {
            tlpRaiz = new TableLayoutPanel();
            grpChave = new GroupBox();
            tlpChave = new TableLayoutPanel();
            txtChavePrivada = new TextBox();
            flpChave = new FlowLayoutPanel();
            btnProcurarChave = new Button();
            btnGerarChaves = new Button();
            btnCopiarPublica = new Button();
            tlpCorpo = new TableLayoutPanel();
            grpCartorios = new GroupBox();
            tlpLista = new TableLayoutPanel();
            lstCartorios = new ListBox();
            flpListaBotoes = new FlowLayoutPanel();
            btnNovo = new Button();
            btnExcluir = new Button();
            btnSolicitacao = new Button();
            tlpDireita = new TableLayoutPanel();
            grpDados = new GroupBox();
            tlpDados = new TableLayoutPanel();
            lblNome = new System.Windows.Forms.Label();
            txtNome = new TextBox();
            lblCns = new System.Windows.Forms.Label();
            txtCns = new TextBox();
            lblIbge = new System.Windows.Forms.Label();
            txtIbge = new TextBox();
            grpResp = new GroupBox();
            tlpResp = new TableLayoutPanel();
            lblRespNome = new System.Windows.Forms.Label();
            txtRespNome = new TextBox();
            lblRespCargo = new System.Windows.Forms.Label();
            txtRespCargo = new TextBox();
            lblRespEmail = new System.Windows.Forms.Label();
            txtRespEmail = new TextBox();
            grpLicenca = new GroupBox();
            tlpLicenca = new TableLayoutPanel();
            lblPeriodo = new System.Windows.Forms.Label();
            flpPeriodo = new FlowLayoutPanel();
            dtpInicio = new DateTimePicker();
            lblAte = new System.Windows.Forms.Label();
            dtpFim = new DateTimePicker();
            chkVincular = new CheckBox();
            lblModelos = new System.Windows.Forms.Label();
            clbModelos = new CheckedListBox();
            flpAcoes = new FlowLayoutPanel();
            btnEmitir = new Button();
            btnSalvar = new Button();
            lblStatus = new System.Windows.Forms.Label();
            tlpRaiz.SuspendLayout();
            grpChave.SuspendLayout();
            tlpChave.SuspendLayout();
            flpChave.SuspendLayout();
            tlpCorpo.SuspendLayout();
            grpCartorios.SuspendLayout();
            tlpLista.SuspendLayout();
            flpListaBotoes.SuspendLayout();
            tlpDireita.SuspendLayout();
            grpDados.SuspendLayout();
            tlpDados.SuspendLayout();
            grpResp.SuspendLayout();
            tlpResp.SuspendLayout();
            grpLicenca.SuspendLayout();
            tlpLicenca.SuspendLayout();
            flpPeriodo.SuspendLayout();
            flpAcoes.SuspendLayout();
            SuspendLayout();
            // tlpRaiz
            tlpRaiz.ColumnCount = 1;
            tlpRaiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRaiz.Dock = DockStyle.Fill;
            tlpRaiz.Name = "tlpRaiz";
            tlpRaiz.Padding = new Padding(16);
            tlpRaiz.RowCount = 4;
            tlpRaiz.RowStyles.Add(new RowStyle());
            tlpRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpRaiz.RowStyles.Add(new RowStyle());
            tlpRaiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tlpRaiz.TabIndex = 0;

            // grpChave
            grpChave.AutoSize = true;
            grpChave.Dock = DockStyle.Top;
            grpChave.Margin = new Padding(0, 0, 0, 14);
            grpChave.Name = "grpChave";
            grpChave.Padding = new Padding(14, 8, 14, 14);
            grpChave.TabStop = false;
            grpChave.Text = "Chave privada do fornecedor (private.xml)";

            // tlpChave
            tlpChave.AutoSize = true;
            tlpChave.ColumnCount = 2;
            tlpChave.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpChave.ColumnStyles.Add(new ColumnStyle());
            tlpChave.Dock = DockStyle.Fill;
            tlpChave.Name = "tlpChave";
            tlpChave.RowCount = 1;
            tlpChave.RowStyles.Add(new RowStyle());
            tlpChave.TabIndex = 0;

            // txtChavePrivada
            txtChavePrivada.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtChavePrivada.Margin = new Padding(3, 3, 12, 3);
            txtChavePrivada.Name = "txtChavePrivada";
            txtChavePrivada.ReadOnly = true;
            txtChavePrivada.TabIndex = 0;

            // flpChave
            flpChave.Anchor = AnchorStyles.Left;
            flpChave.AutoSize = true;
            flpChave.Margin = new Padding(0);
            flpChave.Name = "flpChave";
            flpChave.TabIndex = 1;

            // btnProcurarChave
            btnProcurarChave.AutoSize = true;
            btnProcurarChave.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnProcurarChave.Margin = new Padding(3, 3, 9, 3);
            btnProcurarChave.MinimumSize = new Size(110, 34);
            btnProcurarChave.Name = "btnProcurarChave";
            btnProcurarChave.Padding = new Padding(10, 0, 10, 0);
            btnProcurarChave.TabIndex = 0;
            btnProcurarChave.Text = "Procurar…";
            btnProcurarChave.UseVisualStyleBackColor = true;
            btnProcurarChave.Click += btnProcurarChave_Click;

            // btnGerarChaves
            btnGerarChaves.AutoSize = true;
            btnGerarChaves.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnGerarChaves.Margin = new Padding(3, 3, 9, 3);
            btnGerarChaves.MinimumSize = new Size(110, 34);
            btnGerarChaves.Name = "btnGerarChaves";
            btnGerarChaves.Padding = new Padding(10, 0, 10, 0);
            btnGerarChaves.TabIndex = 1;
            btnGerarChaves.Text = "Gerar novas chaves…";
            btnGerarChaves.UseVisualStyleBackColor = true;
            btnGerarChaves.Click += btnGerarChaves_Click;

            // btnCopiarPublica
            btnCopiarPublica.AutoSize = true;
            btnCopiarPublica.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnCopiarPublica.Margin = new Padding(3, 3, 3, 3);
            btnCopiarPublica.MinimumSize = new Size(110, 34);
            btnCopiarPublica.Name = "btnCopiarPublica";
            btnCopiarPublica.Padding = new Padding(10, 0, 10, 0);
            btnCopiarPublica.TabIndex = 2;
            btnCopiarPublica.Text = "Copiar chave pública";
            btnCopiarPublica.UseVisualStyleBackColor = true;
            btnCopiarPublica.Click += btnCopiarPublica_Click;

            // tlpCorpo
            tlpCorpo.ColumnCount = 2;
            tlpCorpo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));
            tlpCorpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpCorpo.Dock = DockStyle.Fill;
            tlpCorpo.Name = "tlpCorpo";
            tlpCorpo.RowCount = 1;
            tlpCorpo.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpCorpo.TabIndex = 1;

            // grpCartorios
            grpCartorios.Dock = DockStyle.Fill;
            grpCartorios.Margin = new Padding(0, 0, 14, 0);
            grpCartorios.Name = "grpCartorios";
            grpCartorios.Padding = new Padding(14, 8, 14, 14);
            grpCartorios.TabStop = false;
            grpCartorios.Text = "Cartórios cadastrados";

            // tlpLista
            tlpLista.ColumnCount = 1;
            tlpLista.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpLista.Dock = DockStyle.Fill;
            tlpLista.Name = "tlpLista";
            tlpLista.RowCount = 2;
            tlpLista.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpLista.RowStyles.Add(new RowStyle());
            tlpLista.TabIndex = 0;

            lstCartorios.Dock = DockStyle.Fill;
            lstCartorios.FormattingEnabled = true;
            lstCartorios.IntegralHeight = false;
            lstCartorios.Margin = new Padding(3, 4, 3, 10);
            lstCartorios.Name = "lstCartorios";
            lstCartorios.TabIndex = 0;
            lstCartorios.SelectedIndexChanged += lstCartorios_SelectedIndexChanged;

            // flpListaBotoes
            flpListaBotoes.AutoSize = true;
            flpListaBotoes.Dock = DockStyle.Fill;
            flpListaBotoes.Margin = new Padding(0);
            flpListaBotoes.Name = "flpListaBotoes";
            flpListaBotoes.TabIndex = 1;

            // btnNovo
            btnNovo.AutoSize = true;
            btnNovo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnNovo.Margin = new Padding(3, 3, 9, 3);
            btnNovo.MinimumSize = new Size(110, 34);
            btnNovo.Name = "btnNovo";
            btnNovo.Padding = new Padding(10, 0, 10, 0);
            btnNovo.TabIndex = 0;
            btnNovo.Text = "Novo";
            btnNovo.UseVisualStyleBackColor = true;
            btnNovo.Click += btnNovo_Click;

            // btnExcluir
            btnExcluir.AutoSize = true;
            btnExcluir.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnExcluir.Margin = new Padding(3, 3, 9, 3);
            btnExcluir.MinimumSize = new Size(110, 34);
            btnExcluir.Name = "btnExcluir";
            btnExcluir.Padding = new Padding(10, 0, 10, 0);
            btnExcluir.TabIndex = 1;
            btnExcluir.Text = "Excluir";
            btnExcluir.UseVisualStyleBackColor = true;
            btnExcluir.Click += btnExcluir_Click;

            // btnSolicitacao
            btnSolicitacao.AutoSize = true;
            btnSolicitacao.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSolicitacao.Margin = new Padding(3);
            btnSolicitacao.MinimumSize = new Size(110, 34);
            btnSolicitacao.Name = "btnSolicitacao";
            btnSolicitacao.Padding = new Padding(10, 0, 10, 0);
            btnSolicitacao.TabIndex = 2;
            btnSolicitacao.Text = "Ler solicitação…";
            btnSolicitacao.UseVisualStyleBackColor = true;
            btnSolicitacao.Click += btnSolicitacao_Click;

            // tlpDireita
            tlpDireita.ColumnCount = 1;
            tlpDireita.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpDireita.Dock = DockStyle.Fill;
            tlpDireita.Name = "tlpDireita";
            tlpDireita.RowCount = 3;
            tlpDireita.RowStyles.Add(new RowStyle());
            tlpDireita.RowStyles.Add(new RowStyle());
            tlpDireita.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpDireita.TabIndex = 1;

            // grpDados
            grpDados.AutoSize = true;
            grpDados.Dock = DockStyle.Top;
            grpDados.Margin = new Padding(0, 0, 0, 14);
            grpDados.Name = "grpDados";
            grpDados.Padding = new Padding(14, 8, 14, 14);
            grpDados.TabStop = false;
            grpDados.Text = "Dados do cartório";

            // tlpDados
            tlpDados.AutoSize = true;
            tlpDados.ColumnCount = 2;
            tlpDados.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            tlpDados.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpDados.Dock = DockStyle.Fill;
            tlpDados.Name = "tlpDados";
            tlpDados.RowCount = 3;
            tlpDados.RowStyles.Add(new RowStyle());
            tlpDados.RowStyles.Add(new RowStyle());
            tlpDados.RowStyles.Add(new RowStyle());
            tlpDados.TabIndex = 0;

            // lblNome
            lblNome.Anchor = AnchorStyles.Left;
            lblNome.AutoSize = true;
            lblNome.Margin = new Padding(3, 0, 12, 0);
            lblNome.Name = "lblNome";
            lblNome.Text = "Nome do cartório";

            // txtNome
            txtNome.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtNome.Margin = new Padding(3, 6, 3, 6);
            txtNome.Name = "txtNome";
            txtNome.TabIndex = 0;

            // lblCns
            lblCns.Anchor = AnchorStyles.Left;
            lblCns.AutoSize = true;
            lblCns.Margin = new Padding(3, 0, 12, 0);
            lblCns.Name = "lblCns";
            lblCns.Text = "Código CNS";

            // txtCns
            txtCns.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtCns.Margin = new Padding(3, 6, 3, 6);
            txtCns.Name = "txtCns";
            txtCns.TabIndex = 1;

            // lblIbge
            lblIbge.Anchor = AnchorStyles.Left;
            lblIbge.AutoSize = true;
            lblIbge.Margin = new Padding(3, 0, 12, 0);
            lblIbge.Name = "lblIbge";
            lblIbge.Text = "Código IBGE";

            // txtIbge
            txtIbge.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtIbge.Margin = new Padding(3, 6, 3, 6);
            txtIbge.Name = "txtIbge";
            txtIbge.TabIndex = 2;

            // grpResp
            grpResp.AutoSize = true;
            grpResp.Dock = DockStyle.Top;
            grpResp.Margin = new Padding(0, 0, 0, 14);
            grpResp.Name = "grpResp";
            grpResp.Padding = new Padding(14, 8, 14, 14);
            grpResp.TabStop = false;
            grpResp.Text = "Responsável";

            // tlpResp
            tlpResp.AutoSize = true;
            tlpResp.ColumnCount = 2;
            tlpResp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            tlpResp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpResp.Dock = DockStyle.Fill;
            tlpResp.Name = "tlpResp";
            tlpResp.RowCount = 3;
            tlpResp.RowStyles.Add(new RowStyle());
            tlpResp.RowStyles.Add(new RowStyle());
            tlpResp.RowStyles.Add(new RowStyle());
            tlpResp.TabIndex = 0;

            // lblRespNome
            lblRespNome.Anchor = AnchorStyles.Left;
            lblRespNome.AutoSize = true;
            lblRespNome.Margin = new Padding(3, 0, 12, 0);
            lblRespNome.Name = "lblRespNome";
            lblRespNome.Text = "Nome";

            // txtRespNome
            txtRespNome.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtRespNome.Margin = new Padding(3, 6, 3, 6);
            txtRespNome.Name = "txtRespNome";
            txtRespNome.TabIndex = 0;

            // lblRespCargo
            lblRespCargo.Anchor = AnchorStyles.Left;
            lblRespCargo.AutoSize = true;
            lblRespCargo.Margin = new Padding(3, 0, 12, 0);
            lblRespCargo.Name = "lblRespCargo";
            lblRespCargo.Text = "Cargo";

            // txtRespCargo
            txtRespCargo.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtRespCargo.Margin = new Padding(3, 6, 3, 6);
            txtRespCargo.Name = "txtRespCargo";
            txtRespCargo.TabIndex = 1;

            // lblRespEmail
            lblRespEmail.Anchor = AnchorStyles.Left;
            lblRespEmail.AutoSize = true;
            lblRespEmail.Margin = new Padding(3, 0, 12, 0);
            lblRespEmail.Name = "lblRespEmail";
            lblRespEmail.Text = "E-mail";

            // txtRespEmail
            txtRespEmail.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtRespEmail.Margin = new Padding(3, 6, 3, 6);
            txtRespEmail.Name = "txtRespEmail";
            txtRespEmail.TabIndex = 2;

            // grpLicenca
            grpLicenca.Dock = DockStyle.Fill;
            grpLicenca.Margin = new Padding(0);
            grpLicenca.Name = "grpLicenca";
            grpLicenca.Padding = new Padding(14, 8, 14, 14);
            grpLicenca.TabStop = false;
            grpLicenca.Text = "Licença";

            // tlpLicenca
            tlpLicenca.ColumnCount = 2;
            tlpLicenca.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            tlpLicenca.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpLicenca.Dock = DockStyle.Fill;
            tlpLicenca.Name = "tlpLicenca";
            tlpLicenca.RowCount = 3;
            tlpLicenca.RowStyles.Add(new RowStyle());
            tlpLicenca.RowStyles.Add(new RowStyle());
            tlpLicenca.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpLicenca.TabIndex = 0;

            // lblPeriodo
            lblPeriodo.Anchor = AnchorStyles.Left;
            lblPeriodo.AutoSize = true;
            lblPeriodo.Margin = new Padding(3, 0, 12, 0);
            lblPeriodo.Name = "lblPeriodo";
            lblPeriodo.Text = "Período de validade";

            // flpPeriodo
            flpPeriodo.Anchor = AnchorStyles.Left;
            flpPeriodo.AutoSize = true;
            flpPeriodo.Margin = new Padding(0);
            flpPeriodo.Name = "flpPeriodo";
            flpPeriodo.TabIndex = 0;

            // dtpInicio
            dtpInicio.Format = DateTimePickerFormat.Short;
            dtpInicio.Margin = new Padding(3, 6, 3, 6);
            dtpInicio.Name = "dtpInicio";
            dtpInicio.ShowCheckBox = true;
            dtpInicio.Size = new Size(150, 27);
            dtpInicio.TabIndex = 0;

            // lblAte
            lblAte.Anchor = AnchorStyles.Left;
            lblAte.AutoSize = true;
            lblAte.Margin = new Padding(9, 10, 9, 0);
            lblAte.Name = "lblAte";
            lblAte.Text = "até";

            // dtpFim
            dtpFim.Format = DateTimePickerFormat.Short;
            dtpFim.Margin = new Padding(3, 6, 3, 6);
            dtpFim.Name = "dtpFim";
            dtpFim.ShowCheckBox = true;
            dtpFim.Size = new Size(150, 27);
            dtpFim.TabIndex = 2;

            // chkVincular
            chkVincular.Anchor = AnchorStyles.Left;
            chkVincular.AutoSize = true;
            chkVincular.Enabled = false;
            chkVincular.Margin = new Padding(3, 4, 3, 4);
            chkVincular.Name = "chkVincular";
            chkVincular.TabIndex = 1;
            chkVincular.Text = "Vincular a um computador (use “Ler solicitação…”)";
            chkVincular.UseVisualStyleBackColor = true;

            // lblModelos
            lblModelos.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblModelos.AutoSize = true;
            lblModelos.Margin = new Padding(3, 12, 12, 0);
            lblModelos.Name = "lblModelos";
            lblModelos.Text = "Modelos contratados";

            clbModelos.CheckOnClick = true;
            clbModelos.Dock = DockStyle.Fill;
            clbModelos.FormattingEnabled = true;
            clbModelos.IntegralHeight = false;
            clbModelos.Margin = new Padding(3, 6, 3, 3);
            clbModelos.Name = "clbModelos";
            clbModelos.TabIndex = 2;

            // flpAcoes
            flpAcoes.AutoSize = true;
            flpAcoes.Dock = DockStyle.Fill;
            flpAcoes.FlowDirection = FlowDirection.RightToLeft;
            flpAcoes.Margin = new Padding(0, 14, 0, 0);
            flpAcoes.Name = "flpAcoes";
            flpAcoes.TabIndex = 2;

            // btnEmitir
            btnEmitir.AutoSize = true;
            btnEmitir.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnEmitir.Margin = new Padding(12, 3, 3, 3);
            btnEmitir.MinimumSize = new Size(110, 34);
            btnEmitir.Name = "btnEmitir";
            btnEmitir.Padding = new Padding(10, 0, 10, 0);
            btnEmitir.TabIndex = 0;
            btnEmitir.Text = "Emitir licença (.lic)…";
            btnEmitir.UseVisualStyleBackColor = true;
            btnEmitir.BackColor = Color.FromArgb(21, 101, 192);
            btnEmitir.FlatAppearance.BorderSize = 0;
            btnEmitir.FlatStyle = FlatStyle.Flat;
            btnEmitir.Font = new System.Drawing.Font("Segoe UI", 10F, FontStyle.Bold);
            btnEmitir.ForeColor = Color.White;
            btnEmitir.MinimumSize = new Size(200, 40);
            btnEmitir.Click += btnEmitir_Click;

            // btnSalvar
            btnSalvar.AutoSize = true;
            btnSalvar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSalvar.Margin = new Padding(3);
            btnSalvar.MinimumSize = new Size(110, 34);
            btnSalvar.Name = "btnSalvar";
            btnSalvar.Padding = new Padding(10, 0, 10, 0);
            btnSalvar.TabIndex = 1;
            btnSalvar.Text = "Salvar cadastro";
            btnSalvar.UseVisualStyleBackColor = true;
            btnSalvar.Click += btnSalvar_Click;

            lblStatus.AutoEllipsis = true;
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.ForeColor = SystemColors.GrayText;
            lblStatus.Margin = new Padding(3, 0, 3, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 3;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;

            // ---- hierarquia
            tlpRaiz.Controls.Add(grpChave, 0, 0);
            tlpRaiz.Controls.Add(tlpCorpo, 0, 1);
            tlpRaiz.Controls.Add(flpAcoes, 0, 2);
            tlpRaiz.Controls.Add(lblStatus, 0, 3);
            grpChave.Controls.Add(tlpChave);
            tlpChave.Controls.Add(txtChavePrivada, 0, 0);
            tlpChave.Controls.Add(flpChave, 1, 0);
            flpChave.Controls.Add(btnProcurarChave);
            flpChave.Controls.Add(btnGerarChaves);
            flpChave.Controls.Add(btnCopiarPublica);
            tlpCorpo.Controls.Add(grpCartorios, 0, 0);
            tlpCorpo.Controls.Add(tlpDireita, 1, 0);
            grpCartorios.Controls.Add(tlpLista);
            tlpLista.Controls.Add(lstCartorios, 0, 0);
            tlpLista.Controls.Add(flpListaBotoes, 0, 1);
            flpListaBotoes.Controls.Add(btnNovo);
            flpListaBotoes.Controls.Add(btnExcluir);
            flpListaBotoes.Controls.Add(btnSolicitacao);
            tlpDireita.Controls.Add(grpDados, 0, 0);
            tlpDireita.Controls.Add(grpResp, 0, 1);
            tlpDireita.Controls.Add(grpLicenca, 0, 2);
            grpDados.Controls.Add(tlpDados);
            tlpDados.Controls.Add(lblNome, 0, 0);
            tlpDados.Controls.Add(txtNome, 1, 0);
            tlpDados.Controls.Add(lblCns, 0, 1);
            tlpDados.Controls.Add(txtCns, 1, 1);
            tlpDados.Controls.Add(lblIbge, 0, 2);
            tlpDados.Controls.Add(txtIbge, 1, 2);
            grpResp.Controls.Add(tlpResp);
            tlpResp.Controls.Add(lblRespNome, 0, 0);
            tlpResp.Controls.Add(txtRespNome, 1, 0);
            tlpResp.Controls.Add(lblRespCargo, 0, 1);
            tlpResp.Controls.Add(txtRespCargo, 1, 1);
            tlpResp.Controls.Add(lblRespEmail, 0, 2);
            tlpResp.Controls.Add(txtRespEmail, 1, 2);
            grpLicenca.Controls.Add(tlpLicenca);
            tlpLicenca.Controls.Add(lblPeriodo, 0, 0);
            tlpLicenca.Controls.Add(flpPeriodo, 1, 0);
            flpPeriodo.Controls.Add(dtpInicio);
            flpPeriodo.Controls.Add(lblAte);
            flpPeriodo.Controls.Add(dtpFim);
            tlpLicenca.Controls.Add(chkVincular, 1, 1);
            tlpLicenca.Controls.Add(lblModelos, 0, 2);
            tlpLicenca.Controls.Add(clbModelos, 1, 2);
            flpAcoes.Controls.Add(btnEmitir);
            flpAcoes.Controls.Add(btnSalvar);
            // 
            // EmissorForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1120, 720);
            Controls.Add(tlpRaiz);
            Font = new System.Drawing.Font("Segoe UI", 10F);
            MinimumSize = new Size(980, 680);
            Name = "EmissorForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Emissor de Licenças — GeneratorJS";
            tlpRaiz.ResumeLayout(false);
            tlpRaiz.PerformLayout();
            grpChave.ResumeLayout(false);
            grpChave.PerformLayout();
            tlpChave.ResumeLayout(false);
            tlpChave.PerformLayout();
            flpChave.ResumeLayout(false);
            flpChave.PerformLayout();
            tlpCorpo.ResumeLayout(false);
            tlpCorpo.PerformLayout();
            grpCartorios.ResumeLayout(false);
            grpCartorios.PerformLayout();
            tlpLista.ResumeLayout(false);
            tlpLista.PerformLayout();
            flpListaBotoes.ResumeLayout(false);
            flpListaBotoes.PerformLayout();
            tlpDireita.ResumeLayout(false);
            tlpDireita.PerformLayout();
            grpDados.ResumeLayout(false);
            grpDados.PerformLayout();
            tlpDados.ResumeLayout(false);
            tlpDados.PerformLayout();
            grpResp.ResumeLayout(false);
            grpResp.PerformLayout();
            tlpResp.ResumeLayout(false);
            tlpResp.PerformLayout();
            grpLicenca.ResumeLayout(false);
            grpLicenca.PerformLayout();
            tlpLicenca.ResumeLayout(false);
            tlpLicenca.PerformLayout();
            flpPeriodo.ResumeLayout(false);
            flpPeriodo.PerformLayout();
            flpAcoes.ResumeLayout(false);
            flpAcoes.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tlpRaiz;
        private GroupBox grpChave;
        private TableLayoutPanel tlpChave;
        private TextBox txtChavePrivada;
        private FlowLayoutPanel flpChave;
        private Button btnProcurarChave;
        private Button btnGerarChaves;
        private Button btnCopiarPublica;
        private TableLayoutPanel tlpCorpo;
        private GroupBox grpCartorios;
        private TableLayoutPanel tlpLista;
        private ListBox lstCartorios;
        private FlowLayoutPanel flpListaBotoes;
        private Button btnNovo;
        private Button btnExcluir;
        private Button btnSolicitacao;
        private TableLayoutPanel tlpDireita;
        private GroupBox grpDados;
        private TableLayoutPanel tlpDados;
        private System.Windows.Forms.Label lblNome;
        private TextBox txtNome;
        private System.Windows.Forms.Label lblCns;
        private TextBox txtCns;
        private System.Windows.Forms.Label lblIbge;
        private TextBox txtIbge;
        private GroupBox grpResp;
        private TableLayoutPanel tlpResp;
        private System.Windows.Forms.Label lblRespNome;
        private TextBox txtRespNome;
        private System.Windows.Forms.Label lblRespCargo;
        private TextBox txtRespCargo;
        private System.Windows.Forms.Label lblRespEmail;
        private TextBox txtRespEmail;
        private GroupBox grpLicenca;
        private TableLayoutPanel tlpLicenca;
        private System.Windows.Forms.Label lblPeriodo;
        private FlowLayoutPanel flpPeriodo;
        private DateTimePicker dtpInicio;
        private System.Windows.Forms.Label lblAte;
        private DateTimePicker dtpFim;
        private CheckBox chkVincular;
        private System.Windows.Forms.Label lblModelos;
        private CheckedListBox clbModelos;
        private FlowLayoutPanel flpAcoes;
        private Button btnEmitir;
        private Button btnSalvar;
        private System.Windows.Forms.Label lblStatus;
    }
}
