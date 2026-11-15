using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using BetaJsonIntegrator.Core.Configuration;
using BetaJsonIntegrator.Core.Generator;
using BetaJsonIntegrator.Core.Mapping;
using BetaJsonIntegrator.Core.Validation;
using BetaJsonIntegrator.Data.Providers;

namespace BetaJsonIntegrator.Core.Services;

/// <summary>
/// Orquestra o fluxo da spec. (seção 11): carregar layout → validar configuração → ler origem →
/// resolver mapeamentos → gerar JSON → validar com Schema → devolver resultado para visualizar/exportar.
/// Não depende de formulários.
/// </summary>
public sealed class IntegratorService
{
    private static readonly JsonSerializerOptions OutputOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly IIntegratorLog _log;

    public IntegratorService(LayoutRepository repository, IIntegratorLog? log = null)
    {
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _log = log ?? new NullIntegratorLog();
    }

    public LayoutRepository Repository { get; }

    public LayoutBundle LoadLayout(string folderName) => Repository.Load(folderName);

    /// <summary>Valida a configuração (XML + personalizações) sem tocar nos dados.</summary>
    public LayoutValidationResult ValidateLayout(LayoutBundle bundle, string? generateName = null)
    {
        var issues = new List<ValidationIssue>(bundle.ParseIssues);
        if (bundle.Layout == null)
            return new LayoutValidationResult(issues, new MappingPlan());

        var gen = SelectGenerate(bundle.Layout, generateName);
        if (gen == null)
        {
            issues.Add(ValidationIssue.Error("CFG_NO_GENERATE", "Layout",
                string.IsNullOrWhiteSpace(generateName)
                    ? "O layout não possui nenhum <generate>."
                    : $"O <generate> '{generateName}' não existe no layout."));
            return new LayoutValidationResult(issues, new MappingPlan());
        }

        var result = new LayoutValidator().Validate(gen, bundle.Customizations);
        issues.AddRange(result.Issues);
        return new LayoutValidationResult(issues, result.Plan);
    }

