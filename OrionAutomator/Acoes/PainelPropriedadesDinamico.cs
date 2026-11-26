using OrionAutomator.Tema;

namespace OrionAutomator.Acoes
{
    /// <summary>
    /// Painel "Propriedades": monta os campos do no selecionado a partir das &lt;Propriedade&gt; da acao (Acoes.xml).
    /// Tipos: texto, textoLongo, int, numero, bool, lista, arquivo. Valor invalido fica com o texto na cor de perigo
    /// e a descricao da propriedade aparece no rodape ao focar o campo.
    /// </summary>
    public sealed class PainelPropriedadesDinamico : Panel
    {
        /// <summary>Um valor do no mudou (redesenhar o cartao, marcar o fluxo como alterado).</summary>
        public event Action<NoFluxo>? ValorAlterado;

        private readonly Panel _corpo = new();
        private readonly Label _rodape = new();

        private UiConfig? _ui;
        private CatalogoAcoes? _catalogo;
        private NoFluxo? _no;
        private bool _montando;

        public PainelPropriedadesDinamico()
        {
            _rodape.Dock = DockStyle.Bottom;
            _rodape.Height = 54;
            _rodape.Padding = new Padding(10, 6, 10, 6);

            _corpo.Dock = DockStyle.Fill;
            _corpo.AutoScroll = true;

            Controls.Add(_corpo);
            Controls.Add(_rodape);
        }

        public void Configurar(CatalogoAcoes catalogo, UiConfig ui)
        {
            _catalogo = catalogo;
            _ui = ui;

            BackColor = ui.Cor("Superficie");
            _corpo.BackColor = ui.Cor("Superficie");
            _rodape.BackColor = ui.Cor("SuperficieAlt");
            _rodape.ForeColor = ui.Cor("TextoSecundario");
            _rodape.Font = ui.Fonte("ItemMeta");

            if (_corpo.IsHandleCreated) Desenho.TemaRolagem(_corpo, ui.Escuro);
            else _corpo.HandleCreated += (_, _) => Desenho.TemaRolagem(_corpo, ui.Escuro);

            Mostrar(_no);
        }

