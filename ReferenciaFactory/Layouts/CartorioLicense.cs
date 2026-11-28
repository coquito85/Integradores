using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ReferenciaFactory.Layouts;

/// <summary>
/// Licença do cartório: quem é o cliente, o responsável e quais modelos (layouts) ele contratou.
/// Arquivo JSON assinado (RSA/SHA-256). O fornecedor assina com a chave PRIVADA (ferramenta LicenseTool);
/// o aplicativo só conhece a chave PÚBLICA e por isso consegue conferir, mas não consegue emitir.
/// Este arquivo é autocontido de propósito: o aplicativo e a LicenseTool compartilham exatamente o mesmo código.
/// </summary>
public sealed class CartorioLicense
{
    public int Versao { get; set; } = 1;

    public string CartorioId { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Cns { get; set; } = "";
    public string CodigoIbge { get; set; } = "";

    public string ResponsavelNome { get; set; } = "";
    public string ResponsavelCargo { get; set; } = "";
    public string ResponsavelEmail { get; set; } = "";

    /// <summary>yyyy-MM-dd</summary>
    public string EmitidoEm { get; set; } = "";
    /// <summary>yyyy-MM-dd. Início da vigência. Vazio = vale desde a emissão.</summary>
    public string ValidoDe { get; set; } = "";
    /// <summary>yyyy-MM-dd. Fim da vigência. Vazio = sem validade.</summary>
    public string Validade { get; set; } = "";

    /// <summary>Ids dos layouts contratados (comparação sem diferenciar maiúsculas).</summary>
    public List<string> Layouts { get; set; } = new();

    /// <summary>Base64 da assinatura RSA sobre <see cref="Canonical"/>.</summary>
    public string Assinatura { get; set; } = "";

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>Texto fixo que é assinado: não depende de formatação do JSON nem da ordem dos layouts.</summary>
    public string Canonical()
    {
        var layouts = Layouts.Select(l => (l ?? "").Trim().ToUpperInvariant())
                             .Where(l => l.Length > 0).Distinct().OrderBy(l => l, StringComparer.Ordinal);
        var texto = string.Join("\n", new[]
        {
            "GJS-LIC-v" + Versao,
            CartorioId.Trim(), Nome.Trim(), Cns.Trim(), CodigoIbge.Trim(),
            ResponsavelNome.Trim(), ResponsavelCargo.Trim(), ResponsavelEmail.Trim(),
            EmitidoEm.Trim(), Validade.Trim(),
            string.Join(",", layouts),
        });
        // data inicial só entra no texto assinado quando existe: licenças antigas (sem ela) continuam válidas
        if (ValidoDe.Trim().Length > 0) texto += "\nDE:" + ValidoDe.Trim();
        return texto;
    }

    public void Sign(string privateKeyXml)
    {
        using var rsa = RSA.Create();
        rsa.FromXmlString(privateKeyXml);
        var sig = rsa.SignData(Encoding.UTF8.GetBytes(Canonical()), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        Assinatura = Convert.ToBase64String(sig);
    }

    public bool Verify(string publicKeyXml)
    {
        if (string.IsNullOrWhiteSpace(Assinatura)) return false;
        try
        {
            using var rsa = RSA.Create();
            rsa.FromXmlString(publicKeyXml);
            return rsa.VerifyData(Encoding.UTF8.GetBytes(Canonical()), Convert.FromBase64String(Assinatura),
                                  HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch (Exception) { return false; }   // chave ou assinatura malformada = não confere
    }

    public bool TemLayout(string? id) =>
        !string.IsNullOrWhiteSpace(id) && Layouts.Any(l => string.Equals(l?.Trim(), id.Trim(), StringComparison.OrdinalIgnoreCase));

    public DateTime? ValidoDeData() =>
        DateTime.TryParseExact(ValidoDe, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var d) ? d : null;

    public DateTime? ValidadeData() =>
        DateTime.TryParseExact(Validade, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var d) ? d : null;

    public static CartorioLicense Read(string path) =>
        JsonSerializer.Deserialize<CartorioLicense>(File.ReadAllText(path, Encoding.UTF8), Json)
        ?? throw new InvalidDataException("Arquivo de licença vazio.");

    public void Write(string path) => File.WriteAllText(path, JsonSerializer.Serialize(this, Json), new UTF8Encoding(false));
}