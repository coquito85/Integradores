using System.Text.RegularExpressions;

namespace BetaJsonIntegrator.Data.Providers;

/// <summary>
/// Validação e quoting de identificadores SQL (spec. seção 14: nomes de tabelas/colunas vêm do XML
/// e são validados; valores do usuário só entram como parâmetros).
/// </summary>
public static class SqlIdentifier
{
    private static readonly Regex Part = new(@"^[\p{L}_][\p{L}\p{N}_]*$", RegexOptions.Compiled);

    /// <summary>Aceita <c>nome</c> ou <c>schema.nome</c>.</summary>
    public static bool IsValid(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        foreach (var part in name.Split('.'))
            if (!Part.IsMatch(part)) return false;
        return true;
    }

    public static string Quote(string name, char open, char close)
    {
        if (!IsValid(name))
            throw new ArgumentException($"Identificador SQL inválido: '{name}'.", nameof(name));

        return string.Join(".", name.Split('.').Select(p => $"{open}{p}{close}"));
    }
}
