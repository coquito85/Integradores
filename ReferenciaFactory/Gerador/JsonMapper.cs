using System.Text.Json.Nodes;
using ReferenciaFactory.Layouts;

using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;

namespace ReferenciaFactory.Gerador;

public sealed class MapOptions
{
    /// <summary>true = comportamento do Extractor original: &lt;field type="array"/&gt; sem filhos vira null. false = [].</summary>
    public bool EmptyArrayAsNull { get; set; } = true;
}

/// <summary>Estatisticas da conversao (colunas vazias e falhas de conversao) para o relatorio final.</summary>
public sealed class MapStats
{
    public Dictionary<string, long> EmptyByColumn { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Issues { get; } = new();
    public long IssueCount { get; private set; }

    public void Issue(string message)
    {
        IssueCount++;
        if (Issues.Count < 50) Issues.Add(message);
    }

    public void Empty(string column) =>
        EmptyByColumn[column] = EmptyByColumn.TryGetValue(column, out var n) ? n + 1 : 1;
}

/// <summary>
/// Percorre a arvore de campos do layout e monta o JSON de cada linha:
///  - campo com filhos: objeto (ou array com UM objeto, quando type="array");
///  - Columnrefe: valor da coluna na linha;  value: valor fixo;  ambos convertidos pelo type/formate.
/// </summary>
public static class JsonMapper
{
    public static JsonObject MapRow(LayoutDefinition layout, IReadOnlyDictionary<string, object?> row,
        MapOptions options, MapStats stats, long rowNumber)
    {
        var obj = new JsonObject();
        foreach (var child in layout.RootField!.Children)
            obj[child.Name] = MapField(layout, child, row, options, stats, rowNumber);
        return obj;
    }

    private static JsonNode? MapField(LayoutDefinition layout, LayoutField field, IReadOnlyDictionary<string, object?> row,
        MapOptions options, MapStats stats, long rowNumber)
    {
        if (field.Children.Count > 0)
        {
            var obj = new JsonObject();
            foreach (var c in field.Children)
                obj[c.Name] = MapField(layout, c, row, options, stats, rowNumber);

            if (field.Type == "array")
                return new JsonArray(obj);
            return obj;
        }

        if (field.Type == "array")
            return options.EmptyArrayAsNull ? null : new JsonArray();

        object? raw = null;
        string? format = field.Format;

        if (field.ColumnRef is not null)
        {
            row.TryGetValue(field.ColumnRef, out raw);
            format ??= layout.FindColumn(field.ColumnRef)?.Format;
            if (ValueConverter.IsNull(raw) || (raw is string s && s.Length == 0))
                stats.Empty(field.ColumnRef);
        }
        else if (field.Value is not null)
        {
            raw = field.Value;
        }

        try
        {
            return ValueConverter.ToJson(field.Type, raw, format);
        }
        catch (FormatException ex)
        {
            var origin = field.ColumnRef is null ? "valor fixo" : $"coluna {field.ColumnRef}";
            stats.Issue($"Linha {rowNumber}, campo '{field.Name}' ({origin}): {ex.Message}");
            return null;
        }
    }
}