        public void Mostrar(NoFluxo? no)
        {
            _no = no;
            if (_ui == null || _catalogo == null) return;

            _montando = true;
            _corpo.SuspendLayout();
            foreach (Control c in _corpo.Controls.Cast<Control>().ToList()) { _corpo.Controls.Remove(c); c.Dispose(); }
            _rodape.Text = "";

            var def = no == null ? null : _catalogo.Buscar(no.AcaoId);
            if (no == null || def == null)
            {
                _corpo.Controls.Add(new Label
                {
                    Text = no == null ? "Selecione um nó para editar suas propriedades." : $"Ação '{no.AcaoId}' não existe no catálogo.",
                    ForeColor = _ui.Cor("TextoDesabilitado"),
                    Font = _ui.Fonte("Normal"),
                    AutoSize = false,
                    Dock = DockStyle.Top,
                    Height = _ui.Escalar(60),
                    Padding = new Padding(12, 14, 12, 0)
                });
                _corpo.ResumeLayout(true);
                _montando = false;
                return;
            }

            int y = _ui.Escalar(8);
            int margem = _ui.Escalar(12);
            int largura = Math.Max(120, _corpo.ClientSize.Width - 2 * margem - SystemInformation.VerticalScrollBarWidth);

            foreach (var grupo in def.Propriedades.GroupBy(p => p.Categoria))
            {
                var cab = new Label
                {
                    Text = grupo.Key.ToUpperInvariant(),
                    Font = new Font(_ui.Fonte("ItemMeta"), FontStyle.Bold),
                    ForeColor = def.Cor,
                    AutoSize = false,
                    Location = new Point(margem, y),
                    Size = new Size(largura, _ui.Escalar(18)),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                _corpo.Controls.Add(cab);
                y += cab.Height + _ui.Escalar(2);

                foreach (var prop in grupo)
                {
                    var rotulo = new Label
                    {
                        Text = prop.Rotulo,
                        Font = _ui.Fonte("Rotulo"),
                        ForeColor = _ui.Cor("TextoSecundario"),
                        AutoSize = false,
                        Location = new Point(margem, y),
                        Size = new Size(largura, _ui.Escalar(16)),
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    };
                    _corpo.Controls.Add(rotulo);
                    y += rotulo.Height;

                    var editor = CriarEditor(no, prop, new Point(margem, y), largura);
                    _corpo.Controls.Add(editor);
                    y += editor.Height + _ui.Escalar(8);
                }

                y += _ui.Escalar(6);
            }

            _corpo.ResumeLayout(true);
            _montando = false;
        }

        // ── Editores ────────────────────────────────────────────────────────
        private Control CriarEditor(NoFluxo no, DefinicaoPropriedade prop, Point local, int largura)
        {
            var ui = _ui!;
            string valor = no.Texto(prop.Nome, prop.Padrao);
            const AnchorStyles ancora = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            void Gravar(string novo)
            {
                if (_montando) return;
                no.Valores[prop.Nome] = novo;
                ValorAlterado?.Invoke(no);
            }

            void Descrever() => _rodape.Text = prop.Descricao.Length > 0 ? $"{prop.Rotulo}: {prop.Descricao}" : prop.Rotulo;

            switch (prop.Tipo)
            {
                case TipoPropriedade.Bool:
                {
                    var chk = new CheckBox
                    {
                        Text = "Ativado",
                        Checked = valor.Equals("true", StringComparison.OrdinalIgnoreCase),
                        ForeColor = ui.Cor("Texto"),
                        Font = ui.Fonte("Normal"),
                        Location = local,
                        Size = new Size(largura, ui.Escalar(22)),
                        Anchor = ancora
                    };
                    chk.CheckedChanged += (_, _) => Gravar(chk.Checked ? "true" : "false");
                    chk.Enter += (_, _) => Descrever();
                    return chk;
                }

                case TipoPropriedade.Lista:
                {
                    var cmb = new ComboBox
                    {
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = ui.Cor("SuperficieAlt"),
                        ForeColor = ui.Cor("Texto"),
                        Font = ui.Fonte("Normal"),
                        Location = local,
                        Width = largura,
                        Anchor = ancora
                    };
                    cmb.Items.AddRange(prop.Opcoes);
                    int idx = Array.FindIndex(prop.Opcoes, o => o.Equals(valor, StringComparison.OrdinalIgnoreCase));
                    cmb.SelectedIndex = idx >= 0 ? idx : (prop.Opcoes.Length > 0 ? 0 : -1);
                    cmb.SelectedIndexChanged += (_, _) => Gravar(cmb.SelectedItem as string ?? "");
                    cmb.Enter += (_, _) => Descrever();
                    return cmb;
                }

                case TipoPropriedade.Arquivo:
                {
                    var painel = new Panel { Location = local, Size = new Size(largura, ui.Escalar(24)), Anchor = ancora, BackColor = ui.Cor("Superficie") };
                    var txt = CriarCaixa(ui, prop, valor, Gravar, Descrever);
                    txt.Dock = DockStyle.Fill;
                    var btn = new Button
                    {
                        Text = "…",
                        Dock = DockStyle.Right,
                        Width = ui.Escalar(28),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = ui.Cor("SuperficieAlt"),
                        ForeColor = ui.Cor("Primaria"),
                        TabStop = false
                    };
                    btn.FlatAppearance.BorderColor = ui.Cor("Borda");
                    btn.Click += (_, _) =>
                    {
                        using var dlg = new OpenFileDialog { Filter = prop.Filtro, Title = prop.Rotulo };
                        if (dlg.ShowDialog(FindForm()) == DialogResult.OK) txt.Text = dlg.FileName;
                    };
                    painel.Controls.Add(txt);
                    painel.Controls.Add(btn);
                    return painel;
                }

                case TipoPropriedade.TextoLongo:
                {
                    var txt = CriarCaixa(ui, prop, valor, Gravar, Descrever);
                    txt.Multiline = true;
                    txt.AcceptsReturn = true;
                    txt.ScrollBars = ScrollBars.Vertical;
                    txt.Location = local;
                    txt.Size = new Size(largura, ui.Escalar(72));
                    txt.Anchor = ancora;
                    return txt;
                }

                default:   // texto, int, numero
                {
                    var txt = CriarCaixa(ui, prop, valor, Gravar, Descrever);
                    txt.Location = local;
                    txt.Width = largura;
                    txt.Anchor = ancora;
                    return txt;
                }
            }
        }

        private static TextBox CriarCaixa(UiConfig ui, DefinicaoPropriedade prop, string valor, Action<string> gravar, Action descrever)
        {
            var txt = new TextBox
            {
                Text = valor,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = ui.Cor("SuperficieAlt"),
                ForeColor = ui.Cor("Texto"),
                Font = ui.Fonte("Normal")
            };

            void Validar() =>
                txt.ForeColor = prop.Validar(txt.Text).Length == 0 ? ui.Cor("Texto") : ui.Cor("Perigo");

            txt.TextChanged += (_, _) => { Validar(); gravar(txt.Text); };
            txt.Enter += (_, _) => descrever();
            Validar();
            return txt;
        }
    }
}
