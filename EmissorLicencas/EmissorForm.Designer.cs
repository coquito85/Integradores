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
            grpChave = new GroupBox();
            btnCopiarPublica = new Button();
            txtChavePrivada = new TextBox();
            btnGerarChaves = new Button();
            btnProcurarChave = new Button();
            grpCartorios = new GroupBox();
            lstCartorios = new ListBox();
            btnNovo = new Button();
            btnExcluir = new Button();
            tlpDados = new TableLayoutPanel();
            txtRespEmail = new TextBox();
            lblRespEmail = new Label();
            txtRespNome = new TextBox();
            txtRespCargo = new TextBox();
            lblRespCargo = new Label();
            lblRespNome = new Label();
            lblNome = new Label();
            txtNome = new TextBox();
            lblCns = new Label();
            txtCns = new TextBox();
            lblIbge = new Label();
            txtIbge = new TextBox();
            tlpLicenca = new TableLayoutPanel();
            lblPeriodo = new Label();
            flpPeriodo = new FlowLayoutPanel();
            dtpInicio = new DateTimePicker();
            lblAte = new Label();
            dtpFim = new DateTimePicker();
            lblModelos = new Label();
            clbModelos = new CheckedListBox();
            btnEmitir = new Button();
            btnSalvar = new Button();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            lblStatus = new Label();
            grpChave.SuspendLayout();
            grpCartorios.SuspendLayout();
            tlpDados.SuspendLayout();
            tlpLicenca.SuspendLayout();
            flpPeriodo.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // grpChave
            // 
            grpChave.Controls.Add(btnCopiarPublica);
            grpChave.Controls.Add(txtChavePrivada);
            grpChave.Controls.Add(btnGerarChaves);
            grpChave.Controls.Add(btnProcurarChave);
            grpChave.Location = new Point(30, 39);
            grpChave.Margin = new Padding(0, 0, 0, 14);
            grpChave.Name = "grpChave";
            grpChave.Padding = new Padding(14, 8, 14, 14);
            grpChave.Size = new Size(968, 92);
            grpChave.TabIndex = 0;
            grpChave.TabStop = false;
            grpChave.Text = "Chave privada do fornecedor (private.xml)";
            // 
            // btnCopiarPublica
            // 
            btnCopiarPublica.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnCopiarPublica.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            btnCopiarPublica.Location = new Point(513, 60);
            btnCopiarPublica.Name = "btnCopiarPublica";
            btnCopiarPublica.Padding = new Padding(10, 0, 10, 0);
            btnCopiarPublica.Size = new Size(146, 23);
            btnCopiarPublica.TabIndex = 2;
            btnCopiarPublica.Text = "Copiar chave pública";
            btnCopiarPublica.UseVisualStyleBackColor = true;
            btnCopiarPublica.Click += btnCopiarPublica_Click;
            // 
            // txtChavePrivada
            // 
            txtChavePrivada.Location = new Point(17, 29);
            txtChavePrivada.Margin = new Padding(3, 3, 12, 3);
            txtChavePrivada.Name = "txtChavePrivada";
            txtChavePrivada.ReadOnly = true;
            txtChavePrivada.Size = new Size(909, 25);
            txtChavePrivada.TabIndex = 0;
            // 
            // btnGerarChaves
            // 
            btnGerarChaves.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnGerarChaves.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            btnGerarChaves.Location = new Point(664, 60);
            btnGerarChaves.Margin = new Padding(3, 3, 9, 3);
            btnGerarChaves.Name = "btnGerarChaves";
            btnGerarChaves.Padding = new Padding(10, 0, 10, 0);
            btnGerarChaves.Size = new Size(150, 23);
            btnGerarChaves.TabIndex = 1;
            btnGerarChaves.Text = "Gerar novas chaves…";
            btnGerarChaves.UseVisualStyleBackColor = true;
            btnGerarChaves.Click += btnGerarChaves_Click;
            // 
            // btnProcurarChave
            // 
            btnProcurarChave.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnProcurarChave.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            btnProcurarChave.Location = new Point(816, 60);
            btnProcurarChave.Margin = new Padding(3, 3, 9, 3);
            btnProcurarChave.Name = "btnProcurarChave";
            btnProcurarChave.Padding = new Padding(10, 0, 10, 0);
            btnProcurarChave.Size = new Size(110, 23);
            btnProcurarChave.TabIndex = 0;
            btnProcurarChave.Text = "Procurar…";
            btnProcurarChave.UseVisualStyleBackColor = true;
            btnProcurarChave.Click += btnProcurarChave_Click;
            // 
            // grpCartorios
            // 
            grpCartorios.Controls.Add(lstCartorios);
            grpCartorios.Location = new Point(30, 134);
            grpCartorios.Margin = new Padding(0, 0, 14, 0);
            grpCartorios.Name = "grpCartorios";
            grpCartorios.Padding = new Padding(5);
            grpCartorios.Size = new Size(388, 400);
            grpCartorios.TabIndex = 0;
            grpCartorios.TabStop = false;
            grpCartorios.Text = "Cartórios cadastrados";
            // 
            // lstCartorios
            // 
            lstCartorios.Dock = DockStyle.Fill;
            lstCartorios.FormattingEnabled = true;
            lstCartorios.IntegralHeight = false;
            lstCartorios.Location = new Point(5, 23);
            lstCartorios.Margin = new Padding(3, 4, 3, 10);
            lstCartorios.Name = "lstCartorios";
            lstCartorios.Size = new Size(378, 372);
            lstCartorios.TabIndex = 0;
            lstCartorios.SelectedIndexChanged += lstCartorios_SelectedIndexChanged;
            // 
            // btnNovo
            // 
            btnNovo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnNovo.Font = new Font("Segoe UI", 9F);
            btnNovo.Location = new Point(181, 542);
            btnNovo.Margin = new Padding(3, 3, 9, 3);
            btnNovo.Name = "btnNovo";
            btnNovo.Padding = new Padding(10, 0, 10, 0);
            btnNovo.Size = new Size(110, 30);
            btnNovo.TabIndex = 0;
            btnNovo.Text = "Novo";
            btnNovo.UseVisualStyleBackColor = true;
            btnNovo.Click += btnNovo_Click;
            // 
            // btnExcluir
            // 
            btnExcluir.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnExcluir.Font = new Font("Segoe UI", 9F);
            btnExcluir.Location = new Point(303, 542);
            btnExcluir.Name = "btnExcluir";
            btnExcluir.Padding = new Padding(10, 0, 10, 0);
            btnExcluir.Size = new Size(110, 30);
            btnExcluir.TabIndex = 1;
            btnExcluir.Text = "Excluir";
            btnExcluir.UseVisualStyleBackColor = true;
            btnExcluir.Click += btnExcluir_Click;
            // 
            // tlpDados
            // 
            tlpDados.AutoSize = true;
            tlpDados.ColumnCount = 2;
            tlpDados.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            tlpDados.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpDados.Controls.Add(txtRespEmail, 1, 5);
            tlpDados.Controls.Add(lblRespEmail, 0, 5);
            tlpDados.Controls.Add(txtRespNome, 1, 3);
            tlpDados.Controls.Add(txtRespCargo, 1, 4);
            tlpDados.Controls.Add(lblRespCargo, 0, 4);
            tlpDados.Controls.Add(lblRespNome, 0, 3);
            tlpDados.Controls.Add(lblNome, 0, 0);
            tlpDados.Controls.Add(txtNome, 1, 0);
            tlpDados.Controls.Add(lblCns, 0, 1);
            tlpDados.Controls.Add(txtCns, 1, 1);
            tlpDados.Controls.Add(lblIbge, 0, 2);
            tlpDados.Controls.Add(txtIbge, 1, 2);
            tlpDados.Location = new Point(6, 6);
            tlpDados.Name = "tlpDados";
            tlpDados.RowCount = 6;
            tlpDados.RowStyles.Add(new RowStyle());
            tlpDados.RowStyles.Add(new RowStyle());
            tlpDados.RowStyles.Add(new RowStyle());
            tlpDados.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tlpDados.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            tlpDados.RowStyles.Add(new RowStyle(SizeType.Absolute, 11F));
            tlpDados.Size = new Size(554, 217);
            tlpDados.TabIndex = 0;
            // 
            // txtRespEmail
            // 
            txtRespEmail.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtRespEmail.Location = new Point(153, 189);
            txtRespEmail.Margin = new Padding(3, 6, 3, 6);
            txtRespEmail.Name = "txtRespEmail";
            txtRespEmail.Size = new Size(398, 25);
            txtRespEmail.TabIndex = 2;
            // 
            // lblRespEmail
            // 
            lblRespEmail.Anchor = AnchorStyles.Left;
            lblRespEmail.AutoSize = true;
            lblRespEmail.Location = new Point(3, 190);
            lblRespEmail.Margin = new Padding(3, 0, 12, 0);
            lblRespEmail.Name = "lblRespEmail";
            lblRespEmail.Size = new Size(47, 19);
            lblRespEmail.TabIndex = 2;
            lblRespEmail.Text = "E-mail";
            // 
            // txtRespNome
            // 
            txtRespNome.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtRespNome.Location = new Point(153, 117);
            txtRespNome.Margin = new Padding(3, 6, 3, 6);
            txtRespNome.Name = "txtRespNome";
            txtRespNome.Size = new Size(398, 25);
            txtRespNome.TabIndex = 0;
            // 
            // txtRespCargo
            // 
            txtRespCargo.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtRespCargo.Location = new Point(153, 152);
            txtRespCargo.Margin = new Padding(3, 6, 3, 6);
            txtRespCargo.Name = "txtRespCargo";
            txtRespCargo.Size = new Size(398, 25);
            txtRespCargo.TabIndex = 1;
            // 
            // lblRespCargo
            // 
            lblRespCargo.Anchor = AnchorStyles.Left;
            lblRespCargo.AutoSize = true;
            lblRespCargo.Location = new Point(3, 155);
            lblRespCargo.Margin = new Padding(3, 0, 12, 0);
            lblRespCargo.Name = "lblRespCargo";
            lblRespCargo.Size = new Size(46, 19);
            lblRespCargo.TabIndex = 1;
            lblRespCargo.Text = "Cargo";
            // 
            // lblRespNome
            // 
            lblRespNome.Anchor = AnchorStyles.Left;
            lblRespNome.AutoSize = true;
            lblRespNome.Location = new Point(3, 119);
            lblRespNome.Margin = new Padding(3, 0, 12, 0);
            lblRespNome.Name = "lblRespNome";
            lblRespNome.Size = new Size(46, 19);
            lblRespNome.TabIndex = 0;
            lblRespNome.Text = "Nome";
            // 
            // lblNome
            // 
            lblNome.Anchor = AnchorStyles.Left;
            lblNome.AutoSize = true;
            lblNome.Location = new Point(3, 9);
            lblNome.Margin = new Padding(3, 0, 12, 0);
            lblNome.Name = "lblNome";
            lblNome.Size = new Size(117, 19);
            lblNome.TabIndex = 0;
            lblNome.Text = "Nome do cartório";
            // 
            // txtNome
            // 
            txtNome.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtNome.Location = new Point(153, 6);
            txtNome.Margin = new Padding(3, 6, 3, 6);
            txtNome.Name = "txtNome";
            txtNome.Size = new Size(398, 25);
            txtNome.TabIndex = 0;
            // 
            // lblCns
            // 
            lblCns.Anchor = AnchorStyles.Left;
            lblCns.AutoSize = true;
            lblCns.Location = new Point(3, 46);
            lblCns.Margin = new Padding(3, 0, 12, 0);
            lblCns.Name = "lblCns";
            lblCns.Size = new Size(83, 19);
            lblCns.TabIndex = 1;
            lblCns.Text = "Código CNS";
            // 
            // txtCns
            // 
            txtCns.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtCns.Location = new Point(153, 43);
            txtCns.Margin = new Padding(3, 6, 3, 6);
            txtCns.Name = "txtCns";
            txtCns.Size = new Size(398, 25);
            txtCns.TabIndex = 1;
            // 
            // lblIbge
            // 
            lblIbge.Anchor = AnchorStyles.Left;
            lblIbge.AutoSize = true;
            lblIbge.Location = new Point(3, 83);
            lblIbge.Margin = new Padding(3, 0, 12, 0);
            lblIbge.Name = "lblIbge";
            lblIbge.Size = new Size(86, 19);
            lblIbge.TabIndex = 2;
            lblIbge.Text = "Código IBGE";
            // 
            // txtIbge
            // 
            txtIbge.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtIbge.Location = new Point(153, 80);
            txtIbge.Margin = new Padding(3, 6, 3, 6);
            txtIbge.Name = "txtIbge";
            txtIbge.Size = new Size(398, 25);
            txtIbge.TabIndex = 2;
            // 
            // tlpLicenca
            // 
            tlpLicenca.ColumnCount = 2;
            tlpLicenca.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            tlpLicenca.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpLicenca.Controls.Add(lblPeriodo, 0, 0);
            tlpLicenca.Controls.Add(flpPeriodo, 1, 0);
            tlpLicenca.Controls.Add(lblModelos, 0, 1);
            tlpLicenca.Controls.Add(clbModelos, 1, 1);
            tlpLicenca.Location = new Point(6, 6);
            tlpLicenca.Name = "tlpLicenca";
            tlpLicenca.RowCount = 2;
            tlpLicenca.RowStyles.Add(new RowStyle());
            tlpLicenca.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpLicenca.Size = new Size(555, 309);
            tlpLicenca.TabIndex = 0;
            // 
            // lblPeriodo
            // 
            lblPeriodo.Anchor = AnchorStyles.Left;
            lblPeriodo.AutoSize = true;
            lblPeriodo.Location = new Point(3, 9);
            lblPeriodo.Margin = new Padding(3, 0, 12, 0);
            lblPeriodo.Name = "lblPeriodo";
            lblPeriodo.Size = new Size(128, 19);
            lblPeriodo.TabIndex = 0;
            lblPeriodo.Text = "Período de validade";
            // 
            // flpPeriodo
            // 
            flpPeriodo.Anchor = AnchorStyles.Left;
            flpPeriodo.AutoSize = true;
            flpPeriodo.Controls.Add(dtpInicio);
            flpPeriodo.Controls.Add(lblAte);
            flpPeriodo.Controls.Add(dtpFim);
            flpPeriodo.Location = new Point(150, 0);
            flpPeriodo.Margin = new Padding(0);
            flpPeriodo.Name = "flpPeriodo";
            flpPeriodo.Size = new Size(358, 37);
            flpPeriodo.TabIndex = 0;
            // 
            // dtpInicio
            // 
            dtpInicio.Format = DateTimePickerFormat.Short;
            dtpInicio.Location = new Point(3, 6);
            dtpInicio.Margin = new Padding(3, 6, 3, 6);
            dtpInicio.Name = "dtpInicio";
            dtpInicio.ShowCheckBox = true;
            dtpInicio.Size = new Size(150, 25);
            dtpInicio.TabIndex = 0;
            // 
            // lblAte
            // 
            lblAte.Anchor = AnchorStyles.Left;
            lblAte.AutoSize = true;
            lblAte.Location = new Point(165, 14);
            lblAte.Margin = new Padding(9, 10, 9, 0);
            lblAte.Name = "lblAte";
            lblAte.Size = new Size(28, 19);
            lblAte.TabIndex = 1;
            lblAte.Text = "até";
            // 
            // dtpFim
            // 
            dtpFim.Format = DateTimePickerFormat.Short;
            dtpFim.Location = new Point(205, 6);
            dtpFim.Margin = new Padding(3, 6, 3, 6);
            dtpFim.Name = "dtpFim";
            dtpFim.ShowCheckBox = true;
            dtpFim.Size = new Size(150, 25);
            dtpFim.TabIndex = 2;
            // 
            // lblModelos
            // 
            lblModelos.Location = new Point(3, 49);
            lblModelos.Margin = new Padding(3, 12, 12, 0);
            lblModelos.Name = "lblModelos";
            lblModelos.Size = new Size(135, 82);
            lblModelos.TabIndex = 1;
            lblModelos.Text = "Modelos contratados";
            // 
            // clbModelos
            // 
            clbModelos.CheckOnClick = true;
            clbModelos.Dock = DockStyle.Fill;
            clbModelos.FormattingEnabled = true;
            clbModelos.IntegralHeight = false;
            clbModelos.Location = new Point(153, 43);
            clbModelos.Margin = new Padding(3, 6, 3, 3);
            clbModelos.Name = "clbModelos";
            clbModelos.Size = new Size(399, 263);
            clbModelos.TabIndex = 1;
            // 
            // btnEmitir
            // 
            btnEmitir.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnEmitir.BackColor = Color.FromArgb(21, 101, 192);
            btnEmitir.FlatAppearance.BorderSize = 0;
            btnEmitir.FlatStyle = FlatStyle.Flat;
            btnEmitir.Font = new Font("Segoe UI", 9F);
            btnEmitir.ForeColor = Color.White;
            btnEmitir.Location = new Point(615, 549);
            btnEmitir.Margin = new Padding(12, 3, 3, 3);
            btnEmitir.Name = "btnEmitir";
            btnEmitir.Padding = new Padding(10, 0, 10, 0);
            btnEmitir.Size = new Size(200, 34);
            btnEmitir.TabIndex = 0;
            btnEmitir.Text = "Emitir licença (.lic)…";
            btnEmitir.UseVisualStyleBackColor = false;
            btnEmitir.Click += btnEmitir_Click;
            // 
            // btnSalvar
            // 
            btnSalvar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSalvar.Font = new Font("Segoe UI", 9F);
            btnSalvar.Location = new Point(833, 549);
            btnSalvar.Name = "btnSalvar";
            btnSalvar.Padding = new Padding(10, 0, 10, 0);
            btnSalvar.Size = new Size(161, 34);
            btnSalvar.TabIndex = 1;
            btnSalvar.Text = "Salvar cadastro";
            btnSalvar.UseVisualStyleBackColor = true;
            btnSalvar.Click += btnSalvar_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(423, 183);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(575, 351);
            tabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(tlpDados);
            tabPage1.Location = new Point(4, 26);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(567, 321);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Dados do cartório";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(tlpLicenca);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(567, 323);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Licença";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            lblStatus.BorderStyle = BorderStyle.Fixed3D;
            lblStatus.Location = new Point(423, 150);
            lblStatus.Margin = new Padding(3, 12, 12, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(571, 30);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "-";
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // EmissorForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1010, 641);
            Controls.Add(lblStatus);
            Controls.Add(tabControl1);
            Controls.Add(btnEmitir);
            Controls.Add(btnSalvar);
            Controls.Add(btnNovo);
            Controls.Add(grpCartorios);
            Controls.Add(btnExcluir);
            Controls.Add(grpChave);
            Font = new Font("Segoe UI", 10F);
            MinimumSize = new Size(980, 680);
            Name = "EmissorForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Emissor de Licenças — GeneratorJS";
            grpChave.ResumeLayout(false);
            grpChave.PerformLayout();
            grpCartorios.ResumeLayout(false);
            tlpDados.ResumeLayout(false);
            tlpDados.PerformLayout();
            tlpLicenca.ResumeLayout(false);
            tlpLicenca.PerformLayout();
            flpPeriodo.ResumeLayout(false);
            flpPeriodo.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private GroupBox grpChave;
        private TextBox txtChavePrivada;
        private Button btnProcurarChave;
        private Button btnGerarChaves;
        private Button btnCopiarPublica;
        private GroupBox grpCartorios;
        private ListBox lstCartorios;
        private Button btnNovo;
        private Button btnExcluir;
        private TableLayoutPanel tlpDados;
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
        private TableLayoutPanel tlpLicenca;
        private System.Windows.Forms.Label lblPeriodo;
        private FlowLayoutPanel flpPeriodo;
        private DateTimePicker dtpInicio;
        private System.Windows.Forms.Label lblAte;
        private DateTimePicker dtpFim;
        private System.Windows.Forms.Label lblModelos;
        private CheckedListBox clbModelos;
        private Button btnEmitir;
        private Button btnSalvar;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Label lblStatus;
    }
}
