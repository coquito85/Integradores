#nullable disable
namespace BetaJsonIntegrator.App.Forms
{
    partial class frmVisualizadorJSON
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
            lblInfo = new Label();
            btnBuscar = new Button();
            txtBuscar = new TextBox();
            btnSalvarComo = new Button();
            btnCopiar = new Button();
            txtJson = new TextBox();
            pnlTop.SuspendLayout();
            SuspendLayout();
            //
            // pnlTop
            //
            pnlTop.Controls.Add(lblInfo);
            pnlTop.Controls.Add(btnBuscar);
            pnlTop.Controls.Add(txtBuscar);
            pnlTop.Controls.Add(btnSalvarComo);
            pnlTop.Controls.Add(btnCopiar);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(900, 44);
            pnlTop.TabIndex = 1;
            //
            // btnCopiar
            //
            btnCopiar.Location = new Point(12, 8);
            btnCopiar.Name = "btnCopiar";
            btnCopiar.Size = new Size(100, 28);
            btnCopiar.TabIndex = 0;
            btnCopiar.Text = "Copiar";
            btnCopiar.UseVisualStyleBackColor = true;
            btnCopiar.Click += btnCopiar_Click;
            //
            // btnSalvarComo
            //
            btnSalvarComo.Location = new Point(118, 8);
            btnSalvarComo.Name = "btnSalvarComo";
            btnSalvarComo.Size = new Size(120, 28);
            btnSalvarComo.TabIndex = 1;
            btnSalvarComo.Text = "Salvar como...";
            btnSalvarComo.UseVisualStyleBackColor = true;
            btnSalvarComo.Click += btnSalvarComo_Click;
            //
            // txtBuscar
            //
            txtBuscar.Location = new Point(262, 11);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.PlaceholderText = "Buscar no JSON...";
            txtBuscar.Size = new Size(200, 23);
            txtBuscar.TabIndex = 2;
            //
            // btnBuscar
            //
            btnBuscar.Location = new Point(468, 8);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(80, 28);
            btnBuscar.TabIndex = 3;
            btnBuscar.Text = "Buscar";
            btnBuscar.UseVisualStyleBackColor = true;
            btnBuscar.Click += btnBuscar_Click;
            //
            // lblInfo
            //
            lblInfo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblInfo.AutoSize = false;
            lblInfo.Location = new Point(560, 14);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(328, 16);
            lblInfo.TabIndex = 4;
            lblInfo.TextAlign = ContentAlignment.MiddleRight;
            //
            // txtJson
            //
            txtJson.BackColor = SystemColors.Window;
            txtJson.Dock = DockStyle.Fill;
            txtJson.Font = new Font("Consolas", 10F);
            txtJson.HideSelection = false;
            txtJson.Location = new Point(0, 44);
            txtJson.MaxLength = int.MaxValue;
            txtJson.Multiline = true;
            txtJson.Name = "txtJson";
            txtJson.ReadOnly = true;
            txtJson.ScrollBars = ScrollBars.Both;
            txtJson.Size = new Size(900, 606);
            txtJson.TabIndex = 0;
            txtJson.WordWrap = false;
            //
            // frmVisualizadorJSON
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 650);
            Controls.Add(txtJson);
            Controls.Add(pnlTop);
            MinimumSize = new Size(640, 400);
            Name = "frmVisualizadorJSON";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Visualizador JSON";
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlTop;
        private Label lblInfo;
        private Button btnBuscar;
        private TextBox txtBuscar;
        private Button btnSalvarComo;
        private Button btnCopiar;
        private TextBox txtJson;
    }
}
