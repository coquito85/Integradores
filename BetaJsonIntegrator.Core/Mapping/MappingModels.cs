using BetaJsonIntegrator.Core.Configuration;
using BetaJsonIntegrator.Core.Validation;

namespace BetaJsonIntegrator.Core.Mapping
{
    public enum FieldKind
    {
        /// <summary>Valor fixo declarado no XML (ou personalização "fixed").</summary>
        Literal,
        /// <summary>Valor lido de uma coluna da origem.</summary>
        Column,
        /// <summary>Valor padrão (atributo default).</summary>
        Default,
        /// <summary>Array/objeto com filhos.</summary>
        Container,
        /// <summary>Array/objeto sem filhos: gerado vazio.</summary>
        EmptyContainer,
        /// <summary>Sem valor, sem referência e sem regra: não gerado.</summary>
        Pending
    }

    /// <summary>Resultado da resolução (determinística) de um campo do XML.</summary>
    public sealed class ResolvedField
    {
        public FieldConfig Field { get; init; } = null!;
        public FieldKind Kind { get; set; }
        public string? Literal { get; set; }
        public string? Column { get; set; }
        /// <summary>Alias da tabela de origem efetiva do campo.</summary>
        public string Alias { get; set; } = "";
        public string Path => Field.Path;
    }

    /// <summary>Campo que o usuário precisa configurar antes da geração.</summary>
    public sealed class PendingField
    {
        public string Path { get; init; } = "";
        public string Name { get; init; } = "";
        public string Description { get; init; } = "";
        public string Type { get; init; } = "";
        public bool Required { get; init; }
        public string Reason { get; init; } = "";
    }

    public sealed class MappingPlan
    {
        public Dictionary<FieldConfig, ResolvedField> Fields { get; } = new();
        public List<ValidationIssue> Issues { get; } = new();
        public List<PendingField> Pending { get; } = new();

        public bool HasErrors => Issues.Any(i => i.Severity == IssueSeverity.Error);
    }
}
