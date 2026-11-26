using ReferenciaStandard.Repository;
using System.Diagnostics;

namespace OrionAutomator.Tema
{
    /// <summary>Um tema disponivel: embutido na DLL (Tema_*.xml) ou um arquivo .xml em uma das pastas de temas.</summary>
    public sealed record TemaInfo(string Id, string Nome, bool Embutido, string? Arquivo);

    /// <summary>
    /// Catalogo e escolha do tema ativo.
    ///
    /// Um tema e um XML no mesmo formato do UiConfig.xml (so precisa da secao Tema; pode trazer Fontes, Metricas
    /// e ate Janelas). Camadas aplicadas pelo UiConfig, a ultima vence:
    ///   padroes do codigo -> UiConfig.xml embutido -> TEMA ESCOLHIDO -> Config\UiConfig.xml (ajustes finos do usuario)
    ///
    /// Onde ficam os temas:
    ///   - embutidos : ReferenciaStandard\Resources\ReferenciaFactory\Configuracoes\Tema_*.xml
    ///   - instalacao: Config\Temas\*.xml ao lado do .exe (distribuidos junto com o programa)
    ///   - usuario   : %AppData%\OrionAutomator\Temas\*.xml (criados pelo menu "Criar tema personalizado")
    /// O tema escolhido fica salvo em %AppData%\OrionAutomator\tema.txt.
    /// </summary>
    public static class TemaManager
    {
        public const string IdPadrao = "Matrix";

        private const string PrefixoRecurso = "Tema_";
        private const string ArquivoPreferencia = "tema.txt";

        // O padrao (Matrix) e o visual original do programa.
        private static readonly TemaInfo[] _embutidos =
        {
            new("Matrix",   "Matrix (verde escuro)", true, null),
            new("Classico", "Clássico (claro)",      true, null),
            new("Grafite",  "Grafite (escuro)",      true, null),
        };

        private static string _idAtual = LerPreferencia();

        /// <summary>Disparado quando o tema muda ou quando se pede para recarregar. Pode vir de qualquer thread.</summary>
        public static event EventHandler? TemaAlterado;

        /// <summary>Ultimo problema ao ler/gravar temas (diagnostico).</summary>
        public static string? UltimoErro { get; private set; }

        public static string PastaUsuario => Path.Combine(AppPaths.Raiz, "Temas");
        public static string PastaInstalacao => Path.Combine(AppPaths.ConfigExterno, "Temas");

        public static IReadOnlyList<TemaInfo> Listar()
        {
            var lista = new List<TemaInfo>(_embutidos);

            foreach (var pasta in new[] { PastaInstalacao, PastaUsuario })
            {
                try
                {
                    if (!Directory.Exists(pasta)) continue;

                    foreach (var arquivo in Directory.EnumerateFiles(pasta, "*.xml")
                                                     .OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
                    {
                        string id = Path.GetFileNameWithoutExtension(arquivo);
                        if (lista.Any(t => string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase))) continue;
                        lista.Add(new TemaInfo(id, id.Replace('_', ' '), false, arquivo));
                    }
                }
                catch (Exception ex) { UltimoErro = ex.Message; }
            }

            return lista;
        }

        /// <summary>Tema ativo. Se o arquivo do tema salvo sumiu, volta ao padrao sem erro.</summary>
        public static TemaInfo Atual =>
            Listar().FirstOrDefault(t => string.Equals(t.Id, _idAtual, StringComparison.OrdinalIgnoreCase))
            ?? _embutidos[0];

        /// <summary>Escolhe o tema, grava a preferencia e avisa as janelas abertas.</summary>
        public static bool Definir(string id)
        {
            var tema = Listar().FirstOrDefault(t => string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase));
            if (tema == null) return false;

            _idAtual = tema.Id;
            GravarPreferencia(tema.Id);
            TemaAlterado?.Invoke(null, EventArgs.Empty);
            return true;
        }

        /// <summary>Relê os arquivos de tema (depois de editar um XML) e reaplica nas janelas abertas.</summary>
        public static void Recarregar() => TemaAlterado?.Invoke(null, EventArgs.Empty);

        /// <summary>Texto XML do tema, ou null se nao puder ser lido (veja UltimoErro).</summary>
        public static string? LerXml(TemaInfo tema)
        {
            try
            {
                if (tema.Embutido)
                {
                    string? xml = FilesRepository.LerEmbutido(PrefixoRecurso + tema.Id);
                    if (xml == null) UltimoErro = FilesRepository.UltimoErro;
                    return xml;
                }

                return tema.Arquivo != null ? File.ReadAllText(tema.Arquivo) : null;
            }
            catch (Exception ex)
            {
                UltimoErro = ex.Message;
                return null;
            }
        }

        /// <summary>Copia o tema para %AppData%\OrionAutomator\Temas\Meu_Tema.xml (para o usuario editar). Devolve o caminho.</summary>
        public static string? CriarPersonalizado(TemaInfo origem)
        {
            try
            {
                string? xml = LerXml(origem);
                if (xml == null) return null;

                Directory.CreateDirectory(PastaUsuario);

                string caminho;
                int n = 1;
                do
                {
                    caminho = Path.Combine(PastaUsuario, n == 1 ? "Meu_Tema.xml" : $"Meu_Tema_{n}.xml");
                    n++;
                } while (File.Exists(caminho));

                File.WriteAllText(caminho, xml);
                return caminho;
            }
            catch (Exception ex)
            {
                UltimoErro = ex.Message;
                return null;
            }
        }

        public static void AbrirPastaUsuario()
        {
            try
            {
                Directory.CreateDirectory(PastaUsuario);
                Process.Start(new ProcessStartInfo(PastaUsuario) { UseShellExecute = true });
            }
            catch (Exception ex) { UltimoErro = ex.Message; }
        }

        // ── Preferencia ─────────────────────────────────────────────────────
        private static string LerPreferencia()
        {
            try
            {
                string caminho = AppPaths.Arquivo(ArquivoPreferencia);
                if (File.Exists(caminho))
                {
                    string id = File.ReadAllText(caminho).Trim();
                    if (id.Length > 0) return id;
                }
            }
            catch { }
            return IdPadrao;
        }

        private static void GravarPreferencia(string id)
        {
            try
            {
                Directory.CreateDirectory(AppPaths.Raiz);
                File.WriteAllText(AppPaths.Arquivo(ArquivoPreferencia), id);
            }
            catch (Exception ex) { UltimoErro = ex.Message; }
        }
    }
}
