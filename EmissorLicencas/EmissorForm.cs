using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using ReferenciaFactory.Layouts;

namespace EmissorLicencas;

/// <summary>
/// Ferramenta do FORNECEDOR (não distribuir ao cliente): cadastra cartórios, escolhe os modelos contratados
/// e emite o arquivo de licença assinado (.lic) que o cartório importa no GeneratorJS.
/// </summary>
public sealed partial class EmissorForm : Form
{
    private sealed record LayoutItem(string Id, string Nome)
    {
        public override string ToString() => string.IsNullOrWhiteSpace(Nome) ? Id : $"{Nome}  ({Id})";
    }

    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    private readonly string _dadosFile;
    private readonly string _configFile;
    private readonly List<CartorioLicense> _cartorios = new();

    private bool _ocupado;

    /// <summary>Id interno do cartório em edição (gerado do nome na primeira vez; não aparece na tela).</summary>
    private string? _idAtual;

    public EmissorForm()
    {
        InitializeComponent();

        var pasta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GeneratorJS-Emissor");
        Directory.CreateDirectory(pasta);
        _dadosFile = Path.Combine(pasta, "cartorios.json");
        _configFile = Path.Combine(pasta, "config.json");

        CarregarModelos();
        CarregarDados();
        AtualizarLista(null);
        if (lstCartorios.Items.Count > 0) lstCartorios.SelectedIndex = 0; else NovoCadastro();
    }

    // ------------------------------------------------------------------ dados
    private void CarregarModelos()
    {
        try
        {
            // todos os layouts que existem nesta máquina (sem filtro de licença)
            foreach (var l in new LayoutCatalog().All.OrderBy(l => l.Name, StringComparer.OrdinalIgnoreCase))
                clbModelos.Items.Add(new LayoutItem(l.Id, l.Name), false);
        }
        catch (Exception ex)
        {
            Status("Não foi possível listar os modelos desta máquina: " + ex.Message + " Use “Adicionar id”.");
        }
    }

