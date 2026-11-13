using System.Data.Common;

namespace BetaJsonIntegrator.Data.Providers
{
    public sealed class MariaDbProvider : DbProviderBase
    {
        public MariaDbProvider(DbProviderFactory factory, string connectionString) : base(factory, connectionString) { }
        public override string DisplayName => "MariaDB";
        protected override string Quote(string identifier) => "`" + identifier + "`";
    }
}
