using OrionAutomator.Tema;

namespace OrionAutomator.Janelas
{
    partial class frmDesignerFluxos
    {
        private System.ComponentModel.IContainer? components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        // Esqueleto do editor. Aqui ficam so a estrutura (zonas, docks, ordem) e valores iniciais:
        // tamanhos, cores, fontes e visibilidade reais vem do UiConfig.xml (janela "frmDesignerFluxos").
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            menuPrincipal = new MenuStrip();
            mnuArquivo = new ToolStripMenuItem();
            mnuFechar = new ToolStripMenuItem();
            mnuExibir = new ToolStripMenuItem();
            mnuTema = new ToolStripMenuItem();
            mnuTemaItem = new ToolStripMenuItem();

            pnlBarra = new Panel();
            lblFluxo = new Label();

            pnlCatalogo = new Panel();
            pnlCatalogoTitulo = new Panel();
            lblCatalogoTitulo = new Label();
            pnlCatalogoCorpo = new Panel();
            splitCatalogo = new Splitter();

            pnlPropriedades = new Panel();
            pnlPropriedadesTitulo = new Panel();
            lblPropriedadesTitulo = new Label();
            pnlPropriedadesCorpo = new Panel();
            splitPropriedades = new Splitter();

            pnlLog = new Panel();
            pnlLogTitulo = new Panel();
            lblLogTitulo = new Label();
            rtbLog = new RichTextBox();
            splitLog = new Splitter();

            areaCanvas = new AreaCanvas();

            pnlStatus = new Panel();
            lblStatus = new Label();

            menuPrincipal.SuspendLayout();
            pnlBarra.SuspendLayout();
            pnlCatalogo.SuspendLayout();
            pnlCatalogoTitulo.SuspendLayout();
            pnlPropriedades.SuspendLayout();
            pnlPropriedadesTitulo.SuspendLayout();
            pnlLog.SuspendLayout();
            pnlLogTitulo.SuspendLayout();
            pnlStatus.SuspendLayout();
            SuspendLayout();

            // ── menu ──
            menuPrincipal.Items.AddRange(new ToolStripItem[] { mnuArquivo, mnuExibir });
            menuPrincipal.Dock = DockStyle.Top;
            menuPrincipal.Name = "menuPrincipal";
            menuPrincipal.TabIndex = 0;

            mnuArquivo.DropDownItems.AddRange(new ToolStripItem[] { mnuFechar });
            mnuArquivo.Name = "mnuArquivo";
            mnuArquivo.Text = "Arquivo";

            mnuFechar.Name = "mnuFechar";
            mnuFechar.Text = "Voltar ao gerenciador";
            mnuFechar.Click += MnuFechar_Click;

            mnuExibir.DropDownItems.AddRange(new ToolStripItem[] { mnuTema });
            mnuExibir.Name = "mnuExibir";
            mnuExibir.Text = "Exibir";

            // item provisorio: sem ele o submenu "Tema" nao abre; TemaMenu.Preencher o substitui ao abrir
            mnuTema.DropDownItems.AddRange(new ToolStripItem[] { mnuTemaItem });
            mnuTema.Name = "mnuTema";
            mnuTema.Text = "Tema";
            mnuTema.DropDownOpening += MnuTema_DropDownOpening;

            mnuTemaItem.Name = "mnuTemaItem";
            mnuTemaItem.Text = "...";

            // ── faixa superior (futuras ferramentas) ──
            pnlBarra.Controls.Add(lblFluxo);
            pnlBarra.Dock = DockStyle.Top;
            pnlBarra.Name = "pnlBarra";
            pnlBarra.Size = new Size(1280, 46);
            pnlBarra.TabIndex = 1;

            lblFluxo.AutoSize = true;
            lblFluxo.Location = new Point(16, 11);
            lblFluxo.Name = "lblFluxo";
            lblFluxo.Text = "Novo fluxo";

            // ── catalogo de acoes (esquerda) ──
            pnlCatalogo.Controls.Add(pnlCatalogoCorpo);
            pnlCatalogo.Controls.Add(pnlCatalogoTitulo);
            pnlCatalogo.Dock = DockStyle.Left;
            pnlCatalogo.Name = "pnlCatalogo";
            pnlCatalogo.Size = new Size(240, 600);
            pnlCatalogo.TabIndex = 2;

            pnlCatalogoTitulo.Controls.Add(lblCatalogoTitulo);
            pnlCatalogoTitulo.Dock = DockStyle.Top;
            pnlCatalogoTitulo.Name = "pnlCatalogoTitulo";
            pnlCatalogoTitulo.Size = new Size(240, 30);

            ConfigurarTituloPainel(lblCatalogoTitulo, "lblCatalogoTitulo", "Catálogo de Ações");

            pnlCatalogoCorpo.Dock = DockStyle.Fill;
            pnlCatalogoCorpo.Name = "pnlCatalogoCorpo";

            ConfigurarDivisor(splitCatalogo, "splitCatalogo", DockStyle.Left);

            // ── propriedades (direita) ──
            pnlPropriedades.Controls.Add(pnlPropriedadesCorpo);
            pnlPropriedades.Controls.Add(pnlPropriedadesTitulo);
            pnlPropriedades.Dock = DockStyle.Right;
            pnlPropriedades.Name = "pnlPropriedades";
            pnlPropriedades.Size = new Size(280, 600);
            pnlPropriedades.TabIndex = 3;

