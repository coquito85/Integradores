using OrionAutomator.Janelas;

namespace OrionAutomator
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

            // O gerenciador devolve a acao escolhida (mesmo contrato do ManagerModeloForm original).
            // Quando o editor visual existir, e aqui que ele sera aberto.
            while (true)
            {
                using var gerenciador = new frmGerenciadorFluxos();
                if (gerenciador.ShowDialog() != DialogResult.OK) break;

                ExecutarAcao(gerenciador.Acao, gerenciador.CaminhoSelecionado);
            }
        }

        private static void ExecutarAcao(AcaoGerenciador acao, string? caminho)
        {
            // TODO: ligar ao editor/executor. Por enquanto apenas confirma a escolha.
            MessageBox.Show($"Ação: {acao}" + (caminho == null ? "" : $"\n\n{caminho}"),"Orion Automator", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
