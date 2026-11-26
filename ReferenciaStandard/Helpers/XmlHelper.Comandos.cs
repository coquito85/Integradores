using ReferenciaStandard.Entidades;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ReferenciaStandard.Helpers
{
    /// <summary>Leitura de Comandos.xml (menu + barra padrao) e Acoes.xml (catalogo de acoes).</summary>
    public partial class XmlHelper
    {
        public static Entid_ComandosDados LerComandos(string conteudoXml)
        {
            XElement raiz = CarregarSeguro(conteudoXml).Root;
            if (raiz == null) throw new InvalidDataException("XML sem elemento raiz.");

            var dados = new Entid_ComandosDados { Versao = (string)raiz.Attribute("versao") };

            foreach (XElement m in raiz.Elements("Menu"))
                dados.Menus.Add(LerNo(m));

            XElement barra = raiz.Element("Barra");
            if (barra != null)
                foreach (XElement e in barra.Elements())
                    dados.Barra.Add(LerNo(e));

            return dados;
        }

        private static Entid_No LerNo(XElement e)
        {
            var no = new Entid_No { Tag = e.Name.LocalName };
            LerAtributos(e, no);
            foreach (XElement f in e.Elements())
                no.Filhos.Add(LerNo(f));
            return no;
        }

        public static Entid_AcoesDados LerAcoes(string conteudoXml)
        {
            XElement raiz = CarregarSeguro(conteudoXml).Root;
            if (raiz == null) throw new InvalidDataException("XML sem elemento raiz.");

            var dados = new Entid_AcoesDados { Versao = (string)raiz.Attribute("versao") };

            foreach (XElement c in raiz.Elements("Categoria"))
            {
                var cat = new Entid_CategoriaAcaoDados();
                LerAtributos(c, cat);

                foreach (XElement i in c.Elements().Where(x => x.Name.LocalName == "Acao" || x.Name.LocalName == "Grupo"))
                {
                    var item = new Entid_AcaoDados { Tag = i.Name.LocalName };
                    LerAtributos(i, item);
                    foreach (XElement p in i.Elements("Propriedade"))
                        item.Propriedades.Add(LerAtributos(p, new Entid_Atributos()));
                    cat.Itens.Add(item);
                }

                dados.Categorias.Add(cat);
            }

            return dados;
        }
    }
}
