using System.Xml.Linq;
using BetaJsonIntegrator.Core.Configuration;

namespace BetaJsonIntegrator.Core.Mapping
{
    public static class CustomizationModes
    {
        public const string Fixed = "fixed";
        public const string Column = "column";
        public const string Default = "default";
    }

    /// <summary>Uma personalização salva fora do XML do layout (customizations.xml).</summary>
    public sealed class Customization
    {
        public string Name { get; set; } = "";
        /// <summary>Caminho lógico opcional (ex.: declaracoes.adquirentes.ni). Tem precedência sobre o nome.</summary>
        public string? Path { get; set; }
        public string? Type { get; set; }
        public string Mode { get; set; } = CustomizationModes.Fixed;
        public string? Value { get; set; }
        public string? Source { get; set; }
        /// <summary>Quando true, vale mesmo para campos já configurados no XML do layout.</summary>
        public bool Override { get; set; }
    }

    public sealed class CustomizationSet
    {
        public string LayoutIdentifier { get; set; } = "";
        public int Version { get; set; } = 1;
        public List<Customization> Items { get; } = new();

        public Customization? Find(FieldConfig field)
        {
            var byPath = Items.FirstOrDefault(c => !string.IsNullOrEmpty(c.Path) &&
                string.Equals(c.Path, field.Path, StringComparison.OrdinalIgnoreCase));
            if (byPath != null) return byPath;
            return Items.FirstOrDefault(c => string.IsNullOrEmpty(c.Path) &&
                string.Equals(c.Name, field.Name, StringComparison.OrdinalIgnoreCase));
        }

        public void Upsert(Customization item)
        {
            var existing = Items.FirstOrDefault(c =>
                string.Equals(c.Path ?? "", item.Path ?? "", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.Name, item.Name, StringComparison.OrdinalIgnoreCase));
            if (existing != null) Items.Remove(existing);
            Items.Add(item);
        }

        public static CustomizationSet Load(string path)
        {
            var set = new CustomizationSet();
            if (!File.Exists(path)) return set;

            var doc = XmlLayoutReader.LoadFile(path);
            var root = doc.Root;
            if (root == null) return set;

            set.LayoutIdentifier = XmlLayoutParser.Attr(root, "layout") ?? "";
            if (int.TryParse(XmlLayoutParser.Attr(root, "version"), out var v)) set.Version = v;

            foreach (var el in root.Elements().Where(e => e.Name.LocalName.Equals("field", StringComparison.OrdinalIgnoreCase)))
            {
                set.Items.Add(new Customization
                {
                    Name = XmlLayoutParser.Attr(el, "name") ?? "",
                    Path = XmlLayoutParser.Attr(el, "path"),
                    Type = XmlLayoutParser.Attr(el, "type"),
                    Mode = (XmlLayoutParser.Attr(el, "mode") ?? CustomizationModes.Fixed).ToLowerInvariant(),
                    Value = XmlLayoutParser.Attr(el, "value"),
                    Source = XmlLayoutParser.Attr(el, "source"),
                    Override = string.Equals(XmlLayoutParser.Attr(el, "override"), "true", StringComparison.OrdinalIgnoreCase)
                });
            }
            return set;
        }

        public void Save(string path)
        {
            var root = new XElement("Customizations",
                new XAttribute("layout", LayoutIdentifier),
                new XAttribute("version", Version));

            foreach (var c in Items.OrderBy(i => i.Path ?? i.Name, StringComparer.OrdinalIgnoreCase))
            {
                var el = new XElement("field", new XAttribute("name", c.Name));
                if (!string.IsNullOrEmpty(c.Path)) el.Add(new XAttribute("path", c.Path));
                if (!string.IsNullOrEmpty(c.Type)) el.Add(new XAttribute("type", c.Type));
                if (c.Value != null) el.Add(new XAttribute("value", c.Value));
                if (!string.IsNullOrEmpty(c.Source)) el.Add(new XAttribute("source", c.Source));
                el.Add(new XAttribute("mode", c.Mode));
                if (c.Override) el.Add(new XAttribute("override", "true"));
                root.Add(el);
            }

            new XDocument(new XDeclaration("1.0", "utf-8", null), root).Save(path);
        }
    }
}
