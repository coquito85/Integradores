using System.Data;
using System.Globalization;

namespace BetaJsonIntegrator.Data.Providers;

/// <summary>Ordenação (<c>orderby="true"</c>) e filtros para provedores que não usam SQL.</summary>
internal static class DataOrdering
{
    public static DataTable Filter(DataTable source, IReadOnlyList<DataFilter> filters)
    {
        if (filters.Count == 0) return source;

        foreach (var f in filters)
            if (!source.Columns.Contains(f.Column))
                throw new InvalidDataException($"A coluna de filtro '{f.Column}' não existe na origem.");

        var result = source.Clone();
        foreach (DataRow row in source.Rows)
        {
            bool keep = true;
            foreach (var f in filters)
            {
                var text = Convert.ToString(row[f.Column], CultureInfo.InvariantCulture) ?? "";
                if (!string.Equals(text, f.Value, StringComparison.OrdinalIgnoreCase))
                {
                    keep = false;
                    break;
                }
            }
            if (keep) result.ImportRow(row);
        }
        return result;
    }

    public static DataTable Apply(DataTable source, IReadOnlyList<SourceColumn> columns)
    {
        var keys = columns
            .Where(c => c.OrderBy && source.Columns.Contains(c.Name))
            .Select(c => "[" + c.Name.Replace("]", "\\]") + "]")
            .ToList();

        if (keys.Count == 0) return source;

        var view = new DataView(source) { Sort = string.Join(", ", keys) };
        return view.ToTable();
    }
}
