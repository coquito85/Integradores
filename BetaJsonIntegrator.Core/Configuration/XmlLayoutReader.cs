using System.Xml;
using System.Xml.Linq;

namespace BetaJsonIntegrator.Core.Configuration
{
    /// <summary>
    /// Leitura segura de XML: DTD proibido e sem resolução de entidades externas (proteção XXE).
    /// </summary>
    public static class XmlLayoutReader
    {
        private static XmlReaderSettings CreateSettings() => new()
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            MaxCharactersFromEntities = 0,
            MaxCharactersInDocument = 50_000_000
        };

        public static XDocument LoadFile(string path)
        {
            using var stream = File.OpenRead(path);
            using var reader = XmlReader.Create(stream, CreateSettings());
            return XDocument.Load(reader, LoadOptions.SetLineInfo);
        }

        public static XDocument LoadText(string xml)
        {
            using var sr = new StringReader(xml);
            using var reader = XmlReader.Create(sr, CreateSettings());
            return XDocument.Load(reader, LoadOptions.SetLineInfo);
        }
    }
}
