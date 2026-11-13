using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using BetaJsonIntegrator.Core.Configuration;
using BetaJsonIntegrator.Core.Generator;
using BetaJsonIntegrator.Core.Mapping;
using BetaJsonIntegrator.Core.Validation;

namespace BetaJsonIntegrator.Core.Services
{
    public sealed class RunOptions
    {
        /// <summary>Nome do &lt;generate&gt; (padrão: o primeiro).</summary>
        public string? GenerateName { get; set; }
        /// <summary>Gera mesmo com pendências/erros de configuração (prévia). Nunca preenche os campos pendentes.</summary>
        public bool AllowPending { get; set; }
    }

    public sealed class RunResult
    {
        public string LayoutName { get; init; } = "";
        public string LayoutIdentifier { get; init; } = "";
        public int LayoutVersion { get; init; }
        public string SchemaVersion { get; init; } = "";
        public string GenerateName { get; init; } = "";
        public string SourceName { get; init; } = "";
        public string ConfigHash { get; set; } = "";
        public DateTime StartedAt { get; init; } = DateTime.Now;
        public TimeSpan Duration { get; set; }
        public int RecordCount { get; set; }
        /// <summary>Execução barrada por pendências/erros de configuração.</summary>
        public bool Blocked { get; set; }
        public bool SchemaValidated { get; set; }
        public string? Json { get; set; }
        public List<ValidationIssue> Issues { get; } = new();
        public List<PendingField> Pending { get; } = new();

        public int ErrorCount => Issues.Count(i => i.Severity == IssueSeverity.Error);
        public int WarningCount => Issues.Count(i => i.Severity == IssueSeverity.Warning);
        public bool HasJson => Json != null;
        /// <summary>JSON gerado, sem erros e (se houver Schema) validado.</summary>
        public bool IsExportReady => HasJson && !Blocked && ErrorCount == 0;
    }

    /// <summary>Orquestra o fluxo operacional: carregar → validar → ler dados → gerar → validar Schema.</summary>
    public sealed class IntegratorService
    {
        private readonly LayoutValidator _validator = new();
        private readonly JsonGenerator _generator = new();
        private readonly IIntegratorLog _log;

        public LayoutRepository Repository { get; }

        public IntegratorService(LayoutRepository repository, IIntegratorLog? log = null)
        {
            Repository = repository;
            _log = log ?? NullIntegratorLog.Instance;
        }

        public LayoutBundle LoadLayout(string folderName)
        {
            var bundle = Repository.Load(folderName);
            _log.Info($"Layout carregado: pasta={bundle.FolderName} id={bundle.Metadata.Identifier} versao={bundle.Metadata.LayoutVersion}");
            return bundle;
        }

        public static GenerateConfig? SelectGenerate(LayoutBundle bundle, string? name)
        {
            var gens = bundle.Layout?.Generates;
            if (gens == null || gens.Count == 0) return null;
            if (string.IsNullOrWhiteSpace(name)) return gens[0];
            return gens.FirstOrDefault(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public LayoutValidationResult ValidateLayout(LayoutBundle bundle, string? generateName = null)
        {
            var gen = SelectGenerate(bundle, generateName);
            if (gen == null)
            {
                var r = new LayoutValidationResult();
                r.Issues.AddRange(bundle.ParseIssues);
                r.Issues.Add(ValidationIssue.Error("GENERATE_NOT_FOUND", generateName ?? "", "Configuração <generate> não encontrada."));
                return r;
            }
            var res = _validator.Validate(gen, bundle.Customizations);
            res.Issues.InsertRange(0, bundle.ParseIssues);
            return res;
        }

        public RunResult Run(LayoutBundle bundle, IDataProvider provider, RunOptions? options = null)
        {
            options ??= new RunOptions();
            var sw = Stopwatch.StartNew();
            var gen = SelectGenerate(bundle, options.GenerateName);

            var result = new RunResult
            {
                LayoutName = bundle.DisplayName,
                LayoutIdentifier = bundle.Metadata.Identifier,
                LayoutVersion = bundle.Metadata.LayoutVersion,
                SchemaVersion = bundle.Metadata.SchemaVersion,
                GenerateName = gen?.Name ?? "",
                SourceName = provider.DisplayName,
                ConfigHash = ComputeHash(bundle)
            };

            _log.Info($"Execução iniciada: layout={result.LayoutIdentifier} v{result.LayoutVersion} generate='{result.GenerateName}' origem={provider.DisplayName} hash={result.ConfigHash[..12]}");

            try
            {
                var validation = ValidateLayout(bundle, options.GenerateName);
                result.Issues.AddRange(validation.Issues);
                result.Pending.AddRange(validation.Plan.Pending);

                if (gen == null || !bundle.IsUsable)
                {
                    result.Blocked = true;
                    return Finish(result, sw, "layout inválido");
                }

                if (validation.HasErrors && !options.AllowPending)
                {
                    result.Blocked = true;
                    return Finish(result, sw, "bloqueada por erros/pendências de configuração");
                }

                // ---- dados ----
                var tables = new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase);
                foreach (var t in gen.Tables)
                {
                    tables[t.Alias] = provider.Load(t);
                    _log.Info($"Tabela '{t.Alias}' carregada: {tables[t.Alias].Rows.Count} registro(s)");
                }

                // ---- geração ----
                var gr = _generator.Generate(gen, validation.Plan, tables);
                result.Issues.AddRange(gr.Issues);
                result.RecordCount = gr.RecordCount;
                result.Json = JsonGenerator.Serialize(gr.Root);

                // ---- schema ----
                if (!string.IsNullOrWhiteSpace(bundle.SchemaText))
                {
                    using var schema = JsonSchemaValidator.FromText(bundle.SchemaText);
                    result.Issues.AddRange(schema.Validate(gr.Root));
                    result.SchemaValidated = true;
                }
                else
                {
                    result.Issues.Add(ValidationIssue.Warning("NO_SCHEMA", "", "Layout sem schema.json: validação estrutural não executada."));
                }
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                result.Issues.Add(ValidationIssue.Error("RUN_FAILED", "", $"Falha na execução: {ex.GetType().Name}: {ex.Message}"));
                _log.Error($"Falha: {ex.GetType().Name}: {ex.Message}");
            }
            return Finish(result, sw, "concluída");
        }

        private RunResult Finish(RunResult r, Stopwatch sw, string status)
        {
            r.Duration = sw.Elapsed;
            _log.Info($"Execução {status}: registros={r.RecordCount} erros={r.ErrorCount} avisos={r.WarningCount} duracao={r.Duration.TotalSeconds:F2}s schema={(r.SchemaValidated ? "ok" : "nao")}");
            return r;
        }

        /// <summary>Hash SHA-256 de layout.xml + schema.json + customizations.xml usados na execução.</summary>
        public static string ComputeHash(LayoutBundle bundle)
        {
            using var sha = SHA256.Create();
            var sb = new StringBuilder();
            sb.Append(bundle.LayoutXmlText).Append('\u0001').Append(bundle.SchemaText ?? "").Append('\u0001');
            if (File.Exists(bundle.CustomizationsPath)) sb.Append(File.ReadAllText(bundle.CustomizationsPath));
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString())));
        }
    }
}
