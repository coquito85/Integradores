using System.Data;
using System.Data.Common;
using System.Text;

namespace BetaJsonIntegrator.Data.Providers;

/// <summary>SELECT gerado a partir do layout, com parâmetros separados do texto SQL.</summary>
public sealed record SelectStatement(string Sql, IReadOnlyList<KeyValuePair<string, object>> Parameters);

/// <summary>
/// Base dos provedores de banco. Monta o SELECT somente com as colunas declaradas no XML
/// (identificadores validados e entre aspas) e envia filtros como parâmetros.
/// O <see cref="DbProviderFactory"/> é fornecido pelo App, mantendo o Data sem dependência de drivers.
/// </summary>
public abstract class DbQueryProvider : IDataProvider
{
    private readonly DbProviderFactory _factory;
    private readonly string _connectionString;

    protected DbQueryProvider(DbProviderFactory factory, string connectionString)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("A string de conexão não pode ser vazia.", nameof(connectionString));
        _connectionString = connectionString;
    }

    public abstract string Name { get; }
    protected abstract char OpenQuote { get; }
    protected abstract char CloseQuote { get; }

    /// <summary>Marcador do parâmetro no SQL. Padrão nomeado (@p0); OleDb/Access usa posicional (?).</summary>
    protected virtual string ParameterMarker(int index) => "@p" + index;

    public int CommandTimeoutSeconds { get; set; } = 120;

    public SelectStatement BuildSelect(DataRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Columns.Count == 0)
            throw new ArgumentException("O layout não declara colunas para a consulta.", nameof(request));

        string Q(string id) => SqlIdentifier.Quote(id, OpenQuote, CloseQuote);

        var sql = new StringBuilder("SELECT ");
        sql.Append(string.Join(", ", request.Columns.Select(c => Q(c.Name))));
        sql.Append(" FROM ").Append(Q(request.TableName));

        var parameters = new List<KeyValuePair<string, object>>();
        if (request.Filters.Count > 0)
        {
            var conditions = new List<string>();
            foreach (var f in request.Filters)
            {
                var pname = "@p" + parameters.Count;
                conditions.Add($"{Q(f.Column)} = {ParameterMarker(parameters.Count)}");
                // em provedores posicionais a ordem de inclusão é o que vale
                parameters.Add(new KeyValuePair<string, object>(pname, f.Value));
            }
            sql.Append(" WHERE ").Append(string.Join(" AND ", conditions));
        }

        var order = request.Columns.Where(c => c.OrderBy).Select(c => Q(c.Name)).ToList();
        if (order.Count > 0) sql.Append(" ORDER BY ").Append(string.Join(", ", order));

        return new SelectStatement(sql.ToString(), parameters);
    }

    public DataTable Load(DataRequest request)
    {
        var select = BuildSelect(request);

        using var connection = _factory.CreateConnection()
            ?? throw new InvalidOperationException("O provedor de banco não criou a conexão.");
        connection.ConnectionString = _connectionString;
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = select.Sql;
        command.CommandTimeout = CommandTimeoutSeconds;
        foreach (var p in select.Parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = p.Key;
            parameter.Value = p.Value;
            command.Parameters.Add(parameter);
        }

        using var reader = command.ExecuteReader();
        var table = new DataTable("Source");
        table.Load(reader);
        return table;
    }
}
