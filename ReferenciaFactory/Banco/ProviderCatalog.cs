using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace ReferenciaFactory.Banco
{

    public sealed class ProviderField
    {
        public required string Id { get; init; }
        public required string Label { get; init; }
        /// <summary>text | number | password | file | bool</summary>
        public string Kind { get; init; } = "text";
        public bool Required { get; init; }
        public string? Placeholder { get; init; }
        public string? Default { get; init; }
        public string? Hint { get; init; }
    }

    /// <summary>Descreve um banco suportado: campos do formulario, fabrica ADO.NET, dialeto SQL e montagem da string de conexao.</summary>
    public sealed class ProviderInfo
    {
        public required string Key { get; init; }
        public required string DisplayName { get; init; }
        /// <summary>"Servidor" ou "Arquivo"</summary>
        public required string Category { get; init; }
        public required ISqlDialect Dialect { get; init; }
        public required Func<DbProviderFactory> Factory { get; init; }
        public required List<ProviderField> Fields { get; init; }
        public required Func<IReadOnlyDictionary<string, string>, string> BuildConnectionString { get; init; }
        /// <summary>Nomes de provider usados no GeneratorJS_Database.xml antigo (migracao).</summary>
        public string[] LegacyNames { get; init; } = Array.Empty<string>();
    }

    public static class ProviderCatalog
    {
        private static readonly string[] SecretFields = { "password", "dbpassword" };
        public static bool IsSecretField(string id) => SecretFields.Contains(id, StringComparer.OrdinalIgnoreCase);

        private static string F(IReadOnlyDictionary<string, string> f, string key) =>
            f.TryGetValue(key, out var v) ? v.Trim() : "";

        private static bool B(IReadOnlyDictionary<string, string> f, string key, bool def = false) =>
            f.TryGetValue(key, out var v) && bool.TryParse(v, out var b) ? b : def;

        private static DbConnectionStringBuilder Start(IReadOnlyDictionary<string, string> f)
        {
            var b = new DbConnectionStringBuilder();
            var extra = F(f, "extra");
            if (extra.Length > 0) b.ConnectionString = extra;   // opcoes avancadas vem primeiro; o formulario prevalece
            return b;
        }

        private static List<ProviderField> ServerFields(string defaultPort, string serverHint, bool windowsAuth = false)
        {
            var l = new List<ProviderField>
        {
            new() { Id = "server", Label = "Servidor", Required = true, Placeholder = serverHint },
            new() { Id = "port", Label = "Porta", Kind = "number", Placeholder = defaultPort },
            new() { Id = "database", Label = "Banco de dados", Required = true },
        };
            if (windowsAuth) l.Add(new() { Id = "integrated", Label = "Autenticacao do Windows", Kind = "bool", Default = "false" });
            l.Add(new() { Id = "user", Label = "Usuario" });
            l.Add(new() { Id = "password", Label = "Senha", Kind = "password" });
            return l;
        }

        private static readonly ProviderField ExtraField = new()
        {
            Id = "extra",
            Label = "Opcoes avancadas",
            Placeholder = "chave=valor;chave=valor",
            Hint = "Anexadas a string de conexao (timeout, SSL, charset...)."
        };

        public static readonly IReadOnlyList<ProviderInfo> All = new List<ProviderInfo>
    {
        new()
        {
            Key = "sqlserver", DisplayName = "SQL Server", Category = "Servidor",
            Dialect = new SqlServerDialect(),
            Factory = () => Microsoft.Data.SqlClient.SqlClientFactory.Instance,
            LegacyNames = new[] { "System.Data.SqlClient", "Microsoft.Data.SqlClient", "SQLOLEDB.1", "SQLNCLI11", "MSOLEDBSQL" },
            Fields = ServerFields("1433", "localhost\\SQLEXPRESS", windowsAuth: true)
                .Append(new ProviderField { Id = "trustCert", Label = "Confiar no certificado do servidor", Kind = "bool", Default = "true" })
                .Append(ExtraField).ToList(),
            BuildConnectionString = f =>
            {
                var b = Start(f);
                var server = F(f, "server");
                if (F(f, "port").Length > 0) server += "," + F(f, "port");
                b["Data Source"] = server;
                b["Initial Catalog"] = F(f, "database");
                if (B(f, "integrated")) b["Integrated Security"] = "true";
                else { b["User ID"] = F(f, "user"); b["Password"] = F(f, "password"); }
                b["TrustServerCertificate"] = B(f, "trustCert", true).ToString().ToLowerInvariant();
                b["Application Name"] = "GeneratorJS";
                return b.ConnectionString;
            }
        },
        new()
        {
            Key = "postgres", DisplayName = "PostgreSQL", Category = "Servidor",
            Dialect = new PostgreSqlDialect(),
            Factory = () => Npgsql.NpgsqlFactory.Instance,
            LegacyNames = new[] { "Npgsql" },
            Fields = ServerFields("5432", "localhost").Append(ExtraField).ToList(),
            BuildConnectionString = f =>
            {
                var b = Start(f);
                b["Host"] = F(f, "server");
                if (F(f, "port").Length > 0) b["Port"] = F(f, "port");
                b["Database"] = F(f, "database");
                b["Username"] = F(f, "user");
                b["Password"] = F(f, "password");
                return b.ConnectionString;
            }
        },
        new()
        {
            Key = "mysql", DisplayName = "MySQL / MariaDB", Category = "Servidor",
            Dialect = new MySqlDialect(),
            Factory = () => MySqlConnector.MySqlConnectorFactory.Instance,
            LegacyNames = new[] { "MySql.Data.MySqlClient", "MySqlConnector" },
            Fields = ServerFields("3306", "localhost").Append(ExtraField).ToList(),
            BuildConnectionString = f =>
            {
                var b = Start(f);
                b["Server"] = F(f, "server");
                if (F(f, "port").Length > 0) b["Port"] = F(f, "port");
                b["Database"] = F(f, "database");
                b["User ID"] = F(f, "user");
                b["Password"] = F(f, "password");
                return b.ConnectionString;
            }
        },
        new()
        {
            Key = "oracle", DisplayName = "Oracle", Category = "Servidor",
            Dialect = new OracleDialect(),
            Factory = () => Oracle.ManagedDataAccess.Client.OracleClientFactory.Instance,
            LegacyNames = new[] { "Oracle.ManagedDataAccess.Client" },
            Fields = new List<ProviderField>
            {
                new() { Id = "server", Label = "Servidor", Required = true, Placeholder = "localhost" },
                new() { Id = "port", Label = "Porta", Kind = "number", Placeholder = "1521" },
                new() { Id = "database", Label = "Service name", Required = true, Placeholder = "XEPDB1" },
                new() { Id = "user", Label = "Usuario" },
                new() { Id = "password", Label = "Senha", Kind = "password" },
                ExtraField
            },
            BuildConnectionString = f =>
            {
                var b = Start(f);
                var port = F(f, "port").Length > 0 ? F(f, "port") : "1521";
                b["Data Source"] = $"//{F(f, "server")}:{port}/{F(f, "database")}";
                b["User Id"] = F(f, "user");
                b["Password"] = F(f, "password");
                return b.ConnectionString;
            }
        },
        new()
        {
            Key = "firebird", DisplayName = "Firebird / InterBase", Category = "Servidor",
            Dialect = new FirebirdDialect(),
            Factory = () => FirebirdSql.Data.FirebirdClient.FirebirdClientFactory.Instance,
            LegacyNames = new[] { "FirebirdSql.Data.FirebirdClient", "InterBase.Data.IBClient" },
            Fields = new List<ProviderField>
            {
                new() { Id = "server", Label = "Servidor", Required = true, Placeholder = "localhost" },
                new() { Id = "port", Label = "Porta", Kind = "number", Placeholder = "3050" },
                new() { Id = "database", Label = "Banco (caminho ou alias)", Required = true, Placeholder = "C:\\dados\\banco.fdb" },
                new() { Id = "user", Label = "Usuario", Placeholder = "SYSDBA" },
                new() { Id = "password", Label = "Senha", Kind = "password" },
                ExtraField
            },
            BuildConnectionString = f =>
            {
                var b = Start(f);
                b["DataSource"] = F(f, "server");
                if (F(f, "port").Length > 0) b["Port"] = F(f, "port");
                b["Database"] = F(f, "database");
                b["User"] = F(f, "user");
                b["Password"] = F(f, "password");
                if (!b.ContainsKey("Charset")) b["Charset"] = "UTF8";
                return b.ConnectionString;
            }
        },
        new()
        {
            Key = "sqlite", DisplayName = "SQLite", Category = "Arquivo",
            Dialect = new SqliteDialect(),
            Factory = () => Microsoft.Data.Sqlite.SqliteFactory.Instance,
            LegacyNames = new[] { "System.Data.SQLite" },
            Fields = new List<ProviderField>
            {
                new() { Id = "file", Label = "Arquivo do banco", Kind = "file", Required = true, Placeholder = "C:\\dados\\banco.db" },
                ExtraField
            },
            BuildConnectionString = f =>
            {
                var b = Start(f);
                b["Data Source"] = F(f, "file");
                if (!b.ContainsKey("Mode")) b["Mode"] = "ReadOnly";   // o gerador so le
                return b.ConnectionString;
            }
        },
        new()
        {
            Key = "access", DisplayName = "Microsoft Access", Category = "Arquivo",
            Dialect = new AccessDialect(),
            Factory = () => System.Data.OleDb.OleDbFactory.Instance,
            LegacyNames = new[] { "System.Data.OleDb", "Microsoft.ACE.OLEDB.12.0", "Microsoft.Jet.OLEDB.4.0" },
            Fields = new List<ProviderField>
            {
                new() { Id = "file", Label = "Arquivo (.accdb / .mdb)", Kind = "file", Required = true, Placeholder = "C:\\dados\\banco.accdb" },
                new() { Id = "oledb", Label = "Provider OLE DB", Default = "Microsoft.ACE.OLEDB.12.0",
                        Hint = "Use Microsoft.Jet.OLEDB.4.0 para .mdb em aplicativo 32 bits. Exige o driver instalado, com a mesma arquitetura do aplicativo." },
                new() { Id = "dbpassword", Label = "Senha do banco", Kind = "password" },
                ExtraField
            },
            BuildConnectionString = f =>
            {
                var b = Start(f);
                b["Provider"] = F(f, "oledb").Length > 0 ? F(f, "oledb") : "Microsoft.ACE.OLEDB.12.0";
                b["Data Source"] = F(f, "file");
                if (F(f, "dbpassword").Length > 0) b["Jet OLEDB:Database Password"] = F(f, "dbpassword");
                return b.ConnectionString;
            }
        },
    };

        public static bool TryGet(string? key, [NotNullWhen(true)] out ProviderInfo? provider)
        {
            provider = All.FirstOrDefault(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            return provider is not null;
        }

        public static ProviderInfo Get(string? key) =>
            TryGet(key, out var p) ? p : throw new GeneratorException($"Provedor '{key}' nao suportado.");

        /// <summary>Converte o ProviderName antigo (ex.: "System.Data.SqlClient") para a chave nova.</summary>
        public static ProviderInfo? FromLegacyName(string? providerName) =>
            All.FirstOrDefault(p => p.LegacyNames.Contains(providerName ?? "", StringComparer.OrdinalIgnoreCase));

        public static string BuildConnectionString(ConnectionProfile profile)
        {
            var p = Get(profile.ProviderKey);
            if (profile.Mode == "raw")
            {
                if (string.IsNullOrWhiteSpace(profile.RawConnectionString))
                    throw new GeneratorException("Informe a string de conexao.");
                return profile.RawConnectionString!;
            }
            foreach (var field in p.Fields.Where(x => x.Required))
                if (!profile.Fields.TryGetValue(field.Id, out var v) || string.IsNullOrWhiteSpace(v))
                    throw new GeneratorException($"Preencha o campo \"{field.Label}\".");
            return p.BuildConnectionString(profile.Fields);
        }
    }
}
