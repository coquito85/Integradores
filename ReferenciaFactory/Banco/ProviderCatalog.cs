using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;

namespace ReferenciaFactory.Banco
{

    public sealed class ProviderField
    {
        public required string Id { get; init; }
        public required string Label { get; init; }
        /// <summary>text | number | password | file | folder | bool | tribool | enum | combo</summary>
        public string Kind { get; init; } = "text";
        public bool Required { get; init; }
        public string? Placeholder { get; init; }
        public string? Default { get; init; }
        public string? Hint { get; init; }
        /// <summary>null = grupo principal; "Avançado" = recolhido.</summary>
        public string? Group { get; init; }
        /// <summary>Filtro do OpenFileDialog (Kind = "file").</summary>
        public string? Filter { get; init; }
        /// <summary>Hint calculado na hora (ex.: arquitetura e drivers detectados). Tem prioridade sobre Hint.</summary>
        public Func<string?>? HintSource { get; init; }
        /// <summary>Opcoes para Kind = "combo" / "enum" (no combo o usuario ainda pode digitar outro valor).</summary>
        public Func<IReadOnlyList<OleDbOption>>? OptionsSource { get; init; }
    }

    /// <summary>Descreve uma origem de dados suportada: campos do formulario, fabrica ADO.NET, dialeto SQL e montagem da string de conexao.</summary>
    public sealed class ProviderInfo
    {
        public required string Key { get; init; }
        public required string DisplayName { get; init; }
        /// <summary>
        /// Tipo de origem mostrado na tela: "Banco de dados" | "Excel" | "CSV" | "TXT".
        /// Categorias com mais de um provider (Banco de dados) mostram a segunda linha de botoes.
        /// </summary>
        public required string Category { get; init; }
        /// <summary>Sigla exibida no botao do provider (SQL, PG, CSV...).</summary>
        public required string Abbr { get; init; }
        /// <summary>false para CSV/TXT/Excel (nao existe string de conexao completa).</summary>
        public bool SupportsRaw { get; init; } = true;
        /// <summary>
        /// true para Excel/CSV/TXT: o arquivo e lido por FileSourceLoader e carregado numa base SQLite em memoria,
        /// entao dialeto, SchemaReader, filtros e streaming funcionam igual a um banco.
        /// </summary>
        public bool IsFile { get; init; }

        public required ISqlDialect Dialect { get; init; }
        public required Func<DbProviderFactory> Factory { get; init; }
        public required List<ProviderField> Fields { get; init; }
        public required Func<IReadOnlyDictionary<string, string>, string> BuildConnectionString { get; init; }
        /// <summary>Nomes de provider usados no GeneratorJS_Database.xml antigo (migracao).</summary>
        public string[] LegacyNames { get; init; } = Array.Empty<string>();
    }

    public static class ProviderCatalog
    {
        public const string CatDatabase = "Banco de dados";
        public const string CatExcel = "Excel";
        public const string CatCsv = "CSV";
        public const string CatTxt = "TXT";

        private static readonly string[] SecretFields = { "password", "dbpassword" };
        public static bool IsSecretField(string id) => SecretFields.Contains(id, StringComparer.OrdinalIgnoreCase);

        // ------------------------------------------------------------ helpers de leitura dos campos
        private static string F(IReadOnlyDictionary<string, string> f, string key) =>
            f.TryGetValue(key, out var v) ? v.Trim() : "";

        private static bool B(IReadOnlyDictionary<string, string> f, string key, bool def = false) =>
            f.TryGetValue(key, out var v) && bool.TryParse(v, out var b) ? b : def;

        /// <summary>Tri-estado (IsBooleanType do DatabaseConnection): ausente = padrao do driver.</summary>
        private static bool? OptBool(IReadOnlyDictionary<string, string> f, string key) =>
            f.TryGetValue(key, out var v) && bool.TryParse(v, out var b) ? b : null;

        private static void SetText(DbConnectionStringBuilder b, string key, string value)
        {
            if (value.Length > 0) b[key] = value;
        }

        private static void SetBool(DbConnectionStringBuilder b, string key, bool? value)
        {
            if (value.HasValue) b[key] = value.Value ? "true" : "false";
        }

