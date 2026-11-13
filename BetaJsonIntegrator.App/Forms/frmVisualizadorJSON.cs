namespace BetaJsonIntegrator.App.Forms
{
    public partial class frmVisualizadorJSON : Form
    {
        public frmVisualizadorJSON(string json, string titulo)
        {
            InitializeComponent();
            Text = "Visualizador JSON — " + titulo;
            txtJson.Text = json;
            lblInfo.Text = $"{json.Length:N0} caracteres";
            AcceptButton = btnBuscar;
        }

        private void btnCopiar_Click(object sender, EventArgs e)
        {
            if (txtJson.TextLength == 0) return;
            Clipboard.SetText(txtJson.Text);
            lblInfo.Text = "JSON copiado para a área de transferência.";
        }

        private void btnSalvarComo_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Title = "Salvar JSON",
                Filter = "JSON (*.json)|*.json|Todos os arquivos (*.*)|*.*",
                FileName = "declaracoes.json",
                OverwritePrompt = true // não sobrescreve sem confirmação
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            File.WriteAllText(dlg.FileName, txtJson.Text);
            lblInfo.Text = "Arquivo salvo.";
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            var termo = txtBuscar.Text;
            if (string.IsNullOrEmpty(termo)) return;

            int start = txtJson.SelectionStart + txtJson.SelectionLength;
            int idx = txtJson.Text.IndexOf(termo, start, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) idx = txtJson.Text.IndexOf(termo, 0, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                lblInfo.Text = "Texto não encontrado.";
                return;
            }
            txtJson.Select(idx, termo.Length);
            txtJson.ScrollToCaret();
            lblInfo.Text = "";
        }
    }
}
