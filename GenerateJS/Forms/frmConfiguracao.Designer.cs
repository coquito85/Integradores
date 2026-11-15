#nullable disable
namespace GenerateJS.Forms
{
    partial class frmConfiguracao
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
            pg_config = new PropertyGrid();
            pnlRodape = new Panel();
            btnTestar = new Button();
            btnCancelar = new Button();
            btnGravar = new Button();
            pnlRodape.SuspendLayout();
            SuspendLayout();
            //
            // pg_config
            //
            pg_config.Dock = DockStyle.Fill;
            pg_config.HelpVisible = true;
            pg_config.Location = new Point(0, 0);
            pg_config.Name = "pg_config";
            pg_config.PropertySort = PropertySort.Categorized;
            pg_config.Size = new Size(640, 564);
            pg_config.TabIndex = 0;
            pg_config.ToolbarVisible = false;
            //
            // pnlRodape
            //
            pnlRodape.BackColor = Color.FromArgb(241, 245, 249);
            pnlRodape.Controls.Add(btnTestar);
            pnlRodape.Controls.Add(btnCancelar);
            pnlRodape.Controls.Add(btnGravar);
            pnlRodape.Dock = DockStyle.Bottom;
            pnlRodape.Location = new Point(0, 564);
            pnlRodape.Name = "pnlRodape";
            pnlRodape.Size = new Size(640, 56);
            pnlRodape.TabIndex = 1;
            //
            // btnTestar
            //
            btnTestar.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnTestar.Location = new Point(12, 12);
            btnTestar.Name = "btnTestar";
            btnTestar.Size = new Size(150, 32);
            btnTestar.TabIndex = 0;
            btnTestar.Text = "Testar origem";
            btnTestar.UseVisualStyleBackColor = true;
            btnTestar.Click += btnTestar_Click;
            //
            // btnCancelar
            //
            btnCancelar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancelar.DialogResult = DialogResult.Cancel;
            btnCancelar.Location = new Point(420, 12);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(100, 32);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            //
            // btnGravar
            //
            btnGravar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGravar.BackColor = Color.FromArgb(30, 41, 59);
            btnGravar.FlatStyle = FlatStyle.Flat;
            btnGravar.ForeColor = Color.White;
            btnGravar.Location = new Point(528, 12);
            btnGravar.Name = "btnGravar";
            btnGravar.Size = new Size(100, 32);
            btnGravar.TabIndex = 2;
            btnGravar.Text = "Gravar";
            btnGravar.UseVisualStyleBackColor = false;
            btnGravar.Click += btnGravar_Click;
            //
            // frmConfiguracao
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancelar;
            ClientSize = new Size(640, 620);
            Controls.Add(pg_config);
            Controls.Add(pnlRodape);
            MinimumSize = new Size(520, 480);
            Name = "frmConfiguracao";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Configurações";
            pnlRodape.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private PropertyGrid pg_config;
        private Panel pnlRodape;
        private Button btnTestar;
        private Button btnCancelar;
        private Button btnGravar;
    }
}