    public RunResult Run(LayoutBundle bundle, IDataProvider provider, RunOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        ArgumentNullException.ThrowIfNull(provider);
        options ??= new RunOptions();

        var started = DateTime.Now;
        var clock = Stopwatch.StartNew();
        var hash = ComputeHash(bundle);
        var gen = bundle.Layout == null ? null : SelectGenerate(bundle.Layout, options.GenerateName);

        _log.Info($"Execução iniciada: layout={bundle.FolderName} v{bundle.Metadata.LayoutVersion} " +
                  $"generate={gen?.Name} origem={provider.Name} hash={hash[..12]}");

        var issues = new List<ValidationIssue>();
        string? json = null;
        int records = 0;
        bool blocked = false;
        bool schemaValidated = false;
        bool preview = false;
        IReadOnlyList<FieldConfig> pending = Array.Empty<FieldConfig>();

        try
        {
            var validation = ValidateLayout(bundle, options.GenerateName);
            issues.AddRange(validation.Issues);
            pending = validation.Plan.Pending.ToList();

            // em prévia, a pendência deixa de ser erro (continua listada como aviso)
            if (options.AllowPending)
                for (int i = 0; i < issues.Count; i++)
                    if (issues[i].Code == MappingResolver.PendingCode && issues[i].Severity == IssueSeverity.Error)
                        issues[i] = issues[i] with { Severity = IssueSeverity.Warning };

            bool configErrors = issues.Any(i => i.Severity == IssueSeverity.Error);
            blocked = gen == null || configErrors || (pending.Count > 0 && !options.AllowPending);

            if (!blocked && gen != null)
            {
                preview = pending.Count > 0;
                var table = gen.Tables[0];

                var tableName = string.IsNullOrWhiteSpace(options.TableSource) ? table.Name : options.TableSource!.Trim();
                if (!SqlIdentifier.IsValid(tableName))
                {
                    issues.Add(ValidationIssue.Error("SRC_TABLE_INVALID", "origem", "Nome de tabela de origem inválido."));
                }
                else
                {
                    var request = new DataRequest
                    {
                        TableName = tableName,
                        Columns = table.Columns.Select(c => new SourceColumn(c.Name, c.OrderBy)).ToList(),
                        Filters = options.Filters ?? Array.Empty<DataFilter>()
                    };

                    var data = LoadSource(provider, request, issues);
                    if (data != null)
                    {
                        CheckSourceColumns(table, validation.Plan, data, issues);
                        if (data.Rows.Count == 0)
                            issues.Add(ValidationIssue.Warning("SRC_EMPTY", "origem", "A origem não retornou registros."));

                        var output = new JsonGenerator().Generate(gen, table, validation.Plan, data);
                        issues.AddRange(output.Issues);
                        records = output.RecordCount;
                        json = output.Root.ToJsonString(OutputOptions);

                        schemaValidated = RunSchema(bundle, json, issues);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // falha inesperada: registra só o tipo (a mensagem pode conter dados) e devolve erro ao usuário
            _log.Error("Falha inesperada na execução: " + ex.GetType().Name);
            issues.Add(ValidationIssue.Error("RUN_UNEXPECTED", "", "Falha inesperada durante a geração: " + ex.GetType().Name));
            json = null;
        }

        clock.Stop();
        var result = new RunResult
        {
            LayoutFolder = bundle.FolderName,
            LayoutName = bundle.DisplayName,
            LayoutIdentifier = bundle.Metadata.Identifier,
            LayoutVersion = bundle.Metadata.LayoutVersion,
            SchemaVersion = bundle.Metadata.SchemaVersion,
            GenerateName = gen?.Name ?? "",
            SourceName = provider.Name,
            ConfigHash = hash,
            StartedAt = started,
            Duration = clock.Elapsed,
            RecordCount = records,
            Json = json,
            Issues = issues,
            Pending = pending,
            Blocked = blocked,
            SchemaValidated = schemaValidated,
            IsPreview = preview
        };

        _log.Info($"Execução concluída: layout={bundle.FolderName} registros={records} erros={result.ErrorCount} " +
                  $"avisos={result.WarningCount} bloqueada={blocked} prévia={preview} duração={clock.Elapsed.TotalSeconds:F1}s");
        return result;
    }

    // ------------------------------------------------------------------ etapas
    private System.Data.DataTable? LoadSource(IDataProvider provider, DataRequest request, List<ValidationIssue> issues)
    {
        try
        {
            return provider.Load(request);
        }
        catch (Exception ex)
        {
            _log.Error($"Falha ao ler a origem '{provider.Name}': {ex.GetType().Name}");
            issues.Add(ValidationIssue.Error("SRC_LOAD_FAILED", "origem",
                $"Falha ao ler a origem '{provider.Name}': {ex.Message}"));
            return null;
        }
    }

    private static void CheckSourceColumns(TableConfig table, MappingPlan plan, System.Data.DataTable data, List<ValidationIssue> issues)
    {
        var referenced = new HashSet<string>(
            plan.Fields.Values.Where(r => r.Kind == FieldKind.Column && r.Column != null).Select(r => r.Column!),
            StringComparer.OrdinalIgnoreCase);

        foreach (var col in table.Columns)
        {
            if (data.Columns.Contains(col.Name)) continue;

            var path = $"{table.Alias}.{col.Name}";
            issues.Add(referenced.Contains(col.Name)
                ? ValidationIssue.Error("SRC_COLUMN_MISSING", path, "A coluna usada pelo layout não existe na origem de dados.")
                : ValidationIssue.Warning("SRC_COLUMN_MISSING", path, "A coluna declarada no layout não existe na origem de dados."));
        }
    }

    private static bool RunSchema(LayoutBundle bundle, string json, List<ValidationIssue> issues)
    {
        if (!bundle.HasSchema)
        {
            issues.Add(ValidationIssue.Info("SCHEMA_NONE", "schema.json",
                "O layout não possui JSON Schema; a conformidade estrutural não foi validada."));
            return false;
        }

        var result = JsonSchemaValidator.Validate(json, bundle.SchemaText!);
        issues.AddRange(result.Issues);

        if (result.UnsupportedKeywords.Count > 0)
            issues.Add(ValidationIssue.Info("SCHEMA_UNSUPPORTED", "schema.json",
                "Palavras-chave do Schema não avaliadas por este validador: " + string.Join(", ", result.UnsupportedKeywords) + "."));

        return !result.Issues.Any(i => i.Code == "SCHEMA_INVALID");
    }

    private static GenerateConfig? SelectGenerate(LayoutConfiguration layout, string? name) =>
        string.IsNullOrWhiteSpace(name)
            ? layout.Generates.FirstOrDefault()
            : layout.Generates.FirstOrDefault(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));

    private static string ComputeHash(LayoutBundle bundle)
    {
        var text = bundle.LayoutXmlText + "\n--\n" + bundle.Customizations.ToXml() + "\n--\n" + (bundle.SchemaText ?? "");
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    }
}
