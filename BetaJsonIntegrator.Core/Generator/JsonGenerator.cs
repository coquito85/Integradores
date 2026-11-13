using System.Data;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using BetaJsonIntegrator.Core.Configuration;
using BetaJsonIntegrator.Core.Mapping;
using BetaJsonIntegrator.Core.Validation;

namespace BetaJsonIntegrator.Core.Generator
{
    public sealed class GenerationResult
    {
        public JsonObject Root { get; init; } = new();
        public List<ValidationIssue> Issues { get; } = new();
        /// <summary>Quantidade de registros lidos da tabela principal (primeiro alias).</summary>
        public int RecordCount { get; set; }
    }

    /// <summary>
    /// Monta o JSON a partir do plano de mapeamento e dos DataTables.
    /// Regras de estrutura:
    ///  - Array de topo (filho direto de Refealias): um elemento por registro da tabela
    ///    (ou por grupo, se houver groupBy).
    ///  - Array interno com campos e sem alias: um elemento por registro do conjunto corrente
    ///    (1 registro, ou N quando o array de topo usa groupBy).
    ///  - Array interno com alias/key: elementos vêm das linhas da outra tabela
    ///    (child[key] == pai[parentKey]).
    ///  - Array sem campos: vazio.
    /// </summary>
    public sealed class JsonGenerator
    {
        public static readonly JsonSerializerOptions OutputOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static string Serialize(JsonNode node, bool indented = true) =>
            node.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = indented,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

        public GenerationResult Generate(GenerateConfig gen, MappingPlan plan, IReadOnlyDictionary<string, DataTable> tables)
        {
            var result = new GenerationResult();
            var run = new Run(gen, plan, tables, result);
            run.Execute();
            return result;
        }

        // ------------------------------------------------------------------
        private sealed class Run
        {
            private readonly GenerateConfig _gen;
            private readonly MappingPlan _plan;
            private readonly IReadOnlyDictionary<string, DataTable> _tables;
            private readonly GenerationResult _result;
            private readonly HashSet<string> _reported = new(StringComparer.OrdinalIgnoreCase);

            public Run(GenerateConfig gen, MappingPlan plan, IReadOnlyDictionary<string, DataTable> tables, GenerationResult result)
            {
                _gen = gen; _plan = plan; _tables = tables; _result = result;
            }

            private JsonObject Root => _result.Root;

            public void Execute()
            {
                bool first = true;
                foreach (var json in _gen.JsonDefinitions)
                {
                    foreach (var alias in json.Aliases)
                    {
                        var table = _gen.FindTable(alias.Name);
                        if (table == null || !_tables.TryGetValue(table.Alias, out var dt)) continue;

                        CheckColumns(table, dt);
                        var rows = SortedRows(table, dt);
                        if (first) { _result.RecordCount = rows.Count; first = false; }

                        foreach (var field in alias.Fields)
                            BuildTopLevel(field, table, rows);
                    }
                }
                AppendSuppressedCounts();
            }

            private void AppendSuppressedCounts()
            {
                foreach (var kv in _suppressed)
                {
                    if (!_firstIndex.TryGetValue(kv.Key, out var idx)) continue;
                    var i = _result.Issues[idx];
                    _result.Issues[idx] = new ValidationIssue
                    {
                        Severity = i.Severity, Code = i.Code, Path = i.Path,
                        Message = i.Message + $" (+{kv.Value} ocorrência(s) semelhante(s) em outros registros)"
                    };
                }
            }

            private void BuildTopLevel(FieldConfig field, TableConfig table, List<DataRow> rows)
            {
                if (!_plan.Fields.TryGetValue(field, out var rf) || rf.Kind == FieldKind.Pending) return;

                if (field.IsArray && rf.Kind == FieldKind.Container)
                {
                    var arr = new JsonArray();
                    var groups = Group(field, rows);
                    for (int i = 0; i < groups.Count; i++)
                        arr.Add(BuildElement(field, groups[i], $"{field.Name}[{i}]"));
                    Root[field.Name] = arr;
                    return;
                }

                // propriedades de topo que não são array: usam o primeiro registro
                var set = rows.Count > 0 ? new List<DataRow> { rows[0] } : new List<DataRow>();
                if (rows.Count > 1 && rf.Kind == FieldKind.Column)
                    Report(IssueSeverity.Warning, "ROOT_FIELD_MULTIPLE_ROWS", field.Name,
                        "Propriedade de topo vinda de coluna, mas há vários registros: usado apenas o primeiro.");
                AddField(Root, field, set, field.Name);
            }

