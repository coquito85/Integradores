using System.Xml.Linq;
using BetaJsonIntegrator.Core.Configuration;

namespace BetaJsonIntegrator.Core.Mapping;

public static class CustomizationModes
{
    public const string Fixed = "fixed";
    public const string Column = "column";
    public const string Default = "default";
}

/// <summary>Personalização de um campo (spec. seção 7.1).</summary>
public sealed class Customization
{
    public string Name { get; set; } = "";

    /// <summary>Caminho completo (ex.: <c>declaracoes.codigoIbge</c>). Se vazio, vale para qualquer campo com este nome.</summary>
    public string? Path { get; set; }

    public string? Type { get; set; }
    public string Mode { get; set; } = CustomizationModes.Fixed;
    public string? Value { get; set; }
    public string? Source { get; set; }

    /// <summary>Se false, só preenche campos pendentes; se true, também substitui o que o XML já define.</summary>
    public bool Override { get; set; }

    public string Key => string.IsNullOrEmpty(Path) ? Name : Path;
}

/// <summary>
/// Conteúdo de <c>customizations.xml</c>, mantido separado do layout original para facilitar
/// atualização e auditoria. Associado ao identificador e à versão do layout.
/// </summary>
public sealed class CustomizationSet
{
    public string LayoutIdentifier { get; set; } = "";
    public int Version { get; set; } = 1;
    public List<Customization> Items { get; } = new();

    public void Upsert(Customization item)
    {
        var index = Items.FindIndex(c => string.Equals(c.Key, item.Key, StringComparison.OrdinalIgnoreCase));
        if (index >= 0) Items[index] = item;
        else Items.Add(item);
    }

    /// <summary>Caminho completo tem precedência sobre o nome simples.</summary>
    public Customization? Find(FieldConfig field) =>
        Items.FirstOrDefault(c => !string.IsNullOrEmpty(c.Path) &&
                                  string.Equals(c.Path, field.Path, StringComparison.OrdinalIgnoreCase))
        ?? Items.FirstOrDefault(c => string.IsNullOrEmpty(c.Path) &&
                                     string.Equals(c.Name, field.Name, StringComparison.OrdinalIgnoreCase));

    public static CustomizationSet Parse(string xml)
    {
        XDocument doc;
        try
        {
            doc = XmlSafe.Load(xml);
        }
        catch (System.Xml.XmlException ex)
        {
            throw new InvalidDataException("customizations.xml inválido: " + ex.Message, ex);
        }

        var root = doc.Root;
        if (root == null || !string.Equals(root.Name.LocalName, "Customizations", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("customizations.xml: o elemento raiz deve ser <Customizations>.");

        var set = new CustomizationSet
        {
            LayoutIdentifier = Get(root, "layout") ?? "",
            Version = int.TryParse(Get(root, "version"), out var v) && v > 0 ? v : 1
        };

        foreach (var el in root.Elements().Where(e => string.Equals(e.Name.LocalName, "field", StringComparison.OrdinalIgnoreCase)))
        {
            var name = (Get(el, "name") ?? "").Trim();
            if (name.Length == 0) continue;

            var mode = (Get(el, "mode") ?? CustomizationModes.Fixed).Trim().ToLowerInvariant();
            if (mode != CustomizationModes.Column && mode != CustomizationModes.Default)
                mode = CustomizationModes.Fixed;

            var path = Get(el, "path");
            set.Items.Add(new Customization
            {
                Name = name,
                Path = string.IsNullOrWhiteSpace(path) ? null : path.Trim(),
                Type = Get(el, "type"),
                Mode = mode,
                Value = Get(el, "value"),
                Source = Get(el, "source"),
                Override = string.Equals(Get(el, "override"), "true", StringComparison.OrdinalIgnoreCase)
            });
        }
        return set;
    }

    public string ToXml()
    {
        var root = new XElement("Customizations",
            new XAttribute("layout", LayoutIdentifier),
            new XAttribute("version", Version));

        foreach (var c in Items)
        {
            var el = new XElement("field", new XAttribute("name", c.Name));
            if (!string.IsNullOrEmpty(c.Path)) el.Add(new XAttribute("path", c.Path));
            if (!string.IsNullOrEmpty(c.Type)) el.Add(new XAttribute("type", c.Type));
            if (c.Mode == CustomizationModes.Column)
            {
                el.Add(new XAttribute("source", c.Source ?? ""));
            }
            else
            {
                el.Add(new XAttribute("value", c.Value ?? ""));
            }
            el.Add(new XAttribute("mode", c.Mode));
            if (c.Override) el.Add(new XAttribute("override", "true"));
            root.Add(el);
        }

        return "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine + root;
    }

    private static string? Get(XElement el, string name) =>
        el.Attributes().FirstOrDefault(a => string.Equals(a.Name.LocalName, name, StringComparison.OrdinalIgnoreCase))?.Value;
}
