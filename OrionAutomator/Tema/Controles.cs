using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace OrionAutomator.Tema
{
    internal static class Desenho
    {
        public static GraphicsPath Arredondado(Rectangle r, int raio)
        {
            int d = Math.Max(1, raio * 2);
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string? subApp, string? subIdList);

        /// <summary>Barras de rolagem escuras (Windows 10 1809+ / 11). Falha silenciosa nas versoes antigas.</summary>
        public static void TemaEscuro(Control c)
        {
            try { SetWindowTheme(c.Handle, "DarkMode_Explorer", null); } catch { }
        }
    }

    /// <summary>Botao plano arredondado com hover/pressionado e emblema (badge) opcional.</summary>
    public class ModernButton : Button
    {
        private bool _hover, _down;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public Color CorFundo { get; set; } = Color.FromArgb(163, 230, 53);
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public Color CorHover { get; set; } = Color.FromArgb(190, 242, 100);
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public Color CorPressionado { get; set; } = Color.FromArgb(132, 204, 22);
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public Color CorBorda { get; set; } = Color.Transparent;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public Color CorTexto { get; set; } = Color.FromArgb(10, 10, 11);
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public Color CorDesabilitado { get; set; } = Color.FromArgb(28, 28, 33);
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public Color CorTextoDesabilitado { get; set; } = Color.FromArgb(82, 82, 91);
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public Color CorEmblema { get; set; } = Color.FromArgb(244, 63, 94);
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public int Raio { get; set; } = 10;

        private string? _emblema;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? Emblema { get => _emblema; set { _emblema = value; Invalidate(); } }

        public ModernButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            UseVisualStyleBackColor = false;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Cursor = Enabled ? Cursors.Hand : Cursors.Default; base.OnEnabledChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? BackColor);

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            Color fundo = !Enabled ? CorDesabilitado : _down ? CorPressionado : _hover ? CorHover : CorFundo;
            Color texto = Enabled ? CorTexto : CorTextoDesabilitado;

            using (var path = Desenho.Arredondado(rect, Math.Min(Raio, Math.Min(Width, Height) / 2)))
            using (var br = new SolidBrush(fundo))
            {
                g.FillPath(br, path);
                if (CorBorda.A > 0 && Enabled)
                {
                    using var pen = new Pen(_hover ? CorTexto : CorBorda);
                    g.DrawPath(pen, path);
                }
            }

            TextRenderer.DrawText(g, Text, Font, ClientRectangle, texto,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

            if (!string.IsNullOrEmpty(_emblema))
            {
                var r = new Rectangle(Width - 18, 2, 16, 16);
                using var br = new SolidBrush(CorEmblema);
                g.FillEllipse(br, r);
                using var f = new Font("Segoe UI", 7f, FontStyle.Bold);
                TextRenderer.DrawText(g, _emblema, f, r, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }
    }

    /// <summary>Interruptor (pilula) no lugar do CheckBox. Mantem Checked/CheckedChanged.</summary>
    public class ToggleSwitch : CheckBox
    {
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public Color CorAtiva { get; set; } = Color.FromArgb(163, 230, 53);
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public Color CorInativa { get; set; } = Color.FromArgb(43, 43, 51);

        public ToggleSwitch()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            AutoSize = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? BackColor);

            const int w = 40, h = 22;
            int y = (Height - h) / 2;
            var trilho = new Rectangle(0, y, w, h);

            using (var path = Desenho.Arredondado(trilho, h / 2))
            using (var br = new SolidBrush(Enabled ? (Checked ? CorAtiva : CorInativa) : Color.FromArgb(40, 40, 46)))
                g.FillPath(br, path);

            int d = h - 6;
            int x = Checked ? w - d - 3 : 3;
            using (var br = new SolidBrush(Checked ? Color.FromArgb(10, 10, 11) : Color.FromArgb(161, 161, 170)))
                g.FillEllipse(br, x, y + 3, d, d);

            TextRenderer.DrawText(g, Text, Font, new Rectangle(w + 10, 0, Width - w - 10, Height),
                Enabled ? ForeColor : Color.FromArgb(82, 82, 91),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }

    /// <summary>ListBox owner-draw sem flicker e com barra de rolagem escura.</summary>
    public class ListaFluxos : ListBox
    {
        public ListaFluxos()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            DrawMode = DrawMode.OwnerDrawFixed;
            BorderStyle = BorderStyle.None;
            IntegralHeight = false;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Desenho.TemaEscuro(this);
        }
    }

    // ── Menu de contexto escuro ─────────────────────────────────────────────
    public sealed class MenuColorTable : ProfessionalColorTable
    {
        private readonly UiConfig _c;
        public MenuColorTable(UiConfig c) { _c = c; UseSystemColors = false; }

        public override Color ToolStripDropDownBackground => _c.Cor("Superficie");
        public override Color MenuBorder => _c.Cor("Borda");
        public override Color MenuItemBorder => Color.Transparent;
        public override Color MenuItemSelected => _c.Cor("SuperficieAlt");
        public override Color MenuItemSelectedGradientBegin => _c.Cor("SuperficieAlt");
        public override Color MenuItemSelectedGradientEnd => _c.Cor("SuperficieAlt");
        public override Color ImageMarginGradientBegin => _c.Cor("Superficie");
        public override Color ImageMarginGradientMiddle => _c.Cor("Superficie");
        public override Color ImageMarginGradientEnd => _c.Cor("Superficie");
        public override Color SeparatorDark => _c.Cor("Borda");
        public override Color SeparatorLight => _c.Cor("Borda");
    }

    public sealed class MenuRenderer : ToolStripProfessionalRenderer
    {
        private readonly UiConfig _c;
        public MenuRenderer(UiConfig c) : base(new MenuColorTable(c)) { _c = c; RoundedEdges = false; }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            Color normal = e.Item.Tag is Color tag ? tag : _c.Cor("Texto");
            e.TextColor = !e.Item.Enabled ? _c.Cor("TextoDesabilitado")
                        : e.Item.Selected ? _c.Cor("Primaria")
                        : normal;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = _c.Cor("TextoSecundario");
            base.OnRenderArrow(e);
        }
    }
}
