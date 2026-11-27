using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;

using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;

namespace ReferenciaFactory.Banco;

/// <summary>
/// Tudo que muda de um banco para outro na geracao de SQL fica aqui: aspas de identificador,
/// marcador de parametro, limite de linhas e catalogo (tabelas/colunas).
/// Para suportar um banco novo: crie uma classe aqui e registre-a em ProviderCatalog.
/// </summary>
public interface ISqlDialect
{
    string Key { get; }
    /// <summary>Quote sempre (escapando o caractere de fechamento).</summary>
    string Quote(string identifier);
    /// <summary>Usado so quando o esquema nao pode ser lido: bancos que dobram caixa deixam nomes simples sem aspas.</summary>
    string QuoteLoose(string identifier);
    /// <summary>"dbo.Tabela" -> [dbo].[Tabela]</summary>
    string QuoteQualified(string name, bool loose);
    bool PositionalParameters { get; }
    string ParameterMarker(int index);
    string BuildSelect(string columns, string from, string? where, string? orderBy, int? limit);

    /// <summary>SQL do catalogo: devolve (schema, nome) ou so (nome). Vazio quando usa GetSchema (Access).</summary>
    string? ListTablesSql { get; }
    bool TablesHaveSchema { get; }
    /// <summary>Colunas: parametros sao (schema, tabela) ou (tabela). Retorna (nome, tipo).</summary>
    string? ListColumnsSql { get; }
    bool UsesGetSchema { get; }
}

public abstract class SqlDialectBase : ISqlDialect
{
    private static readonly Regex Simple = new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

    public abstract string Key { get; }
    protected virtual string OpenQuote => "\"";
    protected virtual string CloseQuote => "\"";
    /// <summary>True para bancos que transformam nomes sem aspas em MAIUSCULAS/minusculas (PostgreSQL, Oracle, Firebird).</summary>
    protected virtual bool FoldsCase => false;

    public string Quote(string identifier) =>
        OpenQuote + identifier.Replace(CloseQuote, CloseQuote + CloseQuote) + CloseQuote;

    public string QuoteLoose(string identifier) =>
        FoldsCase && Simple.IsMatch(identifier) ? identifier : Quote(identifier);

    public string QuoteQualified(string name, bool loose) =>
        string.Join(".", name.Split('.').Select(p => loose ? QuoteLoose(p.Trim()) : Quote(p.Trim())));

    public virtual bool PositionalParameters => false;
    public virtual string ParameterMarker(int index) => "@p" + index;

    public abstract string BuildSelect(string columns, string from, string? where, string? orderBy, int? limit);

    public virtual string? ListTablesSql => null;
    public virtual bool TablesHaveSchema => false;
    public virtual string? ListColumnsSql => null;
    public virtual bool UsesGetSchema => false;

    protected static string Compose(string head, string columns, string from, string? where, string? orderBy, string? tail = null)
    {
        var sb = new StringBuilder();
        sb.Append("SELECT ").Append(head).Append(columns).Append(" FROM ").Append(from);
        if (!string.IsNullOrWhiteSpace(where)) sb.Append(" WHERE ").Append(where);
        if (!string.IsNullOrWhiteSpace(orderBy)) sb.Append(" ORDER BY ").Append(orderBy);
        if (tail is not null) sb.Append(' ').Append(tail);
        return sb.ToString();
    }
}

public sealed class SqlServerDialect : SqlDialectBase
{
    public override string Key => "sqlserver";
    protected override string OpenQuote => "[";
    protected override string CloseQuote => "]";
    public override string BuildSelect(string c, string f, string? w, string? o, int? limit) =>
        Compose(limit is null ? "" : $"TOP ({limit}) ", c, f, w, o);

    public override string? ListTablesSql =>
        "SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_SCHEMA, TABLE_NAME";
    public override bool TablesHaveSchema => true;
    public override string? ListColumnsSql =>
        "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @p0 AND TABLE_NAME = @p1 ORDER BY ORDINAL_POSITION";
}

