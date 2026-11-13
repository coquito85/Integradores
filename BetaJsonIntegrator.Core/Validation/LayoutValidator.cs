using BetaJsonIntegrator.Core.Configuration;
using BetaJsonIntegrator.Core.Mapping;

namespace BetaJsonIntegrator.Core.Validation
{
    public sealed class LayoutValidationResult
    {
        public MappingPlan Plan { get; init; } = new();
        public List<ValidationIssue> Issues { get; init; } = new();
        public bool HasErrors => Issues.Any(i => i.Severity == IssueSeverity.Error);
    }

    /// <summary>Validação da configuração (antes de tocar nos dados).</summary>
    public sealed class LayoutValidator
    {
        private readonly MappingResolver _resolver = new();

        public LayoutValidationResult Validate(GenerateConfig gen, CustomizationSet? customizations)
        {
            var issues = new List<ValidationIssue>();

            if (gen.Tables.Count == 0)
                issues.Add(ValidationIssue.Error("NO_TABLE", gen.Name, "Nenhuma <table> declarada em <generate>."));

            foreach (var dup in gen.Tables.GroupBy(t => t.Alias, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
                issues.Add(ValidationIssue.Error("DUPLICATE_ALIAS", dup.Key, $"Alias de tabela duplicado: '{dup.Key}'."));

            foreach (var t in gen.Tables)
            {
                if (string.IsNullOrWhiteSpace(t.Name))
                    issues.Add(ValidationIssue.Error("TABLE_WITHOUT_NAME", t.Alias, "Tabela sem atributo name."));
                if (t.Columns.Count == 0)
                    issues.Add(ValidationIssue.Warning("NO_COLUMNS", t.Alias,
                        "Tabela sem <selection>/<Column>: referências de coluna não poderão ser verificadas."));
                foreach (var dup in t.Columns.GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
                    issues.Add(ValidationIssue.Error("DUPLICATE_COLUMN", $"{t.Alias}.{dup.Key}", "Coluna declarada mais de uma vez."));
            }

            if (gen.JsonDefinitions.Count == 0 || gen.JsonDefinitions.All(j => j.Aliases.Count == 0))
                issues.Add(ValidationIssue.Error("NO_JSONFIELDS", gen.Name, "Nenhuma estrutura <Jsonfields>/<Refealias> definida."));

            // nomes duplicados entre irmãos
            foreach (var j in gen.JsonDefinitions)
                foreach (var a in j.Aliases)
                    CheckSiblings(a.Fields, issues);

            var plan = _resolver.Analyze(gen, customizations);
            issues.AddRange(plan.Issues);
            return new LayoutValidationResult { Plan = plan, Issues = issues };
        }

        private static void CheckSiblings(List<FieldConfig> fields, List<ValidationIssue> issues)
        {
            foreach (var dup in fields.GroupBy(f => f.Name, StringComparer.Ordinal).Where(g => g.Count() > 1))
                issues.Add(ValidationIssue.Error("DUPLICATE_FIELD", dup.First().Path,
                    $"Propriedade '{dup.Key}' definida mais de uma vez no mesmo nível."));
            foreach (var f in fields) CheckSiblings(f.Children, issues);
        }
    }
}
