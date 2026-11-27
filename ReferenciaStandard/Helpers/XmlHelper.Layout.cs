
using ReferenciaStandard.Entidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace ReferenciaStandard.Helpers
{
    /// <summary>
    /// Le o XML de layout (formato SATUBA_DOI2026): Layout &gt; generate &gt; table &gt; selection &gt; Column
    /// e Layout &gt; Jsonfields &gt; Refealias &gt; field (recursivo). Nomes de elementos/atributos sao case-insensitive.
    /// So interpreta o XML; a conferencia das regras (LayoutValidator) fica em ReferenciaFactory.
    /// </summary>
    public partial class XmlHelper
    {
        public static LayoutDefinition LerLayout(string xml, string filePath, string source)
        {
            XDocument doc;
            try
            {
                doc = CarregarSeguro(xml);
            }
            catch (XmlException ex)
            {
                throw new GeneratorException("XML invalido em '" + Path.GetFileName(filePath) + "': " + ex.Message, ex);
            }
            catch (InvalidDataException ex)
            {
                throw new GeneratorException("XML invalido em '" + Path.GetFileName(filePath) + "': " + ex.Message, ex);
            }

            XElement root = doc.Root ?? throw new GeneratorException("XML sem elemento raiz.");
            if (!LayoutIs(root, "Layout"))
                throw new GeneratorException("Elemento raiz deveria ser <Layout>, mas e <" + root.Name.LocalName + ">.");

            string id = LayoutAttr(root, "identifier");
            if (string.IsNullOrWhiteSpace(id))
                throw new GeneratorException("O <Layout> precisa do atributo identifier (codigo do layout).");

            XElement generate = LayoutEls(root, "generate").FirstOrDefault();
            XElement table = generate == null ? null : LayoutEls(generate, "table").FirstOrDefault();
            if (generate == null || table == null)
                throw new GeneratorException("Layout sem <generate><table>.");

            string idLayout = id.Trim();
            var layout = new LayoutDefinition
            {
                Id = idLayout,
                Name = LayoutAttr(root, "name") ?? idLayout,
                GenerateName = LayoutAttr(generate, "name"),
                FilePath = filePath,
                Source = source,
                TableName = (LayoutAttr(table, "name") ?? "").Trim(),
                TableAlias = (LayoutAttr(table, "alias") ?? "t").Trim(),
                Xml = xml
            };

            foreach (XElement sel in LayoutEls(table, "selection"))
                foreach (XElement col in LayoutEls(sel, "Column"))
                {
                    string name = LayoutAttr(col, "name");
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    layout.Columns.Add(new LayoutColumn
                    {
                        Name = name.Trim(),
                        Type = (LayoutAttr(col, "type") ?? "string").Trim().ToLowerInvariant(),
                        Format = LayoutAttr(col, "formate") ?? LayoutAttr(col, "format"),
                        Description = LayoutAttr(col, "description"),
                        OrderBy = string.Equals(LayoutAttr(col, "orderby"), "true", StringComparison.OrdinalIgnoreCase)
                    });
                }

            XElement jsonfields = LayoutEls(root, "Jsonfields").FirstOrDefault();
            if (jsonfields != null)
            {
                XElement refe = LayoutEls(jsonfields, "Refealias")
                                     .FirstOrDefault(r => string.Equals(LayoutAttr(r, "name"), layout.TableAlias, StringComparison.OrdinalIgnoreCase))
                                 ?? LayoutEls(jsonfields, "Refealias").FirstOrDefault();
                if (refe != null)
                {
                    List<LayoutField> tops = LayoutEls(refe, "field").Select(LerCampoLayout).ToList();
                    layout.RootField = tops.FirstOrDefault(f => f.Type == "array" && f.Children.Count > 0) ?? tops.FirstOrDefault();
                }
            }

            return layout;
        }

        private static LayoutField LerCampoLayout(XElement e)
        {
            var f = new LayoutField
            {
                Name = LayoutAttr(e, "name") ?? "",
                Type = (LayoutAttr(e, "type") ?? "string").Trim().ToLowerInvariant(),
                Value = LayoutAttr(e, "value"),
                ColumnRef = LayoutNullIfEmpty(LayoutAttr(e, "Columnrefe")),
                Format = LayoutAttr(e, "formate") ?? LayoutAttr(e, "format")
            };
            foreach (XElement c in LayoutEls(e, "field")) f.Children.Add(LerCampoLayout(c));
            return f;
        }

        private static string LayoutNullIfEmpty(string s) { return string.IsNullOrWhiteSpace(s) ? null : s.Trim(); }

        private static bool LayoutIs(XElement e, string name)
        {
            return string.Equals(e.Name.LocalName, name, StringComparison.OrdinalIgnoreCase);
        }

        private static IEnumerable<XElement> LayoutEls(XElement parent, string name)
        {
            return parent.Elements().Where(e => LayoutIs(e, name));
        }

        private static string LayoutAttr(XElement e, string name)
        {
            XAttribute a = e.Attributes().FirstOrDefault(x => string.Equals(x.Name.LocalName, name, StringComparison.OrdinalIgnoreCase));
            return a == null ? null : a.Value;
        }
    }
}
