using System.Data;

namespace BetaJsonIntegrator.Data.Providers;

/// <summary>
/// Provedor em memória: serve um <see cref="DataTable"/> já existente. Permite o primeiro marco
/// (spec. seção 15.1) sem banco de dados e é a base dos testes automatizados.
/// </summary>
public sealed class DataTableProvider : IDataProvider
{
    private readonly DataTable _table;

    public DataTableProvider(DataTable table, string name = "Memória")
    {
        _table = table ?? throw new ArgumentNullException(nameof(table));
        Name = name;
    }

    public string Name { get; }

    public DataTable Load(DataRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var filtered = DataOrdering.Filter(_table, request.Filters);
        return DataOrdering.Apply(filtered, request.Columns);
    }
}
