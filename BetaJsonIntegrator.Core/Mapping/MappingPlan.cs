using BetaJsonIntegrator.Core.Configuration;

namespace BetaJsonIntegrator.Core.Mapping;

public enum FieldKind
{
    /// <summary>Sem valor, referência nem regra: precisa de personalização (spec. seção 7).</summary>
    Pending,

    /// <summary>Valor fixo declarado no XML ou em personalização.</summary>
    Literal,

    /// <summary>Valor lido de uma coluna da origem.</summary>
    Column,

    /// <summary>Valor padrão (<c>default</c> ou modo "padrão" da personalização).</summary>
    Default,

    /// <summary>Array/objeto com campos filhos.</summary>
    Container,

    /// <summary>Array/objeto declarado sem filhos: gerado vazio, de forma explícita (spec. seção 8).</summary>
    EmptyContainer
}

public sealed class ResolvedField
{
    public FieldKind Kind { get; init; }

    /// <summary>Texto do valor fixo (Literal) ou padrão (Default).</summary>
    public string? Literal { get; init; }

    /// <summary>Nome da coluna (Column), já com a grafia declarada na seleção.</summary>
    public string? Column { get; init; }

    public bool FromCustomization { get; init; }

    /// <summary>XML legado: a coluna foi indicada em <c>value</c> em vez de <c>source</c>.</summary>
    public bool LegacyColumnInValue { get; init; }
}

/// <summary>Resultado determinístico da resolução de todos os campos de um generate.</summary>
public sealed class MappingPlan
{
    /// <summary>Chave por referência ao <see cref="FieldConfig"/>.</summary>
    public Dictionary<FieldConfig, ResolvedField> Fields { get; } = new();

    public List<FieldConfig> Pending { get; } = new();

    public ResolvedField? Get(FieldConfig field) =>
        Fields.TryGetValue(field, out var resolved) ? resolved : null;
}
