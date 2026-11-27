namespace GenerateJS.Gerenciador
{
    partial class frmTelaGeneratorJS
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer? components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _web = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)_web).BeginInit();
            SuspendLayout();
            // 
            // _web
            // 
            _web.AllowExternalDrop = true;
            _web.BackColor = Color.Black;
            _web.CreationProperties = null;
            _web.DefaultBackgroundColor = Color.Black;
            _web.Dock = DockStyle.Fill;
            _web.Location = new Point(0, 0);
            _web.Name = "_web";
            _web.Size = new Size(1284, 729);
            _web.TabIndex = 0;
            _web.ZoomFactor = 1D;
            // 
            // frmTelaGeneratorJS
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(1284, 729);
            Controls.Add(_web);
            MinimumSize = new Size(1100, 700);
            Name = "frmTelaGeneratorJS";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "GeneratorJS — Integrador";
            Load += frmTelaGeneratorJS_Load;
            ((System.ComponentModel.ISupportInitialize)_web).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 _web;
    }
}