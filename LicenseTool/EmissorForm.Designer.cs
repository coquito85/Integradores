namespace LicenseTool
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
            grpCadastro = new GroupBox();
            tlpForm = new TableLayoutPanel();
            lblId = new Label();
            txtId = new TextBox();
            lblNome = new Label();
            txtNome = new TextBox();
            lblCns = new Label();
            txtCns = new TextBox();
            lblIbge = new Label();
            txtIbge = new TextBox();
            lblRespNome = new Label();
            txtRespNome = new TextBox();
            lblRespCargo = new Label();
            txtRespCargo = new TextBox();
            lblRespEmail = new Label();
            txtRespEmail = new TextBox();
            lblValidade = new Label();
            dtpValidade = new DateTimePicker();
            lblModelos = new Label();
            tlpModelos = new TableLayoutPanel();
            clbModelos = new CheckedListBox();
            txtOutroId = new TextBox();
            btnAdicionarId = new Button();
            flpAcoes = new FlowLayoutPanel();
            btnEmitir = new Button();
            btnSalvar = new Button();
            lblStatus = new Label();
            tlpRaiz.SuspendLayout();
            grpChave.SuspendLayout();
            tlpChave.SuspendLayout();
            flpChave.SuspendLayout();
            tlpCorpo.SuspendLayout();
            grpCartorios.SuspendLayout();
            tlpLista.SuspendLayout();
            flpListaBotoes.SuspendLayout();
            grpCadastro.SuspendLayout();
            tlpForm.SuspendLayout();
            tlpModelos.SuspendLayout();
            flpAcoes.SuspendLayout();
            SuspendLayout();
            // 
            // tlpRaiz
            // 
            tlpRaiz.ColumnCount = 1;
            tlpRaiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRaiz.Controls.Add(grpChave, 0, 0);
            tlpRaiz.Controls.Add(tlpCorpo, 0, 1);
            tlpRaiz.Controls.Add(flpAcoes, 0, 2);
            tlpRaiz.Controls.Add(lblStatus, 0, 3);
            tlpRaiz.Dock = DockStyle.Fill;
            tlpRaiz.Location = new Point(0, 0);
            tlpRaiz.Name = "tlpRaiz";
            tlpRaiz.Padding = new Padding(12);
            tlpRaiz.RowCount = 4;
            tlpRaiz.RowStyles.Add(new RowStyle());
            tlpRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpRaiz.RowStyles.Add(new RowStyle());
            tlpRaiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            tlpRaiz.Size = new Size(1024, 681);
            tlpRaiz.TabIndex = 0;
            // 
            // grpChave
            // 
            grpChave.AutoSize = true;
            grpChave.Controls.Add(tlpChave);
            grpChave.Dock = DockStyle.Top;
            grpChave.Location = new Point(15, 15);
            grpChave.Name = "grpChave";
            grpChave.Padding = new Padding(8);
            grpChave.Size = new Size(994, 75);
            grpChave.TabIndex = 0;
            grpChave.TabStop = false;
            grpChave.Text = "Chave privada do fornecedor (private.xml)";
            // 
            // tlpChave
            // 
            tlpChave.AutoSize = true;
            tlpChave.ColumnCount = 2;
            tlpChave.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpChave.ColumnStyles.Add(new ColumnStyle());
            tlpChave.Controls.Add(txtChavePrivada, 0, 0);
            tlpChave.Controls.Add(flpChave, 1, 0);
            tlpChave.Dock = DockStyle.Fill;
            tlpChave.Location = new Point(8, 25);
            tlpChave.Name = "tlpChave";
            tlpChave.RowCount = 1;
            tlpChave.RowStyles.Add(new RowStyle());
            tlpChave.Size = new Size(978, 42);
            tlpChave.TabIndex = 0;
            // 
            // txtChavePrivada
            // 
            txtChavePrivada.Dock = DockStyle.Fill;
            txtChavePrivada.Location = new Point(3, 3);
            txtChavePrivada.Name = "txtChavePrivada";
            txtChavePrivada.ReadOnly = true;
            txtChavePrivada.Size = new Size(803, 24);
            txtChavePrivada.TabIndex = 0;
            // 
            // flpChave
            // 
            flpChave.AutoSize = true;
            flpChave.Controls.Add(btnProcurarChave);
            flpChave.Controls.Add(btnGerarChaves);
            flpChave.Controls.Add(btnCopiarPublica);
            flpChave.Dock = DockStyle.Fill;
            flpChave.Location = new Point(812, 3);
            flpChave.Name = "flpChave";
            flpChave.Size = new Size(163, 108);
            flpChave.TabIndex = 1;
            // 
            // btnProcurarChave
            // 
            btnProcurarChave.AutoSize = true;
            btnProcurarChave.Location = new Point(3, 3);
            btnProcurarChave.MinimumSize = new Size(90, 30);
            btnProcurarChave.Name = "btnProcurarChave";
            btnProcurarChave.Padding = new Padding(8, 0, 8, 0);
            btnProcurarChave.Size = new Size(93, 30);
            btnProcurarChave.TabIndex = 0;
            btnProcurarChave.Text = "Procurar…";
            btnProcurarChave.UseVisualStyleBackColor = true;
            btnProcurarChave.Click += btnProcurarChave_Click;
            // 
            // btnGerarChaves
            // 
            btnGerarChaves.AutoSize = true;
            btnGerarChaves.Location = new Point(3, 39);
            btnGerarChaves.MinimumSize = new Size(90, 30);
            btnGerarChaves.Name = "btnGerarChaves";
            btnGerarChaves.Padding = new Padding(8, 0, 8, 0);
            btnGerarChaves.Size = new Size(157, 30);
            btnGerarChaves.TabIndex = 1;
            btnGerarChaves.Text = "Gerar novas chaves…";
            btnGerarChaves.UseVisualStyleBackColor = true;
            btnGerarChaves.Click += btnGerarChaves_Click;
            // 
            // btnCopiarPublica
            // 
            btnCopiarPublica.AutoSize = true;
            btnCopiarPublica.Location = new Point(3, 75);
            btnCopiarPublica.MinimumSize = new Size(90, 30);
            btnCopiarPublica.Name = "btnCopiarPublica";
            btnCopiarPublica.Padding = new Padding(8, 0, 8, 0);
            btnCopiarPublica.Size = new Size(156, 30);
            btnCopiarPublica.TabIndex = 2;
            btnCopiarPublica.Text = "Copiar chave pública";
            btnCopiarPublica.UseVisualStyleBackColor = true;
            btnCopiarPublica.Click += btnCopiarPublica_Click;
            // 
            // tlpCorpo
            // 
            tlpCorpo.ColumnCount = 2;
            tlpCorpo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 270F));
            tlpCorpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpCorpo.Controls.Add(grpCartorios, 0, 0);
            tlpCorpo.Controls.Add(grpCadastro, 1, 0);
            tlpCorpo.Dock = DockStyle.Fill;
            tlpCorpo.Location = new Point(15, 96);
            tlpCorpo.Name = "tlpCorpo";
            tlpCorpo.RowCount = 1;
            tlpCorpo.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpCorpo.Size = new Size(994, 502);
            tlpCorpo.TabIndex = 1;
            // 
            // grpCartorios
            // 
            grpCartorios.Controls.Add(tlpLista);
            grpCartorios.Dock = DockStyle.Fill;
            grpCartorios.Location = new Point(3, 3);
            grpCartorios.Name = "grpCartorios";
            grpCartorios.Padding = new Padding(8);
            grpCartorios.Size = new Size(264, 496);
            grpCartorios.TabIndex = 0;
            grpCartorios.TabStop = false;
            grpCartorios.Text = "Cartórios cadastrados";
            // 
            // tlpLista
            // 
            tlpLista.ColumnCount = 1;
            tlpLista.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpLista.Controls.Add(lstCartorios, 0, 0);
            tlpLista.Controls.Add(flpListaBotoes, 0, 1);
            tlpLista.Dock = DockStyle.Fill;
            tlpLista.Location = new Point(8, 25);
            tlpLista.Name = "tlpLista";
            tlpLista.RowCount = 2;
            tlpLista.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpLista.RowStyles.Add(new RowStyle());
            tlpLista.Size = new Size(248, 463);
            tlpLista.TabIndex = 0;
            // 
            // lstCartorios
            // 
            lstCartorios.Dock = DockStyle.Fill;
            lstCartorios.FormattingEnabled = true;
            lstCartorios.IntegralHeight = false;
            lstCartorios.Location = new Point(3, 3);
            lstCartorios.Name = "lstCartorios";
            lstCartorios.Size = new Size(242, 415);
            lstCartorios.TabIndex = 0;
            lstCartorios.SelectedIndexChanged += lstCartorios_SelectedIndexChanged;
            // 
            // flpListaBotoes
            // 
            flpListaBotoes.AutoSize = true;
            flpListaBotoes.Controls.Add(btnNovo);
            flpListaBotoes.Controls.Add(btnExcluir);
            flpListaBotoes.Dock = DockStyle.Fill;
            flpListaBotoes.Location = new Point(3, 424);
            flpListaBotoes.Name = "flpListaBotoes";
            flpListaBotoes.Size = new Size(242, 36);
            flpListaBotoes.TabIndex = 1;
            // 
            // btnNovo
            // 
            btnNovo.AutoSize = true;
            btnNovo.Location = new Point(3, 3);
            btnNovo.MinimumSize = new Size(90, 30);
            btnNovo.Name = "btnNovo";
            btnNovo.Padding = new Padding(8, 0, 8, 0);
            btnNovo.Size = new Size(90, 30);
            btnNovo.TabIndex = 0;
            btnNovo.Text = "Novo";
            btnNovo.UseVisualStyleBackColor = true;
            btnNovo.Click += btnNovo_Click;
            // 
            // btnExcluir
            // 
            btnExcluir.AutoSize = true;
            btnExcluir.Location = new Point(99, 3);
            btnExcluir.MinimumSize = new Size(90, 30);
            btnExcluir.Name = "btnExcluir";
            btnExcluir.Padding = new Padding(8, 0, 8, 0);
            btnExcluir.Size = new Size(90, 30);
            btnExcluir.TabIndex = 1;
            btnExcluir.Text = "Excluir";
            btnExcluir.UseVisualStyleBackColor = true;
            btnExcluir.Click += btnExcluir_Click;
            // 
            // grpCadastro
            // 
            grpCadastro.Controls.Add(tlpForm);
            grpCadastro.Dock = DockStyle.Fill;
            grpCadastro.Location = new Point(273, 3);
            grpCadastro.Name = "grpCadastro";
            grpCadastro.Padding = new Padding(8);
            grpCadastro.Size = new Size(718, 496);
            grpCadastro.TabIndex = 1;
            grpCadastro.TabStop = false;
            grpCadastro.Text = "Cadastro e licença";
            // 
            // tlpForm
            // 
            tlpForm.ColumnCount = 2;
            tlpForm.ColumnStyles.Add(new ColumnStyle());
            tlpForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpForm.Controls.Add(lblId, 0, 0);
            tlpForm.Controls.Add(txtId, 1, 0);
            tlpForm.Controls.Add(lblNome, 0, 1);
            tlpForm.Controls.Add(txtNome, 1, 1);
            tlpForm.Controls.Add(lblCns, 0, 2);
            tlpForm.Controls.Add(txtCns, 1, 2);
            tlpForm.Controls.Add(lblIbge, 0, 3);
            tlpForm.Controls.Add(txtIbge, 1, 3);
            tlpForm.Controls.Add(lblRespNome, 0, 4);
            tlpForm.Controls.Add(txtRespNome, 1, 4);
            tlpForm.Controls.Add(lblRespCargo, 0, 5);
            tlpForm.Controls.Add(txtRespCargo, 1, 5);
            tlpForm.Controls.Add(lblRespEmail, 0, 6);
            tlpForm.Controls.Add(txtRespEmail, 1, 6);
            tlpForm.Controls.Add(lblValidade, 0, 7);
            tlpForm.Controls.Add(dtpValidade, 1, 7);
            tlpForm.Controls.Add(lblModelos, 0, 8);
            tlpForm.Controls.Add(tlpModelos, 1, 8);
            tlpForm.Dock = DockStyle.Fill;
            tlpForm.Location = new Point(8, 25);
            tlpForm.Name = "tlpForm";
            tlpForm.RowCount = 9;
            tlpForm.RowStyles.Add(new RowStyle());
            tlpForm.RowStyles.Add(new RowStyle());
            tlpForm.RowStyles.Add(new RowStyle());
            tlpForm.RowStyles.Add(new RowStyle());
            tlpForm.RowStyles.Add(new RowStyle());
            tlpForm.RowStyles.Add(new RowStyle());
            tlpForm.RowStyles.Add(new RowStyle());
            tlpForm.RowStyles.Add(new RowStyle());
            tlpForm.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpForm.Size = new Size(702, 463);
            tlpForm.TabIndex = 0;
            // 
            // lblId
            // 
            lblId.Anchor = AnchorStyles.Left;
            lblId.AutoSize = true;
            lblId.Location = new Point(3, 9);
            lblId.Margin = new Padding(3, 8, 12, 3);
            lblId.Name = "lblId";
            lblId.Size = new Size(121, 17);
            lblId.TabIndex = 0;
            lblId.Text = "Código do cartório";
            // 
            // txtId
            // 
            txtId.Dock = DockStyle.Fill;
            txtId.Location = new Point(148, 3);
            txtId.Name = "txtId";
            txtId.Size = new Size(551, 24);
            txtId.TabIndex = 0;
            // 
            // lblNome
            // 
            lblNome.Anchor = AnchorStyles.Left;
            lblNome.AutoSize = true;
            lblNome.Location = new Point(3, 39);
            lblNome.Margin = new Padding(3, 8, 12, 3);
            lblNome.Name = "lblNome";
            lblNome.Size = new Size(114, 17);
            lblNome.TabIndex = 1;
            lblNome.Text = "Nome do cartório";
            // 
            // txtNome
            // 
            txtNome.Dock = DockStyle.Fill;
            txtNome.Location = new Point(148, 33);
            txtNome.Name = "txtNome";
            txtNome.Size = new Size(551, 24);
            txtNome.TabIndex = 1;
            // 
            // lblCns
            // 
            lblCns.Anchor = AnchorStyles.Left;
            lblCns.AutoSize = true;
            lblCns.Location = new Point(3, 69);
            lblCns.Margin = new Padding(3, 8, 12, 3);
            lblCns.Name = "lblCns";
            lblCns.Size = new Size(80, 17);
            lblCns.TabIndex = 2;
            lblCns.Text = "Código CNS";
            // 
            // txtCns
            // 
            txtCns.Dock = DockStyle.Fill;
            txtCns.Location = new Point(148, 63);
            txtCns.Name = "txtCns";
            txtCns.Size = new Size(551, 24);
            txtCns.TabIndex = 2;
            // 
            // lblIbge
            // 
            lblIbge.Anchor = AnchorStyles.Left;
            lblIbge.AutoSize = true;
            lblIbge.Location = new Point(3, 99);
            lblIbge.Margin = new Padding(3, 8, 12, 3);
            lblIbge.Name = "lblIbge";
            lblIbge.Size = new Size(81, 17);
            lblIbge.TabIndex = 3;
            lblIbge.Text = "Código IBGE";
            // 
            // txtIbge
            // 
            txtIbge.Dock = DockStyle.Fill;
            txtIbge.Location = new Point(148, 93);
            txtIbge.Name = "txtIbge";
            txtIbge.Size = new Size(551, 24);
            txtIbge.TabIndex = 3;
            // 
            // lblRespNome
            // 
            lblRespNome.Anchor = AnchorStyles.Left;
            lblRespNome.AutoSize = true;
            lblRespNome.Location = new Point(3, 129);
            lblRespNome.Margin = new Padding(3, 8, 12, 3);
            lblRespNome.Name = "lblRespNome";
            lblRespNome.Size = new Size(81, 17);
            lblRespNome.TabIndex = 4;
            lblRespNome.Text = "Responsável";
            // 
            // txtRespNome
            // 
            txtRespNome.Dock = DockStyle.Fill;
            txtRespNome.Location = new Point(148, 123);
            txtRespNome.Name = "txtRespNome";
            txtRespNome.Size = new Size(551, 24);
            txtRespNome.TabIndex = 4;
            // 
            // lblRespCargo
            // 
            lblRespCargo.Anchor = AnchorStyles.Left;
            lblRespCargo.AutoSize = true;
            lblRespCargo.Location = new Point(3, 159);
            lblRespCargo.Margin = new Padding(3, 8, 12, 3);
            lblRespCargo.Name = "lblRespCargo";
            lblRespCargo.Size = new Size(44, 17);
            lblRespCargo.TabIndex = 5;
            lblRespCargo.Text = "Cargo";
            // 
            // txtRespCargo
            // 
            txtRespCargo.Dock = DockStyle.Fill;
            txtRespCargo.Location = new Point(148, 153);
            txtRespCargo.Name = "txtRespCargo";
            txtRespCargo.Size = new Size(551, 24);
            txtRespCargo.TabIndex = 5;
            // 
            // lblRespEmail
            // 
            lblRespEmail.Anchor = AnchorStyles.Left;
            lblRespEmail.AutoSize = true;
            lblRespEmail.Location = new Point(3, 189);
            lblRespEmail.Margin = new Padding(3, 8, 12, 3);
            lblRespEmail.Name = "lblRespEmail";
            lblRespEmail.Size = new Size(44, 17);
            lblRespEmail.TabIndex = 6;
            lblRespEmail.Text = "E-mail";
            // 
            // txtRespEmail
            // 
            txtRespEmail.Dock = DockStyle.Fill;
            txtRespEmail.Location = new Point(148, 183);
            txtRespEmail.Name = "txtRespEmail";
            txtRespEmail.Size = new Size(551, 24);
            txtRespEmail.TabIndex = 6;
            // 
            // lblValidade
            // 
            lblValidade.Anchor = AnchorStyles.Left;
            lblValidade.AutoSize = true;
            lblValidade.Location = new Point(3, 219);
            lblValidade.Margin = new Padding(3, 8, 12, 3);
            lblValidade.Name = "lblValidade";
            lblValidade.Size = new Size(58, 17);
            lblValidade.TabIndex = 7;
            lblValidade.Text = "Validade";
            // 
            // dtpValidade
            // 
            dtpValidade.Format = DateTimePickerFormat.Short;
            dtpValidade.Location = new Point(148, 213);
            dtpValidade.Name = "dtpValidade";
            dtpValidade.ShowCheckBox = true;
            dtpValidade.Size = new Size(140, 24);
            dtpValidade.TabIndex = 7;
            // 
            // lblModelos
            // 
            lblModelos.AutoSize = true;
            lblModelos.Location = new Point(3, 248);
            lblModelos.Margin = new Padding(3, 8, 8, 3);
            lblModelos.Name = "lblModelos";
            lblModelos.Size = new Size(134, 17);
            lblModelos.TabIndex = 8;
            lblModelos.Text = "Modelos contratados";
            // 
            // tlpModelos
            // 
            tlpModelos.ColumnCount = 2;
            tlpModelos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpModelos.ColumnStyles.Add(new ColumnStyle());
            tlpModelos.Controls.Add(clbModelos, 0, 0);
            tlpModelos.Controls.Add(txtOutroId, 0, 1);
            tlpModelos.Controls.Add(btnAdicionarId, 1, 1);
            tlpModelos.Dock = DockStyle.Fill;
            tlpModelos.Location = new Point(148, 243);
            tlpModelos.Name = "tlpModelos";
            tlpModelos.RowCount = 2;
            tlpModelos.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpModelos.RowStyles.Add(new RowStyle());
            tlpModelos.Size = new Size(551, 217);
            tlpModelos.TabIndex = 8;
            // 
            // clbModelos
            // 
            clbModelos.CheckOnClick = true;
            tlpModelos.SetColumnSpan(clbModelos, 2);
            clbModelos.Dock = DockStyle.Fill;
            clbModelos.FormattingEnabled = true;
            clbModelos.IntegralHeight = false;
            clbModelos.Location = new Point(3, 3);
            clbModelos.Name = "clbModelos";
            clbModelos.Size = new Size(545, 175);
            clbModelos.TabIndex = 0;
            // 
            // txtOutroId
            // 
            txtOutroId.Dock = DockStyle.Fill;
            txtOutroId.Location = new Point(3, 184);
            txtOutroId.Name = "txtOutroId";
            txtOutroId.PlaceholderText = "id de um modelo que não está na lista";
            txtOutroId.Size = new Size(435, 24);
            txtOutroId.TabIndex = 1;
            // 
            // btnAdicionarId
            // 
            btnAdicionarId.AutoSize = true;
            btnAdicionarId.Location = new Point(444, 184);
            btnAdicionarId.MinimumSize = new Size(90, 30);
            btnAdicionarId.Name = "btnAdicionarId";
            btnAdicionarId.Padding = new Padding(8, 0, 8, 0);
            btnAdicionarId.Size = new Size(104, 30);
            btnAdicionarId.TabIndex = 2;
            btnAdicionarId.Text = "Adicionar id";
            btnAdicionarId.UseVisualStyleBackColor = true;
            btnAdicionarId.Click += btnAdicionarId_Click;
            // 
            // flpAcoes
            // 
            flpAcoes.AutoSize = true;
            flpAcoes.Controls.Add(btnEmitir);
            flpAcoes.Controls.Add(btnSalvar);
            flpAcoes.Dock = DockStyle.Fill;
            flpAcoes.FlowDirection = FlowDirection.RightToLeft;
            flpAcoes.Location = new Point(15, 604);
            flpAcoes.Name = "flpAcoes";
            flpAcoes.Size = new Size(994, 36);
            flpAcoes.TabIndex = 2;
            // 
            // btnEmitir
            // 
            btnEmitir.AutoSize = true;
            btnEmitir.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnEmitir.Location = new Point(830, 3);
            btnEmitir.MinimumSize = new Size(90, 30);
            btnEmitir.Name = "btnEmitir";
            btnEmitir.Padding = new Padding(8, 0, 8, 0);
            btnEmitir.Size = new Size(161, 30);
            btnEmitir.TabIndex = 0;
            btnEmitir.Text = "Emitir licença (.lic)…";
            btnEmitir.UseVisualStyleBackColor = true;
            btnEmitir.Click += btnEmitir_Click;
            // 
            // btnSalvar
            // 
            btnSalvar.AutoSize = true;
            btnSalvar.Location = new Point(700, 3);
            btnSalvar.MinimumSize = new Size(90, 30);
            btnSalvar.Name = "btnSalvar";
            btnSalvar.Padding = new Padding(8, 0, 8, 0);
            btnSalvar.Size = new Size(124, 30);
            btnSalvar.TabIndex = 1;
            btnSalvar.Text = "Salvar cadastro";
            btnSalvar.UseVisualStyleBackColor = true;
            btnSalvar.Click += btnSalvar_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoEllipsis = true;
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.Location = new Point(15, 643);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(994, 26);
            lblStatus.TabIndex = 3;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // EmissorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1024, 681);
            Controls.Add(tlpRaiz);
            Font = new Font("Segoe UI", 9.5F);
            MinimumSize = new Size(900, 620);
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
            grpCartorios.ResumeLayout(false);
            tlpLista.ResumeLayout(false);
            tlpLista.PerformLayout();
            flpListaBotoes.ResumeLayout(false);
            flpListaBotoes.PerformLayout();
            grpCadastro.ResumeLayout(false);
            tlpForm.ResumeLayout(false);
            tlpForm.PerformLayout();
            tlpModelos.ResumeLayout(false);
            tlpModelos.PerformLayout();
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
        private GroupBox grpCadastro;
        private TableLayoutPanel tlpForm;
        private System.Windows.Forms.Label lblId;
        private TextBox txtId;
        private System.Windows.Forms.Label lblNome;
        private TextBox txtNome;
        private System.Windows.Forms.Label lblCns;
        private TextBox txtCns;
        private System.Windows.Forms.Label lblIbge;
        private TextBox txtIbge;
        private System.Windows.Forms.Label lblRespNome;
        private TextBox txtRespNome;
        private System.Windows.Forms.Label lblRespCargo;
        private TextBox txtRespCargo;
        private System.Windows.Forms.Label lblRespEmail;
        private TextBox txtRespEmail;
        private System.Windows.Forms.Label lblValidade;
        private DateTimePicker dtpValidade;
        private System.Windows.Forms.Label lblModelos;
        private TableLayoutPanel tlpModelos;
        private CheckedListBox clbModelos;
        private TextBox txtOutroId;
        private Button btnAdicionarId;
        private FlowLayoutPanel flpAcoes;
        private Button btnEmitir;
        private Button btnSalvar;
        private System.Windows.Forms.Label lblStatus;
    }
}
