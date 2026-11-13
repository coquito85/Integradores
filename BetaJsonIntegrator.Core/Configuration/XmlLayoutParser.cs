using System.Xml;
using System.Xml.Linq;
using BetaJsonIntegrator.Core.Validation;

namespace BetaJsonIntegrator.Core.Configuration
{
    public sealed class ParseResult
    {
        public LayoutConfiguration? Layout { get; set; }
        public List<ValidationIssue> Issues { get; } = new();
        public bool Success => Layout != null && Issues.All(i => i.Severity != IssueSeverity.Error);
    }

    /// <summary>
    /// Interpreta o XML de layout (formato legado do SATUBA + extensões source/default/required/...).
    /// Nomes de elementos e atributos são tratados sem diferenciar maiúsculas/minúsculas.
    /// </summary>
    public static class XmlLayoutParser
    {
        public static ParseResult ParseText(string xml)
        {
            var result = new ParseResult();
            try
            {
                return Parse(XmlLayoutReader.LoadText(xml));
            }
            catch (XmlException ex)
            {
                result.Issues.Add(ValidationIssue.Error("XML_INVALID", $"linha {ex.LineNumber}",
                    "XML inválido: " + ex.Message));
                return result;
            }
        }

        public static ParseResult ParseFile(string path)
        {
            var result = new ParseResult();
            try
            {
                return Parse(XmlLayoutReader.LoadFile(path));
            }
            catch (XmlException ex)
            {
                result.Issues.Add(ValidationIssue.Error("XML_INVALID", $"linha {ex.LineNumber}",
                    "XML inválido: " + ex.Message));
                return result;
            }
        }

        public static ParseResult Parse(XDocument doc)
        {
            var result = new ParseResult();
            var root = doc.Root;
            if (root == null)
            {
                result.Issues.Add(ValidationIssue.Error("XML_EMPTY", "", "O XML não possui elemento raiz."));
                return result;
            }

            var layout = new LayoutConfiguration
            {
                Name = Attr(root, "name") ?? "",
                Identifier = Attr(root, "identifier", "id") ?? ""
            };
            if (!Is(root, "layout"))
                result.Issues.Add(ValidationIssue.Warning("ROOT_NOT_LAYOUT", Line(root),
                    $"Elemento raiz '{root.Name.LocalName}' diferente de 'Layout'."));

            // Jsonfields soltos no nível do Layout (fora de <generate>).
            var rootJson = root.Elements().Where(e => Is(e, "jsonfields")).ToList();

            foreach (var gEl in root.Elements().Where(e => Is(e, "generate")))
            {
                var gen = new GenerateConfig { Name = Attr(gEl, "name") ?? "" };

                foreach (var tEl in gEl.Elements().Where(e => Is(e, "table")))
                    gen.Tables.Add(ParseTable(tEl, result));

                var jsonEls = gEl.Elements().Where(e => Is(e, "jsonfields")).ToList();
                if (jsonEls.Count == 0) jsonEls = rootJson;
                foreach (var jEl in jsonEls)
                    gen.JsonDefinitions.Add(ParseJsonFields(jEl, result));

                layout.Generates.Add(gen);
            }

            if (layout.Generates.Count == 0)
                result.Issues.Add(ValidationIssue.Error("NO_GENERATE", "", "Nenhum elemento <generate> encontrado."));

            result.Layout = layout;
            return result;
        }

        private static TableConfig ParseTable(XElement tEl, ParseResult result)
        {
            var table = new TableConfig
            {
                Name = Attr(tEl, "name") ?? "",
                Alias = Attr(tEl, "alias") ?? ""
            };
            if (table.Alias.Length == 0) table.Alias = table.Name;

            foreach (var cEl in tEl.Descendants().Where(e => Is(e, "column")))
            {
                var type = FieldTypes.Normalize(Attr(cEl, "type"));
                if (type == null)
                {
                    result.Issues.Add(ValidationIssue.Error("UNKNOWN_COLUMN_TYPE", $"{table.Alias}.{Attr(cEl, "name")}",
                        $"Tipo de coluna desconhecido: '{Attr(cEl, "type")}' ({Line(cEl)})."));
                    type = FieldTypes.String;
                }
                table.Columns.Add(new ColumnConfig
                {
                    Name = Attr(cEl, "name") ?? "",
                    Type = type,
                    Description = Attr(cEl, "description"),
                    Format = Attr(cEl, "formate", "format"),
                    OrderBy = Bool(Attr(cEl, "orderby", "orderBy"))
                });
            }
            return table;
        }

        private static JsonFieldsConfig ParseJsonFields(XElement jEl, ParseResult result)
        {
            var json = new JsonFieldsConfig { Name = Attr(jEl, "name") ?? "" };
            foreach (var aEl in jEl.Elements().Where(e => Is(e, "refealias", "refalias", "alias")))
            {
                var alias = new AliasConfig { Name = Attr(aEl, "name") ?? "" };
                foreach (var fEl in aEl.Elements().Where(e => Is(e, "field")))
                    alias.Fields.Add(ParseField(fEl, null, result));
                json.Aliases.Add(alias);
            }
            return json;
        }

        private static FieldConfig ParseField(XElement el, FieldConfig? parent, ParseResult result)
        {
            var rawType = Attr(el, "type") ?? "string";
            var field = new FieldConfig
            {
                Name = Attr(el, "name") ?? "",
                RawType = rawType,
                Type = FieldTypes.Normalize(rawType),
                Value = Attr(el, "value"),
                Source = Attr(el, "source", "Columnrefe", "columnref"),
                Default = Attr(el, "default"),
                Format = Attr(el, "formate", "format"),
                Description = Attr(el, "description"),
                Required = Bool(Attr(el, "required")),
                Configurable = Bool(Attr(el, "configurable")),
                ChildAlias = Attr(el, "alias", "from"),
                Key = Attr(el, "key"),
                ParentKey = Attr(el, "parentKey", "parentkey"),
                EmptyDeclared = Bool(Attr(el, "empty")),
                Line = (el as IXmlLineInfo)?.HasLineInfo() == true ? ((IXmlLineInfo)el).LineNumber : 0,
                Parent = parent
            };
            var groupBy = Attr(el, "groupBy", "groupby");
            if (!string.IsNullOrWhiteSpace(groupBy))
                field.GroupBy = groupBy.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (string.IsNullOrWhiteSpace(field.Name))
                result.Issues.Add(ValidationIssue.Error("FIELD_WITHOUT_NAME", Line(el), "Elemento <field> sem atributo name."));

            foreach (var child in el.Elements().Where(e => Is(e, "field")))
                field.Children.Add(ParseField(child, field, result));

            return field;
        }

        // ---- helpers ----
        private static bool Is(XElement e, params string[] names) =>
            names.Any(n => string.Equals(e.Name.LocalName, n, StringComparison.OrdinalIgnoreCase));

        public static string? Attr(XElement e, params string[] names)
        {
            foreach (var n in names)
            {
                var a = e.Attributes().FirstOrDefault(x =>
                    string.Equals(x.Name.LocalName, n, StringComparison.OrdinalIgnoreCase));
                if (a != null) return a.Value;
            }
            return null;
        }

        private static bool Bool(string? s) =>
            s != null && (s.Equals("true", StringComparison.OrdinalIgnoreCase) || s == "1");

        private static string Line(XElement e) =>
            e is IXmlLineInfo li && li.HasLineInfo() ? $"linha {li.LineNumber}" : "";
    }
}