public sealed class PostgreSqlDialect : SqlDialectBase
{
    public override string Key => "postgres";
    protected override bool FoldsCase => true;
    public override string BuildSelect(string c, string f, string? w, string? o, int? limit) =>
        Compose("", c, f, w, o, limit is null ? null : $"LIMIT {limit}");

    public override string? ListTablesSql =>
        "SELECT table_schema, table_name FROM information_schema.tables WHERE table_schema NOT IN ('pg_catalog','information_schema') ORDER BY table_schema, table_name";
    public override bool TablesHaveSchema => true;
    public override string? ListColumnsSql =>
        "SELECT column_name, data_type FROM information_schema.columns WHERE table_schema = @p0 AND table_name = @p1 ORDER BY ordinal_position";
}

public sealed class MySqlDialect : SqlDialectBase
{
    public override string Key => "mysql";
    protected override string OpenQuote => "`";
    protected override string CloseQuote => "`";
    public override string BuildSelect(string c, string f, string? w, string? o, int? limit) =>
        Compose("", c, f, w, o, limit is null ? null : $"LIMIT {limit}");

    public override string? ListTablesSql =>
        "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE() ORDER BY TABLE_NAME";
    public override string? ListColumnsSql =>
        "SELECT COLUMN_NAME, DATA_TYPE FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @p0 ORDER BY ORDINAL_POSITION";
}

public sealed class SqliteDialect : SqlDialectBase
{
    public override string Key => "sqlite";
    public override string BuildSelect(string c, string f, string? w, string? o, int? limit) =>
        Compose("", c, f, w, o, limit is null ? null : $"LIMIT {limit}");

    public override string? ListTablesSql =>
        "SELECT name FROM sqlite_master WHERE type IN ('table','view') AND name NOT LIKE 'sqlite_%' ORDER BY name";
    public override string? ListColumnsSql =>
        "SELECT name, type FROM pragma_table_info(@p0)";
}

public sealed class OracleDialect : SqlDialectBase
{
    public override string Key => "oracle";
    protected override bool FoldsCase => true;
    public override string ParameterMarker(int index) => ":p" + index;
    // FETCH FIRST exige Oracle 12c ou superior.
    public override string BuildSelect(string c, string f, string? w, string? o, int? limit) =>
        Compose("", c, f, w, o, limit is null ? null : $"FETCH FIRST {limit} ROWS ONLY");

    public override string? ListTablesSql =>
        "SELECT table_name FROM user_tables UNION SELECT view_name FROM user_views ORDER BY 1";
    public override string? ListColumnsSql =>
        "SELECT column_name, data_type FROM user_tab_columns WHERE table_name = :p0 ORDER BY column_id";
}

public sealed class FirebirdDialect : SqlDialectBase
{
    public override string Key => "firebird";
    protected override bool FoldsCase => true;
    public override string BuildSelect(string c, string f, string? w, string? o, int? limit) =>
        Compose(limit is null ? "" : $"FIRST {limit} ", c, f, w, o);

    public override string? ListTablesSql =>
        "SELECT TRIM(RDB$RELATION_NAME) FROM RDB$RELATIONS WHERE COALESCE(RDB$SYSTEM_FLAG, 0) = 0 ORDER BY 1";
    public override string? ListColumnsSql =>
        "SELECT TRIM(RDB$FIELD_NAME), '' FROM RDB$RELATION_FIELDS WHERE TRIM(RDB$RELATION_NAME) = @p0 ORDER BY RDB$FIELD_POSITION";
}

public sealed class AccessDialect : SqlDialectBase
{
    public override string Key => "access";
    protected override string OpenQuote => "[";
    protected override string CloseQuote => "]";
    public override bool PositionalParameters => true;
    public override string ParameterMarker(int index) => "?";
    public override string BuildSelect(string c, string f, string? w, string? o, int? limit) =>
        Compose(limit is null ? "" : $"TOP {limit} ", c, f, w, o);
    public override bool UsesGetSchema => true;
}
