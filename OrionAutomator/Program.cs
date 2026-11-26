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
            // Novo fluxo / abrir arquivo / abrir no editor abrem o editor (frmDesignerFluxos); ao fechar o editor
            // o gerenciador volta.
            while (true)
            {
                using var gerenciador = new frmGerenciadorFluxos();
                if (gerenciador.ShowDialog() != DialogResult.OK) break;

                ExecutarAcao(gerenciador.Acao, gerenciador.CaminhoSelecionado);
            }
        }

        private static void ExecutarAcao(AcaoGerenciador acao, string? caminho)
        {
            switch (acao)
            {
                case AcaoGerenciador.NovoFluxo:
                case AcaoGerenciador.AbrirArquivo:
                case AcaoGerenciador.AbrirNoEditor:
                    using (var editor = new frmDesignerFluxos { CaminhoArquivo = caminho })
                        editor.ShowDialog();
                    break;

                default:
                    // TODO: ligar ao executor (Executar / Picture in Picture). Por enquanto apenas confirma a escolha.
                    MessageBox.Show($"Ação: {acao}" + (caminho == null ? "" : $"\n\n{caminho}"), "Orion Automator", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }
    }
}
