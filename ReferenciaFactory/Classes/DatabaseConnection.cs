using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;

namespace ReferenciaFactory.Classes
{
    #region Tipos de apoio

    public sealed record QueryParam(string Name, object? Value);

    public sealed record TestResult(bool Ok, string Message, string? ServerVersion, long ElapsedMs);

    public sealed record TableRef(string? Schema, string Name)
    {
        public string FullName => string.IsNullOrEmpty(Schema) ? Name : Schema + "." + Name;
    }

    public sealed record ColumnRef(string Name, string Type);

    /// <summary>Opcao de provider OLE DB oferecida na tela (o usuario ainda pode digitar qualquer outro).</summary>
    public sealed record OleDbOption(string Value, string Label, bool Installed);

    /// <summary>Nomes reais (como o banco os conhece) para a tabela e colunas do layout.</summary>
    public sealed class SchemaMap
    {
        public required TableRef Table { get; init; }
        /// <summary>nome no layout (case-insensitive) -> nome real no banco. Nulo se as colunas nao puderam ser lidas.</summary>
        public Dictionary<string, string>? Columns { get; init; }
    }

    #endregion

    /// <summary>
    /// Uma conexao aberta + dialeto. O gerador so executa SELECT.
    /// Reune: fabrica ADO.NET, montagem da string de conexao, providers OLE DB (Access),
    /// abertura/teste, execucao em streaming e leitura de schema.
    /// </summary>
    public sealed class DatabaseConnection : IDisposable
    {
        #region Campos e construtor

        public ProviderInfo Provider { get; }
        public ISqlDialect Dialect => Provider.Dialect;
        public DbConnection Connection { get; }
        public int CommandTimeoutSeconds { get; set; } = 300;

        private DatabaseConnection(ProviderInfo provider, DbConnection connection)
        {
            Provider = provider;
            Connection = connection;
        }

        public void Dispose() => Connection.Dispose();

        #endregion

        #region Fabrica ADO.NET (antes ProviderInfo.Factory)

        public static DbProviderFactory GetFactory(string providerKey)
        {
            if (string.IsNullOrWhiteSpace(providerKey))
                throw new ArgumentException("A chave do provedor nao pode ser vazia.", nameof(providerKey));

            return providerKey.Trim().ToLowerInvariant() switch
            {
                "sqlserver" => Microsoft.Data.SqlClient.SqlClientFactory.Instance,
                "postgres" => Npgsql.NpgsqlFactory.Instance,
                "mysql" => MySqlConnector.MySqlConnectorFactory.Instance,
                "oracle" => Oracle.ManagedDataAccess.Client.OracleClientFactory.Instance,
                "firebird" => FirebirdSql.Data.FirebirdClient.FirebirdClientFactory.Instance,
                // Excel/CSV/TXT sao carregados numa base SQLite em memoria (FileSourceLoader)
                "sqlite" or "excel" or "csv" or "txt" => Microsoft.Data.Sqlite.SqliteFactory.Instance,
                "access" => System.Data.OleDb.OleDbFactory.Instance,
                _ => throw new GeneratorException($"Provedor '{providerKey}' nao suportado.")
            };
        }

        #endregion

        #region String de conexao (antes ProviderInfo.BuildConnectionString)

        public static string BuildConnectionString(ConnectionProfile profile)
        {
            var p = ProviderCatalog.Get(profile.ProviderKey);
            if (profile.Mode == "raw" && p.SupportsRaw)
            {
                if (string.IsNullOrWhiteSpace(profile.RawConnectionString))
                    throw new GeneratorException("Informe a string de conexao.");
                return profile.RawConnectionString!;
            }
            foreach (var field in p.Fields.Where(x => x.Required))
                if (!profile.Fields.TryGetValue(field.Id, out var v) || string.IsNullOrWhiteSpace(v))
                    throw new GeneratorException($"Preencha o campo \"{field.Label}\".");
            return BuildFromFields(p.Key, profile.Fields);
        }

