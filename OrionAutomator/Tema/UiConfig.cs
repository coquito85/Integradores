using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;
using ReferenciaStandard.Repository;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace OrionAutomator.Tema
{
    public sealed record FonteSpec(string Familia, float Tamanho, FontStyle Estilo);

    /// <summary>
    /// Tema + propriedades de controles. Camadas (a ultima vence, mesclando por nome):
    ///   1) padroes embutidos no codigo (a UI continua bonita mesmo sem nenhum XML);
    ///   2) UiConfig.xml embutido na DLL ReferenciaStandard;
    ///   3) Config\UiConfig.xml ao lado do .exe (opcional, so precisa conter o que muda).
    /// A leitura do XML e feita pelo XmlHelper; aqui so se converte texto em Color/Font.
    /// </summary>
    public sealed class UiConfig
    {
        public const string NomeArquivo = "UiConfig";

        public Dictionary<string, Color> Cores { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, FonteSpec> Fontes { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> Metricas { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Entid_JanelaDados> Janelas { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Problemas de leitura (null = tudo ok). Os padroes embutidos continuam valendo.</summary>
        public string? Erro { get; private set; }

        /// <summary>Fator de escala da tela (DeviceDpi / 96). Usado em tudo que e desenhado manualmente.</summary>
        public float Escala { get; set; } = 1f;

        private readonly Dictionary<string, Font> _cacheFontes = new(StringComparer.OrdinalIgnoreCase);

        private UiConfig() { AplicarPadroes(); }

        public static UiConfig Carregar()
        {
            var cfg = new UiConfig();
            var erros = new List<string>();

            cfg.LerCamada(FilesRepository.LerEmbutido(NomeArquivo), "UiConfig.xml embutido", erros);

            // sobreposicao externa e opcional: arquivo ausente nao e erro, arquivo invalido e (e e ignorado)
            string? externo = FilesRepository.LerExterno(NomeArquivo);
            if (externo != null) cfg.LerCamada(externo, "Config\\UiConfig.xml", erros);

            cfg.Erro = erros.Count == 0 ? null : string.Join(" | ", erros);
            return cfg;
        }

        // ── Consultas ───────────────────────────────────────────────────────
        public Color Cor(string nome) => Cores.TryGetValue(nome, out var c) ? c : Color.Magenta;
        public int Metrica(string nome, int padrao) => Metricas.TryGetValue(nome, out var m) ? m : padrao;
        public int Escalar(int valor) => (int)Math.Round(valor * Escala);

        public Font Fonte(string nome)
        {
            if (_cacheFontes.TryGetValue(nome, out var f)) return f;
            if (!Fontes.TryGetValue(nome, out var s)) s = new FonteSpec("Segoe UI", 9f, FontStyle.Regular);
            f = CriarFonte(s);
            _cacheFontes[nome] = f;
            return f;
        }

        public Color? ResolverCor(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
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
        private void LerCamada(string? xml, string origem, List<string> erros)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                erros.Add($"{origem}: não encontrado ({FilesRepository.UltimoErro})");
                return;
            }

            try { Aplicar(XmlHelper.LerUiConfig(xml)); }
            catch (Exception ex) { erros.Add($"{origem}: {ex.Message}"); }
        }

        private void Aplicar(Entid_UiConfigDados dados)
        {
            // cores podem referenciar outras cores (valor="@Primaria"): repete ate nao haver mais o que resolver
            var pendentes = dados.Cores.Where(c => !string.IsNullOrEmpty(c.Nome)).ToList();
            for (int passada = 0; passada < 4 && pendentes.Count > 0; passada++)
            {
                var proximos = new List<Entid_Atributos>();
                foreach (var e in pendentes)
                {
                    var cor = ResolverCor(e.Texto("valor"));
                    if (cor.HasValue) Cores[e.Nome!] = cor.Value; else proximos.Add(e);
                }
                if (proximos.Count == pendentes.Count) break;
                pendentes = proximos;
            }

            foreach (var e in dados.Fontes)
            {
                if (string.IsNullOrEmpty(e.Nome)) continue;
                float tam = (float)(e.Numero("tamanho") ?? 0);
                Fontes[e.Nome] = new FonteSpec(e.Texto("familia") ?? "Segoe UI", tam > 0 ? tam : 9f, ParseEstilo(e.Texto("estilo")));
                _cacheFontes.Remove(e.Nome);
            }

            foreach (var e in dados.Metricas)
                if (!string.IsNullOrEmpty(e.Nome) && e.Int("valor") is { } v) Metricas[e.Nome] = v;

            foreach (var j in dados.Janelas)
            {
                if (string.IsNullOrEmpty(j.Nome)) continue;

                if (!Janelas.TryGetValue(j.Nome, out var atual))
                    Janelas[j.Nome] = atual = new Entid_JanelaDados { Nome = j.Nome };
                atual.Mesclar(j);

                foreach (var c in j.Controles)
                {
                    if (string.IsNullOrEmpty(c.Nome)) continue;
                    var existente = atual.Controles.FirstOrDefault(x => string.Equals(x.Nome, c.Nome, StringComparison.OrdinalIgnoreCase));
                    if (existente == null) atual.Controles.Add(c); else existente.Mesclar(c);
                }
            }
        }

        private static FontStyle ParseEstilo(string? s)
        {
            var r = FontStyle.Regular;
            if (string.IsNullOrWhiteSpace(s)) return r;
            foreach (var parte in s.Split(new[] { '|', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
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
            C("Fundo", "#000000"); C("Superficie", "#040A05"); C("SuperficieAlt", "#0A1A0E"); C("Borda", "#0F3D1A");
            C("Primaria", "#00FF41"); C("PrimariaHover", "#5CFF85"); C("PrimariaPressionada", "#00C832"); C("PrimariaSuave", "#00200A");
            C("TextoSobrePrimaria", "#000000"); C("Texto", "#D6FFE0"); C("TextoSecundario", "#4FBF6B"); C("TextoDesabilitado", "#1F5C2E");
            C("Perigo", "#F43F5E"); C("PerigoHover", "#FB7185"); C("Aviso", "#FACC15"); C("Sucesso", "#00FF41");
        }
    }

    /// <summary>Aplica as propriedades do XML nos controles de um Form, procurando-os pelo Name.</summary>
    public static class UiConfigApplier
    {
        private sealed class Margem
        {
            public int? Direita, Inferior;   // valores logicos (96 dpi); a escala e aplicada a cada reposicionamento
            public bool Ligada;
        }

        private static readonly ConditionalWeakTable<Control, Margem> _margens = new();

        /// <summary>
        /// Aplica o XML no formulario. "escala" e 1 durante o construtor (o AutoScale do WinForms ainda vai
        /// multiplicar tudo ao abrir a janela) e DeviceDpi/96 quando reaplicado com a janela ja aberta (F5).
        /// Devolve o estado "habilitado" que o XML definiu para cada controle (padrao: true), para que as
        /// regras de licenca possam desabilitar e depois restaurar sem passar por cima do XML.
        /// </summary>
        public static Dictionary<string, bool> Aplicar(Form form, UiConfig cfg, float escala, bool inicial,
                                                       params ToolStripItem[] itensMenu)
        {
            var habilitadoXml = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            form.BackColor = cfg.Cor("Fundo");
            form.ForeColor = cfg.Cor("Texto");

            if (!cfg.Janelas.TryGetValue(form.Name, out var j)) return habilitadoXml;

            int S(int v) => (int)Math.Round(v * escala);

            if (j.Texto("titulo") is { } titulo) form.Text = titulo;

            int largura = j.Int("largura") ?? 0, altura = j.Int("altura") ?? 0;
            if (inicial && largura > 0 && altura > 0) form.ClientSize = new Size(S(largura), S(altura));

            int minL = j.Int("minLargura") ?? 0, minA = j.Int("minAltura") ?? 0;
            if (minL > 0 || minA > 0) form.MinimumSize = new Size(S(minL), S(minA));

            Desenho.BarraTitulo(form,
                j.Bool("barraTituloEscura") ?? false,
                cfg.ResolverCor(j.Texto("corBarraTitulo")),
                cfg.ResolverCor(j.Texto("corTextoBarraTitulo")),
                cfg.ResolverCor(j.Texto("corBordaJanela")));

            foreach (var cc in j.Controles)
            {
                var ctl = form.Controls.Find(cc.Nome!, true).FirstOrDefault();
                if (ctl != null)
                {
                    AplicarControle(ctl, cc, cfg, escala);
                    if (cc.Bool("habilitado") is { } h) habilitadoXml[ctl.Name] = h;
                    continue;
                }

                var item = itensMenu.FirstOrDefault(i => string.Equals(i.Name, cc.Nome, StringComparison.OrdinalIgnoreCase));
                if (item != null)
                {
                    AplicarItem(item, cc);
                    if (cc.Bool("habilitado") is { } h) habilitadoXml[item.Name!] = h;
                }
            }

            return habilitadoXml;
        }

        private static void AplicarItem(ToolStripItem item, Entid_Atributos cc)
        {
            if (cc.Texto("texto") is { } t) item.Text = t;
            if (cc.Bool("visivel") is { } v) item.Visible = v;
            if (cc.Bool("habilitado") is { } h) item.Enabled = h;
        }

        private static void AplicarControle(Control ctl, Entid_Atributos cc, UiConfig cfg, float escala)
        {
            int S(int v) => (int)Math.Round(v * escala);

            if (cc.Texto("dock") is { } d && Enum.TryParse<DockStyle>(d, true, out var dock)) ctl.Dock = dock;
            if (cc.Texto("ancora") is { } a && Enum.TryParse<AnchorStyles>(a.Replace('|', ','), true, out var anc)) ctl.Anchor = anc;
            if (cc.Texto("texto") is { } t) ctl.Text = t;

            if (cc.Int("largura") is not null || cc.Int("altura") is not null)
                ctl.Size = new Size(cc.Int("largura") is { } w ? S(w) : ctl.Width,
                                    cc.Int("altura") is { } h2 ? S(h2) : ctl.Height);

            if (ctl.Dock == DockStyle.None && (cc.Tem("x") || cc.Tem("y")))
                ctl.Location = new Point(cc.Int("x") is { } x ? S(x) : ctl.Left,
                                         cc.Int("y") is { } y ? S(y) : ctl.Top);

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
                    b.Raio = cc.Int("raio") ?? cfg.Metrica("RaioBotao", 10);   // logico: o botao escala ao desenhar
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

        // margemDireita / margemInferior: reposiciona quando o pai (ou o proprio controle) muda de tamanho.
        // O handler e ligado uma unica vez por controle; reaplicar o XML (F5) so atualiza os valores.
        private static void AplicarMargens(Control ctl, Entid_Atributos cc)
        {
            int? mr = cc.Int("margemDireita"), mb = cc.Int("margemInferior");
            if (!mr.HasValue && !mb.HasValue) return;
            var pai = ctl.Parent;
            if (pai == null) return;

            var m = _margens.GetOrCreateValue(ctl);
            m.Direita = mr;
            m.Inferior = mb;

            void Reposicionar()
            {
                float e = ctl.IsHandleCreated ? ctl.DeviceDpi / 96f : 1f;
                int x = m.Direita.HasValue ? pai.ClientSize.Width - (int)Math.Round(m.Direita.Value * e) - ctl.Width : ctl.Left;
                int y = m.Inferior.HasValue ? pai.ClientSize.Height - (int)Math.Round(m.Inferior.Value * e) - ctl.Height : ctl.Top;
                if (ctl.Location.X != x || ctl.Location.Y != y) ctl.Location = new Point(x, y);
            }

            if (!m.Ligada)
            {
                m.Ligada = true;
                pai.SizeChanged += (_, _) => Reposicionar();
                ctl.SizeChanged += (_, _) => Reposicionar();
            }

            Reposicionar();
            ctl.BringToFront();
        }
    }
}