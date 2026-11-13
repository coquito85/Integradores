using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using BetaJsonIntegrator.Core.Validation;

namespace BetaJsonIntegrator.Core.Services
{
    public enum ExistsPolicy
    {
        /// <summary>Não grava e informa os arquivos existentes (padrão — pede confirmação ao usuário).</summary>
        Fail,
        Overwrite,
        /// <summary>Grava com sufixo yyyyMMdd-HHmmss, preservando o arquivo anterior.</summary>
        Timestamp
    }

    public sealed class ExportResult
    {
        public bool Written { get; set; }
        public string JsonPath { get; set; } = "";
        public string ReportPath { get; set; } = "";
        public List<string> ExistingFiles { get; } = new();
    }

    /// <summary>Exporta Output/&lt;Layout&gt;/&lt;Ano&gt;/declaracoes.json e validation-report.json.</summary>
    public sealed class ExportService
    {
        public const string JsonFileName = "declaracoes.json";
        public const string ReportFileName = "validation-report.json";

        public string OutputRoot { get; }

        public ExportService(string outputRoot) => OutputRoot = outputRoot;

        public static int GuessYear(string? generateName)
        {
            var m = Regex.Match(generateName ?? "", @"\b(20\d{2})\b");
            return m.Success ? int.Parse(m.Groups[1].Value) : DateTime.Now.Year;
        }

        public string GetTargetFolder(string layoutFolder, int year) =>
            Path.Combine(OutputRoot, layoutFolder, year.ToString());

        public ExportResult Export(string layoutFolder, RunResult run, ExistsPolicy policy = ExistsPolicy.Fail)
        {
            if (run.Json == null) throw new InvalidOperationException("Não há JSON gerado para exportar.");

            var folder = GetTargetFolder(layoutFolder, GuessYear(run.GenerateName));
            var jsonPath = Path.Combine(folder, JsonFileName);
            var reportPath = Path.Combine(folder, ReportFileName);
            var res = new ExportResult { JsonPath = jsonPath, ReportPath = reportPath };

            if (File.Exists(jsonPath)) res.ExistingFiles.Add(jsonPath);
            if (File.Exists(reportPath)) res.ExistingFiles.Add(reportPath);

            if (res.ExistingFiles.Count > 0)
            {
                if (policy == ExistsPolicy.Fail) return res;
                if (policy == ExistsPolicy.Timestamp)
                {
                    var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    res.JsonPath = Path.Combine(folder, $"declaracoes-{stamp}.json");
                    res.ReportPath = Path.Combine(folder, $"validation-report-{stamp}.json");
                }
            }

            Directory.CreateDirectory(folder);
            File.WriteAllText(res.JsonPath, run.Json);
            File.WriteAllText(res.ReportPath, BuildReport(run));
            res.Written = true;
            return res;
        }

        public static string BuildReport(RunResult run)
        {
            var issues = new JsonArray();
            foreach (var i in run.Issues)
                issues.Add(new JsonObject
                {
                    ["severity"] = i.Severity.ToString(),
                    ["code"] = i.Code,
                    ["path"] = i.Path,
                    ["message"] = i.Message
                });

            var report = new JsonObject
            {
                ["layout"] = run.LayoutName,
                ["identifier"] = run.LayoutIdentifier,
                ["layoutVersion"] = run.LayoutVersion,
                ["schemaVersion"] = run.SchemaVersion,
                ["generate"] = run.GenerateName,
                ["source"] = run.SourceName,
                ["configHash"] = run.ConfigHash,
                ["startedAt"] = run.StartedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                ["durationSeconds"] = Math.Round(run.Duration.TotalSeconds, 2),
                ["records"] = run.RecordCount,
                ["schemaValidated"] = run.SchemaValidated,
                ["errors"] = run.ErrorCount,
                ["warnings"] = run.WarningCount,
                ["issues"] = issues
            };
            return report.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        /// <summary>Relatório avulso (para o botão "Exportar relatório" da tela de validação).</summary>
        public static void SaveReportTo(string path, RunResult run) => File.WriteAllText(path, BuildReport(run));
    }
}
