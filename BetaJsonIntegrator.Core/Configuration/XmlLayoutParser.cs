using System.Xml;
using System.Xml.Linq;
using BetaJsonIntegrator.Core.Validation;

namespace BetaJsonIntegrator.Core.Configuration;

public sealed class ParseResult
{
    public ParseResult(LayoutConfiguration? layout, IReadOnlyList<ValidationIssue> issues)
    {
        Layout = layout;
        Issues = issues;
    }

    /// <summary>Null apenas quando o XML é malformado ou a raiz não é &lt;Layout&gt;.</summary>
    public LayoutConfiguration? Layout { get; }

    public IReadOnlyList<ValidationIssue> Issues { get; }
}

/// <summary>
/// Interpreta o XML do layout (spec. seção 5). Preserva a estrutura legada do SATUBA:
/// elementos <c>generate/table/selection/Column</c>, <c>Jsonfields/Refealias/field</c> e os atributos
/// <c>Columnrefe</c> e <c>formate</c>. Atributos novos: <c>source</c>, <c>default</c>, <c>required</c>,
/// <c>configurable</c>, <c>format</c>, <c>culture</c>. Nomes de elementos e atributos não diferenciam maiúsculas.
/// </summary>
public static class XmlLayoutParser
{
    public static ParseResult ParseFile(string path) => ParseText(File.ReadAllText(path));

    public static ParseResult ParseText(string xml)
    {
        var issues = new List<ValidationIssue>();

        XDocument doc;
        try
        {
            doc = XmlSafe.Load(xml ?? "");
        }
        catch (XmlException ex)
        {
            issues.Add(ValidationIssue.Error("XML_MALFORMED", $"linha {ex.LineNumber}",
                "XML inválido: " + ex.Message));
            return new ParseResult(null, issues);
        }

        var root = doc.Root;
        if (root == null || !Is(root, "Layout"))
        {
            issues.Add(ValidationIssue.Error("XML_ROOT", "", "O elemento raiz do arquivo deve ser <Layout>."));
            return new ParseResult(null, issues);
        }

        var layout = new LayoutConfiguration
        {
            Name = Attr(root, "name") ?? "",
            Identifier = Attr(root, "identifier") ?? ""
        };
        if (layout.Identifier.Length == 0)
            issues.Add(ValidationIssue.Warning("CFG_NO_IDENTIFIER", "Layout",
                "O elemento <Layout> não possui o atributo 'identifier'."));

        var rootDefinitions = new List<JsonDefinition>();
        foreach (var el in root.Elements())
        {
            if (Is(el, "generate")) layout.Generates.Add(ParseGenerate(el, issues));
            else if (Is(el, "Jsonfields")) rootDefinitions.Add(ParseJsonfields(el, issues));
        }

        if (layout.Generates.Count == 0)
            issues.Add(ValidationIssue.Error("CFG_NO_GENERATE", "Layout", "O layout não possui nenhum <generate>."));

        // Jsonfields declarado ao lado do generate (estrutura do SATUBA) é associado pelo alias da tabela.
        foreach (var def in rootDefinitions)
        {
            var target = layout.Generates.FirstOrDefault(g =>
                             g.Tables.Any(t => def.Aliases.Any(a => Eq(a.Name, t.Alias))))
                         ?? layout.Generates.FirstOrDefault();
            target?.JsonDefinitions.Add(def);
        }

        if (layout.Generates.Count > 0 && layout.Generates.All(g => g.JsonDefinitions.Count == 0))
            issues.Add(ValidationIssue.Error("CFG_NO_JSONFIELDS", "Layout",
                "O layout não possui a estrutura <Jsonfields> que define o JSON de saída."));

        return new ParseResult(layout, issues);
    }

    // ------------------------------------------------------------------ generate / table / column
    private static GenerateConfig ParseGenerate(XElement el, List<ValidationIssue> issues)
    {
        var gen = new GenerateConfig { Name = Attr(el, "name") ?? "" };
        var ctx = $"generate[{gen.Name}]";

        foreach (var child in el.Elements())
        {
            if (Is(child, "table")) gen.Tables.Add(ParseTable(child, ctx, issues));
            else if (Is(child, "Jsonfields")) gen.JsonDefinitions.Add(ParseJsonfields(child, issues));
        }
        return gen;
    }

    private static TableConfig ParseTable(XElement el, string parentCtx, List<ValidationIssue> issues)
    {
        var table = new TableConfig
        {
            Name = (Attr(el, "name") ?? "").Trim(),
            Alias = (Attr(el, "alias") ?? "").Trim()
        };
        var ctx = $"{parentCtx}/table[{table.Alias}]";

        foreach (var holder in el.Elements())
        {
            if (Is(holder, "selection"))
            {
                foreach (var c in holder.Elements().Where(x => Is(x, "Column")))
                    table.Columns.Add(ParseColumn(c, ctx, issues));
            }
            else if (Is(holder, "Column"))
            {
                table.Columns.Add(ParseColumn(holder, ctx, issues));
            }
        }
        return table;
    }

