using System.Text;
using BetaJsonIntegrator.Core;
using BetaJsonIntegrator.Core.Services;
using BetaJsonIntegrator.Core.Validation;
using BetaJsonIntegrator.Data.Providers;
using ReferenciaFactory;

namespace BetaJsonIntegrator.App.Forms
{
    /// <summary>
    /// Tela principal: layout ativo, origem de dados, geração/validação/exportação e painel de mensagens.
    /// Fluxo (spec. seção 11): layout → origem → gerar → validar → visualizar → exportar.
    /// </summary>
    public partial class frmPrincipal : Form
    {
        private readonly LayoutRepository _repo;
        private readonly IntegratorService _svc;
        private readonly ExportService _export;

        private LayoutBundle? _bundle;
        private RunResult? _run;

        public frmPrincipal()
        {
            InitializeComponent();

            _repo = new LayoutRepository(AppPaths.Layouts);
            _svc = new IntegratorService(_repo, new FileIntegratorLog(AppPaths.Logs));
            _export = new ExportService(AppPaths.Output);

            cboFonte.SelectedIndex = 0;
            cboDelimitador.SelectedIndex = 0;
            cboEncoding.SelectedIndex = 0;
        }

        private void frmPrincipal_Load(object sender, EventArgs e)
        {
            AtualizarEstado();
            Mensagem("Selecione um layout para começar.");

            var layouts = _repo.List();
            if (layouts.Count == 1) CarregarLayout(layouts[0].FolderName);
        }

        // ------------------------------------------------------------------ layout
        private void SelecionarLayout()
        {
            using var f = new frmLayouts(_repo);
            if (f.ShowDialog(this) == DialogResult.OK && f.SelectedFolder != null)
                CarregarLayout(f.SelectedFolder);
        }

        private void CarregarLayout(string pasta)
        {
            try
            {
                _bundle = _svc.LoadLayout(pasta);
                _run = null;

                cboGenerate.Items.Clear();
                if (_bundle.Layout != null)
                    foreach (var g in _bundle.Layout.Generates) cboGenerate.Items.Add(g.Name);
                if (cboGenerate.Items.Count > 0) cboGenerate.SelectedIndex = 0;

                Mensagem($"Layout carregado: {_bundle.DisplayName} (versão {_bundle.Metadata.LayoutVersion}).");
                var val = _svc.ValidateLayout(_bundle, cboGenerate.SelectedItem as string);
                var pend = val.Plan.Pending.Count;
                if (val.HasErrors)
                    Mensagem($"Configuração com {val.Issues.Count(i => i.Severity == IssueSeverity.Error)} erro(s) e {pend} campo(s) pendente(s). Abra \"Configuração XML\" para resolver.", IssueSeverity.Warning);
                else
                    Mensagem("Configuração válida.");
            }
            catch (Exception ex) when (ex is IOException or ArgumentException)
            {
                Mensagem("Não foi possível carregar o layout: " + ex.Message, IssueSeverity.Error);
                _bundle = null;
            }
            AtualizarEstado();
        }

        private void AbrirConfiguracao()
        {
            if (_bundle == null) return;
            using var f = new frmConfiguracaoXML(_svc, _bundle);
            f.ShowDialog(this);
            if (f.Changed) CarregarLayout(_bundle.FolderName);
        }

        // ------------------------------------------------------------------ origem de dados
        private void cboFonte_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool csv = cboFonte.SelectedIndex == 0;
            pnlCsv.Visible = csv;
            pnlBanco.Visible = !csv;
        }

