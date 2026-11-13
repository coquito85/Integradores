#nullable disable
namespace GenerateJS.Forms
{
    partial class frmValidacao
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
            lblTotais = new Label();
            cboSeveridade = new ComboBox();
            lblFiltro = new Label();
            pnlBottom = new Panel();
            btnFechar = new Button();
            btnExportar = new Button();
            dgvIssues = new DataGridView();
            colSeveridade = new DataGridViewTextBoxColumn();
            colCodigo = new DataGridViewTextBoxColumn();
            colCaminho = new DataGridViewTextBoxColumn();
            colMensagem = new DataGridViewTextBoxColumn();
            pnlTop.SuspendLayout();
            pnlBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvIssues).BeginInit();
            SuspendLayout();
            //
            // pnlTop
            //
            pnlTop.Controls.Add(lblTotais);
            pnlTop.Controls.Add(cboSeveridade);
            pnlTop.Controls.Add(lblFiltro);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(980, 44);
            pnlTop.TabIndex = 2;
            //
            // lblFiltro
            //
            lblFiltro.AutoSize = true;
            lblFiltro.Location = new Point(12, 15);
            lblFiltro.Name = "lblFiltro";
            lblFiltro.Size = new Size(64, 15);
            lblFiltro.TabIndex = 0;
            lblFiltro.Text = "Severidade:";
            //
            // cboSeveridade
            //
            cboSeveridade.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSeveridade.FormattingEnabled = true;
            cboSeveridade.Items.AddRange(new object[] { "Todas", "Erros", "Avisos", "Informações" });
            cboSeveridade.Location = new Point(90, 11);
            cboSeveridade.Name = "cboSeveridade";
            cboSeveridade.Size = new Size(140, 23);
            cboSeveridade.TabIndex = 1;
            cboSeveridade.SelectedIndexChanged += cboSeveridade_SelectedIndexChanged;
            //
            // lblTotais
            //
            lblTotais.AutoSize = true;
            lblTotais.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTotais.Location = new Point(250, 15);
            lblTotais.Name = "lblTotais";
            lblTotais.Size = new Size(0, 15);
            lblTotais.TabIndex = 2;
            //
            // pnlBottom
            //
            pnlBottom.Controls.Add(btnFechar);
            pnlBottom.Controls.Add(btnExportar);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(0, 506);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(980, 48);
            pnlBottom.TabIndex = 1;
            //
            // btnExportar
            //
            btnExportar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExportar.Location = new Point(738, 10);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(140, 28);
            btnExportar.TabIndex = 0;
            btnExportar.Text = "Exportar relatório...";
            btnExportar.UseVisualStyleBackColor = true;
            btnExportar.Click += btnExportar_Click;
            //
            // btnFechar
            //
            btnFechar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnFechar.Location = new Point(884, 10);
            btnFechar.Name = "btnFechar";
            btnFechar.Size = new Size(84, 28);
            btnFechar.TabIndex = 1;
            btnFechar.Text = "Fechar";
            btnFechar.UseVisualStyleBackColor = true;
            btnFechar.Click += btnFechar_Click;
            //
            // dgvIssues
            //
            dgvIssues.AllowUserToAddRows = false;
            dgvIssues.AllowUserToDeleteRows = false;
            dgvIssues.AllowUserToResizeRows = false;
            dgvIssues.BackgroundColor = SystemColors.Window;
            dgvIssues.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvIssues.Columns.AddRange(new DataGridViewColumn[] { colSeveridade, colCodigo, colCaminho, colMensagem });
            dgvIssues.Dock = DockStyle.Fill;
            dgvIssues.Location = new Point(0, 44);
            dgvIssues.Name = "dgvIssues";
            dgvIssues.ReadOnly = true;
            dgvIssues.RowHeadersVisible = false;
            dgvIssues.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvIssues.Size = new Size(980, 462);
            dgvIssues.TabIndex = 0;
            dgvIssues.CellFormatting += dgvIssues_CellFormatting;
            //
            // colSeveridade
            //
            colSeveridade.HeaderText = "Severidade";
            colSeveridade.Name = "colSeveridade";
            colSeveridade.ReadOnly = true;
            colSeveridade.Width = 80;
            //
            // colCodigo
            //
            colCodigo.HeaderText = "Código";
            colCodigo.Name = "colCodigo";
            colCodigo.ReadOnly = true;
            colCodigo.Width = 190;
            //
            // colCaminho
            //
            colCaminho.HeaderText = "Caminho da propriedade";
            colCaminho.Name = "colCaminho";
            colCaminho.ReadOnly = true;
            colCaminho.Width = 280;
            //
            // colMensagem
            //
            colMensagem.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colMensagem.HeaderText = "Mensagem";
            colMensagem.Name = "colMensagem";
            colMensagem.ReadOnly = true;
            //
            // frmValidacao
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(980, 554);
            Controls.Add(dgvIssues);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);
            MinimumSize = new Size(700, 380);
            Name = "frmValidacao";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Validação";
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            pnlBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvIssues).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlTop;
        private Label lblTotais;
        private ComboBox cboSeveridade;
        private Label lblFiltro;
        private Panel pnlBottom;
        private Button btnFechar;
        private Button btnExportar;
        private DataGridView dgvIssues;
        private DataGridViewTextBoxColumn colSeveridade;
        private DataGridViewTextBoxColumn colCodigo;
        private DataGridViewTextBoxColumn colCaminho;
        private DataGridViewTextBoxColumn colMensagem;
    }
}
