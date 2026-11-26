using System.Diagnostics;

namespace OrionAutomator.Tema
{
    /// <summary>Monta o menu "Tema" (lista de temas + atalhos) para qualquer janela usar da mesma forma.</summary>
    public static class TemaMenu
    {
        private const string Titulo = "Orion Automator";

        /// <summary>Recria os itens do menu: um por tema (o ativo marcado com ●), mais as acoes de personalizacao.</summary>
        public static void Preencher(ToolStripItemCollection itens)
        {
            foreach (var antigo in itens.Cast<ToolStripItem>().ToList())
            {
                itens.Remove(antigo);
                antigo.Dispose();
            }

            string idAtual = TemaManager.Atual.Id;

            foreach (var tema in TemaManager.Listar())
            {
                string id = tema.Id;
                bool ativo = string.Equals(id, idAtual, StringComparison.OrdinalIgnoreCase);

                var item = new ToolStripMenuItem((ativo ? "●  " : "     ") + tema.Nome);
                item.Click += (_, _) => TemaManager.Definir(id);
                itens.Add(item);
            }

            itens.Add(new ToolStripSeparator());

            var criar = new ToolStripMenuItem("Criar tema personalizado a partir do atual...");
            criar.Click += (_, _) => CriarPersonalizado();
            itens.Add(criar);

            var pasta = new ToolStripMenuItem("Abrir pasta dos meus temas");
            pasta.Click += (_, _) => TemaManager.AbrirPastaUsuario();
            itens.Add(pasta);

            var recarregar = new ToolStripMenuItem("Recarregar tema (F5)");
            recarregar.Click += (_, _) => TemaManager.Recarregar();
            itens.Add(recarregar);
        }

        private static void CriarPersonalizado()
        {
            string? caminho = TemaManager.CriarPersonalizado(TemaManager.Atual);
            if (caminho == null)
            {
                MessageBox.Show("Não foi possível criar o tema personalizado.\n\n" + TemaManager.UltimoErro,
                    Titulo, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            TemaManager.Definir(Path.GetFileNameWithoutExtension(caminho));

            // abre a pasta com o arquivo selecionado: edite as cores, salve e pressione F5 na janela
            try { Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{caminho}\"") { UseShellExecute = true }); }
            catch { }
        }
    }
}
