using BetaJsonIntegrator.Core.Configuration;
using BetaJsonIntegrator.Core.Validation;
using BetaJsonIntegrator.Data.Providers;

namespace BetaJsonIntegrator.Core.Services;

public sealed class RunOptions
{
    /// <summary>Nome do &lt;generate&gt;; vazio usa o primeiro.</summary>
    public string? GenerateName { get; init; }

    /// <summary>Prévia: gera mesmo com campos pendentes (omitidos). Uma prévia não pode ser exportada.</summary>
    public bool AllowPending { get; init; }

    /// <summary>Substitui o nome da tabela do XML (equivale ao TableSource do modelo).</summary>
    public string? TableSource { get; init; }

    /// <summary>Filtros de igualdade, enviados como parâmetros (equivale ao FiltersSource do modelo).</summary>
    public IReadOnlyList<DataFilter>? Filters { get; init; }
}

public sealed class RunResult
{
    public string LayoutFolder { get; init; } = "";
    public string LayoutName { get; init; } = "";
    public string LayoutIdentifier { get; init; } = "";
    public int LayoutVersion { get; init; }
    public string SchemaVersion { get; init; } = "";
    public string GenerateName { get; init; } = "";
    public string SourceName { get; init; } = "";

    /// <summary>Hash SHA-256 de layout + personalizações + schema usados (rastreabilidade, spec. seção 14).</summary>
    public string ConfigHash { get; init; } = "";

    public DateTime StartedAt { get; init; }
    public TimeSpan Duration { get; init; }
    public int RecordCount { get; init; }

    /// <summary>JSON formatado; null se a geração foi bloqueada ou a origem falhou.</summary>
    public string? Json { get; init; }

    public IReadOnlyList<ValidationIssue> Issues { get; init; } = Array.Empty<ValidationIssue>();
    public IReadOnlyList<FieldConfig> Pending { get; init; } = Array.Empty<FieldConfig>();

    /// <summary>A configuração (erros ou pendências) impediu a geração.</summary>
    public bool Blocked { get; init; }

    public bool SchemaValidated { get; init; }

    /// <summary>Gerado com campos pendentes omitidos.</summary>
    public bool IsPreview { get; init; }

    public int ErrorCount => Issues.Count(i => i.Severity == IssueSeverity.Error);
    public int WarningCount => Issues.Count(i => i.Severity == IssueSeverity.Warning);

    public bool IsExportReady => !Blocked && !IsPreview && Json != null && ErrorCount == 0;
}
