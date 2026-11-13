using System.Text.RegularExpressions;
using BetaJsonIntegrator.Core.Configuration;
using BetaJsonIntegrator.Core.Validation;

namespace BetaJsonIntegrator.Core.Mapping
{
    /// <summary>
    /// Resolve cada campo do XML de forma determinística.
    /// Precedência: 1) value literal  2) Columnrefe/source  3) default  4) pendente.
    /// Personalizações preenchem campos pendentes (ou substituem, se Override = true).
    /// Compatibilidade legado: value sem source cujo conteúdo é o nome de uma coluna da origem
    /// é tratado como referência de coluna.
    /// </summary>
    public sealed class MappingResolver
    {
        private static readonly Regex IdentifierRegex = new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        public MappingPlan Analyze(GenerateConfig gen, CustomizationSet? customizations)
        {
            var plan = new MappingPlan();

            foreach (var json in gen.JsonDefinitions)
            {
                foreach (var alias in json.Aliases)
                {
                    var table = gen.FindTable(alias.Name);
                    if (table == null)
                    {
                        plan.Issues.Add(ValidationIssue.Error("ALIAS_NOT_FOUND", alias.Name,
                            $"Refealias '{alias.Name}' não corresponde a nenhuma tabela/alias declarado em <generate>."));
                        continue;
                    }
                    foreach (var field in alias.Fields)
                        Resolve(field, table, gen, customizations, plan);
                }
            }
            return plan;
        }

        private void Resolve(FieldConfig field, TableConfig table, GenerateConfig gen,
            CustomizationSet? customizations, MappingPlan plan)
        {
            var rf = new ResolvedField { Field = field, Alias = table.Alias };
            plan.Fields[field] = rf;
            var path = field.Path;

            if (field.Type == null)
            {
                plan.Issues.Add(ValidationIssue.Error("UNKNOWN_TYPE", path,
                    $"Tipo '{field.RawType}' desconhecido (linha {field.Line})."));
                rf.Kind = FieldKind.Pending;
                return;
            }

            // ---- contêineres ----
            if (field.IsContainer)
            {
                ResolveContainer(field, rf, table, gen, customizations, plan);
                return;
            }

            // ---- folhas ----
            var cust = customizations?.Find(field);
            bool configuredInXml = field.Value != null || field.Source != null || field.Default != null;

            if (cust != null && (cust.Override || !configuredInXml))
            {
                ApplyCustomization(field, rf, table, cust, plan);
                return;
            }

            if (field.Value != null)
            {
                if (field.Source != null)
                    plan.Issues.Add(ValidationIssue.Warning("VALUE_AND_SOURCE", path,
                        "Campo possui value e source; o valor literal tem precedência."));

                if (field.Source == null && IsLegacyReferenceCandidate(field))
                {
                    var col = table.FindColumn(field.Value);
                    if (col != null)
                    {
                        rf.Kind = FieldKind.Column;
                        rf.Column = col.Name;
                        plan.Issues.Add(ValidationIssue.Info("LEGACY_VALUE_AS_COLUMN", path,
                            $"value=\"{field.Value}\" interpretado como referência à coluna '{col.Name}' (XML legado). Prefira source=."));
                        return;
                    }
                    if (field.Type == FieldTypes.String)
                    {
                        plan.Issues.Add(ValidationIssue.Warning("LEGACY_VALUE_AMBIGUOUS", path,
                            $"value=\"{field.Value}\" não corresponde a nenhuma coluna de '{table.Alias}'; será usado como valor literal. Se for coluna, use source=."));
                    }
                    else
                    {
                        MarkPending(plan, field, rf, "INVALID_COLUMN_REFERENCE",
                            $"value=\"{field.Value}\" não é um valor {field.Type} válido nem uma coluna de '{table.Alias}'.");
                        return;
                    }
                }

                rf.Kind = FieldKind.Literal;
                rf.Literal = field.Value;
                return;
            }

            if (field.Source != null)
            {
                var col = table.Columns.Count == 0 ? null : table.FindColumn(field.Source);
                if (table.Columns.Count > 0 && col == null)
                {
                    MarkPending(plan, field, rf, "INVALID_COLUMN_REFERENCE",
                        $"A coluna '{field.Source}' não existe em <selection> da tabela '{table.Alias}' (linha {field.Line}).");
                    return;
                }
                rf.Kind = FieldKind.Column;
                rf.Column = col?.Name ?? field.Source;
                return;
            }

            if (field.Default != null)
            {
                rf.Kind = FieldKind.Default;
                rf.Literal = field.Default;
                return;
            }

            // 4. sem configuração
            MarkPending(plan, field, rf, field.Required ? "NOT_CONFIGURED_REQUIRED" : "NOT_CONFIGURED",
                "Campo sem valor, sem referência de coluna e sem regra padrão." +
                (field.Required ? " Obrigatório." : ""));
        }

