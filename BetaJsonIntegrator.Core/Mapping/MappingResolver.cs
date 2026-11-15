using BetaJsonIntegrator.Core.Configuration;
using BetaJsonIntegrator.Core.Validation;

namespace BetaJsonIntegrator.Core.Mapping;

/// <summary>
/// Resolve cada campo do JSON de forma determinística (spec. seção 6):
/// 1) value literal; 2) coluna (source / Columnrefe); 3) default; 4) sem configuração → pendente.
/// Personalizações preenchem campos pendentes e, com <c>override</c>, substituem o definido no XML.
/// O motor nunca inventa valores: o que não tem origem fica Pending.
/// </summary>
public sealed class MappingResolver
{
    public const string PendingCode = "FIELD_PENDING";

    public MappingPlan Resolve(GenerateConfig generate, CustomizationSet customizations, List<ValidationIssue> issues)
    {
        var plan = new MappingPlan();
        var columns = new Dictionary<string, ColumnConfig>(StringComparer.OrdinalIgnoreCase);
        foreach (var table in generate.Tables)
            foreach (var column in table.Columns)
                if (column.Name.Length > 0) columns.TryAdd(column.Name, column);

        var usedCustomizations = new HashSet<Customization>();

        foreach (var def in generate.JsonDefinitions)
            foreach (var alias in def.Aliases)
                foreach (var field in alias.Fields)
                    ResolveField(field, columns, customizations, usedCustomizations, plan, issues);

        foreach (var c in customizations.Items.Where(item => !usedCustomizations.Contains(item)))
            issues.Add(ValidationIssue.Warning("CUSTOM_UNUSED", c.Key,
                "A personalização não corresponde a nenhum campo do layout."));

        return plan;
    }

    private static void ResolveField(
        FieldConfig field,
        Dictionary<string, ColumnConfig> columns,
        CustomizationSet customizations,
        HashSet<Customization> used,
        MappingPlan plan,
        List<ValidationIssue> issues)
    {
        var custom = customizations.Find(field);
        if (custom != null) used.Add(custom);

        if (field.HasChildren)
        {
            plan.Fields[field] = new ResolvedField { Kind = FieldKind.Container };
            foreach (var child in field.Children)
                ResolveField(child, columns, customizations, used, plan, issues);
            return;
        }

        if (field.IsContainer)
        {
            // array/object sem filhos: vazio explícito, compatível com a spec (seção 8).
            plan.Fields[field] = new ResolvedField { Kind = FieldKind.EmptyContainer };
            return;
        }

        var configured = FromXml(field, columns, issues);

        ResolvedField? chosen;
        if (custom != null && (custom.Override || configured == null))
        {
            chosen = FromCustomization(custom);
        }
        else
        {
            if (custom != null)
                issues.Add(ValidationIssue.Info("CUSTOM_IGNORED", field.Path,
                    "Personalização ignorada: o campo já está configurado no layout (marque 'sobrescrever' para substituí-lo)."));
            chosen = configured;
        }

        if (chosen == null)
        {
            plan.Fields[field] = new ResolvedField { Kind = FieldKind.Pending };
            plan.Pending.Add(field);

            // tipo inválido já foi reportado pelo parser; evita ruído duplicado
            if (field.Type != null)
            {
                issues.Add(field.Required
                    ? ValidationIssue.Error(PendingCode, field.Path,
                        $"Campo obrigatório '{field.Name}' sem valor, referência ou regra padrão (linha {field.Line}).")
                    : ValidationIssue.Warning(PendingCode, field.Path,
                        $"Campo '{field.Name}' sem valor, referência ou regra padrão (linha {field.Line}). Informe uma personalização."));
            }
            return;
        }

        if (chosen.Kind == FieldKind.Column)
        {
            if (columns.TryGetValue(chosen.Column!, out var declared))
            {
                chosen = new ResolvedField
                {
                    Kind = FieldKind.Column,
                    Column = declared.Name, // grafia declarada na seleção
                    FromCustomization = chosen.FromCustomization,
                    LegacyColumnInValue = chosen.LegacyColumnInValue
                };
            }
            else
            {
                issues.Add(ValidationIssue.Error("COLUMN_NOT_FOUND", field.Path,
                    $"O campo '{field.Name}' (linha {field.Line}) referencia a coluna '{chosen.Column}', " +
                    "que não existe na seleção (<selection>) do layout."));
            }
        }

        plan.Fields[field] = chosen;
    }

    private static ResolvedField? FromXml(FieldConfig f, Dictionary<string, ColumnConfig> columns, List<ValidationIssue> issues)
    {
        // 1) value literal
        if (!string.IsNullOrEmpty(f.Value))
        {
            // XML legado: <field name="ni" type="string" value="cpfcnpjAdquirente"/> usa value como nome de coluna.
            if (string.IsNullOrEmpty(f.Source) && f.Type == FieldTypes.String &&
                columns.TryGetValue(f.Value, out var legacy))
            {
                issues.Add(ValidationIssue.Info("LEGACY_VALUE_AS_COLUMN", f.Path,
                    $"O atributo 'value' coincide com a coluna '{legacy.Name}' e foi tratado como referência de coluna " +
                    $"(XML legado). Prefira source=\"{legacy.Name}\"."));
                return new ResolvedField { Kind = FieldKind.Column, Column = legacy.Name, LegacyColumnInValue = true };
            }

            if (!string.IsNullOrEmpty(f.Source))
                issues.Add(ValidationIssue.Warning("FIELD_VALUE_AND_SOURCE", f.Path,
                    "O campo possui 'value' e referência de coluna; o valor fixo tem precedência."));

            return new ResolvedField { Kind = FieldKind.Literal, Literal = f.Value };
        }

        // 2) coluna
        if (!string.IsNullOrEmpty(f.Source))
            return new ResolvedField { Kind = FieldKind.Column, Column = f.Source };

        // 3) valor padrão
        if (f.Default != null)
            return new ResolvedField { Kind = FieldKind.Default, Literal = f.Default };

        return null;
    }

    private static ResolvedField? FromCustomization(Customization c)
    {
        switch (c.Mode)
        {
            case CustomizationModes.Column:
                var column = c.Source ?? c.Value;
                return string.IsNullOrWhiteSpace(column)
                    ? null
                    : new ResolvedField { Kind = FieldKind.Column, Column = column.Trim(), FromCustomization = true };

            case CustomizationModes.Default:
                return c.Value == null
                    ? null
                    : new ResolvedField { Kind = FieldKind.Default, Literal = c.Value, FromCustomization = true };

            default:
                return string.IsNullOrEmpty(c.Value)
                    ? null
                    : new ResolvedField { Kind = FieldKind.Literal, Literal = c.Value, FromCustomization = true };
        }
    }
}
