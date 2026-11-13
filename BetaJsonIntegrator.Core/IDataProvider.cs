using System.Data;
using BetaJsonIntegrator.Core.Configuration;

namespace BetaJsonIntegrator.Core
{
    /// <summary>
    /// Contrato comum de dados: qualquer origem (MariaDB, SQL Server, CSV, memória)
    /// devolve um DataTable por tabela/alias declarado no layout.
    /// </summary>
    public interface IDataProvider
    {
        string DisplayName { get; }

        /// <summary>Carrega os registros da tabela lógica (colunas conforme &lt;selection&gt;).</summary>
        DataTable Load(TableConfig table);
    }
}
