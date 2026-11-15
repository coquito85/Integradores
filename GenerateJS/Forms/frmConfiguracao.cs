using BetaJsonIntegrator.Core.Services;
using ReferenciaFactory;

namespace GenerateJS.Forms
{
    /// <summary>
    /// Configurações da execução em diálogo separado (PropertyGrid), como no GeneratorJS:
    /// fonte de dados (CSV ou banco) e geração (layout, configuração, prévia, destino).
    /// Trabalha sobre uma cópia: Cancelar não altera a configuração atual.
    /// </summary>
    public partial class frmConfiguracao : Form
    {
        private readonly IntegratorSettings _work;

        /// <summary>Configuração gravada (válida quando DialogResult = OK).</summary>
        public IntegratorSettings? Settings { get; private set; }

        public frmConfiguracao(LayoutRepository repo, IntegratorSettings current)
        {
            InitializeComponent();

            _work = current.Clone();
            _work.LayoutResolver = code =>
            {
                var item = repo.FindByIdentifier(code);
                return item == null ? null : repo.Load(item.FolderName);
            };
            _work.ResolverLayout(); // identifica se existe layout (bundle) para o LayoutCode atual

            pg_config.SelectedObject = null;
            pg_config.SelectedObject = _work;
        }

        private void btnGravar_Click(object sender, EventArgs e)
        {
            var erros = _work.Validate();
            if (erros.Count > 0)
            {
                MessageBox.Show(this, string.Join(Environment.NewLine + Environment.NewLine, erros),
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Settings = _work;
            DialogResult = DialogResult.OK;
            Close();
        }

        private async void btnTestar_Click(object sender, EventArgs e)
        {
            if (!_work.EhBanco)
            {
                TestarArquivo();
                return;
            }

            var erro = _work.ValidateConnectionOnly();
            if (erro != null)
            {
                MessageBox.Show(this, erro, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var factory = _work.CreateFactory();
            var cs = _work.ConnectionString.Trim();

            btnTestar.Enabled = false;
            try
            {
                await Task.Run(() =>
                {
                    using var db = new DatabaseConnection(cs, factory);
                    db.Open();
                    db.ExecuteScalar("SELECT 1");
                });
                MessageBox.Show(this, "Conexão estabelecida com sucesso.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // a mensagem do driver pode conter o servidor, mas nunca a senha informada
                MessageBox.Show(this, "Falha ao conectar: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTestar.Enabled = true;
            }
        }

        private void TestarArquivo()
        {
            var caminho = _work.Arquivo.Trim();
            if (caminho.Length == 0 || !File.Exists(caminho))
            {
                MessageBox.Show(this, "Informe um arquivo CSV existente.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var kb = new FileInfo(caminho).Length / 1024.0;
            MessageBox.Show(this, $"Arquivo encontrado: {Path.GetFileName(caminho)} ({kb:N0} KB).",
                Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
