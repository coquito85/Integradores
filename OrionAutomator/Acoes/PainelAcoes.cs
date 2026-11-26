using OrionAutomator.Tema;

namespace OrionAutomator.Acoes
{
    /// <summary>
    /// Painel "Catalogo de Acoes": monta categorias, subtitulos e acoes a partir do Acoes.xml (CatalogoAcoes).
    /// Nada aqui conhece "Clique" ou "Mover": acrescentar uma acao e editar o XML.
    /// - arrastar uma acao para o canvas cria o no; duplo clique adiciona ao fim do fluxo;
    /// - acao sem executor registrado aparece esmaecida e nao arrasta (habilita sozinha quando o executor existir);
    /// - a busca filtra por rotulo; clique no cabecalho da categoria recolhe/expande.
    /// </summary>
    public sealed class PainelAcoes : Panel
    {
        public event Action<DefinicaoAcao>? AcaoSolicitada;

        private readonly TextBox _busca = new();
        private readonly Panel _topo = new();
        private readonly FlowLayoutPanel _lista = new();
        private readonly ToolTip _dica = new();
        private readonly HashSet<string> _recolhidas = new(StringComparer.OrdinalIgnoreCase);

        private CatalogoAcoes? _catalogo;
        private RegistroExecutores? _registro;
        private UiConfig? _ui;

        public PainelAcoes()
        {
            _topo.Dock = DockStyle.Top;
            _topo.Height = 34;
            _topo.Padding = new Padding(6, 5, 6, 5);

            _busca.Dock = DockStyle.Fill;
            _busca.PlaceholderText = "Pesquisar ações...";
            _busca.TextChanged += (_, _) => Reconstruir();
            _topo.Controls.Add(_busca);

            _lista.Dock = DockStyle.Fill;
            _lista.FlowDirection = FlowDirection.TopDown;
            _lista.WrapContents = false;
            _lista.AutoScroll = true;
            _lista.Padding = new Padding(0, 4, 0, 4);
            _lista.Resize += (_, _) => AjustarLargura();

            Controls.Add(_lista);
            Controls.Add(_topo);
        }

        /// <summary>Liga o painel ao catalogo/executores e aplica o tema. Chame de novo ao trocar de tema.</summary>
        public void Configurar(CatalogoAcoes catalogo, RegistroExecutores registro, UiConfig ui)
        {
            _catalogo = catalogo;
            _registro = registro;
            _ui = ui;

            BackColor = ui.Cor("Superficie");
            _topo.BackColor = ui.Cor("SuperficieAlt");
            _lista.BackColor = ui.Cor("Superficie");
            _busca.BackColor = ui.Cor("Superficie");
            _busca.ForeColor = ui.Cor("Texto");
            _busca.BorderStyle = BorderStyle.FixedSingle;
            _busca.Font = ui.Fonte("Normal");

            if (_lista.IsHandleCreated) Desenho.TemaRolagem(_lista, ui.Escuro);
            else _lista.HandleCreated += (_, _) => Desenho.TemaRolagem(_lista, ui.Escuro);

            Reconstruir();
        }

        private string Filtro => _busca.Text.Trim();

        private void Reconstruir()
        {
            if (_catalogo == null || _registro == null || _ui == null) return;

            string filtro = Filtro;
            bool filtrando = filtro.Length > 0;

            _lista.SuspendLayout();
            foreach (Control c in _lista.Controls.Cast<Control>().ToList()) { _lista.Controls.Remove(c); c.Dispose(); }

            foreach (var cat in _catalogo.Categorias)
            {
                var itens = filtrando
                    ? cat.Itens.Where(i => i.Acao != null && Contem(i.Acao.Rotulo, filtro)).ToList()
                    : cat.Itens;
                if (itens.Count == 0) continue;

                bool recolhida = !filtrando && _recolhidas.Contains(cat.Nome);
                var cab = new LinhaCategoria(_ui, cat, recolhida);
                cab.Click += (_, _) =>
                {
                    if (!_recolhidas.Remove(cat.Nome)) _recolhidas.Add(cat.Nome);
                    BeginInvoke(new Action(Reconstruir));   // adiado: a propria linha clicada e descartada na reconstrucao
                };
                _lista.Controls.Add(cab);

                if (recolhida) continue;

                foreach (var item in itens)
                {
                    if (item.Grupo != null)
                    {
                        if (!filtrando) _lista.Controls.Add(new LinhaGrupo(_ui, item.Grupo));
                        continue;
                    }

                    var acao = item.Acao!;
                    bool disponivel = _registro.Tem(acao.Executor);
                    var linha = new LinhaAcao(_ui, acao, disponivel);
                    linha.Solicitada += a => AcaoSolicitada?.Invoke(a);
                    _dica.SetToolTip(linha, disponivel
                        ? (acao.Descricao.Length > 0 ? acao.Descricao : acao.Rotulo)
                        : $"{acao.Rotulo} — em breve (executor ainda não implementado)");
                    _lista.Controls.Add(linha);
                }
            }

            AjustarLargura();
            _lista.ResumeLayout(true);
        }

