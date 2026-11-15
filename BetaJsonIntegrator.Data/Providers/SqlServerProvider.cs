using System.Data.Common;

namespace BetaJsonIntegrator.Data.Providers;

/// <summary>SQL Server (identificadores entre colchetes). Passe <c>SqlClientFactory.Instance</c>.</summary>
public sealed class SqlServerProvider : DbQueryProvider
{
    public SqlServerProvider(DbProviderFactory factory, string connectionString)
        : base(factory, connectionString) { }

    public override string Name => "SQL Server";
    protected override char OpenQuote => '[';
    protected override char CloseQuote => ']';
}
