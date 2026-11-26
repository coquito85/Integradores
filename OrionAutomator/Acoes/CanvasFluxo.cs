using OrionAutomator.Tema;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace OrionAutomator.Acoes
{
    /// <summary>
    /// Canvas do editor: estende a AreaCanvas (grade + tema) com cartoes de acao, conexoes, arrastar/soltar do
    /// catalogo, selecao, mover, excluir, zoom (roda do mouse) e pan (botao do meio/direito).
    /// O visual de cada cartao vem da DefinicaoAcao (icone, cor, resumo) lida do Acoes.xml.
    ///
    /// Limite desta primeira versao: o fluxo e uma CADEIA (cada no tem uma saida). Um no solto no canvas e ligado ao
    /// fim da cadeia. Ramificacoes (If/laços) e religar conexoes a mao ainda nao existem.
    /// </summary>
    public class CanvasFluxo : AreaCanvas
    {
        /// <summary>Prefixo do texto levado no arrastar/soltar: "orion-acao:&lt;id da acao&gt;".</summary>
        public const string PrefixoArrasto = "orion-acao:";

        private const int LarguraNo = 210, AlturaNo = 64, EspacoVertical = 40, RaioPorta = 5;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModeloFluxo Modelo { get; private set; } = new();

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CatalogoAcoes? Catalogo { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public UiConfig? Ui
        {
            get => _ui;
            set { _ui = value; RecriarFontes(); Invalidate(); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public NoFluxo? Selecionado { get; private set; }

        public event EventHandler? SelecaoAlterada;
        /// <summary>Estrutura ou valores do fluxo mudaram (marcar como alterado, atualizar contadores).</summary>
        public event EventHandler? FluxoAlterado;
        /// <summary>Mensagem para o log (ex.: "O fluxo já tem um nó de Início.").</summary>
        public event Action<string>? Aviso;

        private UiConfig? _ui;
        private float _zoom = 1f;
        private PointF _offset = new(40, 40);   // pixels de tela

        private NoFluxo? _arrastando;
        private PointF _deltaArrasto;
        private bool _moveu;
        private bool _panando;
        private Point _panInicio;
        private PointF _offsetInicio;

        private Font? _fTitulo, _fResumo, _fIcone;

        public CanvasFluxo()
        {
            AllowDrop = true;
            TabStop = true;
        }

        private float K => DeviceDpi / 96f;
        private float Escala => _zoom * K;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Zoom
        {
            get => _zoom;
            set => DefinirZoom(value, new PointF(Width / 2f, Height / 2f));
        }

        // ══ Modelo ═══════════════════════════════════════════════════════════
        /// <summary>Fluxo novo com o nó de Início (se o catalogo conhece "fluxo.inicio").</summary>
        public void Novo(string nome = "Novo fluxo")
        {
            Carregar(new ModeloFluxo { Nome = nome }, ajustar: false);

            var def = Catalogo?.Todas.FirstOrDefault(a => a.Executor == "fluxo.inicio");
            if (def != null) Modelo.Nos.Add(NoFluxo.Criar(def, new Point(40, 40)));

            _zoom = 1f;
            _offset = new PointF(40 * K, 40 * K);
            Invalidate();
            FluxoAlterado?.Invoke(this, EventArgs.Empty);
        }

        public void Carregar(ModeloFluxo modelo, bool ajustar = true)
        {
            Modelo = modelo;
            Selecionado = null;
            SelecaoAlterada?.Invoke(this, EventArgs.Empty);
            if (ajustar) Ajustar();
            Invalidate();
            FluxoAlterado?.Invoke(this, EventArgs.Empty);
        }

        public NoFluxo? AdicionarNo(DefinicaoAcao def, Point posicaoLogica, bool conectar = true)
        {
            if (def.Executor == "fluxo.inicio" && Modelo.Nos.Any(n => Catalogo?.Buscar(n.AcaoId)?.Executor == "fluxo.inicio"))
            {
                Aviso?.Invoke("⊘ O fluxo já tem um nó de Início.");
                return null;
            }

            var no = NoFluxo.Criar(def, posicaoLogica);
            Modelo.Nos.Add(no);
            if (conectar) ConectarAoFim(no, def);

            Selecionar(no);
            Invalidate();
            FluxoAlterado?.Invoke(this, EventArgs.Empty);
            return no;
        }

        /// <summary>Duplo clique no catalogo: adiciona logo abaixo do ultimo no da cadeia.</summary>
        public NoFluxo? AdicionarAoFinal(DefinicaoAcao def)
        {
            var fim = UltimoDaCadeia();
            var pos = fim == null
                ? new Point(40, 40)
                : new Point(fim.Posicao.X, fim.Posicao.Y + AlturaNo + EspacoVertical);
            var no = AdicionarNo(def, pos, conectar: true);
            if (no != null) MostrarNo(no);
            return no;
        }

        public void ExcluirSelecionado()
        {
            var no = Selecionado;
            if (no == null) return;

            if (Catalogo?.Buscar(no.AcaoId)?.Executor == "fluxo.inicio")
            {
                Aviso?.Invoke("⊘ O nó de Início não pode ser excluído.");
                return;
            }

            foreach (var anterior in Modelo.Nos.Where(n => n.ProximoId == no.Id))
                anterior.ProximoId = no.ProximoId;   // religa: A → X → B vira A → B

            Modelo.Nos.Remove(no);
            Selecionar(null);
            Invalidate();
            FluxoAlterado?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>O painel de propriedades alterou um valor: redesenha o cartao (titulo/resumo) e avisa.</summary>
        public void NotificarValorAlterado(NoFluxo no)
        {
            Invalidate();
            FluxoAlterado?.Invoke(this, EventArgs.Empty);
        }

        public void NotificarEstado(NoFluxo no) => Invalidate();

        public void Selecionar(NoFluxo? no)
        {
            if (ReferenceEquals(Selecionado, no)) return;
            Selecionado = no;
            Invalidate();
            SelecaoAlterada?.Invoke(this, EventArgs.Empty);
        }

        private NoFluxo? Inicio() =>
            Modelo.Nos.FirstOrDefault(n => Catalogo?.Buscar(n.AcaoId)?.Executor == "fluxo.inicio") ?? Modelo.Nos.FirstOrDefault();

        private NoFluxo? UltimoDaCadeia()
        {
            var atual = Inicio();
            var visitados = new HashSet<string>();
            while (atual != null && visitados.Add(atual.Id))
            {
                var prox = Modelo.Buscar(atual.ProximoId);
                if (prox == null) return atual;
                atual = prox;
            }
            return atual;
        }

        private void ConectarAoFim(NoFluxo novo, DefinicaoAcao def)
        {
            if (def.Executor == "fluxo.inicio") return;

            var fim = UltimoDaCadeia();
            if (fim == null || fim == novo) return;
            if (Catalogo?.Buscar(fim.AcaoId)?.Executor == "fluxo.fim") return;   // nada depois do Fim
            fim.ProximoId = novo.Id;
        }

        // ══ Layout, zoom e pan ═══════════════════════════════════════════════
        /// <summary>Coloca a cadeia numa coluna (Início em cima) e os nos soltos abaixo dela.</summary>
        public void OrganizarVertical()
        {
            var ordem = new List<NoFluxo>();
            var atual = Inicio();
            var vistos = new HashSet<string>();
            while (atual != null && vistos.Add(atual.Id)) { ordem.Add(atual); atual = Modelo.Buscar(atual.ProximoId); }
            ordem.AddRange(Modelo.Nos.Where(n => !vistos.Contains(n.Id)));

            int y = 40;
            foreach (var n in ordem)
            {
                n.Posicao = new Point(40, y);
                y += AlturaNo + EspacoVertical;
            }

            Ajustar();
            FluxoAlterado?.Invoke(this, EventArgs.Empty);
        }

        public void Ajustar()
        {
            if (Modelo.Nos.Count == 0 || Width < 50 || Height < 50) { _zoom = 1f; _offset = new PointF(40 * K, 40 * K); Invalidate(); return; }

            int minX = Modelo.Nos.Min(n => n.Posicao.X), minY = Modelo.Nos.Min(n => n.Posicao.Y);
            int maxX = Modelo.Nos.Max(n => n.Posicao.X) + LarguraNo, maxY = Modelo.Nos.Max(n => n.Posicao.Y) + AlturaNo;

            float margem = 40 * K;
            float zx = (Width - 2 * margem) / ((maxX - minX) * K);
            float zy = (Height - 2 * margem) / ((maxY - minY) * K);
            _zoom = Math.Clamp(Math.Min(zx, zy), 0.25f, 1.5f);

            float largura = (maxX - minX) * Escala, altura = (maxY - minY) * Escala;
            _offset = new PointF((Width - largura) / 2f - minX * Escala, (Height - altura) / 2f - minY * Escala);
            Invalidate();
        }

        public void ZoomMais() => Zoom = _zoom * 1.2f;
        public void ZoomMenos() => Zoom = _zoom / 1.2f;
        public void Zoom100() => Zoom = 1f;

        private void DefinirZoom(float novo, PointF ancoraTela)
        {
            novo = Math.Clamp(novo, 0.25f, 3f);
            var mundo = ParaMundo(ancoraTela);
            _zoom = novo;
            _offset = new PointF(ancoraTela.X - mundo.X * Escala, ancoraTela.Y - mundo.Y * Escala);
            Invalidate();
        }

        /// <summary>Rola o canvas ate o no ficar visivel.</summary>
        private void MostrarNo(NoFluxo no)
        {
            var r = RetTela(no);
            float dx = 0, dy = 0;
            if (r.Left < 10) dx = 10 - r.Left; else if (r.Right > Width - 10) dx = Width - 10 - r.Right;
            if (r.Top < 10) dy = 10 - r.Top; else if (r.Bottom > Height - 10) dy = Height - 10 - r.Bottom;
            if (dx != 0 || dy != 0) { _offset = new PointF(_offset.X + dx, _offset.Y + dy); Invalidate(); }
        }

        private PointF ParaMundo(PointF tela) => new((tela.X - _offset.X) / Escala, (tela.Y - _offset.Y) / Escala);

        private RectangleF RetTela(NoFluxo n) => new(
            _offset.X + n.Posicao.X * Escala, _offset.Y + n.Posicao.Y * Escala, LarguraNo * Escala, AlturaNo * Escala);

        private NoFluxo? NoEm(PointF mundo)
        {
            for (int i = Modelo.Nos.Count - 1; i >= 0; i--)
            {
                var n = Modelo.Nos[i];
                if (new RectangleF(n.Posicao.X, n.Posicao.Y, LarguraNo, AlturaNo).Contains(mundo)) return n;
            }
            return null;
        }

        private Point Encaixar(PointF mundo)
        {
            int g = Math.Max(1, TamanhoGrade > 0 ? TamanhoGrade : 20);
            return new Point((int)Math.Round(mundo.X / g) * g, (int)Math.Round(mundo.Y / g) * g);
        }

        // ══ Mouse e teclado ══════════════════════════════════════════════════
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();

            var mundo = ParaMundo(e.Location);
            var hit = NoEm(mundo);

            if (e.Button == MouseButtons.Left)
            {
                Selecionar(hit);
                if (hit != null)
                {
                    _arrastando = hit;
                    _deltaArrasto = new PointF(mundo.X - hit.Posicao.X, mundo.Y - hit.Posicao.Y);
                    _moveu = false;
                }
            }
            else if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Right && hit == null))
            {
                _panando = true;
                _panInicio = e.Location;
                _offsetInicio = _offset;
                Cursor = Cursors.SizeAll;
            }
            else if (e.Button == MouseButtons.Right)
            {
                Selecionar(hit);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_panando)
            {
                _offset = new PointF(_offsetInicio.X + (e.X - _panInicio.X), _offsetInicio.Y + (e.Y - _panInicio.Y));
                Invalidate();
            }
            else if (_arrastando != null)
            {
                var mundo = ParaMundo(e.Location);
                _arrastando.Posicao = new Point(
                    (int)Math.Round(mundo.X - _deltaArrasto.X), (int)Math.Round(mundo.Y - _deltaArrasto.Y));
                _moveu = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_panando) { _panando = false; Cursor = Cursors.Default; }

            if (_arrastando != null)
            {
                if (_moveu)
                {
                    _arrastando.Posicao = Encaixar(_arrastando.Posicao);
                    FluxoAlterado?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
                _arrastando = null;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            DefinirZoom(_zoom * (float)Math.Pow(1.1, e.Delta / 120.0), e.Location);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Delete) { ExcluirSelecionado(); e.Handled = true; }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        // ══ Arrastar e soltar (vindo do catalogo) ════════════════════════════
        private DefinicaoAcao? AcaoDoArrasto(DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.StringFormat) is string s && s.StartsWith(PrefixoArrasto, StringComparison.Ordinal))
                return Catalogo?.Buscar(s.Substring(PrefixoArrasto.Length));
            return null;
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            e.Effect = AcaoDoArrasto(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            e.Effect = AcaoDoArrasto(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            var def = AcaoDoArrasto(e);
            if (def == null) return;

            var mundo = ParaMundo(PointToClient(new Point(e.X, e.Y)));
            var pos = Encaixar(new PointF(mundo.X - LarguraNo / 2f, mundo.Y - AlturaNo / 2f));
            AdicionarNo(def, pos, conectar: true);
            Focus();
        }

        // ══ Desenho ══════════════════════════════════════════════════════════
        private void RecriarFontes()
        {
            _fTitulo?.Dispose(); _fResumo?.Dispose(); _fIcone?.Dispose();
            _fTitulo = _fResumo = _fIcone = null;
            if (_ui == null) return;

            // tamanhos em PIXELS logicos: a escala (zoom x DPI) vem da transformacao do Graphics
            var familia = _ui.Fonte("Normal").FontFamily;
            _fTitulo = new Font(familia, 13f, FontStyle.Bold, GraphicsUnit.Pixel);
            _fResumo = new Font(familia, 11f, FontStyle.Regular, GraphicsUnit.Pixel);
            _fIcone = new Font("Segoe UI Emoji", 16f, FontStyle.Regular, GraphicsUnit.Pixel);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);   // fundo, grade e mensagem
            if (_ui == null || _fTitulo == null) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            var estado = g.Save();
            g.TranslateTransform(_offset.X, _offset.Y);
            g.ScaleTransform(Escala, Escala);

            foreach (var n in Modelo.Nos)
            {
                var prox = Modelo.Buscar(n.ProximoId);
                if (prox != null) DesenharConexao(g, n, prox);
            }
            foreach (var n in Modelo.Nos) DesenharNo(g, n);

            g.Restore(estado);
        }

        private void DesenharConexao(Graphics g, NoFluxo de, NoFluxo para)
        {
            var a = new PointF(de.Posicao.X + LarguraNo, de.Posicao.Y + AlturaNo / 2f);
            var b = new PointF(para.Posicao.X, para.Posicao.Y + AlturaNo / 2f);
            float dx = Math.Max(40f, Math.Abs(b.X - a.X) / 2f);

            using var pen = new Pen(_ui!.Cor("TextoSecundario"), 2f);
            pen.CustomEndCap = new AdjustableArrowCap(4f, 4f);
            g.DrawBezier(pen, a, new PointF(a.X + dx, a.Y), new PointF(b.X - dx, b.Y), b);
        }

        private void DesenharNo(Graphics g, NoFluxo n)
        {
            var ui = _ui!;
            var def = Catalogo?.Buscar(n.AcaoId);
            var retF = new RectangleF(n.Posicao.X, n.Posicao.Y, LarguraNo, AlturaNo);
            var ret = Rectangle.Round(retF);
            Color acento = def?.Cor ?? ui.Cor("Primaria");
            bool selecionado = ReferenceEquals(n, Selecionado);

            Color corBorda = n.Estado switch
            {
                EstadoNo.Executando => ui.Cor("Aviso"),
                EstadoNo.Sucesso => ui.Cor("Sucesso"),
                EstadoNo.Erro => ui.Cor("Perigo"),
                _ => selecionado ? ui.Cor("Primaria") : ui.Cor("Borda")
            };
            Color fundo = n.Estado == EstadoNo.Executando
                ? Mistura(ui.Cor("SuperficieAlt"), ui.Cor("Aviso"), 0.12f)
                : ui.Cor("SuperficieAlt");

            using var caminho = Desenho.Arredondado(ret, 6);
            using (var br = new SolidBrush(fundo)) g.FillPath(br, caminho);

            // faixa de destaque a esquerda (recortada pelo cartao arredondado)
            g.SetClip(caminho, CombineMode.Intersect);
            using (var br = new SolidBrush(acento)) g.FillRectangle(br, ret.X, ret.Y, 5, ret.Height);
            g.ResetClip();

            using (var pen = new Pen(corBorda, selecionado || n.Estado != EstadoNo.Normal ? 2f : 1f)) g.DrawPath(pen, caminho);

            // icone
            var tile = new Rectangle(ret.X + 14, ret.Y + (ret.Height - 34) / 2, 34, 34);
            using (var br = new SolidBrush(Mistura(ui.Cor("Fundo"), acento, 0.18f))) g.FillRectangle(br, tile);
            using (var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var br = new SolidBrush(acento))
                g.DrawString(def?.Icone ?? "?", _fIcone!, br, tile, fmt);

            // titulo e resumo
            var areaTexto = new RectangleF(ret.X + 56, 0, ret.Width - 64, 20);
            using var fmtTexto = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
            using (var br = new SolidBrush(ui.Cor("Texto")))
                g.DrawString(n.Titulo(def), _fTitulo!, br, new RectangleF(areaTexto.X, ret.Y + 12, areaTexto.Width, 20), fmtTexto);

            string resumo = def != null ? def.FormatarResumo(n) : "ação desconhecida";
            if (resumo.Length > 0)
                using (var br = new SolidBrush(ui.Cor("TextoSecundario")))
                    g.DrawString(resumo, _fResumo!, br, new RectangleF(areaTexto.X, ret.Y + 34, areaTexto.Width, 18), fmtTexto);

            // portas: entrada (esq.) e saida (dir.)
            bool inicio = def?.Executor == "fluxo.inicio", fim = def?.Executor == "fluxo.fim";
            if (!inicio) DesenharPorta(g, new PointF(ret.X, ret.Y + ret.Height / 2f), ui.Cor("Superficie"), ui.Cor("Primaria"));
            if (!fim) DesenharPorta(g, new PointF(ret.Right, ret.Y + ret.Height / 2f), ui.Cor("Superficie"), ui.Cor("Primaria"));
        }

        private static void DesenharPorta(Graphics g, PointF centro, Color fundo, Color borda)
        {
            var r = new RectangleF(centro.X - RaioPorta, centro.Y - RaioPorta, RaioPorta * 2, RaioPorta * 2);
            using (var br = new SolidBrush(fundo)) g.FillEllipse(br, r);
            using (var pen = new Pen(borda, 2f)) g.DrawEllipse(pen, r);
        }

        private static Color Mistura(Color a, Color b, float t) => Color.FromArgb(
            255,
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t));

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _fTitulo?.Dispose(); _fResumo?.Dispose(); _fIcone?.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
