
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
    /// Leitura centralizada dos XMLs de configuracao (UiConfig.xml e Regras.xml).
    /// Esta camada so devolve texto organizado (Entid_*): quem usa converte para cor, fonte, data etc.
    /// </summary>
    public partial class XmlHelper
    {
        private static XDocument CarregarSeguro(string conteudoXml)
        {
            if (string.IsNullOrWhiteSpace(conteudoXml))
                throw new InvalidDataException("XML vazio.");

            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,   // sem DTD/entidades externas
                XmlResolver = null
            };

            using (var texto = new StringReader(conteudoXml.TrimStart('\uFEFF', ' ', '\t', '\r', '\n')))
            using (var leitor = XmlReader.Create(texto, settings))
                return XDocument.Load(leitor);
        }

        private static Entid_Atributos LerAtributos(XElement elemento, Entid_Atributos destino)
        {
            foreach (XAttribute a in elemento.Attributes())
            {
                destino.Valores[a.Name.LocalName] = a.Value;
                if (string.Equals(a.Name.LocalName, "nome", StringComparison.OrdinalIgnoreCase))
                    destino.Nome = a.Value;
            }
            return destino;
        }

        private static IEnumerable<XElement> Filhos(XElement pai, string nome)
        {
            return pai == null ? Enumerable.Empty<XElement>() : pai.Elements(nome);
        }

        /// <summary>Le UiConfig.xml: Tema (Cores, Fontes, Metricas) e Janelas (cada uma com seus Controles).</summary>
        public static Entid_UiConfigDados LerUiConfig(string conteudoXml)
        {
            XElement raiz = CarregarSeguro(conteudoXml).Root;
            if (raiz == null) throw new InvalidDataException("XML sem elemento raiz.");

            var dados = new Entid_UiConfigDados { Versao = (string)raiz.Attribute("versao") };

            XElement tema = raiz.Element("Tema");
            foreach (XElement e in Filhos(tema == null ? null : tema.Element("Cores"), "Cor"))
                dados.Cores.Add(LerAtributos(e, new Entid_Atributos()));
            foreach (XElement e in Filhos(tema == null ? null : tema.Element("Fontes"), "Fonte"))
                dados.Fontes.Add(LerAtributos(e, new Entid_Atributos()));
            foreach (XElement e in Filhos(tema == null ? null : tema.Element("Metricas"), "Metrica"))
                dados.Metricas.Add(LerAtributos(e, new Entid_Atributos()));

            foreach (XElement j in Filhos(raiz.Element("Janelas"), "Janela"))
            {
                var janela = new Entid_JanelaDados();
                LerAtributos(j, janela);
                foreach (XElement c in j.Elements("Controle"))
                    janela.Controles.Add(LerAtributos(c, new Entid_Atributos()));
                dados.Janelas.Add(janela);
            }

            return dados;
        }

        /// <summary>Le Regras.xml: Licenca, Versao e Bloqueios/Funcionalidade.</summary>
        public static Entid_RegrasDados LerRegras(string conteudoXml)
        {
            XElement raiz = CarregarSeguro(conteudoXml).Root;
            if (raiz == null) throw new InvalidDataException("XML sem elemento raiz.");

            var dados = new Entid_RegrasDados();

            XElement licenca = raiz.Element("Licenca");
            if (licenca != null) dados.Licenca = LerAtributos(licenca, new Entid_Atributos());

            XElement versao = raiz.Element("Versao");
            if (versao != null) dados.Versao = LerAtributos(versao, new Entid_Atributos());

            foreach (XElement f in Filhos(raiz.Element("Bloqueios"), "Funcionalidade"))
                dados.Bloqueios.Add(LerAtributos(f, new Entid_Atributos()));

            return dados;
        }
    }
}
