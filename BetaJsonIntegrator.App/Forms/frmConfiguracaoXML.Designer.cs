#nullable disable
namespace BetaJsonIntegrator.App.Forms
{
    partial class frmConfiguracaoXML
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
            tabPrincipal = new TabControl();
            tabXml = new TabPage();
            txtXml = new TextBox();
            pnlXmlBottom = new Panel();
            chkNovaVersao = new CheckBox();
            btnSalvarXml = new Button();
            btnValidarXml = new Button();
            tabEstrutura = new TabPage();
            treeEstrutura = new TreeView();
            tabPers = new TabPage();
            dgvPers = new DataGridView();
            colPCaminho = new DataGridViewTextBoxColumn();
            colPDescricao = new DataGridViewTextBoxColumn();
            colPTipo = new DataGridViewTextBoxColumn();
            colPObrig = new DataGridViewCheckBoxColumn();
            colPModo = new DataGridViewComboBoxColumn();
            colPValor = new DataGridViewTextBoxColumn();
            colPSobrescrever = new DataGridViewCheckBoxColumn();
            pnlPersBottom = new Panel();
            lblPersInfo = new Label();
            btnSalvarPers = new Button();
            lstResultado = new ListBox();
            pnlRodape = new Panel();
            btnFechar = new Button();
            tabPrincipal.SuspendLayout();
            tabXml.SuspendLayout();
            pnlXmlBottom.SuspendLayout();
            tabEstrutura.SuspendLayout();
            tabPers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPers).BeginInit();
            pnlPersBottom.SuspendLayout();
            pnlRodape.SuspendLayout();
            SuspendLayout();
            //
            // tabPrincipal
            //
            tabPrincipal.Controls.Add(tabXml);
            tabPrincipal.Controls.Add(tabEstrutura);
            tabPrincipal.Controls.Add(tabPers);
            tabPrincipal.Dock = DockStyle.Fill;
            tabPrincipal.Location = new Point(0, 0);
            tabPrincipal.Name = "tabPrincipal";
            tabPrincipal.SelectedIndex = 0;
            tabPrincipal.Size = new Size(1000, 480);
            tabPrincipal.TabIndex = 0;
            //
            // tabXml
            //
            tabXml.Controls.Add(txtXml);
            tabXml.Controls.Add(pnlXmlBottom);
            tabXml.Location = new Point(4, 24);
            tabXml.Name = "tabXml";
            tabXml.Padding = new Padding(3);
            tabXml.Size = new Size(992, 452);
            tabXml.TabIndex = 0;
            tabXml.Text = "XML do layout";
            tabXml.UseVisualStyleBackColor = true;
            //
            // txtXml
            //
            txtXml.AcceptsReturn = true;
            txtXml.AcceptsTab = true;
            txtXml.Dock = DockStyle.Fill;
            txtXml.Font = new Font("Consolas", 10F);
            txtXml.Location = new Point(3, 3);
            txtXml.MaxLength = int.MaxValue;
            txtXml.Multiline = true;
            txtXml.Name = "txtXml";
            txtXml.ScrollBars = ScrollBars.Both;
            txtXml.Size = new Size(986, 406);
            txtXml.TabIndex = 0;
            txtXml.WordWrap = false;
            //
            // pnlXmlBottom
            //
            pnlXmlBottom.Controls.Add(chkNovaVersao);
            pnlXmlBottom.Controls.Add(btnSalvarXml);
            pnlXmlBottom.Controls.Add(btnValidarXml);
            pnlXmlBottom.Dock = DockStyle.Bottom;
            pnlXmlBottom.Location = new Point(3, 409);
            pnlXmlBottom.Name = "pnlXmlBottom";
            pnlXmlBottom.Size = new Size(986, 40);
            pnlXmlBottom.TabIndex = 1;
            //
            // btnValidarXml
            //
            btnValidarXml.Location = new Point(6, 6);
            btnValidarXml.Name = "btnValidarXml";
            btnValidarXml.Size = new Size(110, 28);
            btnValidarXml.TabIndex = 0;
            btnValidarXml.Text = "Validar";
            btnValidarXml.UseVisualStyleBackColor = true;
            btnValidarXml.Click += btnValidarXml_Click;
            //
            // btnSalvarXml
            //
            btnSalvarXml.Location = new Point(122, 6);
            btnSalvarXml.Name = "btnSalvarXml";
            btnSalvarXml.Size = new Size(110, 28);
            btnSalvarXml.TabIndex = 1;
            btnSalvarXml.Text = "Salvar XML";
            btnSalvarXml.UseVisualStyleBackColor = true;
            btnSalvarXml.Click += btnSalvarXml_Click;
            //
            // chkNovaVersao
            //
            chkNovaVersao.AutoSize = true;
            chkNovaVersao.Location = new Point(250, 11);
            chkNovaVersao.Name = "chkNovaVersao";
            chkNovaVersao.Size = new Size(260, 19);
            chkNovaVersao.TabIndex = 2;
            chkNovaVersao.Text = "Salvar como nova versão (arquiva a anterior)";
            chkNovaVersao.UseVisualStyleBackColor = true;
            //
            // tabEstrutura
            //
            tabEstrutura.Controls.Add(treeEstrutura);
            tabEstrutura.Location = new Point(4, 24);
            tabEstrutura.Name = "tabEstrutura";
            tabEstrutura.Padding = new Padding(3);
            tabEstrutura.Size = new Size(992, 452);
            tabEstrutura.TabIndex = 1;
            tabEstrutura.Text = "Estrutura interpretada";
            tabEstrutura.UseVisualStyleBackColor = true;
            //
            // treeEstrutura
            //
            treeEstrutura.Dock = DockStyle.Fill;
            treeEstrutura.Location = new Point(3, 3);
            treeEstrutura.Name = "treeEstrutura";
            treeEstrutura.Size = new Size(986, 446);
            treeEstrutura.TabIndex = 0;
            //
            // tabPers
            //
            tabPers.Controls.Add(dgvPers);
            tabPers.Controls.Add(pnlPersBottom);
            tabPers.Location = new Point(4, 24);
            tabPers.Name = "tabPers";
            tabPers.Padding = new Padding(3);
            tabPers.Size = new Size(992, 452);
            tabPers.TabIndex = 2;
            tabPers.Text = "Personalizações";
            tabPers.UseVisualStyleBackColor = true;
            //
            // dgvPers
            //
            dgvPers.AllowUserToAddRows = false;
            dgvPers.AllowUserToDeleteRows = false;
            dgvPers.AllowUserToResizeRows = false;
            dgvPers.BackgroundColor = SystemColors.Window;
            dgvPers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPers.Columns.AddRange(new DataGridViewColumn[] { colPCaminho, colPDescricao, colPTipo, colPObrig, colPModo, colPValor, colPSobrescrever });
            dgvPers.Dock = DockStyle.Fill;
            dgvPers.Location = new Point(3, 3);
            dgvPers.Name = "dgvPers";
            dgvPers.RowHeadersVisible = false;
            dgvPers.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvPers.Size = new Size(986, 406);
            dgvPers.TabIndex = 0;
            //
            // colPCaminho
            //
            colPCaminho.HeaderText = "Propriedade JSON";
            colPCaminho.Name = "colPCaminho";
            colPCaminho.ReadOnly = true;
            colPCaminho.Width = 230;
            //
            // colPDescricao
            //
            colPDescricao.HeaderText = "Descrição";
            colPDescricao.Name = "colPDescricao";
            colPDescricao.ReadOnly = true;
            colPDescricao.Width = 200;
            //
            // colPTipo
            //
            colPTipo.HeaderText = "Tipo";
            colPTipo.Name = "colPTipo";
            colPTipo.ReadOnly = true;
            colPTipo.Width = 70;
            //
            // colPObrig
            //
            colPObrig.HeaderText = "Obrig.";
            colPObrig.Name = "colPObrig";
            colPObrig.ReadOnly = true;
            colPObrig.Width = 50;
            //
            // colPModo
            //
            colPModo.HeaderText = "Modo";
            colPModo.Items.AddRange(new object[] { "Valor fixo", "Coluna", "Padrão" });
            colPModo.Name = "colPModo";
            colPModo.Width = 100;
            //
            // colPValor
            //
            colPValor.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colPValor.HeaderText = "Valor / coluna da origem";
            colPValor.Name = "colPValor";
            //
            // colPSobrescrever
            //
            colPSobrescrever.HeaderText = "Sobrescreve XML";
            colPSobrescrever.Name = "colPSobrescrever";
            colPSobrescrever.Width = 100;
            //
            // pnlPersBottom
            //
            pnlPersBottom.Controls.Add(lblPersInfo);
            pnlPersBottom.Controls.Add(btnSalvarPers);
            pnlPersBottom.Dock = DockStyle.Bottom;
            pnlPersBottom.Location = new Point(3, 409);
            pnlPersBottom.Name = "pnlPersBottom";
            pnlPersBottom.Size = new Size(986, 40);
            pnlPersBottom.TabIndex = 1;
            //
            // btnSalvarPers
            //
            btnSalvarPers.Location = new Point(6, 6);
            btnSalvarPers.Name = "btnSalvarPers";
            btnSalvarPers.Size = new Size(170, 28);
            btnSalvarPers.TabIndex = 0;
            btnSalvarPers.Text = "Salvar e revalidar";
            btnSalvarPers.UseVisualStyleBackColor = true;
            btnSalvarPers.Click += btnSalvarPers_Click;
            //
            // lblPersInfo
            //
            lblPersInfo.Location = new Point(190, 12);
            lblPersInfo.Name = "lblPersInfo";
            lblPersInfo.Size = new Size(780, 16);
            lblPersInfo.TabIndex = 1;
            //
            // lstResultado
            //
            lstResultado.Dock = DockStyle.Bottom;
            lstResultado.FormattingEnabled = true;
            lstResultado.HorizontalScrollbar = true;
            lstResultado.ItemHeight = 15;
            lstResultado.Location = new Point(0, 480);
            lstResultado.Name = "lstResultado";
            lstResultado.Size = new Size(1000, 109);
            lstResultado.TabIndex = 1;
            //
            // pnlRodape
            //
            pnlRodape.Controls.Add(btnFechar);
            pnlRodape.Dock = DockStyle.Bottom;
            pnlRodape.Location = new Point(0, 589);
            pnlRodape.Name = "pnlRodape";
            pnlRodape.Size = new Size(1000, 44);
            pnlRodape.TabIndex = 2;
            //
            // btnFechar
            //
            btnFechar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnFechar.Location = new Point(898, 8);
            btnFechar.Name = "btnFechar";
            btnFechar.Size = new Size(90, 28);
            btnFechar.TabIndex = 0;
            btnFechar.Text = "Fechar";
            btnFechar.UseVisualStyleBackColor = true;
            btnFechar.Click += btnFechar_Click;
            //
            // frmConfiguracaoXML
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 633);
            Controls.Add(tabPrincipal);
            Controls.Add(lstResultado);
            Controls.Add(pnlRodape);
            MinimumSize = new Size(800, 520);
            Name = "frmConfiguracaoXML";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Configuração XML";
            tabPrincipal.ResumeLayout(false);
            tabXml.ResumeLayout(false);
            tabXml.PerformLayout();
            pnlXmlBottom.ResumeLayout(false);
            pnlXmlBottom.PerformLayout();
            tabEstrutura.ResumeLayout(false);
            tabPers.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvPers).EndInit();
            pnlPersBottom.ResumeLayout(false);
            pnlRodape.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabPrincipal;
        private TabPage tabXml;
        private TextBox txtXml;
        private Panel pnlXmlBottom;
        private CheckBox chkNovaVersao;
        private Button btnSalvarXml;
        private Button btnValidarXml;
        private TabPage tabEstrutura;
        private TreeView treeEstrutura;
        private TabPage tabPers;
        private DataGridView dgvPers;
        private DataGridViewTextBoxColumn colPCaminho;
        private DataGridViewTextBoxColumn colPDescricao;
        private DataGridViewTextBoxColumn colPTipo;
        private DataGridViewCheckBoxColumn colPObrig;
        private DataGridViewComboBoxColumn colPModo;
        private DataGridViewTextBoxColumn colPValor;
        private DataGridViewCheckBoxColumn colPSobrescrever;
        private Panel pnlPersBottom;
        private Label lblPersInfo;
        private Button btnSalvarPers;
        private ListBox lstResultado;
        private Panel pnlRodape;
        private Button btnFechar;
    }
}