        private void AjustarLargura()
        {
            int w = Math.Max(50, _lista.ClientSize.Width - 1);
            foreach (Control c in _lista.Controls)
            {
                c.Margin = Padding.Empty;
                c.Width = w;
            }
        }

        private static bool Contem(string texto, string filtro) =>
            texto.Contains(filtro, StringComparison.CurrentCultureIgnoreCase);

        protected override void Dispose(bool disposing)
        {
            if (disposing) _dica.Dispose();
            base.Dispose(disposing);
        }

        // ══ Linhas ════════════════════════════════════════════════════════════
        private sealed class LinhaCategoria : Control
        {
            private readonly UiConfig _ui;
            private readonly CategoriaAcao _cat;
            private readonly bool _recolhida;

            public LinhaCategoria(UiConfig ui, CategoriaAcao cat, bool recolhida)
            {
                _ui = ui; _cat = cat; _recolhida = recolhida;
                Height = ui.Escalar(26);
                Cursor = Cursors.Hand;
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.Clear(_ui.Cor("SuperficieAlt"));
                string seta = _recolhida ? "▸" : "▾";
                using var negrito = new Font(_ui.Fonte("Rotulo"), FontStyle.Bold);
                const TextFormatFlags fl = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;
                TextRenderer.DrawText(g, seta, _ui.Fonte("Rotulo"), new Rectangle(8, 0, 16, Height), _cat.Cor, fl);
                TextRenderer.DrawText(g, _cat.Nome, negrito, new Rectangle(26, 0, Width - 30, Height), _cat.Cor, fl);
            }
        }

        private sealed class LinhaGrupo : Control
        {
            private readonly UiConfig _ui;
            private readonly string _texto;

            public LinhaGrupo(UiConfig ui, string texto)
            {
                _ui = ui; _texto = texto;
                Height = ui.Escalar(22);
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.Clear(_ui.Cor("Superficie"));
                var fonte = _ui.Fonte("ItemMeta");
                var medida = TextRenderer.MeasureText(g, _texto, fonte, Size.Empty, TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, _texto, fonte, new Point(10, (Height - medida.Height) / 2), _ui.Cor("TextoSecundario"), TextFormatFlags.NoPadding);
                using var pen = new Pen(_ui.Cor("Borda"));
                int x = 10 + medida.Width + 8;
                if (x < Width - 8) g.DrawLine(pen, x, Height / 2, Width - 8, Height / 2);
            }
        }

        private sealed class LinhaAcao : Control
        {
            private readonly UiConfig _ui;
            private readonly DefinicaoAcao _acao;
            private readonly bool _disponivel;
            private bool _hover;
            private Point _clique = Point.Empty;

            public event Action<DefinicaoAcao>? Solicitada;

            public LinhaAcao(UiConfig ui, DefinicaoAcao acao, bool disponivel)
            {
                _ui = ui; _acao = acao; _disponivel = disponivel;
                Height = ui.Escalar(32);
                Cursor = disponivel ? Cursors.Hand : Cursors.Default;
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.ResizeRedraw | ControlStyles.StandardDoubleClick, true);
            }

            protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
            protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                _clique = e.Button == MouseButtons.Left ? e.Location : Point.Empty;
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                if (!_disponivel || e.Button != MouseButtons.Left || _clique == Point.Empty) return;

                var arrasto = SystemInformation.DragSize;
                if (Math.Abs(e.X - _clique.X) < arrasto.Width && Math.Abs(e.Y - _clique.Y) < arrasto.Height) return;

                _clique = Point.Empty;
                DoDragDrop(CanvasFluxo.PrefixoArrasto + _acao.Id, DragDropEffects.Copy);
            }

            protected override void OnMouseDoubleClick(MouseEventArgs e)
            {
                base.OnMouseDoubleClick(e);
                if (_disponivel && e.Button == MouseButtons.Left) Solicitada?.Invoke(_acao);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.Clear(_hover && _disponivel ? _ui.Cor("SuperficieAlt") : _ui.Cor("Superficie"));

                int lado = Height - 10;
                var tile = new Rectangle(12, 5, lado, lado);
                Color acento = _disponivel ? _acao.Cor : _ui.Cor("TextoDesabilitado");
                using (var br = new SolidBrush(Color.FromArgb(255,
                    _ui.Cor("Fundo").R + (int)((acento.R - _ui.Cor("Fundo").R) * 0.18f),
                    _ui.Cor("Fundo").G + (int)((acento.G - _ui.Cor("Fundo").G) * 0.18f),
                    _ui.Cor("Fundo").B + (int)((acento.B - _ui.Cor("Fundo").B) * 0.18f))))
                    g.FillRectangle(br, tile);

                using var fIcone = new Font("Segoe UI Symbol", 9.5f);
                const TextFormatFlags centro = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;
                TextRenderer.DrawText(g, _acao.Icone, fIcone, tile, acento, centro);

                TextRenderer.DrawText(g, _acao.Rotulo, _ui.Fonte("Normal"),
                    new Rectangle(tile.Right + 10, 0, Width - tile.Right - 14, Height),
                    _disponivel ? _ui.Cor("Texto") : _ui.Cor("TextoDesabilitado"),
                    TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
            }
        }
    }
}
