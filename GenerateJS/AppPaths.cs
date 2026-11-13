namespace GenerateJS
{
    /// <summary>Pastas de execução (spec. seção 13), relativas ao executável.</summary>
    internal static class AppPaths
    {
        public static string Base => AppContext.BaseDirectory;
        public static string Layouts => Path.Combine(Base, "Configurations", "Layouts");
        public static string Output => Path.Combine(Base, "Output");
        public static string Logs => Path.Combine(Base, "Logs");
    }
}
