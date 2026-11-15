using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using BetaJsonIntegrator.Core.Validation;

namespace BetaJsonIntegrator.Core.Services;

/// <summary>O que fazer quando os arquivos de saída já existem (spec. seção 14: não sobrescrever sem política explícita).</summary>
public enum ExistsPolicy
{
    /// <summary>Não grava e devolve a lista de arquivos existentes.</summary>
    Fail,
    Overwrite,

    /// <summary>Grava com data/hora no nome, preservando os atuais.</summary>
    Timestamp
}

public sealed class ExportResult
{
    public bool Written { get; init; }
    public string? JsonPath { get; init; }
    public string? ReportPath { get; init; }
    public IReadOnlyList<string> ExistingFiles { get; init; } = Array.Empty<string>();
}

/// <summary>Grava <c>Output/&lt;layout&gt;/&lt;ano&gt;/declaracoes.json</c> e <c>validation-report.json</c> (spec. seção 13).</summary>
public sealed class ExportService
{
    private const string JsonName = "declaracoes";
    private const string ReportName = "validation-report";

    private static readonly UTF8Encoding Utf8NoBom = new(false);

    private static readonly JsonSerializerOptions ReportOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly string _outputRoot;

    public ExportService(string outputRoot)
    {
        if (string.IsNullOrWhiteSpace(outputRoot))
            throw new ArgumentException("Informe a pasta de saída.", nameof(outputRoot));
        _outputRoot = outputRoot;
    }

    public ExportResult Export(string layoutFolder, RunResult run, ExistsPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(run);
        if (!run.IsExportReady || run.Json == null)
            throw new InvalidOperationException("Só é possível exportar um JSON gerado sem erros e sem campos pendentes.");

        var safeFolder = Path.GetFileName(layoutFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var dir = Path.Combine(_outputRoot, safeFolder, DateTime.Now.Year.ToString());

        var jsonPath = Path.Combine(dir, JsonName + ".json");
        var reportPath = Path.Combine(dir, ReportName + ".json");

        var existing = new[] { jsonPath, reportPath }.Where(File.Exists).ToList();
        if (existing.Count > 0)
        {
            switch (policy)
            {
                case ExistsPolicy.Fail:
                    return new ExportResult { Written = false, ExistingFiles = existing };

                case ExistsPolicy.Timestamp:
                    var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    jsonPath = Path.Combine(dir, $"{JsonName}-{stamp}.json");
                    reportPath = Path.Combine(dir, $"{ReportName}-{stamp}.json");
                    break;
            }
        }

        Directory.CreateDirectory(dir);
        WriteAtomic(jsonPath, run.Json);
        WriteAtomic(reportPath, BuildReport(run));

        return new ExportResult { Written = true, JsonPath = jsonPath, ReportPath = reportPath };
    }

    /// <summary>Salva apenas o relatório de validação (usado pela tela de Validação).</summary>
    public static void SaveReportTo(string path, RunResult run)
    {
        ArgumentNullException.ThrowIfNull(run);
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        WriteAtomic(path, BuildReport(run));
    }

    /// <summary>Relatório sem dados pessoais: só metadados da execução e ocorrências com caminho.</summary>
    public static string BuildReport(RunResult run)
    {
        var issues = new JsonArray();
        foreach (var i in run.Issues)
        {
            issues.Add(new JsonObject
            {
                ["severity"] = i.Severity.ToString(),
                ["code"] = i.Code,
                ["path"] = i.Path,
                ["message"] = i.Message
            });
        }

        var pending = new JsonArray();
        foreach (var p in run.Pending)
        {
            pending.Add(new JsonObject
            {
                ["path"] = p.Path,
                ["type"] = p.Type ?? p.RawType,
                ["required"] = p.Required
            });
        }

        var report = new JsonObject
        {
            ["layout"] = new JsonObject
            {
                ["folder"] = run.LayoutFolder,
                ["name"] = run.LayoutName,
                ["identifier"] = run.LayoutIdentifier,
                ["version"] = run.LayoutVersion,
                ["schemaVersion"] = run.SchemaVersion,
                ["configHash"] = run.ConfigHash
            },
            ["generate"] = run.GenerateName,
            ["source"] = run.SourceName,
            ["generatedAt"] = run.StartedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
            ["durationSeconds"] = Math.Round(run.Duration.TotalSeconds, 2),
            ["recordCount"] = run.RecordCount,
            ["schemaValidated"] = run.SchemaValidated,
            ["preview"] = run.IsPreview,
            ["blocked"] = run.Blocked,
            ["errorCount"] = run.ErrorCount,
            ["warningCount"] = run.WarningCount,
            ["pending"] = pending,
            ["issues"] = issues
        };

        return report.ToJsonString(ReportOptions);
    }

    private static void WriteAtomic(string path, string content)
    {
        var temp = path + ".tmp";
        File.WriteAllText(temp, content, Utf8NoBom);
        File.Move(temp, path, overwrite: true);
    }
}
