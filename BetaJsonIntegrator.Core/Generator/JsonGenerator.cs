using System.Data;
using System.Text.Json.Nodes;
using BetaJsonIntegrator.Core.Configuration;
using BetaJsonIntegrator.Core.Mapping;
using BetaJsonIntegrator.Core.Validation;

namespace BetaJsonIntegrator.Core.Generator;

public sealed class GeneratorOutput
{
    public GeneratorOutput(JsonObject root, int recordCount, IReadOnlyList<ValidationIssue> issues)
    {
        Root = root;
        RecordCount = recordCount;
        Issues = issues;
    }

    public JsonObject Root { get; }
    public int RecordCount { get; }
    public IReadOnlyList<ValidationIssue> Issues { get; }
}

/// <summary>
/// Monta objetos, arrays e arrays aninhados a partir do plano de mapeamento (spec. seções 8 e 9).
/// Cada registro da origem gera um elemento do array principal (ex.: <c>declaracoes</c>); arrays aninhados
/// com campos filhos geram um elemento por registro, montado com os dados do mesmo registro.
/// Agrupamento de vários registros em uma declaração (chave de associação) é evolução prevista na spec.
/// Campos pendentes (modo prévia) são omitidos, nunca preenchidos com valores inventados.
/// </summary>
public sealed class JsonGenerator
{
    private const int MaxIssues = 500;

    private sealed class Context
    {
        public Context(MappingPlan plan, DataTable data, Dictionary<string, ColumnConfig> columns)
        {
            Plan = plan;
            Data = data;
            Columns = columns;
        }

        public MappingPlan Plan { get; }
        public DataTable Data { get; }
        public Dictionary<string, ColumnConfig> Columns { get; }
        public List<ValidationIssue> Issues { get; } = new();
        public int Suppressed { get; set; }

        public void Add(ValidationIssue issue)
        {
            if (Issues.Count < MaxIssues) Issues.Add(issue);
            else Suppressed++;
        }
    }

    public GeneratorOutput Generate(GenerateConfig generate, TableConfig table, MappingPlan plan, DataTable data)
    {
        ArgumentNullException.ThrowIfNull(generate);
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(data);

        var columns = new Dictionary<string, ColumnConfig>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in table.Columns)
            if (c.Name.Length > 0) columns.TryAdd(c.Name, c);

        var ctx = new Context(plan, data, columns);
        var root = new JsonObject();

        var aliases = generate.JsonDefinitions
            .SelectMany(d => d.Aliases)
            .Where(a => string.Equals(a.Name, table.Alias, StringComparison.OrdinalIgnoreCase));

        foreach (var alias in aliases)
        {
            foreach (var field in alias.Fields)
            {
                var resolved = plan.Get(field);

                if (resolved?.Kind == FieldKind.Container && field.Type == FieldTypes.Array)
                {
                    // array principal: um elemento por registro da origem
                    var array = new JsonArray();
                    for (int i = 0; i < data.Rows.Count; i++)
                        array.Add(BuildObject(field.Children, data.Rows[i], $"{field.Name}[{i}]", ctx));
                    root[field.Name] = array;
                }
                else if (data.Rows.Count > 0)
                {
                    var node = BuildNode(field, data.Rows[0], field.Name, ctx, out var include);
                    if (include) root[field.Name] = node;
                }
            }
        }

        if (ctx.Suppressed > 0)
            ctx.Issues.Add(ValidationIssue.Info("DATA_ISSUES_TRUNCATED", "",
                $"Mais {ctx.Suppressed} ocorrência(s) de conversão não foram listadas."));

        return new GeneratorOutput(root, data.Rows.Count, ctx.Issues);
    }

    private static JsonObject BuildObject(IEnumerable<FieldConfig> fields, DataRow row, string path, Context ctx)
    {
        var obj = new JsonObject();
        foreach (var child in fields)
        {
            var node = BuildNode(child, row, path + "." + child.Name, ctx, out var include);
            if (include) obj[child.Name] = node;
        }
        return obj;
    }

    private static JsonNode? BuildNode(FieldConfig field, DataRow row, string path, Context ctx, out bool include)
    {
        include = true;
        var resolved = ctx.Plan.Get(field);

        switch (resolved?.Kind)
        {
            case FieldKind.Container:
                if (field.Type == FieldTypes.Array)
                    return new JsonArray { BuildObject(field.Children, row, path + "[0]", ctx) };
                return BuildObject(field.Children, row, path, ctx);

            case FieldKind.EmptyContainer:
                return field.Type == FieldTypes.Array ? new JsonArray() : new JsonObject();

            case FieldKind.Literal:
            case FieldKind.Default:
                return ConvertValue(field, resolved.Literal, path, ctx);

            case FieldKind.Column:
                {
                    object? raw = null;
                    if (resolved.Column != null && ctx.Data.Columns.Contains(resolved.Column))
                        raw = row[resolved.Column];
                    // coluna ausente na origem já é reportada uma única vez pelo IntegratorService
                    return ConvertValue(field, raw, path, ctx);
                }

            default: // Pending ou sem plano: omitido
                include = false;
                return null;
        }
    }

    private static JsonNode? ConvertValue(FieldConfig field, object? raw, string path, Context ctx)
    {
        ColumnConfig? column = null;
        var resolved = ctx.Plan.Get(field);
        if (resolved?.Column != null) ctx.Columns.TryGetValue(resolved.Column, out column);

        var format = field.Format ?? column?.Format;
        var culture = field.Culture ?? column?.Culture;

        if (!JsonValueConverter.TryConvert(field.Type, raw, format, culture, out var node, out var error))
        {
            ctx.Add(ValidationIssue.Error("DATA_CONVERSION", path, error ?? "Valor não convertível."));
            return null;
        }

        if (node == null && field.Required)
            ctx.Add(ValidationIssue.Error("DATA_REQUIRED_EMPTY", path, "Campo obrigatório sem valor na origem."));

        return node;
    }
}
