using System.Data.Common;

namespace BetaJsonIntegrator.Data.Providers
{
    public sealed class SqlServerProvider : DbProviderBase
    {
        public SqlServerProvider(DbProviderFactory factory, string connectionString) : base(factory, connectionString) { }
        public override string DisplayName => "SQL Server";
        protected override string Quote(string identifier) => "[" + identifier + "]";
    }
}
