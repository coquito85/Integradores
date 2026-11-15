namespace BetaJsonIntegrator.Core.Configuration;

/// <summary>Nomes canônicos de tipo (spec. seção 9).</summary>
public static class FieldTypes
{
    public const string String = "string";
    public const string Bool = "bool";
    public const string Decimal = "decimal";
    public const string Integer = "integer";
    public const string DateTime = "datetime";
    public const string Array = "array";
    public const string Object = "object";

    /// <summary>Tipo de campo do JSON. Retorna null se desconhecido.</summary>
    public static string? NormalizeField(string? raw) => raw?.Trim().ToLowerInvariant() switch
    {
        "string" or "text" => String,
        "bool" or "boolean" => Bool,
        "decimal" or "number" => Decimal,
        "integer" or "int" => Integer,
        "datetime" or "date" => DateTime,
        "array" => Array,
        "object" => Object,
        _ => null
    };

    /// <summary>Tipo de coluna de origem; aceita os nomes do XML legado (int, long, money, varchar...).</summary>
    public static string? NormalizeColumn(string? raw) => raw?.Trim().ToLowerInvariant() switch
    {
        null or "" => String,
        "string" or "text" or "varchar" => String,
        "bool" or "boolean" => Bool,
        "decimal" or "double" or "float" or "money" or "currency" or "number" => Decimal,
        "int" or "integer" or "long" or "byte" => Integer,
        "datetime" or "date" or "time" => DateTime,
        _ => null
    };
}

public sealed class LayoutConfiguration
{
    public string Name { get; set; } = "";
    public string Identifier { get; set; } = "";
    public List<GenerateConfig> Generates { get; } = new();
}

public sealed class GenerateConfig
{
    public string Name { get; set; } = "";
    public List<TableConfig> Tables { get; } = new();

    /// <summary>Estruturas <c>Jsonfields</c> associadas a este generate (pelo alias da tabela).</summary>
    public List<JsonDefinition> JsonDefinitions { get; } = new();
}

public sealed class TableConfig
{
    public string Name { get; set; } = "";
    public string Alias { get; set; } = "";
    public List<ColumnConfig> Columns { get; } = new();
}

public sealed class ColumnConfig
{
    public string Name { get; set; } = "";
    public string? RawType { get; set; }

    /// <summary>Tipo canônico (sempre preenchido; desconhecido vira string com aviso).</summary>
    public string Type { get; set; } = FieldTypes.String;

    public bool OrderBy { get; set; }
    public string? Format { get; set; }
    public string? Culture { get; set; }
    public string Description { get; set; } = "";
    public int Line { get; set; }
}

public sealed class JsonDefinition
{
    public string Name { get; set; } = "";
    public List<AliasConfig> Aliases { get; } = new();
}

/// <summary><c>Refealias</c>: vincula uma estrutura JSON a um alias de tabela.</summary>
public sealed class AliasConfig
{
    public string Name { get; set; } = "";
    public int Line { get; set; }
    public List<FieldConfig> Fields { get; } = new();
}

/// <summary>
/// Campo da estrutura JSON. A resolução (literal, coluna, padrão, pendente) NÃO é guardada aqui:
/// fica no <c>MappingPlan</c>, para que o layout e as personalizações permaneçam separados.
/// </summary>
public sealed class FieldConfig
{
    public string Name { get; set; } = "";
    public string? RawType { get; set; }

    /// <summary>Tipo canônico ou null se o tipo do XML for desconhecido.</summary>
    public string? Type { get; set; }

    public string? Value { get; set; }

    /// <summary>Coluna de origem: atributo <c>source</c> ou, no XML legado, <c>Columnrefe</c>.</summary>
    public string? Source { get; set; }

    public bool UsesLegacyColumnAttribute { get; set; }
    public string? Default { get; set; }
    public string? Format { get; set; }
    public string? Culture { get; set; }
    public string Description { get; set; } = "";
    public bool Required { get; set; }
    public bool Configurable { get; set; }
    public int Line { get; set; }

    public FieldConfig? Parent { get; set; }

    /// <summary>Caminho no layout, sem o alias: <c>declaracoes.adquirentes.ni</c>.</summary>
    public string Path { get; set; } = "";

    public List<FieldConfig> Children { get; } = new();

    public bool HasChildren => Children.Count > 0;
    public bool IsContainer => Type is FieldTypes.Array or FieldTypes.Object;
}
