using System.Data.Common;

namespace BetaJsonIntegrator.Data.Providers;

/// <summary>MariaDB/MySQL (identificadores entre crases). Passe <c>MySqlConnectorFactory.Instance</c>.</summary>
public sealed class MariaDbProvider : DbQueryProvider
{
    public MariaDbProvider(DbProviderFactory factory, string connectionString)
        : base(factory, connectionString) { }

    public override string Name => "MariaDB";
    protected override char OpenQuote => '`';
    protected override char CloseQuote => '`';
}
