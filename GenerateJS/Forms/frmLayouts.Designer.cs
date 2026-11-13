#nullable disable
namespace GenerateJS.Forms
{
    partial class frmLayouts
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
            pnlTop = new Panel();
            lblContagem = new Label();
            txtPesquisar = new TextBox();
            lblPesquisar = new Label();
            dgvLayouts = new DataGridView();
            colPasta = new DataGridViewTextBoxColumn();
            colNome = new DataGridViewTextBoxColumn();
            colIdentificador = new DataGridViewTextBoxColumn();
            colVersao = new DataGridViewTextBoxColumn();
            colSchema = new DataGridViewTextBoxColumn();
            colAtualizado = new DataGridViewTextBoxColumn();
            colNotas = new DataGridViewTextBoxColumn();
            grpNovo = new GroupBox();
            btnCriar = new Button();
            txtNotas = new TextBox();
            lblNotas = new Label();
            btnProcurarSchema = new Button();
            txtSchema = new TextBox();
            lblSchema = new Label();
            btnProcurarXml = new Button();
            txtXml = new TextBox();
            lblXml = new Label();
            txtPasta = new TextBox();
            lblPasta = new Label();
            pnlBottom = new Panel();
            btnCancelar = new Button();
            btnSelecionar = new Button();
            btnNovaVersao = new Button();
            btnNovo = new Button();
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLayouts).BeginInit();
            grpNovo.SuspendLayout();
            pnlBottom.SuspendLayout();
            SuspendLayout();
            //
            // pnlTop
            //
            pnlTop.Controls.Add(lblContagem);
            pnlTop.Controls.Add(txtPesquisar);
            pnlTop.Controls.Add(lblPesquisar);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(980, 44);
            pnlTop.TabIndex = 3;
            //
            // lblPesquisar
            //
            lblPesquisar.AutoSize = true;
            lblPesquisar.Location = new Point(12, 15);
            lblPesquisar.Name = "lblPesquisar";
            lblPesquisar.Size = new Size(61, 15);
            lblPesquisar.TabIndex = 0;
            lblPesquisar.Text = "Pesquisar:";
            //
            // txtPesquisar
            //
            txtPesquisar.Location = new Point(80, 11);
            txtPesquisar.Name = "txtPesquisar";
            txtPesquisar.PlaceholderText = "Nome, pasta ou identificador";
            txtPesquisar.Size = new Size(280, 23);
            txtPesquisar.TabIndex = 1;
            txtPesquisar.TextChanged += txtPesquisar_TextChanged;
            //
            // lblContagem
            //
            lblContagem.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblContagem.Location = new Point(780, 15);
            lblContagem.Name = "lblContagem";
            lblContagem.Size = new Size(188, 15);
            lblContagem.TabIndex = 2;
            lblContagem.TextAlign = ContentAlignment.MiddleRight;
            //
            // dgvLayouts
            //
            dgvLayouts.AllowUserToAddRows = false;
            dgvLayouts.AllowUserToDeleteRows = false;
            dgvLayouts.AllowUserToResizeRows = false;
            dgvLayouts.BackgroundColor = SystemColors.Window;
            dgvLayouts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLayouts.Columns.AddRange(new DataGridViewColumn[] { colPasta, colNome, colIdentificador, colVersao, colSchema, colAtualizado, colNotas });
            dgvLayouts.Dock = DockStyle.Fill;
            dgvLayouts.Location = new Point(0, 44);
            dgvLayouts.MultiSelect = false;
            dgvLayouts.Name = "dgvLayouts";
            dgvLayouts.ReadOnly = true;
            dgvLayouts.RowHeadersVisible = false;
            dgvLayouts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLayouts.Size = new Size(980, 220);
            dgvLayouts.TabIndex = 0;
            dgvLayouts.CellDoubleClick += dgvLayouts_CellDoubleClick;
            //
            // colPasta
            //
            colPasta.HeaderText = "Pasta";
            colPasta.Name = "colPasta";
            colPasta.ReadOnly = true;
            colPasta.Width = 110;
            //
            // colNome
            //
            colNome.HeaderText = "Nome";
            colNome.Name = "colNome";
            colNome.ReadOnly = true;
            colNome.Width = 200;
            //
            // colIdentificador
            //
            colIdentificador.HeaderText = "Identificador";
            colIdentificador.Name = "colIdentificador";
            colIdentificador.ReadOnly = true;
            colIdentificador.Width = 120;
            //
            // colVersao
            //
            colVersao.HeaderText = "Versão";
            colVersao.Name = "colVersao";
            colVersao.ReadOnly = true;
            colVersao.Width = 60;
            //
            // colSchema
            //
            colSchema.HeaderText = "Schema";
            colSchema.Name = "colSchema";
            colSchema.ReadOnly = true;
            colSchema.Width = 110;
            //
            // colAtualizado
            //
            colAtualizado.HeaderText = "Atualizado em";
            colAtualizado.Name = "colAtualizado";
            colAtualizado.ReadOnly = true;
            colAtualizado.Width = 120;
            //
            // colNotas
            //
            colNotas.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colNotas.HeaderText = "Observações";
            colNotas.Name = "colNotas";
            colNotas.ReadOnly = true;
            //
            // grpNovo
            //
            grpNovo.Controls.Add(btnCriar);
            grpNovo.Controls.Add(txtNotas);
            grpNovo.Controls.Add(lblNotas);
            grpNovo.Controls.Add(btnProcurarSchema);
            grpNovo.Controls.Add(txtSchema);
            grpNovo.Controls.Add(lblSchema);
            grpNovo.Controls.Add(btnProcurarXml);
            grpNovo.Controls.Add(txtXml);
            grpNovo.Controls.Add(lblXml);
            grpNovo.Controls.Add(txtPasta);
            grpNovo.Controls.Add(lblPasta);
            grpNovo.Dock = DockStyle.Bottom;
            grpNovo.Location = new Point(0, 264);
            grpNovo.Name = "grpNovo";
            grpNovo.Size = new Size(980, 160);
            grpNovo.TabIndex = 2;
            grpNovo.TabStop = false;
            grpNovo.Text = "Novo layout";
            grpNovo.Visible = false;
            //
            // lblPasta
            //
            lblPasta.AutoSize = true;
            lblPasta.Location = new Point(12, 28);
            lblPasta.Name = "lblPasta";
            lblPasta.Size = new Size(94, 15);
            lblPasta.TabIndex = 0;
            lblPasta.Text = "Pasta (ex.: SATUBA):";
            //
            // txtPasta
            //
            txtPasta.Location = new Point(150, 25);
            txtPasta.MaxLength = 64;
            txtPasta.Name = "txtPasta";
            txtPasta.Size = new Size(200, 23);
            txtPasta.TabIndex = 1;
            //
            // lblXml
            //
            lblXml.AutoSize = true;
            lblXml.Location = new Point(12, 58);
            lblXml.Name = "lblXml";
            lblXml.Size = new Size(71, 15);
            lblXml.TabIndex = 2;
            lblXml.Text = "XML do layout:";
            //
            // txtXml
            //
            txtXml.Location = new Point(150, 55);
            txtXml.Name = "txtXml";
            txtXml.Size = new Size(640, 23);
            txtXml.TabIndex = 3;
            //
            // btnProcurarXml
            //
            btnProcurarXml.Location = new Point(796, 53);
            btnProcurarXml.Name = "btnProcurarXml";
            btnProcurarXml.Size = new Size(36, 27);
            btnProcurarXml.TabIndex = 4;
            btnProcurarXml.Text = "...";
            btnProcurarXml.UseVisualStyleBackColor = true;
            btnProcurarXml.Click += btnProcurarXml_Click;
            //
            // lblSchema
            //
            lblSchema.AutoSize = true;
            lblSchema.Location = new Point(12, 88);
            lblSchema.Name = "lblSchema";
            lblSchema.Size = new Size(110, 15);
            lblSchema.TabIndex = 5;
            lblSchema.Text = "JSON Schema (opcional):";
            //
            // txtSchema
            //
            txtSchema.Location = new Point(150, 85);
            txtSchema.Name = "txtSchema";
            txtSchema.Size = new Size(640, 23);
            txtSchema.TabIndex = 6;
            //
            // btnProcurarSchema
            //
            btnProcurarSchema.Location = new Point(796, 83);
            btnProcurarSchema.Name = "btnProcurarSchema";
            btnProcurarSchema.Size = new Size(36, 27);
            btnProcurarSchema.TabIndex = 7;
            btnProcurarSchema.Text = "...";
            btnProcurarSchema.UseVisualStyleBackColor = true;
            btnProcurarSchema.Click += btnProcurarSchema_Click;
            //
            // lblNotas
            //
            lblNotas.AutoSize = true;
            lblNotas.Location = new Point(12, 118);
            lblNotas.Name = "lblNotas";
            lblNotas.Size = new Size(70, 15);
            lblNotas.TabIndex = 8;
            lblNotas.Text = "Observações:";
            //
            // txtNotas
            //
            txtNotas.Location = new Point(150, 115);
            txtNotas.Name = "txtNotas";
            txtNotas.Size = new Size(640, 23);
            txtNotas.TabIndex = 9;
            //
            // btnCriar
            //
            btnCriar.Location = new Point(850, 112);
            btnCriar.Name = "btnCriar";
            btnCriar.Size = new Size(110, 28);
            btnCriar.TabIndex = 10;
            btnCriar.Text = "Cadastrar";
            btnCriar.UseVisualStyleBackColor = true;
            btnCriar.Click += btnCriar_Click;
            //
            // pnlBottom
            //
            pnlBottom.Controls.Add(btnCancelar);
            pnlBottom.Controls.Add(btnSelecionar);
            pnlBottom.Controls.Add(btnNovaVersao);
            pnlBottom.Controls.Add(btnNovo);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(0, 424);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(980, 50);
            pnlBottom.TabIndex = 1;
            //
            // btnNovo
            //
            btnNovo.Location = new Point(12, 11);
            btnNovo.Name = "btnNovo";
            btnNovo.Size = new Size(120, 28);
            btnNovo.TabIndex = 0;
            btnNovo.Text = "Novo layout...";
            btnNovo.UseVisualStyleBackColor = true;
            btnNovo.Click += btnNovo_Click;
            //
            // btnNovaVersao
            //
            btnNovaVersao.Location = new Point(138, 11);
            btnNovaVersao.Name = "btnNovaVersao";
            btnNovaVersao.Size = new Size(120, 28);
            btnNovaVersao.TabIndex = 1;
            btnNovaVersao.Text = "Nova versão";
            btnNovaVersao.UseVisualStyleBackColor = true;
            btnNovaVersao.Click += btnNovaVersao_Click;
            //
            // btnSelecionar
            //
            btnSelecionar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSelecionar.Location = new Point(764, 11);
            btnSelecionar.Name = "btnSelecionar";
            btnSelecionar.Size = new Size(110, 28);
            btnSelecionar.TabIndex = 2;
            btnSelecionar.Text = "Selecionar";
            btnSelecionar.UseVisualStyleBackColor = true;
            btnSelecionar.Click += btnSelecionar_Click;
            //
            // btnCancelar
            //
            btnCancelar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancelar.Location = new Point(880, 11);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(88, 28);
            btnCancelar.TabIndex = 3;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            btnCancelar.Click += btnCancelar_Click;
            //
            // frmLayouts
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(980, 474);
            Controls.Add(dgvLayouts);
            Controls.Add(grpNovo);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);
            MinimumSize = new Size(860, 360);
            Name = "frmLayouts";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Layouts";
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLayouts).EndInit();
            grpNovo.ResumeLayout(false);
            grpNovo.PerformLayout();
            pnlBottom.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlTop;
        private Label lblContagem;
        private TextBox txtPesquisar;
        private Label lblPesquisar;
        private DataGridView dgvLayouts;
        private DataGridViewTextBoxColumn colPasta;
        private DataGridViewTextBoxColumn colNome;
        private DataGridViewTextBoxColumn colIdentificador;
        private DataGridViewTextBoxColumn colVersao;
        private DataGridViewTextBoxColumn colSchema;
        private DataGridViewTextBoxColumn colAtualizado;
        private DataGridViewTextBoxColumn colNotas;
        private GroupBox grpNovo;
        private Button btnCriar;
        private TextBox txtNotas;
        private Label lblNotas;
        private Button btnProcurarSchema;
        private TextBox txtSchema;
        private Label lblSchema;
        private Button btnProcurarXml;
        private TextBox txtXml;
        private Label lblXml;
        private TextBox txtPasta;
        private Label lblPasta;
        private Panel pnlBottom;
        private Button btnCancelar;
        private Button btnSelecionar;
        private Button btnNovaVersao;
        private Button btnNovo;
    }
}
