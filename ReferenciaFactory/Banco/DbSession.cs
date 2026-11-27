using System.Data;
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
            throw new GeneratorException($"Falha ao conectar em {provider.DisplayName}: {ex.Message}", ex);
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