        private void ResolveContainer(FieldConfig field, ResolvedField rf, TableConfig table, GenerateConfig gen,
            CustomizationSet? customizations, MappingPlan plan)
        {
            var path = field.Path;
            var effective = table;

            if (field.IsArray && !string.IsNullOrWhiteSpace(field.ChildAlias))
            {
                var child = gen.FindTable(field.ChildAlias);
                if (child == null)
                {
                    plan.Issues.Add(ValidationIssue.Error("ALIAS_NOT_FOUND", path,
                        $"O array usa alias '{field.ChildAlias}', que não existe em <generate>."));
                    rf.Kind = FieldKind.Pending;
                    return;
                }
                if (string.IsNullOrWhiteSpace(field.Key))
                    plan.Issues.Add(ValidationIssue.Error("MISSING_KEY", path,
                        "Array com alias de outra tabela exige o atributo key (coluna da tabela filha)."));
                else
                {
                    if (child.Columns.Count > 0 && child.FindColumn(field.Key) == null)
                        plan.Issues.Add(ValidationIssue.Error("INVALID_COLUMN_REFERENCE", path,
                            $"key='{field.Key}' não existe em '{child.Alias}'."));
                    var pk = field.ParentKey ?? field.Key;
                    if (table.Columns.Count > 0 && table.FindColumn(pk) == null)
                        plan.Issues.Add(ValidationIssue.Error("INVALID_COLUMN_REFERENCE", path,
                            $"parentKey='{pk}' não existe em '{table.Alias}'."));
                }
                effective = child;
            }

            if (field.GroupBy.Length > 0 && field.Parent != null)
                plan.Issues.Add(ValidationIssue.Warning("GROUPBY_IGNORED", path,
                    "groupBy só é aplicado a arrays de topo; será ignorado."));
            foreach (var g in field.GroupBy)
                if (table.Columns.Count > 0 && table.FindColumn(g) == null)
                    plan.Issues.Add(ValidationIssue.Error("INVALID_COLUMN_REFERENCE", path,
                        $"groupBy usa a coluna '{g}', inexistente em '{table.Alias}'."));

            rf.Alias = effective.Alias;

            if (field.Children.Count == 0)
            {
                rf.Kind = FieldKind.EmptyContainer;
                plan.Issues.Add(field.EmptyDeclared
                    ? ValidationIssue.Info("EMPTY_CONTAINER_DECLARED", path, "Coleção vazia declarada explicitamente.")
                    : ValidationIssue.Info("EMPTY_CONTAINER_NO_SOURCE", path,
                        "Coleção sem origem e sem campos: será gerada vazia. Declare empty=\"true\" para registrar essa decisão."));
                return;
            }

            rf.Kind = FieldKind.Container;
            foreach (var child in field.Children)
                Resolve(child, effective, gen, customizations, plan);
        }

        private static void ApplyCustomization(FieldConfig field, ResolvedField rf, TableConfig table,
            Customization cust, MappingPlan plan)
        {
            var path = field.Path;
            switch (cust.Mode)
            {
                case CustomizationModes.Column:
                    var col = table.Columns.Count == 0 ? null : table.FindColumn(cust.Source);
                    if (string.IsNullOrWhiteSpace(cust.Source) || (table.Columns.Count > 0 && col == null))
                    {
                        MarkPending(plan, field, rf, "INVALID_CUSTOMIZATION",
                            $"Personalização aponta para a coluna '{cust.Source}', inexistente em '{table.Alias}'.");
                        return;
                    }
                    rf.Kind = FieldKind.Column;
                    rf.Column = col?.Name ?? cust.Source;
                    break;

                case CustomizationModes.Default:
                case CustomizationModes.Fixed:
                    if (cust.Value == null)
                    {
                        MarkPending(plan, field, rf, "INVALID_CUSTOMIZATION", "Personalização sem value.");
                        return;
                    }
                    rf.Kind = cust.Mode == CustomizationModes.Default ? FieldKind.Default : FieldKind.Literal;
                    rf.Literal = cust.Value;
                    break;

                default:
                    MarkPending(plan, field, rf, "INVALID_CUSTOMIZATION",
                        $"Modo de personalização desconhecido: '{cust.Mode}'.");
                    return;
            }
            plan.Issues.Add(ValidationIssue.Info("CUSTOMIZATION_APPLIED", path,
                $"Personalização aplicada (modo {cust.Mode})."));
        }

        private static void MarkPending(MappingPlan plan, FieldConfig field, ResolvedField rf, string code, string reason)
        {
            rf.Kind = FieldKind.Pending;
            var sev = field.Required || code == "INVALID_COLUMN_REFERENCE" || code == "INVALID_CUSTOMIZATION"
                ? IssueSeverity.Error : IssueSeverity.Warning;
            plan.Issues.Add(new ValidationIssue { Severity = sev, Code = code, Path = field.Path, Message = reason });
            plan.Pending.Add(new PendingField
            {
                Path = field.Path,
                Name = field.Name,
                Description = field.Description ?? "",
                Type = field.Type ?? field.RawType,
                Required = field.Required,
                Reason = reason
            });
        }

        private static bool IsLegacyReferenceCandidate(FieldConfig field) =>
            field.Value != null &&
            IdentifierRegex.IsMatch(field.Value) &&
            !field.Value.Equals("true", StringComparison.OrdinalIgnoreCase) &&
            !field.Value.Equals("false", StringComparison.OrdinalIgnoreCase);
    }
}
