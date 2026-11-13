using BetaJsonIntegrator.Core.Services;
using BetaJsonIntegrator.Core.Validation;

namespace BetaJsonIntegrator.App.Forms
{
    /// <summary>Lista erros, avisos e campos ausentes com o caminho da propriedade.</summary>
    public partial class frmValidacao : Form
    {
        private const int MaxLinhas = 5000;

        private readonly IReadOnlyList<ValidationIssue> _issues;
        private readonly RunResult? _run;

        public frmValidacao(IReadOnlyList<ValidationIssue> issues, RunResult? run = null, string? titulo = null)
        {
            InitializeComponent();
            _issues = issues;
            _run = run;
            Text = "Validação" + (string.IsNullOrWhiteSpace(titulo) ? "" : " — " + titulo);
            btnExportar.Enabled = run != null;
            cboSeveridade.SelectedIndex = 0;
            Preencher();
        }

        private void cboSeveridade_SelectedIndexChanged(object sender, EventArgs e) => Preencher();

        private void Preencher()
        {
            IEnumerable<ValidationIssue> q = _issues;
            switch (cboSeveridade.SelectedIndex)
            {
                case 1: q = q.Where(i => i.Severity == IssueSeverity.Error); break;
                case 2: q = q.Where(i => i.Severity == IssueSeverity.Warning); break;
                case 3: q = q.Where(i => i.Severity == IssueSeverity.Info); break;
            }
            var lista = q.OrderByDescending(i => i.Severity).ThenBy(i => i.Path, StringComparer.OrdinalIgnoreCase).ToList();

            dgvIssues.SuspendLayout();
            dgvIssues.Rows.Clear();
            foreach (var i in lista.Take(MaxLinhas))
                dgvIssues.Rows.Add(SeveridadeTexto(i.Severity), i.Code, i.Path, i.Message);
            dgvIssues.ResumeLayout();

            int e = _issues.Count(i => i.Severity == IssueSeverity.Error);
            int w = _issues.Count(i => i.Severity == IssueSeverity.Warning);
            int n = _issues.Count(i => i.Severity == IssueSeverity.Info);
            lblTotais.Text = $"Erros: {e}   Avisos: {w}   Informações: {n}" +
                             (lista.Count > MaxLinhas ? $"   (exibindo {MaxLinhas} de {lista.Count})" : "");
        }

        private static string SeveridadeTexto(IssueSeverity s) => s switch
        {
            IssueSeverity.Error => "Erro",
            IssueSeverity.Warning => "Aviso",
            _ => "Info"
        };

        private void dgvIssues_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var sev = dgvIssues.Rows[e.RowIndex].Cells[colSeveridade.Index].Value as string;
            var style = dgvIssues.Rows[e.RowIndex].DefaultCellStyle;
            style.ForeColor = sev == "Erro" ? Color.Firebrick : sev == "Aviso" ? Color.DarkGoldenrod : Color.DimGray;
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            if (_run == null) return;
            using var dlg = new SaveFileDialog
            {
                Title = "Exportar relatório de validação",
                Filter = "JSON (*.json)|*.json",
                FileName = "validation-report.json",
                OverwritePrompt = true
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            ExportService.SaveReportTo(dlg.FileName, _run);
            MessageBox.Show(this, "Relatório exportado.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnFechar_Click(object sender, EventArgs e) => Close();
    }
}
