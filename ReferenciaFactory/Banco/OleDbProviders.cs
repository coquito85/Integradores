using System.Data.OleDb;

namespace ReferenciaFactory.Banco
{
    /// <summary>Opcao de provider OLE DB oferecida na tela (o usuario ainda pode digitar qualquer outro).</summary>
    public sealed record OleDbOption(string Value, string Label, bool Installed);

    /// <summary>
    /// Providers OLE DB do Access: lista conhecida + o que realmente esta registrado
    /// para a arquitetura (32/64 bits) deste aplicativo. Nada aqui e fixo por tipo de conexao:
    /// o campo da tela e livre e, em branco, usa o melhor provider instalado.
    /// </summary>
    public static class OleDbProviders
    {
        private const string Fallback = "Microsoft.ACE.OLEDB.12.0";

        // Ordem = preferencia no modo automatico.
        private static readonly (string Name, string Desc)[] Conhecidos =
        {
            ("Microsoft.ACE.OLEDB.16.0", ".accdb e .mdb"),
            ("Microsoft.ACE.OLEDB.12.0", ".accdb e .mdb"),
            ("Microsoft.Jet.OLEDB.4.0",  ".mdb, somente 32 bits"),
        };

        /// <summary>Providers OLE DB registrados para a arquitetura deste processo.</summary>
        public static IReadOnlyList<string> Instalados()
        {
            var lista = new List<string>();
            try
            {
                using var r = OleDbEnumerator.GetRootEnumerator();
                while (r.Read())
                {
                    var nome = Convert.ToString(r["SOURCES_NAME"])?.Trim();
                    if (!string.IsNullOrEmpty(nome)) lista.Add(nome);
                }
            }
            catch { /* sem enumerador: segue so com a lista conhecida */ }
            return lista;
        }

        public static IReadOnlyList<OleDbOption> Opcoes()
        {
            var instalados = Instalados();
            bool Tem(string n) => instalados.Contains(n, StringComparer.OrdinalIgnoreCase);

            var opcoes = Conhecidos
                .Select(k => new OleDbOption(k.Name, $"{k.Name} ({k.Desc}) - {(Tem(k.Name) ? "instalado" : "nao instalado")}", Tem(k.Name)))
                .ToList();

            // qualquer outro ACE/Jet registrado (ex.: ACE 15.0) tambem aparece
            foreach (var n in instalados)
                if ((n.Contains("ACE.OLEDB", StringComparison.OrdinalIgnoreCase) || n.Contains("Jet.OLEDB", StringComparison.OrdinalIgnoreCase))
                    && !opcoes.Any(o => o.Value.Equals(n, StringComparison.OrdinalIgnoreCase)))
                    opcoes.Add(new OleDbOption(n, $"{n} - instalado", true));

            return opcoes;
        }

        /// <summary>Melhor provider instalado (modo automatico).</summary>
        public static string Automatico() =>
            Opcoes().FirstOrDefault(o => o.Installed)?.Value ?? Fallback;

        public static string Dica()
        {
            var bits = Environment.Is64BitProcess ? 64 : 32;
            var achados = Opcoes().Where(o => o.Installed).Select(o => o.Value).ToList();
            var lista = achados.Count > 0 ? string.Join(", ", achados) : "nenhum provider do Access";
            return $"Aplicativo em {bits} bits. Detectados: {lista}. Pode digitar outro provider; em branco usa o automatico.";
        }
    }
}
