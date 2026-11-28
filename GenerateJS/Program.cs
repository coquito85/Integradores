using GenerateJS.Gerenciador;

namespace GenerateJS
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.ThreadException += (_, e) =>
                MessageBox.Show(e.Exception.Message, "GeneratorJS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Run(new frmTelaGeneratorJS());
        }
    }
}



