using BetaJsonIntegrator.Core.Configuration;
using BetaJsonIntegrator.Core.Mapping;
using BetaJsonIntegrator.Core.Services;
using BetaJsonIntegrator.Core.Validation;

namespace GenerateJS.Forms
{
    /// <summary>Visualizar/editar o XML, validar a configuração e manter as personalizações.</summary>
    public partial class frmConfiguracaoXML : Form
    {
        private readonly IntegratorService _svc;
        private LayoutBundle _bundle;

        /// <summary>Layout recarregado após salvamentos.</summary>
        public LayoutBundle Bundle => _bundle;

        /// <summary>True se XML ou personalizações foram salvos.</summary>
        public bool Changed { get; private set; }

        public frmConfiguracaoXML(IntegratorService svc, LayoutBundle bundle)
        {
            InitializeComponent();
            _svc = svc;
            _bundle = bundle;
            txtXml.Text = bundle.LayoutXmlText;
            Atualizar();
        }

        // ------------------------------------------------------------------
        private void Atualizar()
        {
            Text = $"Configuração XML — {_bundle.DisplayName} (versão {_bundle.Metadata.LayoutVersion})";
            var val = _svc.ValidateLayout(_bundle);
            MostrarIssues(val.Issues);
            PreencherArvore(val);
            PreencherPersonalizacoes(val);
        }

        private void MostrarIssues(IEnumerable<ValidationIssue> issues)
        {
            lstResultado.BeginUpdate();
            lstResultado.Items.Clear();
            var lista = issues.OrderByDescending(i => i.Severity).ToList();
            if (lista.Count == 0) lstResultado.Items.Add("Configuração válida, sem pendências.");
            foreach (var i in lista) lstResultado.Items.Add(i.ToString());
            lstResultado.EndUpdate();
        }

        // ---------------- estrutura interpretada ----------------
        private void PreencherArvore(LayoutValidationResult val)
        {
            treeEstrutura.BeginUpdate();
            treeEstrutura.Nodes.Clear();
            var layout = _bundle.Layout;
            if (layout != null)
            {
                var root = treeEstrutura.Nodes.Add($"Layout: {layout.Name}  [{layout.Identifier}]");
                foreach (var gen in layout.Generates)
                {
                    var gn = root.Nodes.Add($"generate: {gen.Name}");
                    var tabelas = gn.Nodes.Add("Tabelas de origem");
                    foreach (var t in gen.Tables)
                    {
                        var tn = tabelas.Nodes.Add($"{t.Alias}  →  {t.Name}");
                        foreach (var c in t.Columns)
                            tn.Nodes.Add($"{c.Name} ({c.Type})" + (c.OrderBy ? "  ↑ ordenação" : "") +
                                         (string.IsNullOrEmpty(c.Description) ? "" : "  — " + c.Description));
                    }
                    var js = gn.Nodes.Add("Estrutura JSON");
                    foreach (var def in gen.JsonDefinitions)
                        foreach (var alias in def.Aliases)
                        {
                            var an = js.Nodes.Add($"Refealias: {alias.Name}");
                            AdicionarCampos(an, alias.Fields, val.Plan);
                        }
                }
                root.ExpandAll();
            }
            treeEstrutura.EndUpdate();
        }

        private static void AdicionarCampos(TreeNode pai, List<FieldConfig> campos, MappingPlan plan)
        {
            foreach (var f in campos)
            {
                plan.Fields.TryGetValue(f, out var rf);
                string origem = rf?.Kind switch
                {
                    FieldKind.Literal => $"= \"{rf.Literal}\"",
                    FieldKind.Column => $"← {rf.Column}",
                    FieldKind.Default => $"padrão \"{rf.Literal}\"",
                    FieldKind.Pending => "⚠ PENDENTE",
                    FieldKind.EmptyContainer => "(vazio)",
                    _ => ""
                };
                var node = pai.Nodes.Add($"{f.Name} : {f.Type ?? f.RawType}  {origem}".TrimEnd());
                if (rf?.Kind == FieldKind.Pending) node.ForeColor = Color.Firebrick;
                AdicionarCampos(node, f.Children, plan);
            }
        }

        // ---------------- XML ----------------
        private void btnValidarXml_Click(object sender, EventArgs e)
        {
            var parse = XmlLayoutParser.ParseText(txtXml.Text);
            var issues = parse.Issues.ToList();
            if (parse.Layout != null && issues.All(i => i.Severity != IssueSeverity.Error))
            {
                // valida a versão editada (ainda não salva) com as personalizações atuais
                var gen = parse.Layout.Generates.FirstOrDefault();
                if (gen != null) issues.AddRange(new LayoutValidator().Validate(gen, _bundle.Customizations).Issues);
            }
            MostrarIssues(issues);
            tabPrincipal.SelectedTab = tabXml;
        }

