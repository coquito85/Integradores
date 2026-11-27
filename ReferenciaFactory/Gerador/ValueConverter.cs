using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;

using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;

namespace ReferenciaFactory.Gerador;

/// <summary>
/// Conversao de valores lidos do banco para JSON. Feita em C# (e nao em CAST/FORMAT no SQL),
/// o que torna o mesmo layout portavel entre SQL Server, PostgreSQL, Access, Oracle etc.
/// </summary>
public static class ValueConverter
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private static readonly CultureInfo Br = new("pt-BR");

    private static readonly string[] DateFormats =
    {
        "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss.FFFFFFF",
        "dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss", "yyyyMMdd"
    };

    public static bool IsNull(object? v) => v is null || v is DBNull;

    /// <summary>
    /// Nos layouts o formato de data vem como "yyyy-mm-dd", onde mm significa MES. Em .NET "mm" e MINUTO,
    /// entao "mm" e tratado como mes, exceto quando esta ao lado de hora (HH:mm) ou segundos (mm:ss).
    /// </summary>
    public static string? NormalizeDateFormat(string? f)
    {
        if (string.IsNullOrWhiteSpace(f)) return f;
        var sb = new StringBuilder();
        for (var i = 0; i < f.Length; i++)
        {
            if (f[i] == 'm' && i + 1 < f.Length && f[i + 1] == 'm')
            {
                var minutes = PrecededByHour(f, i) || FollowedBySecond(f, i + 2);
                sb.Append(minutes ? "mm" : "MM");
                i++;
            }
            else sb.Append(f[i]);
        }
        return sb.ToString();
    }

    private static bool PrecededByHour(string f, int i)
    {
        var j = i - 1;
        if (j >= 0 && (f[j] == ':' || f[j] == '.')) j--;
        return j >= 0 && (f[j] == 'h' || f[j] == 'H');
    }

    private static bool FollowedBySecond(string f, int j)
    {
        if (j < f.Length && (f[j] == ':' || f[j] == '.')) j++;
        return j < f.Length && (f[j] == 's' || f[j] == 'S');
    }

    /// <summary>Converte conforme o type declarado no layout. Lanca FormatException se o valor nao servir.</summary>
    public static JsonNode? ToJson(string type, object? raw, string? format)
    {
        if (IsNull(raw)) return null;
        switch (type.ToLowerInvariant())
        {
            case "bool":
            case "boolean":
                return JsonValue.Create(ToBool(raw!));
            case "decimal":
            case "money":
            case "currency":
                return JsonValue.Create(ToDecimal(raw!));
            case "int":
            case "integer":
            case "long":
                return JsonValue.Create(ToLong(raw!));
            case "double":
            case "float":
                return JsonValue.Create(Convert.ToDouble(ToDecimal(raw!), Inv));
            case "datetime":
            case "date":
                return JsonValue.Create(FormatDate(ToDate(raw!), format, type.Equals("date", StringComparison.OrdinalIgnoreCase)));
            default:
                return JsonValue.Create(ToText(raw!));
        }
    }

    /// <summary>Texto para exibir uma celula na previa (grade da tela).</summary>
    public static string? Display(string type, object? raw, string? format)
    {
        if (IsNull(raw)) return null;
        try
        {
            var node = ToJson(type, raw, format);
            if (node is null) return null;
            if (node is JsonValue v && v.TryGetValue<string>(out var str)) return str;
            return node.ToJsonString();
        }
        catch (FormatException)
        {
            return ToText(raw!);
        }
    }

    public static string ToText(object raw) => raw switch
    {
        string s => s,
        DateTime dt => dt.TimeOfDay == TimeSpan.Zero ? dt.ToString("yyyy-MM-dd", Inv) : dt.ToString("yyyy-MM-ddTHH:mm:ss", Inv),
        DateTimeOffset dto => dto.ToString("yyyy-MM-ddTHH:mm:ss", Inv),
        IFormattable f => f.ToString(null, Inv),
        byte[] bytes => Convert.ToHexString(bytes),
        _ => Convert.ToString(raw, Inv) ?? ""
    };

    public static bool ToBool(object raw)
    {
        switch (raw)
        {
            case bool b: return b;
            case string s:
                switch (s.Trim().ToLowerInvariant())
                {
                    case "true": case "1": case "-1": case "s": case "sim": case "y": case "yes": case "verdadeiro": return true;
                    case "false": case "0": case "n": case "nao": case "não": case "no": case "falso": return false;
                    default: throw new FormatException($"'{s}' nao e um valor booleano.");
                }
            default:
                try { return Convert.ToDecimal(raw, Inv) != 0; }
                catch (Exception) { throw new FormatException($"'{raw}' nao e um valor booleano."); }
        }
    }

    public static decimal ToDecimal(object raw)
    {
        switch (raw)
        {
            case decimal d: return d;
            case string s: return ParseDecimal(s);
            case DateTime: throw new FormatException("Data nao pode virar numero.");
            default:
                try { return Convert.ToDecimal(raw, Inv); }
                catch (Exception) { throw new FormatException($"'{raw}' nao e numerico."); }
        }
    }

    private static decimal ParseDecimal(string s)
    {
        var t = s.Replace("R$", "", StringComparison.OrdinalIgnoreCase).Trim();
        if (t.Length == 0) throw new FormatException("Valor numerico vazio.");
        var comma = t.LastIndexOf(',');
        var dot = t.LastIndexOf('.');
        var ptBr = comma >= 0 && comma > dot;   // 1.234,56  ou  1234,56
        if (decimal.TryParse(t, NumberStyles.Number | NumberStyles.AllowExponent, ptBr ? Br : Inv, out var d)) return d;
        throw new FormatException($"'{s}' nao e numerico.");
    }

    public static long ToLong(object raw)
    {
        switch (raw)
        {
            case long l: return l;
            case int i: return i;
            case string s:
                var d = ParseDecimal(s);
                if (d != decimal.Truncate(d)) throw new FormatException($"'{s}' nao e inteiro.");
                return (long)d;
            default:
                var dec = ToDecimal(raw);
                if (dec != decimal.Truncate(dec)) throw new FormatException($"'{raw}' nao e inteiro.");
                return (long)dec;
        }
    }

    public static DateTime ToDate(object raw)
    {
        switch (raw)
        {
            case DateTime dt: return dt;
            case DateTimeOffset dto: return dto.DateTime;
            case string s:
                var t = s.Trim();
                if (DateTime.TryParseExact(t, DateFormats, Inv, DateTimeStyles.None, out var a)) return a;
                if (DateTime.TryParse(t, Br, DateTimeStyles.None, out var b)) return b;
                if (DateTime.TryParse(t, Inv, DateTimeStyles.None, out var c)) return c;
                throw new FormatException($"'{s}' nao e uma data valida.");
            default:
                throw new FormatException($"'{raw}' nao e uma data valida.");
        }
    }

    private static string FormatDate(DateTime dt, string? format, bool dateOnly)
    {
        var f = NormalizeDateFormat(format);
        if (!string.IsNullOrWhiteSpace(f)) return dt.ToString(f, Inv);
        return dateOnly ? dt.ToString("yyyy-MM-dd", Inv) : dt.ToString("yyyy-MM-ddTHH:mm:ss", Inv);
    }

    /// <summary>Valor digitado num filtro, convertido para o tipo da coluna (vai como parametro, nunca concatenado).</summary>
    public static object ParseFilterValue(string columnType, string text)
    {
        switch (columnType.ToLowerInvariant())
        {
            case "bool": case "boolean": return ToBool(text);
            case "decimal": case "money": case "currency": case "double": case "float": return ToDecimal(text);
            case "int": case "integer": case "long": return ToLong(text);
            case "datetime": case "date": return ToDate(text);
            default: return text;
        }
    }
}