        /// <summary>So grava numeros &gt; 0 (mesmo criterio do AddIf(... &gt; 0) do DatabaseConnection).</summary>
        private static void SetPositive(DbConnectionStringBuilder b, IReadOnlyDictionary<string, string> f, string field, string key)
        {
            if (int.TryParse(F(f, field), out var n) && n > 0) b[key] = n.ToString();
        }

        private static DbConnectionStringBuilder Start(IReadOnlyDictionary<string, string> f)
        {
            var b = new DbConnectionStringBuilder();
            var extra = F(f, "extra");
            if (extra.Length > 0) b.ConnectionString = extra;   // opcoes avancadas vem primeiro; o formulario prevalece
            return b;
        }

        // ------------------------------------------------------------ campos reutilizados
        private static List<ProviderField> ServerFields(string defaultPort, string serverHint, bool windowsAuth = false)
        {
            var l = new List<ProviderField>
        {
            new() { Id = "server", Label = "Servidor", Required = true, Placeholder = serverHint },
            new() { Id = "port", Label = "Porta", Kind = "number", Placeholder = defaultPort },
            new() { Id = "database", Label = "Banco de dados", Required = true },
        };
            if (windowsAuth)
                l.Add(new() { Id = "integrated", Label = "Autenticacao do Windows", Kind = "bool", Default = "false" });
            l.Add(new() { Id = "user", Label = "Usuario" });
            l.Add(new() { Id = "password", Label = "Senha", Kind = "password" });
            return l;
        }

        private static readonly ProviderField ExtraField = new()
        {
            Id = "extra",
            Label = "Opcoes avancadas",
            Group = "Avançado",
            Placeholder = "chave=valor;chave=valor",
            Hint = "Anexadas a string de conexao (qualquer chave que o driver aceite)."
        };

        private static readonly ProviderField ConnectTimeoutField = new()
        {
            Id = "connectTimeout",
            Label = "Timeout de conexao (segundos)",
            Kind = "number",
            Group = "Avançado",
            Placeholder = "15"
        };

        private static readonly ProviderField CommandTimeoutField = new()
        {
            Id = "commandTimeout",
            Label = "Timeout de comando (segundos)",
            Kind = "number",
            Group = "Avançado",
            Placeholder = "300",
            Hint = "Tempo maximo de cada consulta. Vazio = 300. Aplicado pelo gerador, em qualquer banco."
        };

        private static ProviderField TriField(string id, string label, string? hint = null) =>
            new() { Id = id, Label = label, Kind = "tribool", Group = "Avançado", Hint = hint };

        private static Func<IReadOnlyList<OleDbOption>> Choices(params (string Value, string Label)[] items) =>
            () => items.Select(i => new OleDbOption(i.Value, i.Label, true)).ToList();

        /// <summary>Campos avancados: especificos do provider + timeouts + opcoes livres.</summary>
        private static IEnumerable<ProviderField> Advanced(params ProviderField[] specific) =>
            specific.Concat(new[] { ConnectTimeoutField, CommandTimeoutField, ExtraField });

        // ------------------------------------------------------------ opcoes de arquivo
        private static readonly Func<IReadOnlyList<OleDbOption>> EncodingChoices = Choices(
            ("utf-8", "UTF-8"),
            ("windows-1252", "Windows-1252 (ANSI)"),
            ("iso-8859-1", "ISO-8859-1 (Latin-1)"),
            ("utf-16", "UTF-16"));

        private static ProviderField EncodingField() => new()
        {
            Id = "encoding",
            Label = "Codificacao",
            Kind = "enum",
            Default = "utf-8",
            OptionsSource = EncodingChoices,
            Hint = "Acentos estranhos (Ã§, �)? Troque para Windows-1252."
        };

        private static ProviderField HeaderField() => new()
        {
            Id = "header",
            Label = "A primeira linha contem os nomes das colunas",
            Kind = "bool",
            Default = "true",
            Hint = "Os nomes devem bater com as colunas do layout. Sem cabecalho, as colunas viram Coluna1, Coluna2..."
        };

        private static ProviderField SkipRowsField() => new()
        {
            Id = "skipRows",
            Label = "Linhas a ignorar no topo",
            Kind = "number",
            Group = "Avançado",
            Placeholder = "0",
            Hint = "Titulos ou observacoes antes do cabecalho."
        };

        private static string FileConnectionString(IReadOnlyDictionary<string, string> _) => "Data Source=:memory:";

