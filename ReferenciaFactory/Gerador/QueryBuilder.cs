using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;
using ReferenciaFactory.Banco;
using ReferenciaFactory.Layouts;

namespace ReferenciaFactory.Gerador;

public sealed class BuiltQuery
{
    public required string Sql { get; init; }
    public required string CountSql { get; init; }
    public required List<QueryParam> Params { get; init; }
    /// <summary>Colunas na ordem do SELECT (nomes do layout).</summary>
    public required List<LayoutColumn> Columns { get; init; }
}

/// <summary>
/// Monta o SELECT a partir do layout: o equivalente "ORM dinamico" do Extractor, mas com
/// aspas/limite por dialeto e filtros SEMPRE parametrizados (o antigo concatenava "campo = valor" no SQL).
/// </summary>
public static class QueryBuilder
{
    public static BuiltQuery Build(LayoutDefinition layout, ISqlDialect d, string? requestedTable,
        IReadOnlyList<FilterCondition> filters, SchemaMap? map, int? limit)
    {
        var table = string.IsNullOrWhiteSpace(requestedTable) ? layout.TableName : requestedTable.Trim();
        var from = map is not null
            ? string.Join(".", new[] { map.Table.Schema, map.Table.Name }.Where(x => !string.IsNullOrEmpty(x)).Select(x => d.Quote(x!)))
            : d.QuoteQualified(table, loose: true);

        var alias = System.Text.RegularExpressions.Regex.IsMatch(layout.TableAlias, @"^[A-Za-z_][A-Za-z0-9_]*$") ? layout.TableAlias : "t0";
        var fromAlias = $"{from} {alias}";

        string Col(string layoutName)
        {
            if (map?.Columns is not null && map.Columns.TryGetValue(layoutName, out var real))
                return $"{alias}.{d.Quote(real)}";
            return $"{alias}.{d.QuoteLoose(layoutName)}";
        }

        var select = string.Join(", ", layout.Columns.Select(c => Col(c.Name)));
        var order = string.Join(", ", layout.Columns.Where(c => c.OrderBy).Select(c => Col(c.Name)));

        var ps = new List<QueryParam>();
        var conds = new List<string>();
        foreach (var f in filters)
        {
            if (string.IsNullOrWhiteSpace(f.Column)) continue;
            var col = layout.FindColumn(f.Column)
                      ?? throw new GeneratorException($"Filtro invalido: a coluna '{f.Column}' nao faz parte do layout.");
            var expr = Col(col.Name);
            var op = (f.Operator ?? "eq").ToLowerInvariant();

            switch (op)
            {
                case "isnull": conds.Add($"{expr} IS NULL"); continue;
                case "notnull": conds.Add($"{expr} IS NOT NULL"); continue;
            }

            if (string.IsNullOrEmpty(f.Value))
                throw new GeneratorException($"Informe o valor do filtro da coluna '{col.Name}'.");

            object value;
            string sqlOp;
            switch (op)
            {
                case "contains":
                case "startswith":
                    if (col.Type is not ("string" or "text" or "varchar"))
                        throw new GeneratorException($"'{col.Name}' nao e texto: use =, >, < etc.");
                    sqlOp = "LIKE";
                    value = op == "contains" ? $"%{f.Value}%" : $"{f.Value}%";
                    break;
                case "eq": sqlOp = "="; value = Parse(col, f.Value); break;
                case "ne": sqlOp = "<>"; value = Parse(col, f.Value); break;
                case "gt": sqlOp = ">"; value = Parse(col, f.Value); break;
                case "ge": sqlOp = ">="; value = Parse(col, f.Value); break;
                case "lt": sqlOp = "<"; value = Parse(col, f.Value); break;
                case "le": sqlOp = "<="; value = Parse(col, f.Value); break;
                default: throw new GeneratorException($"Operador de filtro desconhecido: '{f.Operator}'.");
            }

            var marker = d.ParameterMarker(ps.Count);
            ps.Add(new QueryParam(marker, value));
            conds.Add($"{expr} {sqlOp} {marker}");
        }

        var where = conds.Count > 0 ? string.Join(" AND ", conds) : null;
        var sql = d.BuildSelect(select, fromAlias, where, order.Length > 0 ? order : null, limit);
        var count = $"SELECT COUNT(*) FROM {fromAlias}" + (where is null ? "" : " WHERE " + where);

        return new BuiltQuery { Sql = sql, CountSql = count, Params = ps, Columns = layout.Columns };
    }

    private static object Parse(LayoutColumn col, string text)
    {
        try { return ValueConverter.ParseFilterValue(col.Type, text); }
        catch (FormatException ex) { throw new GeneratorException($"Filtro em '{col.Name}': {ex.Message}"); }
    }
}
