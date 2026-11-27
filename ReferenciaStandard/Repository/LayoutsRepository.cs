
using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReferenciaStandard.Repository
{
    /// <summary>
    /// Sabe ONDE estao os XML de layout e os devolve como texto (nao interpreta nada):
    ///  - embutidos: Resources\GeneratorJS\Layouts compilados nesta DLL (origem "padrao");
    ///  - externos : Layouts\*.xml ao lado do executavel, opcional (origem "padrao");
    ///  - usuario  : %AppData%\OrionAutomator\GeneratorJS\Layouts, onde a tela grava os importados (origem "importado").
    /// </summary>
    public static class LayoutsRepository
    {
        public const string PastaEmbutida = "GeneratorJS.Layouts";
        public const string OrigemPadrao = "padrao";
        public const string OrigemImportado = "importado";

        /// <summary>Layouts que acompanham o aplicativo (embutidos + pasta ao lado do exe).</summary>
        public static List<LayoutArquivo> ListarPadrao()
        {
            var lista = new List<LayoutArquivo>();

            foreach (KeyValuePair<string, string> item in FilesRepository.ListarEmbutidos(PastaEmbutida))
            {
                lista.Add(new LayoutArquivo
                {
                    Nome = item.Key,
                    Caminho = "(embutido) " + item.Key,
                    Origem = OrigemPadrao,
                    Xml = item.Value
                });
            }

            lista.AddRange(LerPasta(AppPaths.LayoutsExterno, OrigemPadrao));
            return lista;
        }

        /// <summary>Layouts importados pelo usuario. Cria a pasta se ainda nao existir.</summary>
        public static List<LayoutArquivo> ListarUsuario()
        {
            try { Directory.CreateDirectory(AppPaths.LayoutsUsuario); } catch { /* sem pasta: lista vazia */ }
            return (List<LayoutArquivo>)LerPasta(AppPaths.LayoutsUsuario, OrigemImportado);
        }

        /// <summary>Le um XML qualquer escolhido pelo usuario (para importar).</summary>
        public static string LerArquivo(string caminho)
        {
            try
            {
                return File.ReadAllText(caminho);
            }
            catch (Exception ex)
            {
                throw new GeneratorException("Nao foi possivel ler '" + Path.GetFileName(caminho) + "': " + ex.Message, ex);
            }
        }

        /// <summary>Grava o XML na pasta do usuario (nome derivado do identificador do layout). Devolve o caminho gravado.</summary>
        public static string SalvarImportado(string identificador, string xml)
        {
            char[] invalidos = Path.GetInvalidFileNameChars();
            string seguro = new string(identificador.Select(c => invalidos.Contains(c) ? '_' : c).ToArray());

            try
            {
                Directory.CreateDirectory(AppPaths.LayoutsUsuario);
                string destino = Path.Combine(AppPaths.LayoutsUsuario, seguro + ".xml");
                File.WriteAllText(destino, xml);
                return destino;
            }
            catch (Exception ex)
            {
                throw new GeneratorException("Nao foi possivel gravar o layout importado: " + ex.Message, ex);
            }
        }

        private static IEnumerable<LayoutArquivo> LerPasta(string pasta, string origem)
        {
            var lista = new List<LayoutArquivo>();
            if (!Directory.Exists(pasta)) return lista;

            foreach (string arquivo in Directory.EnumerateFiles(pasta, "*.xml", SearchOption.AllDirectories))
            {
                var item = new LayoutArquivo { Nome = Path.GetFileName(arquivo), Caminho = arquivo, Origem = origem };
                try { item.Xml = File.ReadAllText(arquivo); }
                catch (Exception ex) { item.Erro = ex.Message; }
                lista.Add(item);
            }
            return lista;
        }
    }
}
