using System.Globalization;
using System.Xml.Linq;

namespace OrionAutomator.Tema
{
    public sealed record FonteSpec(string Familia, float Tamanho, FontStyle Estilo);

    public sealed class ControleConfig
    {
        public string Nome { get; init; } = "";
        public Dictionary<string, string> Atributos { get; } = new(StringComparer.OrdinalIgnoreCase);

        public bool Tem(string chave) => Atributos.ContainsKey(chave);
        public string? Texto(string chave) => Atributos.TryGetValue(chave, out var v) ? v : null;

        public int? Int(string chave) =>
            Atributos.TryGetValue(chave, out var v) &&
            int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;

        public bool? Bool(string chave)
        {
            if (!Atributos.TryGetValue(chave, out var v)) return null;
            return v.Trim().ToLowerInvariant() switch
            {
                "true" or "1" or "sim" => true,
                "false" or "0" or "nao" or "não" => false,
                _ => null
            };
        }
    }

    public sealed class JanelaConfig
    {
        public string Nome { get; init; } = "";
        public string? Titulo { get; init; }
        public int Largura { get; init; }
        public int Altura { get; init; }
        public int MinLargura { get; init; }
        public int MinAltura { get; init; }
        public Dictionary<string, ControleConfig> Controles { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Tema + propriedades de controles lidos de Config/UiConfig.xml (com padroes embutidos).</summary>
    public sealed class UiConfig
    {
        public Dictionary<string, Color> Cores { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, FonteSpec> Fontes { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> Metricas { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, JanelaConfig> Janelas { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Erro de leitura do XML (null = ok). Os padroes embutidos continuam valendo.</summary>
        public string? Erro { get; private set; }

        private readonly Dictionary<string, Font> _cacheFontes = new(StringComparer.OrdinalIgnoreCase);

        public static string CaminhoPadrao => Path.Combine(AppContext.BaseDirectory, "Config", "UiConfig.xml");

        private UiConfig() { AplicarPadroes(); }

        public static UiConfig Carregar(string? caminho = null)
        {
            var cfg = new UiConfig();
            caminho ??= CaminhoPadrao;
            try
            {
                if (!File.Exists(caminho)) { cfg.Erro = $"Arquivo não encontrado: {caminho}"; return cfg; }
                cfg.Ler(XDocument.Load(caminho));
            }
            catch (Exception ex) { cfg.Erro = ex.Message; }
            return cfg;
        }

        // ── Consultas ───────────────────────────────────────────────────────
        public Color Cor(string nome) => Cores.TryGetValue(nome, out var c) ? c : Color.Magenta;
        public int Metrica(string nome, int padrao) => Metricas.TryGetValue(nome, out var m) ? m : padrao;

        public Font Fonte(string nome)
        {
            if (_cacheFontes.TryGetValue(nome, out var f)) return f;
            if (!Fontes.TryGetValue(nome, out var s)) s = new FonteSpec("Segoe UI", 9f, FontStyle.Regular);
            f = CriarFonte(s);
            _cacheFontes[nome] = f;
            return f;
        }

        public Color? ResolverCor(string? valor, int profundidade = 0)
        {
            if (string.IsNullOrWhiteSpace(valor) || profundidade > 5) return null;
            valor = valor.Trim();

            if (valor[0] == '@')
                return Cores.TryGetValue(valor[1..], out var c) ? c : null;

            if (valor[0] == '#')
            {
                var hex = valor[1..];
                if (hex.Length == 6 && int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
                    return Color.FromArgb(255, Color.FromArgb(rgb));
                if (hex.Length == 8 && uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var argb))
                    return Color.FromArgb((int)argb);
                return null;
            }

            var nomeada = Color.FromName(valor);
            return nomeada.IsKnownColor ? nomeada : null;
        }

        public Font? ResolverFonte(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
            valor = valor.Trim();
            if (valor[0] == '@') return Fonte(valor[1..]);

            var p = valor.Split(';');
            if (p.Length < 2 || !float.TryParse(p[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var tam)) return null;
            return CriarFonte(new FonteSpec(p[0].Trim(), tam, p.Length > 2 ? ParseEstilo(p[2]) : FontStyle.Regular));
        }

        // ── Leitura ─────────────────────────────────────────────────────────
        private void Ler(XDocument doc)
        {
            var raiz = doc.Root ?? throw new InvalidDataException("XML sem elemento raiz.");
            var tema = raiz.Element("Tema");

            if (tema != null)
            {
                foreach (var e in tema.Element("Cores")?.Elements("Cor") ?? Enumerable.Empty<XElement>())
                {
                    var nome = (string?)e.Attribute("nome");
                    var cor = ResolverCor((string?)e.Attribute("valor"));
                    if (nome != null && cor.HasValue) Cores[nome] = cor.Value;
                }

                foreach (var e in tema.Element("Fontes")?.Elements("Fonte") ?? Enumerable.Empty<XElement>())
                {
                    var nome = (string?)e.Attribute("nome");
                    if (nome == null) continue;
                    float.TryParse((string?)e.Attribute("tamanho"), NumberStyles.Float, CultureInfo.InvariantCulture, out var tam);
                    Fontes[nome] = new FonteSpec((string?)e.Attribute("familia") ?? "Segoe UI",
                                                 tam > 0 ? tam : 9f,
                                                 ParseEstilo((string?)e.Attribute("estilo")));
                    _cacheFontes.Remove(nome);
                }

                foreach (var e in tema.Element("Metricas")?.Elements("Metrica") ?? Enumerable.Empty<XElement>())
                {
                    var nome = (string?)e.Attribute("nome");
                    if (nome != null && int.TryParse((string?)e.Attribute("valor"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                        Metricas[nome] = v;
                }
            }

            foreach (var j in raiz.Element("Janelas")?.Elements("Janela") ?? Enumerable.Empty<XElement>())
            {
                var nome = (string?)j.Attribute("nome");
                if (nome == null) continue;

                var jc = new JanelaConfig
                {
                    Nome = nome,
                    Titulo = (string?)j.Attribute("titulo"),
                    Largura = IntAttr(j, "largura"),
                    Altura = IntAttr(j, "altura"),
                    MinLargura = IntAttr(j, "minLargura"),
                    MinAltura = IntAttr(j, "minAltura")
                };

                foreach (var c in j.Elements("Controle"))
                {
                    var cn = (string?)c.Attribute("nome");
                    if (cn == null) continue;
                    var cc = new ControleConfig { Nome = cn };
                    foreach (var a in c.Attributes().Where(a => a.Name != "nome"))
                        cc.Atributos[a.Name.LocalName] = a.Value;
                    jc.Controles[cn] = cc;
                }
                Janelas[nome] = jc;
            }
        }

        private static int IntAttr(XElement e, string nome) =>
            int.TryParse((string?)e.Attribute(nome), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : 0;

        private static FontStyle ParseEstilo(string? s)
        {
            var r = FontStyle.Regular;
            if (string.IsNullOrWhiteSpace(s)) return r;
            foreach (var parte in s.Split('|', ',', ' ', (char)StringSplitOptions.RemoveEmptyEntries))
                if (Enum.TryParse<FontStyle>(parte, true, out var fs)) r |= fs;
            return r;
        }

        private static Font CriarFonte(FonteSpec s)
        {
            try { return new Font(s.Familia, s.Tamanho, s.Estilo); }
            catch { return new Font("Segoe UI", s.Tamanho, s.Estilo); }
        }

        // Padroes embutidos: a UI continua bonita mesmo sem o XML.
        private void AplicarPadroes()
        {
            void C(string n, string hex) => Cores[n] = ResolverCor(hex)!.Value;
            C("Fundo", "#0A0A0B"); C("Superficie", "#131316"); C("SuperficieAlt", "#1C1C21"); C("Borda", "#2B2B33");
            C("Primaria", "#A3E635"); C("PrimariaHover", "#BEF264"); C("PrimariaPressionada", "#84CC16"); C("PrimariaSuave", "#1A2410");
            C("TextoSobrePrimaria", "#0A0A0B"); C("Texto", "#F4F4F5"); C("TextoSecundario", "#A1A1AA"); C("TextoDesabilitado", "#52525B");
            C("Perigo", "#F43F5E"); C("PerigoHover", "#FB7185"); C("Aviso", "#FACC15"); C("Sucesso", "#A3E635");
        }
    }

    /// <summary>Aplica as propriedades do XML nos controles de um Form, procurando-os pelo Name.</summary>
    public static class UiConfigApplier
    {
        public static void Aplicar(Form form, UiConfig cfg, params ToolStripItem[] itensMenu)
        {
            form.BackColor = cfg.Cor("Fundo");
            form.ForeColor = cfg.Cor("Texto");

            if (!cfg.Janelas.TryGetValue(form.Name, out var j)) return;

            if (j.Titulo != null) form.Text = j.Titulo;
            if (j.Largura > 0 && j.Altura > 0) form.ClientSize = new Size(j.Largura, j.Altura);
            if (j.MinLargura > 0 || j.MinAltura > 0) form.MinimumSize = new Size(j.MinLargura, j.MinAltura);

            foreach (var cc in j.Controles.Values)
            {
                var ctl = form.Controls.Find(cc.Nome, true).FirstOrDefault();
                if (ctl != null) { AplicarControle(ctl, cc, cfg); continue; }

                var item = itensMenu.FirstOrDefault(i => string.Equals(i.Name, cc.Nome, StringComparison.OrdinalIgnoreCase));
                if (item != null) AplicarItem(item, cc);
            }
        }

        private static void AplicarItem(ToolStripItem item, ControleConfig cc)
        {
            if (cc.Texto("texto") is { } t) item.Text = t;
            if (cc.Bool("visivel") is { } v) item.Visible = v;
            if (cc.Bool("habilitado") is { } h) item.Enabled = h;
        }

        private static void AplicarControle(Control ctl, ControleConfig cc, UiConfig cfg)
        {
            if (cc.Texto("dock") is { } d && Enum.TryParse<DockStyle>(d, true, out var dock)) ctl.Dock = dock;
            if (cc.Texto("texto") is { } t) ctl.Text = t;

            if (cc.Int("largura") is not null || cc.Int("altura") is not null)
                ctl.Size = new Size(cc.Int("largura") ?? ctl.Width, cc.Int("altura") ?? ctl.Height);

            if (ctl.Dock == DockStyle.None && (cc.Tem("x") || cc.Tem("y")))
                ctl.Location = new Point(cc.Int("x") ?? ctl.Left, cc.Int("y") ?? ctl.Top);

            if (cc.Bool("visivel") is { } v) ctl.Visible = v;
            if (cc.Bool("habilitado") is { } h) ctl.Enabled = h;
            if (cc.Int("tabIndex") is { } ti) ctl.TabIndex = ti;

            if (cfg.ResolverCor(cc.Texto("corFundo")) is { } bg) ctl.BackColor = bg;
            if (cfg.ResolverCor(cc.Texto("corTexto")) is { } fg) ctl.ForeColor = fg;
            if (cfg.ResolverFonte(cc.Texto("fonte")) is { } f) ctl.Font = f;

            switch (ctl)
            {
                case ModernButton b:
                    if (cfg.ResolverCor(cc.Texto("corFundo")) is { } c1) b.CorFundo = c1;
                    if (cfg.ResolverCor(cc.Texto("corHover")) is { } c2) b.CorHover = c2;
                    if (cfg.ResolverCor(cc.Texto("corPressionado")) is { } c3) b.CorPressionado = c3;
                    if (cfg.ResolverCor(cc.Texto("corBorda")) is { } c4) b.CorBorda = c4;
                    if (cfg.ResolverCor(cc.Texto("corTexto")) is { } c5) b.CorTexto = c5;
                    b.CorDesabilitado = cfg.Cor("SuperficieAlt");
                    b.CorTextoDesabilitado = cfg.Cor("TextoDesabilitado");
                    b.Raio = cc.Int("raio") ?? cfg.Metrica("RaioBotao", 10);
                    b.Invalidate();
                    break;

                case ToggleSwitch s:
                    if (cfg.ResolverCor(cc.Texto("corAtiva")) is { } a1) s.CorAtiva = a1;
                    if (cfg.ResolverCor(cc.Texto("corInativa")) is { } a2) s.CorInativa = a2;
                    s.Invalidate();
                    break;

                case TextBoxBase tb:
                    tb.BorderStyle = BorderStyle.FixedSingle;
                    break;
            }

            AplicarMargens(ctl, cc);
        }

        // margemDireita / margemInferior: reposiciona quando o pai (ou o proprio controle) muda de tamanho
        private static void AplicarMargens(Control ctl, ControleConfig cc)
        {
            int? mr = cc.Int("margemDireita"), mb = cc.Int("margemInferior");
            if (!mr.HasValue && !mb.HasValue) return;
            var pai = ctl.Parent;
            if (pai == null) return;

            void Reposicionar()
            {
                int x = mr.HasValue ? pai.ClientSize.Width - mr.Value - ctl.Width : ctl.Left;
                int y = mb.HasValue ? pai.ClientSize.Height - mb.Value - ctl.Height : ctl.Top;
                ctl.Location = new Point(x, y);
            }

            pai.SizeChanged += (_, _) => Reposicionar();
            ctl.SizeChanged += (_, _) => Reposicionar();
            Reposicionar();
            ctl.BringToFront();
        }
    }
}
