using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ReferenciaStandard.Repository
{
    /// <summary>
    /// Acesso centralizado aos arquivos de configuracao (XML) do sistema:
    ///  - embutido: o XML padrao compilado dentro desta DLL (Resources\ReferenciaFactory\Configuracoes);
    ///  - externo : Config\NOME.xml ao lado do executavel (opcional, para personalizar sem recompilar).
    /// </summary>
    public class FilesRepository
    {
        /// <summary>Ultimo motivo de falha de leitura (para diagnostico). Nao e usado para decidir fluxo.</summary>
        public static string UltimoErro { get; private set; }

        /// <summary>Mantido por compatibilidade: le um XML embutido pelo nome (ex.: "UiConfig", "Regras").</summary>
        public static string ManegerResource(string resourceName, string tipoFormat)
        {
            if (!string.Equals(tipoFormat, "xml", StringComparison.OrdinalIgnoreCase))
                return null;

            return LerEmbutido(resourceName);
        }

        /// <summary>Le o XML padrao embutido. Devolve null se nao existir (veja UltimoErro).</summary>
        public static string LerEmbutido(string nome)
        {
            try
            {
                if (!NomeValido(nome)) throw new ArgumentException("Nome de configuracao invalido: " + nome);

                var assembly = typeof(FilesRepository).Assembly;
                string sufixo = "." + nome + ".xml";

                // procura pelo sufixo: independe do namespace raiz e da pasta onde o XML foi colocado
                string recurso = assembly.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith(sufixo, StringComparison.OrdinalIgnoreCase));

                if (recurso == null)
                    throw new FileNotFoundException("Recurso embutido '" + nome + ".xml' nao encontrado.");

                using (Stream stream = assembly.GetManifestResourceStream(recurso))
                using (var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                UltimoErro = ex.Message;
                Debug.WriteLine("Erro ao ler o recurso embutido: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Lista todos os XML embutidos de uma pasta de Resources (ex.: "GeneratorJS.Layouts" = Resources\GeneratorJS\Layouts).
        /// Devolve pares (nome do arquivo, conteudo). Pasta sem XML devolve lista vazia (veja UltimoErro se houve falha).
        /// </summary>
        public static List<KeyValuePair<string, string>> ListarEmbutidos(string pastaRecurso)
        {
            var lista = new List<KeyValuePair<string, string>>();
            try
            {
                var assembly = typeof(FilesRepository).Assembly;
                string marcador = ".Resources." + pastaRecurso.Trim('.') + ".";

                foreach (string recurso in assembly.GetManifestResourceNames())
                {
                    int pos = recurso.IndexOf(marcador, StringComparison.OrdinalIgnoreCase);
                    if (pos < 0 || !recurso.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) continue;

                    string arquivo = recurso.Substring(pos + marcador.Length);
                    using (Stream stream = assembly.GetManifestResourceStream(recurso))
                    using (var reader = new StreamReader(stream))
                        lista.Add(new KeyValuePair<string, string>(arquivo, reader.ReadToEnd()));
                }
            }
            catch (Exception ex)
            {
                UltimoErro = ex.Message;
                Debug.WriteLine("Erro ao listar recursos embutidos: " + ex.Message);
            }
            return lista;
        }

        /// <summary>Le Config\NOME.xml ao lado do executavel. Devolve null se o arquivo nao existir.</summary>
        public static string LerExterno(string nome)
        {
            try
            {
                if (!NomeValido(nome)) throw new ArgumentException("Nome de configuracao invalido: " + nome);

                string caminho = Path.Combine(AppPaths.ConfigExterno, nome + ".xml");
                return File.Exists(caminho) ? File.ReadAllText(caminho) : null;
            }
            catch (Exception ex)
            {
                UltimoErro = ex.Message;
                Debug.WriteLine("Erro ao ler a configuracao externa: " + ex.Message);
                return null;
            }
        }

        // impede "..\" e separadores: o nome vem do codigo, mas a regra e barata
        private static bool NomeValido(string nome)
        {
            return !string.IsNullOrWhiteSpace(nome) && nome == Path.GetFileName(nome);
        }
    }
}
