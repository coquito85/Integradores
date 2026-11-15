using BetaJsonIntegrator.Core.Configuration;
using BetaJsonIntegrator.Core.Mapping;
using BetaJsonIntegrator.Data.Providers;

namespace BetaJsonIntegrator.Core.Validation;

public sealed class LayoutValidationResult
{
    public LayoutValidationResult(IReadOnlyList<ValidationIssue> issues, MappingPlan plan)
    {
        Issues = issues;
        Plan = plan;
    }

    public IReadOnlyList<ValidationIssue> Issues { get; }
    public MappingPlan Plan { get; }
    public bool HasErrors => Issues.Any(i => i.Severity == IssueSeverity.Error);
}

/// <summary>
/// Valida a configuração antes de qualquer geração (spec. seções 11 e 16): estrutura de tabelas e colunas,
/// aliases, referências inexistentes, campos pendentes e nomes duplicados.
/// </summary>
public sealed class LayoutValidator
{
    public LayoutValidationResult Validate(GenerateConfig generate, CustomizationSet customizations)
    {
        var issues = new List<ValidationIssue>();
        ValidateStructure(generate, issues);
        var plan = new MappingResolver().Resolve(generate, customizations, issues);
        ValidateFieldNames(generate, issues);
        return new LayoutValidationResult(issues, plan);
    }

    private static void ValidateStructure(GenerateConfig gen, List<ValidationIssue> issues)
    {
        var ctx = $"generate[{gen.Name}]";

        if (gen.Tables.Count == 0)
            issues.Add(ValidationIssue.Error("CFG_NO_TABLE", ctx, "O generate não possui nenhuma <table>."));
        else if (gen.Tables.Count > 1)
            issues.Add(ValidationIssue.Warning("CFG_MULTIPLE_TABLES", ctx,
                "Há mais de uma <table>; nesta versão somente a primeira é usada como origem."));

        var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var table in gen.Tables)
        {
            var tctx = $"{ctx}/table[{table.Alias}]";

            if (!SqlIdentifier.IsValid(table.Name))
                issues.Add(ValidationIssue.Error("CFG_TABLE_NAME", tctx,
                    $"Nome de tabela inválido: '{table.Name}'. Use apenas letras, números e '_' (opcionalmente schema.tabela)."));

            if (table.Alias.Length == 0)
                issues.Add(ValidationIssue.Error("CFG_TABLE_ALIAS", tctx, "A <table> não possui 'alias'."));
            else if (!aliases.Add(table.Alias))
                issues.Add(ValidationIssue.Error("CFG_TABLE_ALIAS", tctx, $"Alias de tabela repetido: '{table.Alias}'."));

            if (table.Columns.Count == 0)
                issues.Add(ValidationIssue.Error("CFG_NO_COLUMNS", tctx, "A <selection> não possui nenhuma <Column>."));

            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var col in table.Columns)
            {
                if (col.Name.Length > 0 && !SqlIdentifier.IsValid(col.Name))
                    issues.Add(ValidationIssue.Error("CFG_COLUMN_NAME", $"{tctx}/Column[{col.Name}]",
                        $"Nome de coluna inválido: '{col.Name}' (linha {col.Line})."));

                if (col.Name.Length > 0 && !names.Add(col.Name))
                    issues.Add(ValidationIssue.Error("CFG_COLUMN_DUPLICATE", $"{tctx}/Column[{col.Name}]",
                        $"Coluna repetida: '{col.Name}' (linha {col.Line})."));
            }
        }

        foreach (var def in gen.JsonDefinitions)
        {
            foreach (var alias in def.Aliases)
            {
                if (!aliases.Contains(alias.Name))
                    issues.Add(ValidationIssue.Error("CFG_ALIAS_NOT_FOUND", $"Refealias[{alias.Name}]",
                        $"O Refealias '{alias.Name}' (linha {alias.Line}) não corresponde ao alias de nenhuma <table> do generate."));

                if (alias.Fields.Count == 0)
                    issues.Add(ValidationIssue.Warning("CFG_ALIAS_EMPTY", $"Refealias[{alias.Name}]",
                        "O Refealias não possui nenhum <field>."));
            }
        }

        if (gen.Tables.Count > 0 && gen.Tables[0].Alias.Length > 0 &&
            !gen.JsonDefinitions.SelectMany(d => d.Aliases)
                .Any(a => string.Equals(a.Name, gen.Tables[0].Alias, StringComparison.OrdinalIgnoreCase)))
        {
            issues.Add(ValidationIssue.Error("CFG_NO_STRUCTURE", ctx,
                $"Nenhum Refealias referencia o alias '{gen.Tables[0].Alias}' da tabela de origem; não há estrutura JSON a gerar."));
        }
    }

    private static void ValidateFieldNames(GenerateConfig gen, List<ValidationIssue> issues)
    {
        // nomes que diferem só por maiúsculas/minúsculas (JSON diferencia): ex.: Participacao x participacao
        var variants = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        void Walk(IEnumerable<FieldConfig> siblings, string parentPath)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var f in siblings)
            {
                if (f.Name.Length > 0)
                {
                    if (!seen.Add(f.Name))
                        issues.Add(ValidationIssue.Error("CFG_FIELD_DUPLICATE", f.Path,
                            $"Campo '{f.Name}' repetido no mesmo nível (linha {f.Line}); o último valor sobrescreveria o anterior."));

                    if (!variants.TryGetValue(f.Name, out var set))
                        variants[f.Name] = set = new HashSet<string>(StringComparer.Ordinal);
                    set.Add(f.Name);
                }
                Walk(f.Children, f.Path);
            }
        }

        foreach (var def in gen.JsonDefinitions)
            foreach (var alias in def.Aliases)
                Walk(alias.Fields, "");

        foreach (var set in variants.Values.Where(s => s.Count > 1))
            issues.Add(ValidationIssue.Warning("CFG_NAME_CASE", "",
                "Propriedades com grafias que diferem só por maiúsculas/minúsculas: " +
                string.Join(", ", set.OrderBy(n => n, StringComparer.Ordinal)) +
                ". O JSON diferencia maiúsculas; confira com o JSON Schema."));
    }
}
