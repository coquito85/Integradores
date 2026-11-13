using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using BetaJsonIntegrator.Core;
using BetaJsonIntegrator.Core.Configuration;

namespace BetaJsonIntegrator.Data.Providers
{
    /// <summary>
    /// Base para provedores de banco. Recebe um DbProviderFactory (MySqlConnectorFactory.Instance,
    /// Microsoft.Data.SqlClient.SqlClientFactory.Instance...), mantendo este projeto sem dependência
    /// de pacotes específicos de banco. A connection string vem da aplicação, nunca do XML.
    ///
    /// Segurança: nomes de tabela/colunas vêm do XML e são validados por lista restrita de caracteres
    /// e entre aspas/colchetes do dialeto; nenhum valor do usuário é concatenado ao SQL.
    /// </summary>
    public abstract class DbProviderBase : IDataProvider
    {
        private static readonly Regex Identifier = new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        private readonly DbProviderFactory _factory;
        private readonly string _connectionString;

        protected DbProviderBase(DbProviderFactory factory, string connectionString)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("A string de conexão não pode ser vazia.", nameof(connectionString));
            _connectionString = connectionString;
        }

        public abstract string DisplayName { get; }
        protected abstract string Quote(string identifier);

        public DataTable Load(TableConfig table)
        {
            var sql = BuildSelect(table);

            using var conn = _factory.CreateConnection() ?? throw new InvalidOperationException("Provider não criou a conexão.");
            conn.ConnectionString = _connectionString;
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 120;

            using var reader = cmd.ExecuteReader();
            var dt = new DataTable(table.Alias);
            dt.Load(reader);
            return dt;
        }

        public string BuildSelect(TableConfig table)
        {
            var tableName = QuoteQualified(table.Name);
            string cols = table.Columns.Count == 0
                ? "*"
                : string.Join(", ", table.Columns.Select(c => Quote(RequireIdentifier(c.Name, "coluna"))));
            return $"SELECT {cols} FROM {tableName}";
        }

        private string QuoteQualified(string name)
        {
            var parts = name.Split('.');
            if (parts.Length > 3) throw new ArgumentException($"Nome de tabela inválido: '{name}'.");
            return string.Join(".", parts.Select(p => Quote(RequireIdentifier(p, "tabela"))));
        }

        private static string RequireIdentifier(string value, string kind)
        {
            if (!Identifier.IsMatch(value))
                throw new ArgumentException($"Nome de {kind} inválido no layout: '{value}'. Use apenas letras, números e '_'.");
            return value;
        }
    }
}