    private static ColumnConfig ParseColumn(XElement el, string tableCtx, List<ValidationIssue> issues)
    {
        var col = new ColumnConfig
        {
            Name = (Attr(el, "name") ?? "").Trim(),
            RawType = Attr(el, "type"),
            OrderBy = IsTrue(Attr(el, "orderby")),
            Description = Attr(el, "description") ?? "",
            Culture = Attr(el, "culture"),
            Line = Line(el)
        };
        var path = $"{tableCtx}/Column[{col.Name}]";

        if (col.Name.Length == 0)
            issues.Add(ValidationIssue.Error("CFG_COLUMN_NAME", path, $"Coluna sem 'name' (linha {col.Line})."));

        var type = FieldTypes.NormalizeColumn(col.RawType);
        if (type == null)
        {
            issues.Add(ValidationIssue.Warning("CFG_COLUMN_TYPE", path,
                $"Tipo de coluna '{col.RawType}' desconhecido (linha {col.Line}); tratado como string."));
            type = FieldTypes.String;
        }
        col.Type = type;
        col.Format = NormalizeFormat(Attr(el, "formate", "format"), type, path, col.Line, issues);
        return col;
    }

    // ------------------------------------------------------------------ Jsonfields / Refealias / field
    private static JsonDefinition ParseJsonfields(XElement el, List<ValidationIssue> issues)
    {
        var def = new JsonDefinition { Name = Attr(el, "name") ?? "" };
        foreach (var a in el.Elements().Where(x => Is(x, "Refealias")))
        {
            var alias = new AliasConfig { Name = (Attr(a, "name") ?? "").Trim(), Line = Line(a) };
            foreach (var f in a.Elements().Where(x => Is(x, "field")))
                alias.Fields.Add(ParseField(f, null, issues));
            def.Aliases.Add(alias);
        }
        return def;
    }

    private static FieldConfig ParseField(XElement el, FieldConfig? parent, List<ValidationIssue> issues)
    {
        var f = new FieldConfig
        {
            Parent = parent,
            Line = Line(el),
            Name = (Attr(el, "name") ?? "").Trim(),
            RawType = Attr(el, "type"),
            Value = Attr(el, "value"),
            Default = Attr(el, "default"),
            Description = Attr(el, "description") ?? "",
            Culture = Attr(el, "culture"),
            Required = IsTrue(Attr(el, "required")),
            Configurable = IsTrue(Attr(el, "configurable"))
        };

        var newSource = Attr(el, "source");
        var legacySource = Attr(el, "Columnrefe", "ColumnRef", "ColumnReference");
        if (!string.IsNullOrWhiteSpace(newSource))
        {
            f.Source = newSource.Trim();
        }
        else if (!string.IsNullOrWhiteSpace(legacySource))
        {
            f.Source = legacySource.Trim();
            f.UsesLegacyColumnAttribute = true;
        }

        var shownName = f.Name.Length == 0 ? "?" : f.Name;
        f.Path = parent == null ? shownName : parent.Path + "." + shownName;

        if (f.Name.Length == 0)
            issues.Add(ValidationIssue.Error("CFG_FIELD_NAME", f.Path, $"Campo sem 'name' (linha {f.Line})."));

        if (string.IsNullOrWhiteSpace(f.RawType))
        {
            issues.Add(ValidationIssue.Error("CFG_FIELD_TYPE", f.Path, $"Campo sem 'type' (linha {f.Line})."));
        }
        else
        {
            f.Type = FieldTypes.NormalizeField(f.RawType);
            if (f.Type == null)
                issues.Add(ValidationIssue.Error("CFG_FIELD_TYPE", f.Path,
                    $"Tipo '{f.RawType}' não suportado (linha {f.Line}). Use string, bool, decimal, integer, datetime, array ou object."));
        }

        f.Format = NormalizeFormat(Attr(el, "formate", "format"), f.Type, f.Path, f.Line, issues);

        foreach (var child in el.Elements().Where(x => Is(x, "field")))
            f.Children.Add(ParseField(child, f, issues));

        if (f.HasChildren && f.Type != null && !f.IsContainer)
            issues.Add(ValidationIssue.Error("CFG_FIELD_CHILDREN", f.Path,
                $"Campo do tipo '{f.Type}' não pode ter campos filhos (linha {f.Line}); use array ou object."));

        return f;
    }

    // ------------------------------------------------------------------ helpers
    /// <summary>
    /// O XML legado traz <c>yyyy-mm-dd</c>. Em .NET "mm" é minuto, o que geraria datas como 2026-00-15;
    /// quando não há hora no formato, "mm" é lido como mês (MM) e o ajuste é avisado.
    /// </summary>
    private static string? NormalizeFormat(string? format, string? type, string path, int line, List<ValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(format)) return null;
        var fmt = format.Trim();
        if (type == FieldTypes.DateTime &&
            fmt.Contains("mm", StringComparison.Ordinal) &&
            fmt.IndexOfAny(new[] { 'H', 'h', 's', ':' }) < 0)
        {
            var fixedFormat = fmt.Replace("mm", "MM", StringComparison.Ordinal);
            issues.Add(ValidationIssue.Warning("CFG_FORMAT_MM", path,
                $"Formato '{fmt}' (linha {line}) usa 'mm' (minutos em .NET); foi interpretado como '{fixedFormat}'. Corrija o XML para evitar o aviso."));
            return fixedFormat;
        }
        return fmt;
    }

    private static bool Is(XElement el, string name) =>
        string.Equals(el.Name.LocalName, name, StringComparison.OrdinalIgnoreCase);

    private static bool Eq(string? a, string? b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    private static bool IsTrue(string? value) =>
        value != null && (value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase) || value.Trim() == "1");

    private static string? Attr(XElement el, params string[] names)
    {
        foreach (var name in names)
        {
            var attr = el.Attributes().FirstOrDefault(a =>
                string.Equals(a.Name.LocalName, name, StringComparison.OrdinalIgnoreCase));
            if (attr != null) return attr.Value;
        }
        return null;
    }

    private static int Line(XObject node) =>
        node is IXmlLineInfo info && info.HasLineInfo() ? info.LineNumber : 0;
}
