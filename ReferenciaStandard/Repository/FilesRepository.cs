
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;


namespace ReferenciaStandard.Repository
{
    public class FilesRepository
    {
        private static string ManegerContextXml(string strLayout, string pathLocalization)
        {
            switch (strLayout)
            {
                case "Configuracoes":
                    pathLocalization += $"Configuracoes.{strLayout}";
                    break;
            }
            return pathLocalization + ".xml";
        }
        public static string ManegerResource(string resourceName, string tipoFormat)
        {
            string pathFile = "Resources.";
            string ConteudoResource = string.Empty;
            try
            {
                if (tipoFormat == "xml")
                    ConteudoResource = ManegerContextXml(resourceName, pathFile + "ReferenciaFactory.");
                else
                    ConteudoResource = null;

                if (!string.IsNullOrEmpty(ConteudoResource))
                {
                    // Abre o fluxo do recurso embutido

                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ConteudoResource))
                    {
                        if (stream == null)
                            throw new Exception($"Recurso {ConteudoResource} não encontrado.");

                        using (StreamReader reader = new StreamReader(stream))
                            return reader.ReadToEnd();
                    }
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao ler o recurso embutido: {ex.Message}");
                return null;
            }
        }
    }
}