        /// <summary>Campos do formulario -> string de conexao do provedor. Para um banco novo, inclua um case aqui.</summary>
        public static string BuildFromFields(string providerKey, IReadOnlyDictionary<string, string> f)
        {
            var b = Start(f);

            switch (providerKey.ToLowerInvariant())
            {
                case "sqlserver":
                    {
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
                        break;
                    }

                case "postgres":
                    b["Host"] = F(f, "server");
                    if (F(f, "port").Length > 0) b["Port"] = F(f, "port");
                    b["Database"] = F(f, "database");
                    b["Username"] = F(f, "user");
                    b["Password"] = F(f, "password");
                    SetPositive(b, f, "connectTimeout", "Timeout");
                    break;

                case "mysql":
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
                    break;

                case "oracle":
                    {
                        var port = F(f, "port").Length > 0 ? F(f, "port") : "1521";
                        b["Data Source"] = $"//{F(f, "server")}:{port}/{F(f, "database")}";
                        b["User Id"] = F(f, "user");
                        b["Password"] = F(f, "password");
                        SetPositive(b, f, "connectTimeout", "Connection Timeout");
                        break;
                    }

                case "firebird":
                    b["DataSource"] = F(f, "server");
                    if (F(f, "port").Length > 0) b["Port"] = F(f, "port");
                    b["Database"] = F(f, "database");
                    b["User"] = F(f, "user");
                    b["Password"] = F(f, "password");
                    if (F(f, "charset").Length > 0) b["Charset"] = F(f, "charset");
                    else if (!b.ContainsKey("Charset")) b["Charset"] = "UTF8";
                    SetPositive(b, f, "connectTimeout", "Connection Timeout");
                    break;

                case "sqlite":
                    b["Data Source"] = F(f, "file");
                    if (!b.ContainsKey("Mode")) b["Mode"] = "ReadOnly";   // o gerador so le
                    break;

                case "access":
                    b["Provider"] = F(f, "oledb").Length > 0 ? F(f, "oledb") : OleDbAutomatico();
                    b["Data Source"] = F(f, "file");
                    if (F(f, "dbpassword").Length > 0) b["Jet OLEDB:Database Password"] = F(f, "dbpassword");
                    break;

                case "excel":
                case "csv":
                case "txt":
                    return "Data Source=:memory:";   // arquivo lido por FileSourceLoader

                default:
                    throw new GeneratorException($"Provedor '{providerKey}' nao suportado para montagem da string de conexao.");
            }

            return b.ConnectionString;
        }

        // ---- helpers de leitura dos campos
        private static string F(IReadOnlyDictionary<string, string> f, string key) =>
            f.TryGetValue(key, out var v) ? v.Trim() : "";

        private static bool B(IReadOnlyDictionary<string, string> f, string key, bool def = false) =>
            f.TryGetValue(key, out var v) && bool.TryParse(v, out var b) ? b : def;

        /// <summary>Tri-estado: ausente = padrao do driver.</summary>
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

        /// <summary>So grava numeros &gt; 0.</summary>
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

        #endregion

        #region Providers OLE DB (Access)

        private const string OleDbFallback = "Microsoft.ACE.OLEDB.12.0";

        // Ordem = preferencia no modo automatico.
        private static readonly (string Name, string Desc)[] OleDbConhecidos =
        {
            ("Microsoft.ACE.OLEDB.16.0", ".accdb e .mdb"),
            ("Microsoft.ACE.OLEDB.12.0", ".accdb e .mdb"),
            ("Microsoft.Jet.OLEDB.4.0",  ".mdb, somente 32 bits"),
        };

        /// <summary>Providers OLE DB registrados para a arquitetura deste processo.</summary>
        public static IReadOnlyList<string> OleDbInstalados()
        {
            var lista = new List<string>();
            try
            {
                using var r = OleDbEnumerator.GetRootEnumerator();
                while (r.Read())
                {
                    var nome = Convert.ToString(r["SOURCES_NAME"])?.Trim();
                    if (!string.IsNullOrEmpty(nome)) lista.Add(nome);
                }
            }
            catch { /* sem enumerador: segue so com a lista conhecida */ }
            return lista;
        }

        public static IReadOnlyList<OleDbOption> OleDbOpcoes()
        {
            var instalados = OleDbInstalados();
            bool Tem(string n) => instalados.Contains(n, StringComparer.OrdinalIgnoreCase);

            var opcoes = OleDbConhecidos
                .Select(k => new OleDbOption(k.Name, $"{k.Name} ({k.Desc}) - {(Tem(k.Name) ? "instalado" : "nao instalado")}", Tem(k.Name)))
                .ToList();

            // qualquer outro ACE/Jet registrado (ex.: ACE 15.0) tambem aparece
            foreach (var n in instalados)
                if ((n.Contains("ACE.OLEDB", StringComparison.OrdinalIgnoreCase) || n.Contains("Jet.OLEDB", StringComparison.OrdinalIgnoreCase))
                    && !opcoes.Any(o => o.Value.Equals(n, StringComparison.OrdinalIgnoreCase)))
                    opcoes.Add(new OleDbOption(n, $"{n} - instalado", true));

            return opcoes;
        }

        /// <summary>Melhor provider instalado (modo automatico).</summary>
        public static string OleDbAutomatico() =>
            OleDbOpcoes().FirstOrDefault(o => o.Installed)?.Value ?? OleDbFallback;