        private void btnProcurar_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Arquivo CSV de origem",
                Filter = "CSV/Texto (*.csv;*.txt)|*.csv;*.txt|Todos os arquivos (*.*)|*.*"
            };
            if (dlg.ShowDialog(this) == DialogResult.OK) txtArquivo.Text = dlg.FileName;
        }

        private void chkMostrarConexao_CheckedChanged(object sender, EventArgs e) =>
            txtConexao.UseSystemPasswordChar = !chkMostrarConexao.Checked;

        private System.Data.Common.DbProviderFactory FabricaAtual() =>
            cboFonte.SelectedIndex == 1
                ? MySqlConnector.MySqlConnectorFactory.Instance
                : Microsoft.Data.SqlClient.SqlClientFactory.Instance;

        private async void btnTestarConexao_Click(object sender, EventArgs e)
        {
            var cs = txtConexao.Text.Trim();
            if (cs.Length == 0)
            {
                Mensagem("Informe a string de conexão.", IssueSeverity.Warning);
                return;
            }
            var factory = FabricaAtual();
            btnTestarConexao.Enabled = false;
            try
            {
                await Task.Run(() =>
                {
                    using var db = new DatabaseConnection(cs, factory);
                    db.Open();
                    db.ExecuteScalar("SELECT 1");
                });
                Mensagem("Conexão estabelecida com sucesso.");
            }
            catch (Exception ex)
            {
                // a mensagem do driver pode conter o servidor, mas nunca a senha informada
                Mensagem("Falha ao conectar: " + ex.Message, IssueSeverity.Error);
            }
            finally
            {
                btnTestarConexao.Enabled = true;
            }
        }

        /// <summary>Monta o provedor conforme a origem escolhida. A string de conexão fica só em memória.</summary>
        private IDataProvider CriarProvedor()
        {
            switch (cboFonte.SelectedIndex)
            {
                case 0:
                    if (string.IsNullOrWhiteSpace(txtArquivo.Text) || !File.Exists(txtArquivo.Text))
                        throw new InvalidOperationException("Selecione um arquivo CSV existente.");
                    var delim = cboDelimitador.SelectedIndex switch { 1 => ',', 2 => '|', 3 => '\t', _ => ';' };
                    Encoding enc = cboEncoding.SelectedIndex switch
                    {
                        1 => Encoding.GetEncoding(1252),
                        2 => Encoding.Latin1,
                        _ => new UTF8Encoding(false)
                    };
                    return new CsvProvider(txtArquivo.Text, new CsvOptions
                    {
                        Delimiter = delim,
                        Encoding = enc,
                        HasHeader = chkCabecalho.Checked
                    });

                case 1:
                    RequerConexao();
                    return new MariaDbProvider(MySqlConnector.MySqlConnectorFactory.Instance, txtConexao.Text.Trim());

                default:
                    RequerConexao();
                    return new SqlServerProvider(Microsoft.Data.SqlClient.SqlClientFactory.Instance, txtConexao.Text.Trim());
            }
        }

        private void RequerConexao()
        {
            if (string.IsNullOrWhiteSpace(txtConexao.Text))
                throw new InvalidOperationException("Informe a string de conexão.");
        }

        // ------------------------------------------------------------------ execução
        private async void btnGerar_Click(object sender, EventArgs e)
        {
            if (_bundle == null)
            {
                SelecionarLayout();
                if (_bundle == null) return;
            }

            IDataProvider provider;
            try { provider = CriarProvedor(); }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                Mensagem(ex.Message, IssueSeverity.Warning);
                return;
            }

            var bundle = _bundle;
            var opcoes = new RunOptions
            {
                GenerateName = cboGenerate.SelectedItem as string,
                AllowPending = chkPrevia.Checked
            };

            DefinirOcupado(true);
            Mensagem("Gerando JSON...");
            try
            {
                _run = await Task.Run(() => _svc.Run(bundle, provider, opcoes));
            }
            finally
            {
                DefinirOcupado(false);
            }
            MostrarResultado();
        }

        private void MostrarResultado()
        {
            if (_run == null) return;
            var r = _run;

            if (r.Blocked)
            {
                lblResumo.Text = $"Geração bloqueada: {r.ErrorCount} erro(s) e {r.Pending.Count} campo(s) pendente(s) na configuração.";
                Mensagem(lblResumo.Text, IssueSeverity.Error);
                foreach (var p in r.Pending)
                    Mensagem($"Pendente: {p.Path} ({p.Type}){(p.Required ? " — obrigatório" : "")}", IssueSeverity.Warning);
                Mensagem("Use \"Configuração XML\" > Personalizações, ou marque \"prévia\" para gerar sem preencher os pendentes.", IssueSeverity.Info);
            }
            else
            {
                lblResumo.Text = $"{r.RecordCount} registro(s) • {r.ErrorCount} erro(s) • {r.WarningCount} aviso(s) • {r.Duration.TotalSeconds:F1}s" +
                                 (r.SchemaValidated ? " • Schema validado" : " • sem Schema");
                Mensagem(lblResumo.Text, r.ErrorCount > 0 ? IssueSeverity.Error : IssueSeverity.Info);
                foreach (var i in r.Issues.Where(i => i.Severity == IssueSeverity.Error).Take(20))
                    Mensagem($"{i.Path}: {i.Message}", IssueSeverity.Error);
                if (r.ErrorCount > 20) Mensagem($"... e mais {r.ErrorCount - 20} erro(s). Veja a tela de Validação.", IssueSeverity.Info);
            }
            AtualizarEstado();
        }

        private void btnVerJson_Click(object sender, EventArgs e)
        {
            if (_run?.Json == null) return;
            using var f = new frmVisualizadorJSON(_run.Json, _bundle?.DisplayName ?? "");
            f.ShowDialog(this);
        }

        private void btnVerValidacao_Click(object sender, EventArgs e)
        {
            if (_run != null)
            {
                using var f = new frmValidacao(_run.Issues, _run, _bundle?.DisplayName);
                f.ShowDialog(this);
            }
            else if (_bundle != null)
            {
                var val = _svc.ValidateLayout(_bundle, cboGenerate.SelectedItem as string);
                using var f = new frmValidacao(val.Issues, null, _bundle.DisplayName + " (configuração)");
                f.ShowDialog(this);
            }
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            if (_run == null || _bundle == null || !_run.IsExportReady)
            {
                Mensagem("Só é possível exportar um JSON gerado sem erros.", IssueSeverity.Warning);
                return;
            }

            var res = _export.Export(_bundle.FolderName, _run, ExistsPolicy.Fail);
            if (!res.Written && res.ExistingFiles.Count > 0)
            {
                var resp = MessageBox.Show(this,
                    "Já existem arquivos exportados para este layout/ano:\n\n" +
                    string.Join("\n", res.ExistingFiles.Select(Path.GetFileName)) +
                    "\n\nSim = sobrescrever\nNão = salvar com data/hora no nome (preserva os atuais)\nCancelar = não exportar",
                    "Arquivos existentes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (resp == DialogResult.Cancel) return;
                res = _export.Export(_bundle.FolderName, _run,
                    resp == DialogResult.Yes ? ExistsPolicy.Overwrite : ExistsPolicy.Timestamp);
            }

            if (res.Written)
                Mensagem($"Exportado: {res.JsonPath}");
        }

        // ------------------------------------------------------------------ navegação
        private void btnNavLayouts_Click(object sender, EventArgs e) => SelecionarLayout();

        private void btnNavConfig_Click(object sender, EventArgs e)
        {
            if (_bundle == null) { Mensagem("Selecione um layout primeiro.", IssueSeverity.Warning); return; }
            AbrirConfiguracao();
        }

        private void btnNavJson_Click(object sender, EventArgs e)
        {
            if (_run?.Json == null) { Mensagem("Gere o JSON primeiro.", IssueSeverity.Warning); return; }
            btnVerJson_Click(sender, e);
        }

        private void btnNavValidacao_Click(object sender, EventArgs e)
        {
            if (_bundle == null) { Mensagem("Selecione um layout primeiro.", IssueSeverity.Warning); return; }
            btnVerValidacao_Click(sender, e);
        }

        // ------------------------------------------------------------------ UI
        private void DefinirOcupado(bool ocupado)
        {
            prgExecucao.Visible = ocupado;
            pnlConteudo.Enabled = !ocupado;
            pnlNav.Enabled = !ocupado;
            lblStatus.Text = ocupado ? "Processando..." : "Pronto";
        }

        private void AtualizarEstado()
        {
            bool temLayout = _bundle != null;
            lblLayoutAtivo.Text = temLayout ? "Layout ativo: " + _bundle!.DisplayName : "Nenhum layout selecionado";
            lblLayoutDetalhe.Text = temLayout
                ? $"Identificador {_bundle!.Metadata.Identifier}  •  versão {_bundle.Metadata.LayoutVersion}" +
                  (string.IsNullOrWhiteSpace(_bundle.Metadata.SchemaVersion) ? "" : $"  •  Schema {_bundle.Metadata.SchemaVersion}")
                : "Use \"Layouts\" para escolher ou cadastrar um layout.";

            btnGerar.Enabled = temLayout;
            btnNavConfig.Enabled = temLayout;
            btnNavValidacao.Enabled = temLayout;
            btnVerValidacao.Enabled = temLayout;
            btnVerJson.Enabled = _run?.Json != null;
            btnNavJson.Enabled = _run?.Json != null;
            btnExportar.Enabled = _run?.IsExportReady == true;
            lblStatus.Text = "Pronto";
        }

        private void Mensagem(string texto, IssueSeverity nivel = IssueSeverity.Info)
        {
            string prefixo = nivel switch
            {
                IssueSeverity.Error => "[ERRO] ",
                IssueSeverity.Warning => "[AVISO] ",
                _ => ""
            };
            lstMensagens.Items.Add($"{DateTime.Now:HH:mm:ss}  {prefixo}{texto}");
            lstMensagens.TopIndex = lstMensagens.Items.Count - 1;
        }
    }
}