        private void btnSalvarXml_Click(object sender, EventArgs e)
        {
            try
            {
                _bundle = _svc.Repository.SaveLayoutXml(_bundle.FolderName, txtXml.Text, chkNovaVersao.Checked);
                Changed = true;
                chkNovaVersao.Checked = false;
                Atualizar();
            }
            catch (Exception ex) when (ex is InvalidDataException or IOException or ArgumentException)
            {
                MessageBox.Show(this, ex.Message, "Não foi possível salvar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------------- personalizações ----------------
        private static readonly string[] Modos = { "Valor fixo", "Coluna", "Padrão" };

        private static string ModoParaTexto(string modo) => modo switch
        {
            CustomizationModes.Column => Modos[1],
            CustomizationModes.Default => Modos[2],
            _ => Modos[0]
        };

        private static string TextoParaModo(string? texto) =>
            texto == Modos[1] ? CustomizationModes.Column :
            texto == Modos[2] ? CustomizationModes.Default : CustomizationModes.Fixed;

        private void PreencherPersonalizacoes(LayoutValidationResult val)
        {
            dgvPers.Rows.Clear();
            var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var porCaminho = val.Plan.Fields.Keys
                .GroupBy(f => f.Path, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            // 1) campos pendentes (com personalização anterior, se houver)
            foreach (var p in val.Plan.Pending)
            {
                if (!vistos.Add(p.Path)) continue;
                var existente = _bundle.Customizations.Items.FirstOrDefault(c =>
                    string.Equals(c.Path ?? c.Name, p.Path, StringComparison.OrdinalIgnoreCase) ||
                    (string.IsNullOrEmpty(c.Path) && string.Equals(c.Name, p.Name, StringComparison.OrdinalIgnoreCase)));
                AdicionarLinha(p.Path, p.Description, p.Type, p.Required, existente);
            }

            // 2) personalizações já salvas
            foreach (var c in _bundle.Customizations.Items)
            {
                var caminho = c.Path ?? c.Name;
                if (!vistos.Add(caminho)) continue;
                porCaminho.TryGetValue(caminho, out var campo);
                if (campo == null)
                    campo = porCaminho.Values.FirstOrDefault(f => string.Equals(f.Name, c.Name, StringComparison.OrdinalIgnoreCase));
                AdicionarLinha(caminho, campo?.Description ?? "", campo?.Type ?? c.Type ?? "", campo?.Required ?? false, c);
            }

            lblPersInfo.Text = val.Plan.Pending.Count == 0
                ? "Nenhum campo pendente."
                : $"{val.Plan.Pending.Count} campo(s) pendente(s). Informe valor fixo, coluna da origem ou padrão.";
        }

        private void AdicionarLinha(string caminho, string descricao, string tipo, bool obrigatorio, Customization? existente)
        {
            string modo = existente != null ? ModoParaTexto(existente.Mode) : Modos[0];
            string valor = existente == null ? "" : (existente.Mode == CustomizationModes.Column ? existente.Source : existente.Value) ?? "";
            int idx = dgvPers.Rows.Add(caminho, descricao, tipo, obrigatorio, modo, valor, existente?.Override ?? false);
            dgvPers.Rows[idx].Tag = existente;
        }

        private void btnSalvarPers_Click(object sender, EventArgs e)
        {
            dgvPers.EndEdit();
            var set = _bundle.Customizations;

            foreach (DataGridViewRow row in dgvPers.Rows)
            {
                if (row.IsNewRow) continue;
                var caminho = (row.Cells[colPCaminho.Index].Value as string) ?? "";
                var tipo = (row.Cells[colPTipo.Index].Value as string) ?? "";
                var modo = TextoParaModo(row.Cells[colPModo.Index].Value as string);
                var valor = ((row.Cells[colPValor.Index].Value as string) ?? "").Trim();
                var sobrescrever = row.Cells[colPSobrescrever.Index].Value is true;
                var existente = row.Tag as Customization;

                if (existente != null) set.Items.Remove(existente);
                if (valor.Length == 0) continue; // vazio = sem personalização

                var item = new Customization
                {
                    Name = caminho.Split('.').Last(),
                    Path = caminho.Contains('.') ? caminho : null,
                    Type = tipo,
                    Mode = modo,
                    Override = sobrescrever
                };
                if (modo == CustomizationModes.Column) item.Source = valor; else item.Value = valor;
                set.Upsert(item);
            }

            try
            {
                _svc.Repository.SaveCustomizations(_bundle);
                _bundle = _svc.LoadLayout(_bundle.FolderName);
                Changed = true;
                Atualizar(); // revalida antes de liberar a geração
            }
            catch (IOException ex)
            {
                MessageBox.Show(this, ex.Message, "Não foi possível salvar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnFechar_Click(object sender, EventArgs e)
        {
            DialogResult = Changed ? DialogResult.OK : DialogResult.Cancel;
            Close();
        }
    }
}
