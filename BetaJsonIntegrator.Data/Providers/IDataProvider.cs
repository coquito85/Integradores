using System.Data;

namespace BetaJsonIntegrator.Data.Providers;

/// <summary>
/// Contrato comum de origem de dados (spec. seção 17): todo provedor devolve um <see cref="DataTable"/>
/// com as colunas pedidas pelo layout. O Core não conhece banco, CSV nem formulários.
/// </summary>
public interface IDataProvider
{
    /// <summary>Nome amigável da origem (usado em logs e relatórios; nunca contém credenciais).</summary>
    string Name { get; }

    DataTable Load(DataRequest request);
}

/// <summary>Coluna solicitada à origem, derivada do <c>&lt;Column&gt;</c> do XML.</summary>
public sealed class SourceColumn
{
    public SourceColumn(string name, bool orderBy = false)
    {
        Name = name;
        OrderBy = orderBy;
    }

    public string Name { get; }
    public bool OrderBy { get; }
}

/// <summary>Filtro de igualdade (coluna = valor). Sempre enviado como parâmetro, nunca concatenado ao SQL.</summary>
public sealed class DataFilter
{
    public DataFilter(string column, string value)
    {
        Column = column;
        Value = value;
    }

    public string Column { get; }
    public string Value { get; }
}

/// <summary>O que o layout pede à origem.</summary>
public sealed class DataRequest
{
    /// <summary>Tabela lógica (<c>table/@name</c>). Ignorada por provedores de arquivo.</summary>
    public string TableName { get; init; } = "";

    public IReadOnlyList<SourceColumn> Columns { get; init; } = Array.Empty<SourceColumn>();

    public IReadOnlyList<DataFilter> Filters { get; init; } = Array.Empty<DataFilter>();
}