            private JsonObject BuildElement(FieldConfig container, List<DataRow> rowSet, string path)
            {
                var obj = new JsonObject();
                foreach (var child in container.Children)
                    AddField(obj, child, rowSet, path + "." + child.Name);
                return obj;
            }

            private void AddField(JsonObject parent, FieldConfig field, List<DataRow> rowSet, string path)
            {
                if (!_plan.Fields.TryGetValue(field, out var rf)) return;

                switch (rf.Kind)
                {
                    case FieldKind.Pending:
                        return; // já reportado pelo plano; nunca inventar valor

                    case FieldKind.EmptyContainer:
                        parent[field.Name] = field.IsArray ? new JsonArray() : new JsonObject();
                        return;

                    case FieldKind.Container:
                        if (field.IsObject)
                        {
                            parent[field.Name] = BuildElement(field, rowSet, path);
                            return;
                        }
                        parent[field.Name] = BuildArray(field, rf, rowSet, path);
                        return;

                    default:
                        AddLeaf(parent, field, rf, rowSet, path);
                        return;
                }
            }

            private JsonArray BuildArray(FieldConfig field, ResolvedField rf, List<DataRow> rowSet, string path)
            {
                var arr = new JsonArray();
                List<DataRow> source;

                if (!string.IsNullOrWhiteSpace(field.ChildAlias))
                {
                    source = new List<DataRow>();
                    var childTable = _gen.FindTable(field.ChildAlias);
                    if (childTable != null && _tables.TryGetValue(childTable.Alias, out var childDt) && rowSet.Count > 0)
                    {
                        CheckColumns(childTable, childDt);
                        var parentKey = field.ParentKey ?? field.Key!;
                        var parentVal = KeyOf(rowSet[0], parentKey);
                        if (parentVal != null)
                            source = SortedRows(childTable, childDt)
                                .Where(r => string.Equals(KeyOf(r, field.Key!), parentVal, StringComparison.Ordinal))
                                .ToList();
                    }
                }
                else source = rowSet;

                for (int i = 0; i < source.Count; i++)
                    arr.Add(BuildElement(field, new List<DataRow> { source[i] }, $"{path}[{i}]"));
                return arr;
            }

            private void AddLeaf(JsonObject parent, FieldConfig field, ResolvedField rf, List<DataRow> rowSet, string path)
            {
                var type = field.Type!;
                object? raw;
                string? srcFormat = null;
                DataRow? row = rowSet.Count > 0 ? rowSet[0] : null;

                if (rf.Kind == FieldKind.Column)
                {
                    raw = row == null ? null : GetValue(row, rf.Column!);
                    var table = _gen.FindTable(rf.Alias);
                    srcFormat = table?.FindColumn(rf.Column)?.Format;
                }
                else raw = rf.Literal;

                var outFormat = field.Format ?? srcFormat;
                var conv = JsonValueConverter.Convert(raw, type, outFormat, srcFormat);

                if (conv.Error != null)
                {
                    var origin = rf.Kind == FieldKind.Column
                        ? $"coluna '{rf.Column}'" + (row != null ? $" (registro {RowNumber(row)})" : "")
                        : "valor configurado";
                    Report(IssueSeverity.Error, "CONVERSION_ERROR", path, $"{conv.Error} Origem: {origin}.", unique: true);
                    return;
                }
                if (conv.IsNull)
                {
                    if (field.Required)
                        Report(IssueSeverity.Error, "REQUIRED_VALUE_MISSING", path,
                            "Campo obrigatório sem valor" + (row != null ? $" (registro {RowNumber(row)})" : "") + ".", unique: true);
                    return;
                }
                parent[field.Name] = conv.Node;
            }

