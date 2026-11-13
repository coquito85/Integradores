namespace BetaJsonIntegrator.Core.Configuration
{
    /// <summary>Tipos suportados no XML (já normalizados).</summary>
    public static class FieldTypes
    {
        public const string String = "string";
        public const string Bool = "bool";
        public const string Decimal = "decimal";
        public const string Integer = "integer";
        public const string DateTime = "datetime";
        public const string Array = "array";
        public const string Object = "object";

        public static string? Normalize(string? type)
        {
            if (string.IsNullOrWhiteSpace(type)) return String;
            switch (type.Trim().ToLowerInvariant())
            {
                case "string":
                case "text":
                case "varchar":
                    return String;
                case "bool":
                case "boolean":
                    return Bool;
                case "decimal":
                case "number":
                case "double":
                case "float":
                case "numeric":
                    return Decimal;
                case "integer":
                case "int":
                case "int32":
                case "int64":
                case "long":
                    return Integer;
                case "datetime":
                case "date":
                case "timestamp":
                    return DateTime;
                case "array":
                case "list":
                    return Array;
                case "object":
                    return Object;
                default:
                    return null;
            }
        }
    }

    public sealed class LayoutConfiguration
    {
        public string Name { get; set; } = "";
        public string Identifier { get; set; } = "";
        public List<GenerateConfig> Generates { get; } = new();
    }

    /// <summary>Elemento &lt;generate&gt;: tabelas de origem + definições de JSON.</summary>
    public sealed class GenerateConfig
    {
        public string Name { get; set; } = "";
        public List<TableConfig> Tables { get; } = new();
        public List<JsonFieldsConfig> JsonDefinitions { get; } = new();

        public TableConfig? FindTable(string? alias) =>
            Tables.FirstOrDefault(t => string.Equals(t.Alias, alias, StringComparison.OrdinalIgnoreCase));
    }

    public sealed class TableConfig
    {
        public string Name { get; set; } = "";
        public string Alias { get; set; } = "";
        public List<ColumnConfig> Columns { get; } = new();

        public ColumnConfig? FindColumn(string? name) =>
            Columns.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public sealed class ColumnConfig
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = FieldTypes.String;
        public string? Description { get; set; }
        /// <summary>Atributo "formate" da coluna (dica de formato da origem / saída padrão).</summary>
        public string? Format { get; set; }
        public bool OrderBy { get; set; }
    }

    public sealed class JsonFieldsConfig
    {
        public string Name { get; set; } = "";
        public List<AliasConfig> Aliases { get; } = new();
    }

    /// <summary>Elemento &lt;Refealias&gt;: vincula a estrutura JSON a um alias de origem.</summary>
    public sealed class AliasConfig
    {
        public string Name { get; set; } = "";
        public List<FieldConfig> Fields { get; } = new();
    }

    public sealed class FieldConfig
    {
        public string Name { get; set; } = "";
        /// <summary>Tipo normalizado; null quando o tipo declarado é desconhecido.</summary>
        public string? Type { get; set; } = FieldTypes.String;
        public string RawType { get; set; } = "";

        /// <summary>Atributo value (literal; no XML legado também pode ser nome de coluna).</summary>
        public string? Value { get; set; }
        /// <summary>Atributo source (novo) ou Columnrefe (legado).</summary>
        public string? Source { get; set; }
        public string? Default { get; set; }
        public string? Format { get; set; }
        public string? Description { get; set; }
        public bool Required { get; set; }
        public bool Configurable { get; set; }

        /// <summary>Arrays: alias de outra tabela que origina os elementos (junção).</summary>
        public string? ChildAlias { get; set; }
        /// <summary>Arrays com ChildAlias: coluna da tabela filha.</summary>
        public string? Key { get; set; }
        /// <summary>Arrays com ChildAlias: coluna da tabela pai (padrão = Key).</summary>
        public string? ParentKey { get; set; }
        /// <summary>Arrays de topo: colunas de agrupamento (várias linhas → um elemento).</summary>
        public string[] GroupBy { get; set; } = Array.Empty<string>();
        /// <summary>empty="true": array vazio declarado de forma explícita.</summary>
        public bool EmptyDeclared { get; set; }

        public int Line { get; set; }
        public FieldConfig? Parent { get; set; }
        public List<FieldConfig> Children { get; } = new();

        public bool IsArray => Type == FieldTypes.Array;
        public bool IsObject => Type == FieldTypes.Object;
        public bool IsContainer => IsArray || IsObject;

        /// <summary>Caminho lógico sem índices, ex.: declaracoes.adquirentes.ni</summary>
        public string Path => Parent == null ? Name : Parent.Path + "." + Name;
    }
}
