using System;
using System.Collections.Generic;
using System.Globalization;

namespace ReferenciaStandard.Entidades
{
    /// <summary>
    /// Conjunto de atributos de um elemento XML (ex.: um &lt;Controle&gt; ou uma &lt;Funcionalidade&gt;).
    /// Guarda tudo como texto, sem conhecer nenhum atributo especifico: assim um atributo novo no XML
    /// nao exige mudar esta camada, so quem consome o valor.
    /// </summary>
    public class Entid_Atributos
    {
        private readonly Dictionary<string, string> _valores =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string Nome { get; set; }

        public IDictionary<string, string> Valores
        {
            get { return _valores; }
        }

        public bool Tem(string chave)
        {
            return _valores.ContainsKey(chave);
        }

        public string Texto(string chave)
        {
            string v;
            return _valores.TryGetValue(chave, out v) ? v : null;
        }

        public int? Int(string chave)
        {
            int n;
            string v = Texto(chave);
            if (v != null && int.TryParse(v.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out n))
                return n;
            return null;
        }

        public double? Numero(string chave)
        {
            double n;
            string v = Texto(chave);
            if (v != null && double.TryParse(v.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out n))
                return n;
            return null;
        }

        public bool? Bool(string chave)
        {
            string v = Texto(chave);
            if (v == null) return null;

            switch (v.Trim().ToLowerInvariant())
            {
                case "true":
                case "1":
                case "sim":
                    return true;
                case "false":
                case "0":
                case "nao":
                case "não":
                    return false;
                default:
                    return null;
            }
        }

        /// <summary>Copia os atributos de outro elemento por cima destes (usado na sobreposicao de configuracoes).</summary>
        public void Mesclar(Entid_Atributos outro)
        {
            if (outro == null) return;
            foreach (KeyValuePair<string, string> par in outro._valores)
                _valores[par.Key] = par.Value;
        }
    }

    public class Entid_JanelaDados : Entid_Atributos
    {
        public List<Entid_Atributos> Controles { get; } = new List<Entid_Atributos>();
    }

    /// <summary>Conteudo bruto de UiConfig.xml (tema + janelas), ainda sem tipos de interface.</summary>
    public class Entid_UiConfigDados
    {
        public string Versao { get; set; }
        public List<Entid_Atributos> Cores { get; } = new List<Entid_Atributos>();
        public List<Entid_Atributos> Fontes { get; } = new List<Entid_Atributos>();
        public List<Entid_Atributos> Metricas { get; } = new List<Entid_Atributos>();
        public List<Entid_JanelaDados> Janelas { get; } = new List<Entid_JanelaDados>();
    }

    /// <summary>Conteudo bruto de Regras.xml (licenca, versao e bloqueios por funcionalidade).</summary>
    public class Entid_RegrasDados
    {
        public Entid_Atributos Licenca { get; set; }
        public Entid_Atributos Versao { get; set; }
        public List<Entid_Atributos> Bloqueios { get; } = new List<Entid_Atributos>();
    }
}
