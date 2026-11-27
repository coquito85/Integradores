using System.Data;
using System.Data.Common;
using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;

namespace ReferenciaFactory.Banco;

public sealed record TableRef(string? Schema, string Name)
{
    public string FullName => string.IsNullOrEmpty(Schema) ? Name : Schema + "." + Name;
}

public sealed record ColumnRef(string Name, string Type);

/// <summary>Nomes reais (como o banco os conhece) para a tabela e colunas do layout.</summary>
public sealed class SchemaMap
{
    public required TableRef Table { get; init; }
    /// <summary>nome no layout (case-insensitive) -> nome real no banco. Nulo se as colunas nao puderam ser lidas.</summary>
    public Dictionary<string, string>? Columns { get; init; }
}

/// <summary>Introspeccao do catalogo para qualquer banco, via dialeto (ou GetSchema no Access).</summary>
public static class SchemaReader
{
    public static async Task<List<TableRef>> ListTablesAsync(DbSession s, CancellationToken ct = default)
    {
        var result = new List<TableRef>();
        var d = s.Dialect;

        if (d.UsesGetSchema)
        {
            var dt = await Task.Run(() => s.Connection.GetSchema("Tables"), ct).ConfigureAwait(false);
            foreach (DataRow r in dt.Rows)
            {
                var type = Convert.ToString(r["TABLE_TYPE"]) ?? "";
                if (type is "TABLE" or "VIEW")
                    result.Add(new TableRef(null, Convert.ToString(r["TABLE_NAME"]) ?? ""));
            }
            return result.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        await foreach (var row in s.QueryAsync(d.ListTablesSql!, null, ct).ConfigureAwait(false))
        {
            if (d.TablesHaveSchema) result.Add(new TableRef(Convert.ToString(row[0]), Convert.ToString(row[1]) ?? ""));
            else result.Add(new TableRef(null, Convert.ToString(row[0]) ?? ""));
        }
        return result;
    }

    public static async Task<List<ColumnRef>> ListColumnsAsync(DbSession s, TableRef table, CancellationToken ct = default)
    {
        var result = new List<ColumnRef>();
        var d = s.Dialect;

        if (d.UsesGetSchema)
        {
            var dt = await Task.Run(() => s.Connection.GetSchema("Columns", new string?[] { null, null, table.Name, null }), ct).ConfigureAwait(false);
            foreach (DataRow r in dt.Rows.Cast<DataRow>().OrderBy(r => Convert.ToInt64(r["ORDINAL_POSITION"] is DBNull ? 0 : r["ORDINAL_POSITION"])))
                result.Add(new ColumnRef(Convert.ToString(r["COLUMN_NAME"]) ?? "", Convert.ToString(r["DATA_TYPE"]) ?? ""));
            return result;
        }

        var ps = d.TablesHaveSchema
            ? new List<QueryParam> { new(d.ParameterMarker(0), table.Schema), new(d.ParameterMarker(1), table.Name) }
            : new List<QueryParam> { new(d.ParameterMarker(0), table.Name) };

        await foreach (var row in s.QueryAsync(d.ListColumnsSql!, ps, ct).ConfigureAwait(false))
            result.Add(new ColumnRef(Convert.ToString(row[0]) ?? "", Convert.ToString(row[1]) ?? ""));
        return result;
    }

    /// <summary>
    /// Casa o nome da tabela e das colunas do layout com os nomes reais do banco (ignorando maiusculas/minusculas)
    /// e falha ANTES de rodar o SELECT se algo estiver faltando, listando exatamente o que.
    /// </summary>
    public static async Task<SchemaMap?> ResolveAsync(DbSession s, string requestedTable, IEnumerable<string> layoutColumns, CancellationToken ct = default)
    {
        var tables = await ListTablesAsync(s, ct).ConfigureAwait(false);
        if (tables.Count == 0) return null;

        var wanted = requestedTable.Trim();
        var table = tables.FirstOrDefault(t => t.FullName.Equals(wanted, StringComparison.OrdinalIgnoreCase))
                    ?? tables.FirstOrDefault(t => t.Name.Equals(wanted, StringComparison.OrdinalIgnoreCase));
        if (table is null)
        {
            var suggestions = tables
                .Where(t => t.Name.Contains(wanted, StringComparison.OrdinalIgnoreCase) || wanted.Contains(t.Name, StringComparison.OrdinalIgnoreCase))
                .Take(5).Select(t => t.FullName).ToList();
            throw new GeneratorException($"A tabela '{wanted}' nao existe nesta conexao." +
                (suggestions.Count > 0 ? $" Parecidas: {string.Join(", ", suggestions)}." : " Escolha outra na lista de tabelas."));
        }

        var cols = await ListColumnsAsync(s, table, ct).ConfigureAwait(false);
        if (cols.Count == 0) return new SchemaMap { Table = table, Columns = null };

        var real = cols.ToDictionary(c => c.Name, c => c.Name, StringComparer.OrdinalIgnoreCase);
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var missing = new List<string>();
        foreach (var name in layoutColumns)
        {
            if (real.TryGetValue(name, out var actual)) map[name] = actual;
            else missing.Add(name);
        }
        if (missing.Count > 0)
            throw new GeneratorException($"A tabela '{table.FullName}' nao tem {missing.Count} coluna(s) do layout: {string.Join(", ", missing)}.");

        return new SchemaMap { Table = table, Columns = map };
    }
}
