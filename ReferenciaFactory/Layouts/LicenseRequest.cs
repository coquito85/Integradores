using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ReferenciaFactory.Layouts;

/// <summary>
/// Pedido de licença: o cartório gera um código na tela de bloqueio e envia ao fornecedor,
/// que o lê no Emissor de Licenças (preenche o cadastro e, se quiser, vincula a licença à máquina).
/// O código não é secreto: só carrega os dados informados e a identificação desta máquina.
/// </summary>
public sealed class LicenseRequest
{
    public const string Prefix = "GJS1-";

    public string Nome { get; set; } = "";
    public string Cns { get; set; } = "";
    public string Responsavel { get; set; } = "";
    public string Email { get; set; } = "";
    /// <summary>Identificação desta máquina (hash do MachineGuid do Windows).</summary>
    public string Maquina { get; set; } = "";
    public string Versao { get; set; } = "";
    public string GeradoEm { get; set; } = "";

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public string Encode()
    {
        var b64 = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(this, Json))
                         .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return Prefix + b64;
    }

    public static bool TryDecode(string? code, out LicenseRequest request)
    {
        request = new LicenseRequest();
        if (string.IsNullOrWhiteSpace(code)) return false;
        try
        {
            var limpo = new string(code.Where(c => !char.IsWhiteSpace(c)).ToArray());
            if (!limpo.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)) return false;
            var b64 = limpo[Prefix.Length..].Replace('-', '+').Replace('_', '/');
            b64 = b64.PadRight(b64.Length + (4 - b64.Length % 4) % 4, '=');
            var r = JsonSerializer.Deserialize<LicenseRequest>(Convert.FromBase64String(b64), Json);
            if (r is null || string.IsNullOrWhiteSpace(r.Nome)) return false;
            request = r;
            return true;
        }
        catch (Exception) { return false; }
    }

    /// <summary>Identificação estável desta máquina, no formato XXXX-XXXX-XXXX-XXXX-XXXX.</summary>
    public static string MachineId()
    {
        var raw = "";
        try
        {
            if (OperatingSystem.IsWindows())
            {
                // visão de 64 bits mesmo com o aplicativo em x86
                using var baseKey = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry64);
                using var key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
                raw = key?.GetValue("MachineGuid") as string ?? "";
            }
        }
        catch (Exception) { /* cai no nome da máquina */ }
        if (raw.Length == 0) raw = Environment.MachineName;

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("GJS|" + raw.Trim().ToLowerInvariant())))[..20];
        return string.Join("-", Enumerable.Range(0, 5).Select(i => hash.Substring(i * 4, 4)));
    }
}