        public static string OleDbDica()
        {
            var bits = Environment.Is64BitProcess ? 64 : 32;
            var achados = OleDbOpcoes().Where(o => o.Installed).Select(o => o.Value).ToList();
            var lista = achados.Count > 0 ? string.Join(", ", achados) : "nenhum provider do Access";
            return $"Aplicativo em {bits} bits. Detectados: {lista}. Pode digitar outro provider; em branco usa o automatico.";
        }

        /// <summary>
        /// Para bancos OLE DB (Access), o erro "provider is not registered" quase sempre e de arquitetura:
        /// o driver instalado (ACE/Jet) nao e da mesma arquitetura (32/64 bits) do aplicativo.
        /// </summary>
        private static string DriverHint(ProviderInfo provider, Exception ex)
        {
            if (!provider.Key.Equals("access", StringComparison.OrdinalIgnoreCase)) return "";

            var msg = ex.Message ?? "";
            if (!msg.Contains("not registered", StringComparison.OrdinalIgnoreCase)) return "";

            var arq = Environment.Is64BitProcess ? "64" : "32";
            var outra = Environment.Is64BitProcess ? "32" : "64";

            var achados = OleDbInstalados().Where(n => n.Contains("ACE.OLEDB", StringComparison.OrdinalIgnoreCase) || n.Contains("Jet.OLEDB", StringComparison.OrdinalIgnoreCase)).ToList();
            var detectados = achados.Count > 0
                ? $" Providers detectados neste aplicativo: {string.Join(", ", achados)} (escolha um deles no campo Provider OLE DB)."
                : " Nenhum provider do Access foi detectado neste aplicativo.";

            return $"{Environment.NewLine}Dica: o aplicativo esta rodando em {arq} bits, e o driver do Access precisa ter a mesma arquitetura.{detectados} " +
                   $"Instale o Microsoft Access Database Engine ({arq} bits)" +
                   (Environment.Is64BitProcess
                       ? ", ou execute o aplicativo em x86 (32 bits). Para arquivos .mdb em 32 bits, tambem da para usar Microsoft.Jet.OLEDB.4.0."
                       : $". Se o que esta instalado for a versao de {outra} bits, execute o aplicativo em x64 ou instale a versao de {arq} bits.");
        }

        #endregion

        #region Abertura e teste

        public static async Task<DatabaseConnection> OpenAsync(ConnectionProfile profile, CancellationToken ct = default)
        {
            var provider = ProviderCatalog.Get(profile.ProviderKey);
            var connectionString = BuildConnectionString(profile);
            DbConnection? conn = null;
            try
            {
                conn = GetFactory(provider.Key).CreateConnection()
                       ?? throw new GeneratorException($"O driver de {provider.DisplayName} nao criou a conexao.");
                conn.ConnectionString = connectionString;
                await conn.OpenAsync(ct).ConfigureAwait(false);
                return new DatabaseConnection(provider, conn);
            }
            catch (OperationCanceledException)
            {
                conn?.Dispose();
                throw;
            }
            catch (GeneratorException)
            {
                conn?.Dispose();
                throw;
            }
            catch (Exception ex)
            {
                conn?.Dispose();
                throw new GeneratorException($"Falha ao conectar em {provider.DisplayName}: {ex.Message}{DriverHint(provider, ex)}", ex);
            }
        }

        public static async Task<TestResult> TestAsync(ConnectionProfile profile, CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                using var s = await OpenAsync(profile, ct).ConfigureAwait(false);
                string? version = null;
                try { version = s.Connection.ServerVersion; } catch { /* alguns drivers nao expõem */ }
                return new TestResult(true, "Conexao estabelecida.", version, sw.ElapsedMilliseconds);
            }
            catch (GeneratorException ex)
            {
                return new TestResult(false, ex.Message, null, sw.ElapsedMilliseconds);
            }
        }

        #endregion

        #region Execucao (somente leitura)

