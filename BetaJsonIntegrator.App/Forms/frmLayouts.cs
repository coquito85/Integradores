using BetaJsonIntegrator.Core.Services;

namespace BetaJsonIntegrator.App.Forms
{
    /// <summary>Pesquisar, cadastrar, selecionar e versionar layouts.</summary>
    public partial class frmLayouts : Form
    {
        private readonly LayoutRepository _repo;

        /// <summary>Pasta do layout escolhido (válida quando DialogResult = OK).</summary>
        public string? SelectedFolder { get; private set; }

        public frmLayouts(LayoutRepository repo)
        {
            InitializeComponent();
            _repo = repo;
            Carregar();
        }

        private void Carregar(string? selecionar = null)
        {
            var filtro = txtPesquisar.Text.Trim();
            dgvLayouts.Rows.Clear();

            foreach (var l in _repo.List())
            {
                var m = l.Metadata;
                if (filtro.Length > 0 &&
                    !(l.FolderName.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                      m.FriendlyName.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                      m.Identifier.Contains(filtro, StringComparison.OrdinalIgnoreCase)))
                    continue;

                int idx = dgvLayouts.Rows.Add(l.FolderName, m.FriendlyName, m.Identifier, m.LayoutVersion,
                    l.HasSchema ? (string.IsNullOrWhiteSpace(m.SchemaVersion) ? "sim" : m.SchemaVersion) : "—",
                    m.UpdatedAt.ToString("dd/MM/yyyy HH:mm"), m.Notes);
                dgvLayouts.Rows[idx].Tag = l.FolderName;
                if (selecionar != null && string.Equals(selecionar, l.FolderName, StringComparison.OrdinalIgnoreCase))
                {
                    dgvLayouts.ClearSelection();
                    dgvLayouts.Rows[idx].Selected = true;
                }
            }
            lblContagem.Text = $"{dgvLayouts.Rows.Count} layout(s)";
        }

        private string? Atual() =>
            dgvLayouts.CurrentRow?.Tag as string;

        private void txtPesquisar_TextChanged(object sender, EventArgs e) => Carregar(Atual());

        private void btnSelecionar_Click(object sender, EventArgs e)
        {
            var pasta = Atual();
            if (pasta == null)
            {
                MessageBox.Show(this, "Selecione um layout na lista.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SelectedFolder = pasta;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void dgvLayouts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) btnSelecionar_Click(sender, EventArgs.Empty);
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // ---------------- cadastro ----------------
        private void btnNovo_Click(object sender, EventArgs e) => grpNovo.Visible = !grpNovo.Visible;

        private void btnProcurarXml_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Title = "XML do layout", Filter = "XML (*.xml)|*.xml|Todos (*.*)|*.*" };
            if (dlg.ShowDialog(this) == DialogResult.OK) txtXml.Text = dlg.FileName;
        }

        private void btnProcurarSchema_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Title = "JSON Schema", Filter = "JSON (*.json)|*.json|Todos (*.*)|*.*" };
            if (dlg.ShowDialog(this) == DialogResult.OK) txtSchema.Text = dlg.FileName;
        }

        private void btnCriar_Click(object sender, EventArgs e)
        {
            try
            {
                var pasta = txtPasta.Text.Trim();
                if (pasta.Length == 0 || txtXml.Text.Trim().Length == 0)
                {
                    MessageBox.Show(this, "Informe o nome da pasta e o arquivo XML do layout.", Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _repo.Create(pasta, txtXml.Text.Trim(),
                    string.IsNullOrWhiteSpace(txtSchema.Text) ? null : txtSchema.Text.Trim(),
                    txtNotas.Text.Trim());

                txtPasta.Clear(); txtXml.Clear(); txtSchema.Clear(); txtNotas.Clear();
                grpNovo.Visible = false;
                Carregar(pasta);
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException or ArgumentException or System.Text.Json.JsonException)
            {
                MessageBox.Show(this, ex.Message, "Não foi possível cadastrar o layout", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------------- versionamento ----------------
        private void btnNovaVersao_Click(object sender, EventArgs e)
        {
            var pasta = Atual();
            if (pasta == null) return;

            var ok = MessageBox.Show(this,
                "Será criada uma nova versão do layout: os arquivos atuais são arquivados em history\\v<n> " +
                "e o número da versão é incrementado.\n\nContinuar?",
                "Nova versão", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (ok != DialogResult.Yes) return;

            try
            {
                var bundle = _repo.Load(pasta);
                _repo.SaveLayoutXml(pasta, bundle.LayoutXmlText, createVersion: true);
                Carregar(pasta);
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException or ArgumentException)
            {
                MessageBox.Show(this, ex.Message, "Não foi possível versionar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