            pnlPropriedadesTitulo.Controls.Add(lblPropriedadesTitulo);
            pnlPropriedadesTitulo.Dock = DockStyle.Top;
            pnlPropriedadesTitulo.Name = "pnlPropriedadesTitulo";
            pnlPropriedadesTitulo.Size = new Size(280, 30);

            ConfigurarTituloPainel(lblPropriedadesTitulo, "lblPropriedadesTitulo", "Propriedades");

            pnlPropriedadesCorpo.Dock = DockStyle.Fill;
            pnlPropriedadesCorpo.Name = "pnlPropriedadesCorpo";

            ConfigurarDivisor(splitPropriedades, "splitPropriedades", DockStyle.Right);

            // ── log (embaixo, so sob o canvas) ──
            pnlLog.Controls.Add(rtbLog);
            pnlLog.Controls.Add(pnlLogTitulo);
            pnlLog.Dock = DockStyle.Bottom;
            pnlLog.Name = "pnlLog";
            pnlLog.Size = new Size(700, 170);
            pnlLog.TabIndex = 4;

            pnlLogTitulo.Controls.Add(lblLogTitulo);
            pnlLogTitulo.Dock = DockStyle.Top;
            pnlLogTitulo.Name = "pnlLogTitulo";
            pnlLogTitulo.Size = new Size(700, 30);

            ConfigurarTituloPainel(lblLogTitulo, "lblLogTitulo", "Log de Execução");

            rtbLog.BorderStyle = BorderStyle.None;
            rtbLog.Dock = DockStyle.Fill;
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            rtbLog.ScrollBars = RichTextBoxScrollBars.Vertical;

            ConfigurarDivisor(splitLog, "splitLog", DockStyle.Bottom);

            // ── canvas ──
            areaCanvas.Dock = DockStyle.Fill;
            areaCanvas.Name = "areaCanvas";
            areaCanvas.TabIndex = 5;

            // ── status ──
            pnlStatus.Controls.Add(lblStatus);
            pnlStatus.Dock = DockStyle.Bottom;
            pnlStatus.Name = "pnlStatus";
            pnlStatus.Size = new Size(1280, 26);
            pnlStatus.TabIndex = 6;

            lblStatus.Dock = DockStyle.Fill;
            lblStatus.Name = "lblStatus";
            lblStatus.Padding = new Padding(10, 0, 0, 0);
            lblStatus.Text = "● Pronto";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;

            // ── Form ──
            // Ordem de Controls.Add = ordem inversa do encaixe (o ultimo adicionado encaixa primeiro, mais por fora):
            // status e menu nas bordas, depois a faixa superior, os paineis laterais (altura total), o log (so sob o
            // canvas) e por fim o canvas (Fill). Cada divisor entra ANTES do painel que ele redimensiona.
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1280, 780);
            Controls.Add(areaCanvas);
            Controls.Add(splitLog);
            Controls.Add(pnlLog);
            Controls.Add(splitPropriedades);
            Controls.Add(pnlPropriedades);
            Controls.Add(splitCatalogo);
            Controls.Add(pnlCatalogo);
            Controls.Add(pnlBarra);
            Controls.Add(menuPrincipal);
            Controls.Add(pnlStatus);
            MainMenuStrip = menuPrincipal;
            Name = "frmDesignerFluxos";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Orion Automator — Editor de Fluxos";

            menuPrincipal.ResumeLayout(false);
            menuPrincipal.PerformLayout();
            pnlBarra.ResumeLayout(false);
            pnlBarra.PerformLayout();
            pnlCatalogo.ResumeLayout(false);
            pnlCatalogoTitulo.ResumeLayout(false);
            pnlPropriedades.ResumeLayout(false);
            pnlPropriedadesTitulo.ResumeLayout(false);
            pnlLog.ResumeLayout(false);
            pnlLogTitulo.ResumeLayout(false);
            pnlStatus.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private static void ConfigurarTituloPainel(Label l, string nome, string texto)
        {
            l.Dock = DockStyle.Fill;
            l.Name = nome;
            l.Padding = new Padding(10, 0, 0, 0);
            l.Text = texto;
            l.TextAlign = ContentAlignment.MiddleLeft;
        }

        private static void ConfigurarDivisor(Splitter s, string nome, DockStyle lado)
        {
            s.Dock = lado;
            s.Name = nome;
            s.TabStop = false;
            s.MinSize = 120;
            s.MinExtra = 120;
            s.Size = lado == DockStyle.Bottom ? new Size(100, 5) : new Size(5, 100);
        }

        #endregion

        private MenuStrip menuPrincipal;
        private ToolStripMenuItem mnuArquivo;
        private ToolStripMenuItem mnuFechar;
        private ToolStripMenuItem mnuExibir;
        private ToolStripMenuItem mnuTema;
        private ToolStripMenuItem mnuTemaItem;

        private Panel pnlBarra;
        private Label lblFluxo;

        private Panel pnlCatalogo;
        private Panel pnlCatalogoTitulo;
        private Label lblCatalogoTitulo;
        private Panel pnlCatalogoCorpo;
        private Splitter splitCatalogo;

        private Panel pnlPropriedades;
        private Panel pnlPropriedadesTitulo;
        private Label lblPropriedadesTitulo;
        private Panel pnlPropriedadesCorpo;
        private Splitter splitPropriedades;

        private Panel pnlLog;
        private Panel pnlLogTitulo;
        private Label lblLogTitulo;
        private RichTextBox rtbLog;
        private Splitter splitLog;

        private AreaCanvas areaCanvas;

        private Panel pnlStatus;
        private Label lblStatus;
    }
}