        // ------------------------------------------------------------ catalogo
        public static readonly IReadOnlyList<ProviderInfo> All = new List<ProviderInfo>
    {
        new()
        {
            Key = "sqlserver", DisplayName = "SQL Server", Category = CatDatabase, Abbr = "SQL",
            Dialect = new SqlServerDialect(),
            Factory = () => Microsoft.Data.SqlClient.SqlClientFactory.Instance,
            LegacyNames = new[] { "System.Data.SqlClient", "Microsoft.Data.SqlClient", "SQLOLEDB.1", "SQLNCLI11", "MSOLEDBSQL" },
            Fields = ServerFields("1433", "localhost\\SQLEXPRESS", windowsAuth: true)
                .Append(new ProviderField { Id = "trustCert", Label = "Confiar no certificado do servidor", Kind = "bool", Default = "true" })
                .Concat(Advanced(
                    TriField("encrypt", "Criptografar conexao (Encrypt)"),
                    TriField("pooling", "Pooling de conexoes"),
                    TriField("mars", "MultipleActiveResultSets (MARS)"),
                    TriField("persist", "Persist Security Info")))
                .ToList(),
            BuildConnectionString = f =>
            {
                var b = Start(f);
                var server = F(f, "server");
                if (F(f, "port").Length > 0) server += "," + F(f, "port");
                b["Data Source"] = server;
                b["Initial Catalog"] = F(f, "database");
                if (B(f, "integrated")) b["Integrated Security"] = "true";
                else { b["User ID"] = F(f, "user"); b["Password"] = F(f, "password"); }
                b["TrustServerCertificate"] = B(f, "trustCert", true) ? "true" : "false";
                SetBool(b, "Encrypt", OptBool(f, "encrypt"));
                SetBool(b, "Pooling", OptBool(f, "pooling"));
                SetBool(b, "MultipleActiveResultSets", OptBool(f, "mars"));
                SetBool(b, "Persist Security Info", OptBool(f, "persist"));
                SetPositive(b, f, "connectTimeout", "Connect Timeout");
                b["Application Name"] = "GeneratorJS";
                return b.ConnectionString;
            }
        },
        new()
        {
            Key = "postgres", DisplayName = "PostgreSQL", Category = CatDatabase, Abbr = "PG",
            Dialect = new PostgreSqlDialect(),
            Factory = () => Npgsql.NpgsqlFactory.Instance,
            LegacyNames = new[] { "Npgsql" },
            Fields = ServerFields("5432", "localhost").Concat(Advanced()).ToList(),
            BuildConnectionString = f =>
            {
                var b = Start(f);
                b["Host"] = F(f, "server");
                if (F(f, "port").Length > 0) b["Port"] = F(f, "port");
                b["Database"] = F(f, "database");
                b["Username"] = F(f, "user");
                b["Password"] = F(f, "password");
                SetPositive(b, f, "connectTimeout", "Timeout");
                return b.ConnectionString;
            }
        },
        new()
        {
            Key = "mysql", DisplayName = "MySQL / MariaDB", Category = CatDatabase, Abbr = "MY",
            Dialect = new MySqlDialect(),
            Factory = () => MySqlConnector.MySqlConnectorFactory.Instance,
            LegacyNames = new[] { "MySql.Data.MySqlClient", "MySqlConnector" },
            Fields = ServerFields("3306", "localhost")
                .Concat(Advanced(
                    new ProviderField
                    {
                        Id = "sslMode", Label = "Modo SSL", Kind = "enum", Group = "Avançado", Default = "",
                        OptionsSource = Choices(("", "Padrao do driver"), ("None", "None"), ("Preferred", "Preferred"),
                            ("Required", "Required"), ("VerifyCA", "VerifyCA"), ("VerifyFull", "VerifyFull"))
                    },
                    new ProviderField { Id = "charset", Label = "Charset", Group = "Avançado", Placeholder = "utf8mb4" },
                    TriField("allowPublicKey", "Permitir AllowPublicKeyRetrieval", "Necessario em alguns MySQL 8 sem SSL."),
                    TriField("allowZeroDate", "Aceitar datas zeradas (AllowZeroDateTime)"),
                    new ProviderField
                    {
                        Id = "convertZeroDate", Label = "Converter 0000-00-00 em nulo", Kind = "bool", Group = "Avançado", Default = "true",
                        Hint = "Evita erro ao ler datas zeradas do MySQL."
                    }))
                .ToList(),
            BuildConnectionString = f =>
            {
                var b = Start(f);
                b["Server"] = F(f, "server");
                if (F(f, "port").Length > 0) b["Port"] = F(f, "port");
                b["Database"] = F(f, "database");
                b["User ID"] = F(f, "user");
                b["Password"] = F(f, "password");
                SetText(b, "SslMode", F(f, "sslMode"));
                SetText(b, "CharSet", F(f, "charset"));
                SetBool(b, "AllowPublicKeyRetrieval", OptBool(f, "allowPublicKey"));
                SetBool(b, "AllowZeroDateTime", OptBool(f, "allowZeroDate"));
                b["ConvertZeroDateTime"] = B(f, "convertZeroDate", true) ? "true" : "false";
                SetPositive(b, f, "connectTimeout", "Connection Timeout");
                return b.ConnectionString;
            }
        },
        new()
        {
            Key = "oracle", DisplayName = "Oracle", Category = CatDatabase, Abbr = "ORA",
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
            }.Concat(Advanced()).ToList(),
            BuildConnectionString = f =>
            {
                var b = Start(f);
                var port = F(f, "port").Length > 0 ? F(f, "port") : "1521";
                b["Data Source"] = $"//{F(f, "server")}:{port}/{F(f, "database")}";
                b["User Id"] = F(f, "user");
                b["Password"] = F(f, "password");
                SetPositive(b, f, "connectTimeout", "Connection Timeout");
                return b.ConnectionString;
            }
        },
        new()
        {
            Key = "firebird", DisplayName = "Firebird / InterBase", Category = CatDatabase, Abbr = "FB",
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
            }.Concat(Advanced(
                new ProviderField { Id = "charset", Label = "Charset", Group = "Avançado", Placeholder = "UTF8", Hint = "Vazio = UTF8." }))
            .ToList(),
            BuildConnectionString = f =>
            {
                var b = Start(f);
                b["DataSource"] = F(f, "server");
                if (F(f, "port").Length > 0) b["Port"] = F(f, "port");
                b["Database"] = F(f, "database");
                b["User"] = F(f, "user");
                b["Password"] = F(f, "password");
                if (F(f, "charset").Length > 0) b["Charset"] = F(f, "charset");
                else if (!b.ContainsKey("Charset")) b["Charset"] = "UTF8";
                SetPositive(b, f, "connectTimeout", "Connection Timeout");
                return b.ConnectionString;
            }
        },
        new()
        {
            Key = "sqlite", DisplayName = "SQLite", Category = CatDatabase, Abbr = "LITE",
            Dialect = new SqliteDialect(),
            Factory = () => Microsoft.Data.Sqlite.SqliteFactory.Instance,
            LegacyNames = new[] { "System.Data.SQLite" },
            Fields = new List<ProviderField>
            {
                new()
                {
                    Id = "file", Label = "Arquivo do banco", Kind = "file", Required = true,
                    Placeholder = "C:\\dados\\banco.db",
                    Filter = "SQLite (*.db;*.sqlite;*.sqlite3)|*.db;*.sqlite;*.sqlite3|Todos os arquivos (*.*)|*.*"
                },
                CommandTimeoutField,
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
            Key = "access", DisplayName = "Microsoft Access", Category = CatDatabase, Abbr = "ACC",
            Dialect = new AccessDialect(),
            Factory = () => System.Data.OleDb.OleDbFactory.Instance,
            LegacyNames = new[] { "System.Data.OleDb", "Microsoft.ACE.OLEDB.12.0", "Microsoft.Jet.OLEDB.4.0" },
            Fields = new List<ProviderField>
            {
                new()
                {
                    Id = "file", Label = "Arquivo (.accdb / .mdb)", Kind = "file", Required = true,
                    Placeholder = "C:\\dados\\banco.accdb",
                    Filter = "Access (*.accdb;*.mdb)|*.accdb;*.mdb|Todos os arquivos (*.*)|*.*"
                },
                new()
                {
                    Id = "oledb", Label = "Provider OLE DB", Kind = "combo",
                    Placeholder = "Automatico",
                    HintSource = OleDbProviders.Dica,
                    OptionsSource = OleDbProviders.Opcoes
                },
                new() { Id = "dbpassword", Label = "Senha do banco", Kind = "password" },
                CommandTimeoutField,
                ExtraField
            },
            BuildConnectionString = f =>
            {
                var b = Start(f);
                b["Provider"] = F(f, "oledb").Length > 0 ? F(f, "oledb") : OleDbProviders.Automatico();
                b["Data Source"] = F(f, "file");
                if (F(f, "dbpassword").Length > 0) b["Jet OLEDB:Database Password"] = F(f, "dbpassword");
                return b.ConnectionString;
            }
        },

        // ---------------------------------------------------------------- arquivos (lidos por FileSourceLoader)
        new()
        {
            Key = "excel", DisplayName = "Excel", Category = CatExcel, Abbr = "XLS",
            IsFile = true, SupportsRaw = false,
            Dialect = new SqliteDialect(),
            Factory = () => Microsoft.Data.Sqlite.SqliteFactory.Instance,
            Fields = new List<ProviderField>
            {
                new()
                {
                    Id = "file", Label = "Planilha (.xlsx / .xls)", Kind = "file", Required = true,
                    Placeholder = "C:\\dados\\planilha.xlsx",
                    Filter = "Excel (*.xlsx;*.xlsm;*.xls)|*.xlsx;*.xlsm;*.xls|Todos os arquivos (*.*)|*.*",
                    Hint = "Cada aba vira uma tabela. Com uma unica aba, ela e usada independente do nome no layout."
                },
                HeaderField(),
                SkipRowsField()
            },
            BuildConnectionString = FileConnectionString
        },
        new()
        {
            Key = "csv", DisplayName = "CSV", Category = CatCsv, Abbr = "CSV",
            IsFile = true, SupportsRaw = false,
            Dialect = new SqliteDialect(),
            Factory = () => Microsoft.Data.Sqlite.SqliteFactory.Instance,
            Fields = new List<ProviderField>
            {
                new()
                {
                    Id = "file", Label = "Arquivo CSV", Kind = "file", Required = true,
                    Placeholder = "C:\\dados\\arquivo.csv",
                    Filter = "CSV (*.csv)|*.csv|Texto (*.txt)|*.txt|Todos os arquivos (*.*)|*.*"
                },
                new()
                {
                    Id = "delimiter", Label = "Separador", Kind = "enum", Default = "auto",
                    OptionsSource = Choices(("auto", "Detectar automaticamente"), ("semicolon", "Ponto e virgula ( ; )"),
                        ("comma", "Virgula ( , )"), ("tab", "Tabulacao"), ("pipe", "Barra vertical ( | )"))
                },
                EncodingField(),
                HeaderField(),
                SkipRowsField()
            },
            BuildConnectionString = FileConnectionString
        },
        new()
        {
            Key = "txt", DisplayName = "TXT", Category = CatTxt, Abbr = "TXT",
            IsFile = true, SupportsRaw = false,
            Dialect = new SqliteDialect(),
            Factory = () => Microsoft.Data.Sqlite.SqliteFactory.Instance,
            Fields = new List<ProviderField>
            {
                new()
                {
                    Id = "file", Label = "Arquivo TXT", Kind = "file", Required = true,
                    Placeholder = "C:\\dados\\arquivo.txt",
                    Filter = "Texto (*.txt;*.dat;*.tsv)|*.txt;*.dat;*.tsv|Todos os arquivos (*.*)|*.*"
                },
                new()
                {
                    Id = "delimiter", Label = "Formato", Kind = "enum", Default = "tab",
                    OptionsSource = Choices(("tab", "Delimitado por tabulacao"), ("semicolon", "Delimitado por ponto e virgula"),
                        ("comma", "Delimitado por virgula"), ("pipe", "Delimitado por barra vertical"),
                        ("space", "Delimitado por espaco"), ("auto", "Detectar delimitador"), ("fixed", "Largura fixa"))
                },
                new()
                {
                    Id = "widths", Label = "Larguras das colunas (largura fixa)", Placeholder = "10,30,8",
                    Hint = "So usado no formato Largura fixa: tamanho de cada coluna, separado por virgula."
                },
                EncodingField(),
                HeaderField(),
                SkipRowsField()
            },
            BuildConnectionString = FileConnectionString
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
            if (profile.Mode == "raw" && p.SupportsRaw)
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