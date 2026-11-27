
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;

namespace ReferenciaFactory.Banco;

public sealed record QueryParam(string Name, object? Value);

public sealed record TestResult(bool Ok, string Message, string? ServerVersion, long ElapsedMs);

/// <summary>Uma conexao aberta + dialeto. O gerador so executa SELECT.</summary>
public sealed class DbSession : IDisposable
{
    public ProviderInfo Provider { get; }
    public ISqlDialect Dialect => Provider.Dialect;
    public DbConnection Connection { get; }
    public int CommandTimeoutSeconds { get; set; } = 300;

    private DbSession(ProviderInfo provider, DbConnection connection)
    {
        Provider = provider;
        Connection = connection;
    }

    public static async Task<DbSession> OpenAsync(ConnectionProfile profile, CancellationToken ct = default)
    {
        var provider = ProviderCatalog.Get(profile.ProviderKey);
        var connectionString = ProviderCatalog.BuildConnectionString(profile);
        DbConnection? conn = null;
        try
        {
            conn = provider.Factory().CreateConnection()
                   ?? throw new GeneratorException($"O driver de {provider.DisplayName} nao criou a conexao.");
            conn.ConnectionString = connectionString;
            await conn.OpenAsync(ct).ConfigureAwait(false);
            return new DbSession(provider, conn);
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

        var achados = OleDbProviders.Instalados().Where(n => n.Contains("ACE.OLEDB", StringComparison.OrdinalIgnoreCase) || n.Contains("Jet.OLEDB", StringComparison.OrdinalIgnoreCase)).ToList();
        var detectados = achados.Count > 0
            ? $" Providers detectados neste aplicativo: {string.Join(", ", achados)} (escolha um deles no campo Provider OLE DB)."
            : " Nenhum provider do Access foi detectado neste aplicativo.";

        return $"{Environment.NewLine}Dica: o aplicativo esta rodando em {arq} bits, e o driver do Access precisa ter a mesma arquitetura.{detectados} " +
               $"Instale o Microsoft Access Database Engine ({arq} bits)" +
               (Environment.Is64BitProcess
                   ? ", ou execute o aplicativo em x86 (32 bits). Para arquivos .mdb em 32 bits, tambem da para usar Microsoft.Jet.OLEDB.4.0."
                   : $". Se o que esta instalado for a versao de {outra} bits, execute o aplicativo em x64 ou instale a versao de {arq} bits.");
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

    public void Dispose() => Connection.Dispose();
}