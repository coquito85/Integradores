using System;
using System.IO;

namespace ReferenciaStandard.Repository
{
    /// <summary>Pastas e arquivos do aplicativo num unico lugar.</summary>
    public static class AppPaths
    {
        public const string NomeApp = "OrionAutomator";

        /// <summary>%AppData%\OrionAutomator (dados do usuario: agendamentos, recentes, estado local).</summary>
        public static string Raiz { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), NomeApp);

        /// <summary>Pasta "Config" ao lado do executavel (sobreposicoes opcionais de configuracao).</summary>
        public static string ConfigExterno
        {
            get { return Path.Combine(AppContext.BaseDirectory, "Config"); }
        }

        // ── GeneratorJS (gerador de JSON do DOI) ────────────────────────────

        /// <summary>%AppData%\OrionAutomator\GeneratorJS: conexoes, historico, layouts importados e dados do WebView2.</summary>
        public static string GeneratorJs
        {
            get { return Path.Combine(Raiz, "GeneratorJS"); }
        }

        /// <summary>Layouts importados pelo usuario (sobrescrevem os padrao de mesmo identificador).</summary>
        public static string LayoutsUsuario
        {
            get { return Path.Combine(GeneratorJs, "Layouts"); }
        }

        /// <summary>Pasta "Layouts" ao lado do executavel (opcional: layouts padrao extras, sem recompilar).</summary>
        public static string LayoutsExterno
        {
            get { return Path.Combine(AppContext.BaseDirectory, "Layouts"); }
        }

        /// <summary>Meus Documentos\GeneratorJS: pasta sugerida para gravar os JSON gerados.</summary>
        public static string SaidaPadrao
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GeneratorJS"); }
        }

        public static string Arquivo(string nome)
        {
            return Path.Combine(Raiz, nome);
        }
    }
}