    private void CarregarDados()
    {
        try
        {
            if (File.Exists(_dadosFile))
                _cartorios.AddRange(JsonSerializer.Deserialize<List<CartorioLicense>>(File.ReadAllText(_dadosFile, Encoding.UTF8), Json) ?? new());
            if (File.Exists(_configFile))
            {
                var cfg = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(_configFile, Encoding.UTF8), Json);
                if (cfg is not null && cfg.TryGetValue("privateKey", out var p) && File.Exists(p)) txtChavePrivada.Text = p;
            }
        }
        catch (Exception ex) { Status("Falha ao ler os cadastros: " + ex.Message); }
    }

    private void GravarDados()
    {
        File.WriteAllText(_dadosFile, JsonSerializer.Serialize(_cartorios, Json), new UTF8Encoding(false));
    }

    private void GravarConfig()
    {
        File.WriteAllText(_configFile, JsonSerializer.Serialize(new Dictionary<string, string> { ["privateKey"] = txtChavePrivada.Text }, Json), new UTF8Encoding(false));
    }

    private void AtualizarLista(string? selecionarId)
    {
        _ocupado = true;
        _cartorios.Sort((a, b) => string.Compare(a.Nome, b.Nome, StringComparison.OrdinalIgnoreCase));
        lstCartorios.Items.Clear();
        foreach (var c in _cartorios) lstCartorios.Items.Add(c.Nome);
        _ocupado = false;
        if (selecionarId is not null)
        {
            var i = _cartorios.FindIndex(c => c.CartorioId.Equals(selecionarId, StringComparison.OrdinalIgnoreCase));
            if (i >= 0) lstCartorios.SelectedIndex = i;
        }
    }

    // ------------------------------------------------------------------ formulário
    private void AoSelecionar()
    {
        if (_ocupado || lstCartorios.SelectedIndex < 0 || lstCartorios.SelectedIndex >= _cartorios.Count) return;
        Preencher(_cartorios[lstCartorios.SelectedIndex]);
    }

    private void NovoCadastro()
    {
        _ocupado = true;
        lstCartorios.ClearSelected();
        _ocupado = false;
        _idAtual = null;
        Preencher(new CartorioLicense());
        txtNome.Focus();
    }

    private void Preencher(CartorioLicense c)
    {
        _idAtual = string.IsNullOrWhiteSpace(c.CartorioId) ? null : c.CartorioId;
        txtNome.Text = c.Nome;
        txtCns.Text = c.Cns;
        txtIbge.Text = c.CodigoIbge;
        txtRespNome.Text = c.ResponsavelNome;
        txtRespCargo.Text = c.ResponsavelCargo;
        txtRespEmail.Text = c.ResponsavelEmail;

        // intervalo de validade: novo cadastro sugere de hoje até daqui a 1 ano
        var novo = _idAtual is null;
        PreencherData(dtpInicio, c.ValidoDe, DateTime.Today, novo);
        PreencherData(dtpFim, c.Validade, DateTime.Today.AddYears(1), novo);

        // ids da licença que não existem nesta máquina entram na lista para não se perderem
        foreach (var id in c.Layouts)
            if (!clbModelos.Items.Cast<LayoutItem>().Any(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
                clbModelos.Items.Add(new LayoutItem(id, "(sem XML nesta máquina)"), false);
        for (int i = 0; i < clbModelos.Items.Count; i++)
            clbModelos.SetItemChecked(i, c.TemLayout(((LayoutItem)clbModelos.Items[i]).Id));
    }

    private static void PreencherData(DateTimePicker dtp, string valor, DateTime sugerida, bool marcarSeVazio)
    {
        if (DateTime.TryParseExact(valor, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
        { dtp.Value = d; dtp.Checked = true; }
        else
        { dtp.Value = sugerida; dtp.Checked = marcarSeVazio; }
    }

    private static string DataTexto(DateTimePicker dtp) =>
        dtp.Checked ? dtp.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "";

    /// <summary>Gera o id interno a partir do nome: sem acentos, maiúsculo, só letras/números/_.</summary>
    private static string GerarId(string nome)
    {
        var sb = new StringBuilder();
        foreach (var ch in nome.Normalize(NormalizationForm.FormD))
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (cat == UnicodeCategory.NonSpacingMark) continue;
            if (char.IsLetterOrDigit(ch)) sb.Append(char.ToUpperInvariant(ch));
            else if (sb.Length > 0 && sb[^1] != '_') sb.Append('_');
        }
        var id = sb.ToString().Trim('_');
        if (id.Length > 40) id = id[..40].Trim('_');
        return id.Length == 0 ? "CARTORIO" : id;
    }

    private CartorioLicense LerFormulario() => new()
    {
        CartorioId = _idAtual ?? GerarId(txtNome.Text),
        Nome = txtNome.Text.Trim(),
        Cns = txtCns.Text.Trim(),
        CodigoIbge = txtIbge.Text.Trim(),
        ResponsavelNome = txtRespNome.Text.Trim(),
        ResponsavelCargo = txtRespCargo.Text.Trim(),
        ResponsavelEmail = txtRespEmail.Text.Trim(),
        ValidoDe = DataTexto(dtpInicio),
        Validade = DataTexto(dtpFim),
        Layouts = clbModelos.CheckedItems.Cast<LayoutItem>().Select(i => i.Id).ToList(),
    };

    private string? Validar(CartorioLicense c)
    {
        if (c.Nome.Length == 0) return "Informe o nome do cartório.";
        if (c.ResponsavelNome.Length == 0) return "Informe o nome do responsável.";
        if (c.ValidoDe.Length > 0 && c.Validade.Length > 0 && string.CompareOrdinal(c.ValidoDe, c.Validade) > 0)
            return "A data final da validade não pode ser anterior à data inicial.";
        return null;
    }

    // ------------------------------------------------------------------ ações
    private CartorioLicense? SalvarCadastro(bool mostrarMensagem)
    {
        var c = LerFormulario();
        var erro = Validar(c);
        if (erro is not null) { MessageBox.Show(this, erro, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return null; }

        if (_idAtual is null)
        {
            // cadastro novo: se já existe outro com o mesmo nome, ganha um número no id interno
            var baseId = c.CartorioId;
            for (int n = 2; _cartorios.Any(x => x.CartorioId.Equals(c.CartorioId, StringComparison.OrdinalIgnoreCase)); n++)
                c.CartorioId = $"{baseId}_{n}";
        }
        _idAtual = c.CartorioId;

        var i = _cartorios.FindIndex(x => x.CartorioId.Equals(c.CartorioId, StringComparison.OrdinalIgnoreCase));
        if (i >= 0)
        {
            c.EmitidoEm = _cartorios[i].EmitidoEm;   // mantém a data da última emissão
            _cartorios[i] = c;
        }
        else _cartorios.Add(c);

        GravarDados();
        AtualizarLista(c.CartorioId);
        Status($"Cadastro de “{c.Nome}” salvo.");
        if (mostrarMensagem) MessageBox.Show(this, "Cadastro salvo.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        return c;
    }

    private void Excluir()
    {
        if (lstCartorios.SelectedIndex < 0) return;
        var c = _cartorios[lstCartorios.SelectedIndex];
        if (MessageBox.Show(this, $"Excluir o cadastro de “{c.Nome}”?\nLicenças já emitidas continuam valendo.", Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        _cartorios.Remove(c);
        GravarDados();
        AtualizarLista(null);
        if (lstCartorios.Items.Count > 0) lstCartorios.SelectedIndex = 0; else NovoCadastro();
    }

    private void Emitir()
    {
        if (!File.Exists(txtChavePrivada.Text))
        {
            MessageBox.Show(this, "Escolha a chave privada (Procurar…) ou gere um novo par de chaves.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var c = SalvarCadastro(mostrarMensagem: false);
        if (c is null) return;
        if (c.Layouts.Count == 0)
        {
            MessageBox.Show(this, "Marque ao menos um modelo contratado.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dlg = new SaveFileDialog
        {
            Title = "Salvar licença",
            Filter = "Licença do cartório (*.lic)|*.lic",
            FileName = $"{c.CartorioId}.lic",
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            c.EmitidoEm = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            c.Sign(File.ReadAllText(txtChavePrivada.Text));
            c.Write(dlg.FileName);

            var i = _cartorios.FindIndex(x => x.CartorioId.Equals(c.CartorioId, StringComparison.OrdinalIgnoreCase));
            if (i >= 0) _cartorios[i].EmitidoEm = c.EmitidoEm;
            GravarDados();

            Status($"Licença emitida: {dlg.FileName}");
            MessageBox.Show(this,
                $"Licença emitida para {c.Nome}.\n\nModelos: {string.Join(", ", c.Layouts)}\nValidade: {Periodo(c)}\n\nEnvie o arquivo ao cartório: no GeneratorJS, em Layouts > Importar licença.",
                Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Não foi possível emitir a licença:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ------------------------------------------------------------------ chaves
    private void EscolherChave()
    {
        using var dlg = new OpenFileDialog { Title = "Chave privada", Filter = "Chave privada (*.xml)|*.xml", CheckFileExists = true };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        txtChavePrivada.Text = dlg.FileName;
        GravarConfig();
    }

    private void GerarChaves()
    {
        using var dlg = new FolderBrowserDialog { Description = "Pasta onde guardar private.xml e public.xml (fora do repositório Git)" };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        var priv = Path.Combine(dlg.SelectedPath, "private.xml");
        var pub = Path.Combine(dlg.SelectedPath, "public.xml");
        if (File.Exists(priv) && MessageBox.Show(this,
                "Já existe um private.xml nesta pasta. Gerar novas chaves invalida TODAS as licenças emitidas com a chave antiga.\n\nContinuar?",
                Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;

        using var rsa = RSA.Create(3072);
        File.WriteAllText(priv, rsa.ToXmlString(true));
        File.WriteAllText(pub, rsa.ToXmlString(false));
        txtChavePrivada.Text = priv;
        GravarConfig();
        Clipboard.SetText(rsa.ToXmlString(false));

        MessageBox.Show(this,
            "Chaves geradas.\n\n• private.xml: SECRETO. Faça backup e nunca coloque no Git nem envie ao cliente.\n• A chave pública já foi copiada: cole em CartorioStore.PublicKeyXml no aplicativo e recompile.",
            Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void CopiarChavePublica()
    {
        if (!File.Exists(txtChavePrivada.Text))
        {
            MessageBox.Show(this, "Escolha a chave privada primeiro.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            using var rsa = RSA.Create();
            rsa.FromXmlString(File.ReadAllText(txtChavePrivada.Text));
            Clipboard.SetText(rsa.ToXmlString(false));
            Status("Chave pública copiada. Cole em CartorioStore.PublicKeyXml.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Arquivo de chave inválido:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ------------------------------------------------------------------ eventos da tela (ligados no Designer)
    private void btnProcurarChave_Click(object? sender, EventArgs e) => EscolherChave();
    private void btnGerarChaves_Click(object? sender, EventArgs e) => GerarChaves();
    private void btnCopiarPublica_Click(object? sender, EventArgs e) => CopiarChavePublica();
    private void btnNovo_Click(object? sender, EventArgs e) => NovoCadastro();
    private void btnExcluir_Click(object? sender, EventArgs e) => Excluir();
    private void btnSalvar_Click(object? sender, EventArgs e) => SalvarCadastro(mostrarMensagem: true);
    private void btnEmitir_Click(object? sender, EventArgs e) => Emitir();
    private void lstCartorios_SelectedIndexChanged(object? sender, EventArgs e) => AoSelecionar();


    private static string Periodo(CartorioLicense c)
    {
        static string F(string d) => DateTime.ParseExact(d, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
        if (c.ValidoDe.Length > 0 && c.Validade.Length > 0) return $"de {F(c.ValidoDe)} até {F(c.Validade)}";
        if (c.Validade.Length > 0) return $"até {F(c.Validade)}";
        if (c.ValidoDe.Length > 0) return $"a partir de {F(c.ValidoDe)}";
        return "sem prazo";
    }

    private void Status(string texto) => lblStatus.Text = texto;
}