            // ---------------- dados ----------------
            private readonly Dictionary<DataRow, int> _rowNumbers = new();

            private int RowNumber(DataRow row) => _rowNumbers.TryGetValue(row, out var n) ? n : 0;

            private List<DataRow> SortedRows(TableConfig table, DataTable dt)
            {
                var rows = dt.Rows.Cast<DataRow>().ToList();
                for (int i = 0; i < rows.Count; i++) _rowNumbers[rows[i]] = i + 1;

                var order = table.Columns.Where(c => c.OrderBy && dt.Columns.Contains(c.Name)).ToList();
                if (order.Count == 0) return rows;

                IOrderedEnumerable<DataRow>? ordered = null;
                foreach (var col in order)
                {
                    Func<DataRow, object> key = r => OrderKey(r[col.Name]);
                    ordered = ordered == null ? rows.OrderBy(key, new ValueComparer()) : ordered.ThenBy(key, new ValueComparer());
                }
                return ordered!.ToList();
            }

            private static object OrderKey(object v) => v is DBNull ? "" : v;

            private sealed class ValueComparer : IComparer<object>
            {
                public int Compare(object? x, object? y)
                {
                    if (x == null || y == null) return 0;
                    if (x.GetType() == y.GetType() && x is IComparable c) return c.CompareTo(y);
                    return string.Compare(x.ToString(), y.ToString(), StringComparison.OrdinalIgnoreCase);
                }
            }

            private List<List<DataRow>> Group(FieldConfig field, List<DataRow> rows)
            {
                if (field.GroupBy.Length == 0) return rows.Select(r => new List<DataRow> { r }).ToList();

                var map = new Dictionary<string, List<DataRow>>(StringComparer.Ordinal);
                var order = new List<List<DataRow>>();
                foreach (var r in rows)
                {
                    var key = string.Join("\u001f", field.GroupBy.Select(g => KeyOf(r, g) ?? ""));
                    if (!map.TryGetValue(key, out var list)) { list = new List<DataRow>(); map[key] = list; order.Add(list); }
                    list.Add(r);
                }
                return order;
            }

            private object? GetValue(DataRow row, string column)
            {
                if (!row.Table.Columns.Contains(column))
                {
                    Report(IssueSeverity.Error, "MISSING_SOURCE_COLUMN", column,
                        $"A coluna '{column}' não existe na origem de dados.", unique: true);
                    return null;
                }
                return row[column];
            }

            private string? KeyOf(DataRow row, string column)
            {
                if (!row.Table.Columns.Contains(column)) return null;
                var v = row[column];
                return v is DBNull ? null : System.Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture)?.Trim();
            }

            private void CheckColumns(TableConfig table, DataTable dt)
            {
                foreach (var c in table.Columns)
                    if (!dt.Columns.Contains(c.Name))
                        Report(IssueSeverity.Error, "MISSING_SOURCE_COLUMN", $"{table.Alias}.{c.Name}",
                            $"A coluna '{c.Name}' declarada em <selection> não foi encontrada na origem de dados.", unique: true);
            }

            private void Report(IssueSeverity sev, string code, string path, string message, bool unique = false)
            {
                // Evita milhares de mensagens iguais: uma por (código + caminho sem índice).
                var key = code + "|" + System.Text.RegularExpressions.Regex.Replace(path, @"\[\d+\]", "[]");
                if (unique && !_reported.Add(key))
                {
                    _suppressed[key] = _suppressed.GetValueOrDefault(key) + 1;
                    return;
                }
                _firstIndex[key] = _result.Issues.Count;
                _result.Issues.Add(new ValidationIssue { Severity = sev, Code = code, Path = path, Message = message });
            }

            private readonly Dictionary<string, int> _suppressed = new();
            private readonly Dictionary<string, int> _firstIndex = new();
        }
    }
}
