using System.Data;
using BetaJsonIntegrator.Core;
using BetaJsonIntegrator.Core.Configuration;

namespace BetaJsonIntegrator.Data
{
    /// <summary>
    /// Provedor em memória (DataTable por alias). Usado no primeiro marco funcional
    /// (protótipo sem banco) e nos testes.
    /// </summary>
    public sealed class DataTableProvider : IDataProvider
    {
        private readonly Dictionary<string, DataTable> _tables = new(StringComparer.OrdinalIgnoreCase);

        public string DisplayName => "Memória";

        public DataTableProvider Add(string alias, DataTable table)
        {
            _tables[alias] = table;
            return this;
        }

        public DataTable Load(TableConfig table)
        {
            if (_tables.TryGetValue(table.Alias, out var dt)) return dt;
            throw new KeyNotFoundException($"Nenhum DataTable registrado para o alias '{table.Alias}'.");
        }
    }
}