        private DbCommand CreateCommand(string sql, IReadOnlyList<QueryParam>? ps)
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandTimeout = CommandTimeoutSeconds;
            if (ps is not null)
                foreach (var p in ps)
                {
                    var dp = cmd.CreateParameter();
                    dp.ParameterName = Dialect.PositionalParameters ? "p" + cmd.Parameters.Count : p.Name;
                    dp.Value = p.Value ?? DBNull.Value;
                    cmd.Parameters.Add(dp);
                }
            return cmd;
        }

        /// <summary>Le linha a linha (streaming): memoria constante mesmo com milhoes de registros.</summary>
        public async IAsyncEnumerable<object?[]> QueryAsync(string sql, IReadOnlyList<QueryParam>? ps = null,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            using var cmd = CreateCommand(sql, ps);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                var values = new object?[reader.FieldCount];
                reader.GetValues(values!);
                yield return values;
            }
        }

        public async Task<long> ScalarLongAsync(string sql, IReadOnlyList<QueryParam>? ps = null, CancellationToken ct = default)
        {
            using var cmd = CreateCommand(sql, ps);
            var o = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return o is null || o is DBNull ? 0 : Convert.ToInt64(o);
        }

        #endregion

        #region Schema (antes SchemaReader)

        public async Task<List<TableRef>> ListTablesAsync(CancellationToken ct = default)
        {
            var result = new List<TableRef>();
            var d = Dialect;

            if (d.UsesGetSchema)
            {
                var dt = await Task.Run(() => Connection.GetSchema("Tables"), ct).ConfigureAwait(false);
                foreach (DataRow r in dt.Rows)
                {
                    var type = Convert.ToString(r["TABLE_TYPE"]) ?? "";
                    if (type is "TABLE" or "VIEW")
                        result.Add(new TableRef(null, Convert.ToString(r["TABLE_NAME"]) ?? ""));
                }
                return result.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ToList();
            }

            await foreach (var row in QueryAsync(d.ListTablesSql!, null, ct).ConfigureAwait(false))
            {
                if (d.TablesHaveSchema) result.Add(new TableRef(Convert.ToString(row[0]), Convert.ToString(row[1]) ?? ""));
                else result.Add(new TableRef(null, Convert.ToString(row[0]) ?? ""));
            }
            return result;
        }

        public async Task<List<ColumnRef>> ListColumnsAsync(TableRef table, CancellationToken ct = default)
        {
            var result = new List<ColumnRef>();
            var d = Dialect;

            if (d.UsesGetSchema)
            {
                var dt = await Task.Run(() => Connection.GetSchema("Columns", new string?[] { null, null, table.Name, null }), ct).ConfigureAwait(false);
                foreach (DataRow r in dt.Rows.Cast<DataRow>().OrderBy(r => Convert.ToInt64(r["ORDINAL_POSITION"] is DBNull ? 0 : r["ORDINAL_POSITION"])))
                    result.Add(new ColumnRef(Convert.ToString(r["COLUMN_NAME"]) ?? "", Convert.ToString(r["DATA_TYPE"]) ?? ""));
                return result;
            }

            var ps = d.TablesHaveSchema
                ? new List<QueryParam> { new(d.ParameterMarker(0), table.Schema), new(d.ParameterMarker(1), table.Name) }
                : new List<QueryParam> { new(d.ParameterMarker(0), table.Name) };

            await foreach (var row in QueryAsync(d.ListColumnsSql!, ps, ct).ConfigureAwait(false))
                result.Add(new ColumnRef(Convert.ToString(row[0]) ?? "", Convert.ToString(row[1]) ?? ""));
            return result;
        }

        /// <summary>
        /// Casa o nome da tabela e das colunas do layout com os nomes reais do banco (ignorando maiusculas/minusculas)
        /// e falha ANTES de rodar o SELECT se algo estiver faltando, listando exatamente o que.
        /// </summary>
        public async Task<SchemaMap?> ResolveAsync(string requestedTable, IEnumerable<string> layoutColumns, CancellationToken ct = default)
        {
            var tables = await ListTablesAsync(ct).ConfigureAwait(false);
            if (tables.Count == 0) return null;

            var wanted = requestedTable.Trim();
            var table = tables.FirstOrDefault(t => t.FullName.Equals(wanted, StringComparison.OrdinalIgnoreCase))
                        ?? tables.FirstOrDefault(t => t.Name.Equals(wanted, StringComparison.OrdinalIgnoreCase));
            if (table is null)
            {
                var suggestions = tables
                    .Where(t => t.Name.Contains(wanted, StringComparison.OrdinalIgnoreCase) || wanted.Contains(t.Name, StringComparison.OrdinalIgnoreCase))
                    .Take(5).Select(t => t.FullName).ToList();
                throw new GeneratorException($"A tabela '{wanted}' nao existe nesta conexao." +
                    (suggestions.Count > 0 ? $" Parecidas: {string.Join(", ", suggestions)}." : " Escolha outra na lista de tabelas."));
            }

            var cols = await ListColumnsAsync(table, ct).ConfigureAwait(false);
            if (cols.Count == 0) return new SchemaMap { Table = table, Columns = null };

            var real = cols.ToDictionary(c => c.Name, c => c.Name, StringComparer.OrdinalIgnoreCase);
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var missing = new List<string>();
            foreach (var name in layoutColumns)
            {
                if (real.TryGetValue(name, out var actual)) map[name] = actual;
                else missing.Add(name);
            }
            if (missing.Count > 0)
                throw new GeneratorException($"A tabela '{table.FullName}' nao tem {missing.Count} coluna(s) do layout: {string.Join(", ", missing)}.");

            return new SchemaMap { Table = table, Columns = map };
        }

        #endregion
    }
}