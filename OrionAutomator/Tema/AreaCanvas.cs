using System.ComponentModel;

namespace OrionAutomator.Tema
{
    /// <summary>
    /// Area de trabalho do editor: fundo + grade (a cada 5 linhas, uma mais forte). Por enquanto so desenha;
    /// os nos e conexoes entram aqui quando o editor for construido. Cores e tamanho vem do tema/UiConfig.xml.
    /// </summary>
    public class AreaCanvas : Panel
    {
        private Color _corGrade = Color.FromArgb(11, 36, 18);
        private Color _corGradeForte = Color.FromArgb(18, 64, 31);
        private int _tamanhoGrade = 20;
        private string _mensagem = "";

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CorGrade { get => _corGrade; set { _corGrade = value; Invalidate(); } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CorGradeForte { get => _corGradeForte; set { _corGradeForte = value; Invalidate(); } }

        /// <summary>Espacamento da grade em pixels logicos (96 dpi); 0 desliga a grade.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TamanhoGrade { get => _tamanhoGrade; set { _tamanhoGrade = Math.Max(0, value); Invalidate(); } }

        /// <summary>Texto centralizado (ex.: dica quando o fluxo esta vazio). Usa ForeColor.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Mensagem { get => _mensagem; set { _mensagem = value ?? ""; Invalidate(); } }

        public AreaCanvas()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);

            if (_tamanhoGrade > 0)
            {
                int passo = Math.Max(4, (int)Math.Round(_tamanhoGrade * DeviceDpi / 96f));
                using var penFraca = new Pen(_corGrade);
                using var penForte = new Pen(_corGradeForte);

                for (int x = 0, i = 0; x < Width; x += passo, i++)
                    g.DrawLine(i % 5 == 0 ? penForte : penFraca, x, 0, x, Height);
                for (int y = 0, i = 0; y < Height; y += passo, i++)
                    g.DrawLine(i % 5 == 0 ? penForte : penFraca, 0, y, Width, y);
            }

            if (_mensagem.Length > 0)
            {
                // VerticalCenter so funciona com uma linha; a mensagem pode ter varias, entao centraliza-se medindo
                const TextFormatFlags fl = TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding;
                var medida = TextRenderer.MeasureText(g, _mensagem, Font, new Size(Math.Max(10, Width - 40), Math.Max(10, Height)), fl);
                var area = new Rectangle((Width - medida.Width) / 2, (Height - medida.Height) / 2, medida.Width, medida.Height);
                TextRenderer.DrawText(g, _mensagem, Font, area, ForeColor, fl);
            }
        }
    }
}
