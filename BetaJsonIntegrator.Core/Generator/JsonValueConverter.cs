using System.Globalization;
using System.Text.Json.Nodes;
using BetaJsonIntegrator.Core.Configuration;

namespace BetaJsonIntegrator.Core.Generator;

/// <summary>
/// Conversão de valores de origem para o tipo JSON do campo (spec. seção 9).
/// Números usam cultura invariável (ponto decimal) e saem como número JSON, nunca como texto.
/// As mensagens de erro nunca incluem o valor convertido (dados pessoais).
/// </summary>
public static class JsonValueConverter
{
    private static readonly CultureInfo PtBr = CreatePtBr();

    private static readonly string[] DateFormats =
    {
        "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss.fff", "yyyy-MM-ddTHH:mm:ss.fffffff",
        "dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm", "dd-MM-yyyy", "yyyyMMdd"
    };

    /// <summary>
    /// Converte <paramref name="value"/>. Valor nulo/DBNull resulta em JSON null (node = null, retorno true).
    /// Retorna false quando a conversão não é possível (node = null e <paramref name="error"/> preenchido).
    /// </summary>
    public static bool TryConvert(string? type, object? value, string? format, string? cultureName,
        out JsonNode? node, out string? error)
    {
        node = null;
        error = null;

        if (value is null or DBNull) return true;
        if (value is string blank && type != FieldTypes.String && string.IsNullOrWhiteSpace(blank)) return true;

        CultureInfo? culture = null;
        if (!string.IsNullOrWhiteSpace(cultureName))
        {
            try { culture = CultureInfo.GetCultureInfo(cultureName); }
            catch (CultureNotFoundException)
            {
                error = $"Cultura '{cultureName}' desconhecida.";
                return false;
            }
        }

        switch (type)
        {
            case FieldTypes.String:
                node = JsonValue.Create(ToText(value, format));
                return true;

            case FieldTypes.Bool:
                if (TryToBool(value, out var b)) { node = JsonValue.Create(b); return true; }
                error = "Valor não convertível para booleano.";
                return false;

            case FieldTypes.Decimal:
                if (TryToDecimal(value, culture, out var d)) { node = JsonValue.Create(d); return true; }
                error = "Valor não convertível para número decimal.";
                return false;

            case FieldTypes.Integer:
                if (!TryToDecimal(value, culture, out var i))
                {
                    error = "Valor não convertível para número inteiro.";
                    return false;
                }
                if (i != decimal.Truncate(i))
                {
                    error = "O valor não é inteiro.";
                    return false;
                }
                if (i < long.MinValue || i > long.MaxValue)
                {
                    error = "O valor está fora da faixa de inteiros.";
                    return false;
                }
                node = JsonValue.Create((long)i);
                return true;

            case FieldTypes.DateTime:
                if (TryToDateTime(value, culture, out var dt)) { node = JsonValue.Create(FormatDate(dt, format)); return true; }
                error = "Valor não convertível para data.";
                return false;

            default:
                error = $"Tipo '{type}' não pode ser convertido a partir de um valor simples.";
                return false;
        }
    }

    // ------------------------------------------------------------------ texto
    private static string ToText(object value, string? format)
    {
        switch (value)
        {
            case string s: return s;
            case DateTime dt: return FormatDate(dt, format);
            case DateOnly d: return FormatDate(d.ToDateTime(TimeOnly.MinValue), format);
            case DateTimeOffset dto: return FormatDate(dto.DateTime, format);
            case bool b: return b ? "true" : "false";
            case IFormattable f: return f.ToString(null, CultureInfo.InvariantCulture);
            default: return Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
        }
    }

    private static string FormatDate(DateTime dt, string? format)
    {
        var pattern = string.IsNullOrWhiteSpace(format)
            ? (dt.TimeOfDay == TimeSpan.Zero ? "yyyy-MM-dd" : "yyyy-MM-ddTHH:mm:ss")
            : format;
        return dt.ToString(pattern, CultureInfo.InvariantCulture);
    }

    // ------------------------------------------------------------------ booleano
    private static bool TryToBool(object value, out bool result)
    {
        result = false;
        switch (value)
        {
            case bool b:
                result = b;
                return true;

            case string s:
                switch (s.Trim().ToLowerInvariant())
                {
                    case "true": case "1": case "s": case "sim": case "y": case "yes":
                        result = true;
                        return true;
                    case "false": case "0": case "n": case "nao": case "não": case "no":
                        result = false;
                        return true;
                    default:
                        return false;
                }

            default:
                if (TryToDecimal(value, null, out var d) && (d == 0m || d == 1m))
                {
                    result = d == 1m;
                    return true;
                }
                return false;
        }
    }

    // ------------------------------------------------------------------ número
    private static bool TryToDecimal(object value, CultureInfo? culture, out decimal result)
    {
        result = 0m;
        switch (value)
        {
            case decimal m:
                result = m;
                return true;

            case string s:
                return TryParseDecimal(s, culture, out result);

            case bool:
            case DateTime:
            case DateOnly:
            case DateTimeOffset:
                return false;

            case IConvertible c:
                try
                {
                    result = c.ToDecimal(CultureInfo.InvariantCulture);
                    return true;
                }
                catch (Exception ex) when (ex is FormatException or OverflowException or InvalidCastException)
                {
                    return false;
                }

            default:
                return false;
        }
    }

    /// <summary>
    /// Sem cultura informada: o último separador (',' ou '.') é o decimal; quando só há um tipo de separador,
    /// vírgula única = decimal (pt-BR) e ponto único = decimal (invariante); vários pontos = milhar.
    /// "1.234" é lido como 1,234 — informe culture="pt-BR" no campo para tratá-lo como milhar.
    /// </summary>
    internal static bool TryParseDecimal(string text, CultureInfo? culture, out decimal result)
    {
        var s = text.Replace("R$", "").Trim();

        if (culture != null)
            return decimal.TryParse(s, NumberStyles.Number, culture, out result);

        int lastDot = s.LastIndexOf('.');
        int lastComma = s.LastIndexOf(',');

        if (lastDot >= 0 && lastComma >= 0)
        {
            s = lastComma > lastDot
                ? s.Replace(".", "").Replace(',', '.')
                : s.Replace(",", "");
        }
        else if (lastComma >= 0)
        {
            s = s.IndexOf(',') == lastComma ? s.Replace(',', '.') : s.Replace(",", "");
        }
        else if (lastDot >= 0 && s.IndexOf('.') != lastDot)
        {
            s = s.Replace(".", "");
        }

        return decimal.TryParse(s, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out result);
    }

    // ------------------------------------------------------------------ data
    private static bool TryToDateTime(object value, CultureInfo? culture, out DateTime result)
    {
        result = default;
        switch (value)
        {
            case DateTime dt:
                result = dt;
                return true;
            case DateOnly d:
                result = d.ToDateTime(TimeOnly.MinValue);
                return true;
            case DateTimeOffset dto:
                result = dto.DateTime;
                return true;
            case string s:
                var text = s.Trim();
                if (DateTime.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                    return true;
                return DateTime.TryParse(text, culture ?? PtBr, DateTimeStyles.None, out result);
            default:
                return false;
        }
    }

    private static CultureInfo CreatePtBr()
    {
        try { return CultureInfo.GetCultureInfo("pt-BR"); }
        catch (CultureNotFoundException) { return CultureInfo.InvariantCulture; }
    }
}
