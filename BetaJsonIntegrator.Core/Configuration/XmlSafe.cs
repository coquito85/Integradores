using System.Xml;
using System.Xml.Linq;

namespace BetaJsonIntegrator.Core.Configuration;

/// <summary>Leitura de XML sem DTD e sem resolução de entidades externas (spec. seção 14: proteção contra XXE).</summary>
internal static class XmlSafe
{
    public static XDocument Load(string xml)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true
        };

        using var text = new StringReader(xml.TrimStart('\uFEFF'));
        using var reader = XmlReader.Create(text, settings);
        return XDocument.Load(reader, LoadOptions.SetLineInfo);
    }
}
