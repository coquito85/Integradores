using System.Collections.Generic;

namespace ReferenciaStandard.Entidades
{
    /// <summary>
    /// No generico de arvore XML (elemento + atributos + filhos). Serve ao menu e a barra de ferramentas:
    /// a camada de dados nao conhece "Item", "Botao" ou "Separador", so guarda o nome do elemento (Tag).
    /// </summary>
    public class Entid_No : Entid_Atributos
    {
        public string Tag { get; set; }
        public List<Entid_No> Filhos { get; } = new List<Entid_No>();
    }

    /// <summary>Conteudo bruto de Comandos.xml: menus (um por &lt;Menu&gt;) e barra padrao.</summary>
    public class Entid_ComandosDados
    {
        public string Versao { get; set; }
        public List<Entid_No> Menus { get; } = new List<Entid_No>();
        public List<Entid_No> Barra { get; } = new List<Entid_No>();
    }

    /// <summary>Uma acao do catalogo (&lt;Acao&gt;) ou um subtitulo (&lt;Grupo&gt;). Atributo "nome" = id unico.</summary>
    public class Entid_AcaoDados : Entid_Atributos
    {
        public string Tag { get; set; }
        public List<Entid_Atributos> Propriedades { get; } = new List<Entid_Atributos>();
    }

    public class Entid_CategoriaAcaoDados : Entid_Atributos
    {
        public List<Entid_AcaoDados> Itens { get; } = new List<Entid_AcaoDados>();
    }

    /// <summary>Conteudo bruto de Acoes.xml.</summary>
    public class Entid_AcoesDados
    {
        public string Versao { get; set; }
        public List<Entid_CategoriaAcaoDados> Categorias { get; } = new List<Entid_CategoriaAcaoDados>();
    }
}