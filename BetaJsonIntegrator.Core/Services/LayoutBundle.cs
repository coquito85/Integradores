using BetaJsonIntegrator.Core.Configuration;
using BetaJsonIntegrator.Core.Mapping;
using BetaJsonIntegrator.Core.Validation;

namespace BetaJsonIntegrator.Core.Services;

/// <summary>metadata.json (spec. seção 13).</summary>
public sealed class LayoutMetadata
{
    public string FriendlyName { get; set; } = "";
    public string Identifier { get; set; } = "";
    public int LayoutVersion { get; set; } = 1;
    public string SchemaVersion { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string Notes { get; set; } = "";
}

/// <summary>Item da lista de layouts.</summary>
public sealed class LayoutSummary
{
    public LayoutSummary(string folderName, LayoutMetadata metadata, bool hasSchema)
    {
        FolderName = folderName;
        Metadata = metadata;
        HasSchema = hasSchema;
    }

    public string FolderName { get; }
    public LayoutMetadata Metadata { get; }
    public bool HasSchema { get; }
}

/// <summary>Tudo o que pertence a um layout carregado do disco.</summary>
public sealed class LayoutBundle
{
    public required string FolderName { get; init; }
    public required string FolderPath { get; init; }
    public required string LayoutXmlText { get; init; }
    public required LayoutMetadata Metadata { get; init; }
    public required CustomizationSet Customizations { get; init; }

    /// <summary>Null quando o XML é malformado (ver <see cref="ParseIssues"/>).</summary>
    public LayoutConfiguration? Layout { get; init; }

    public IReadOnlyList<ValidationIssue> ParseIssues { get; init; } = Array.Empty<ValidationIssue>();
    public string? SchemaText { get; init; }

    public bool HasSchema => !string.IsNullOrWhiteSpace(SchemaText);

    public string DisplayName =>
        !string.IsNullOrWhiteSpace(Metadata.FriendlyName) ? Metadata.FriendlyName :
        !string.IsNullOrWhiteSpace(Layout?.Name) ? Layout!.Name : FolderName;
}
