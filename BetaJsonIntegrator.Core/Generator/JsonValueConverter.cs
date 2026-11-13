using System.Globalization;
using System.Text.Json.Nodes;
using BetaJsonIntegrator.Core.Configuration;

namespace BetaJsonIntegrator.Core.Generator
{
    public readonly struct ConversionResult
    {
        public JsonNode? Node { get; init; }
        /// <summary>Valor ausente (null/DBNull/vazio): o campo é omitido (ou erro, se obrigatório).</summary>
        public bool IsNull { get; init; }
        /// <summary>Mensagem de erro SEM o valor original (evita vazar dados pessoais).</summary>
        public string? Error { get; init; }

        public static ConversionResult Null() => new() { IsNull = true };
        public static ConversionResult Ok(JsonNode? n) => new() { Node = n };
        public static ConversionResult Fail(string e) => new() { Error = e };
    }

    /// <summary>Conversão de tipos XML → JSON usando sempre cultura invariável.</summary>
    public static class JsonValueConverter
    {
        public const string DefaultDateFormat = "yyyy-MM-dd";

        private static readonly string[] DateInputFormats =
        {
            "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss",
            "dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy", "yyyyMMdd", "ddMMyyyy"
        };

        public static ConversionResult Convert(object? raw, string type, string? outputFormat, string? sourceFormat = null)
        {
            if (raw == null || raw is DBNull) return ConversionResult.Null();
            if (raw is string s0 && s0.Trim().Length == 0) return ConversionResult.Null();

            try
            {
                switch (type)
                {
                    case FieldTypes.String: return ToStringValue(raw, outputFormat);
                    case FieldTypes.Bool: return ToBool(raw);
                    case FieldTypes.Decimal: return ToDecimal(raw);
                    case FieldTypes.Integer: return ToInteger(raw);
                    case FieldTypes.DateTime: return ToDate(raw, outputFormat, sourceFormat);
                    default: return ConversionResult.Fail($"Tipo '{type}' não é um tipo de valor.");
                }
            }
            catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
            {
                return ConversionResult.Fail($"Valor não pôde ser convertido para {type}.");
            }
        }

        private static ConversionResult ToStringValue(object raw, string? format)
        {
            switch (raw)
            {
                case string s: return ConversionResult.Ok(JsonValue.Create(s.Trim()));
                case DateTime dt: return ConversionResult.Ok(JsonValue.Create(dt.ToString(format ?? "s", CultureInfo.InvariantCulture)));
                case DateTimeOffset dto: return ConversionResult.Ok(JsonValue.Create(dto.ToString(format ?? "s", CultureInfo.InvariantCulture)));
                case bool b: return ConversionResult.Ok(JsonValue.Create(b ? "true" : "false"));
                case IFormattable f: return ConversionResult.Ok(JsonValue.Create(f.ToString(null, CultureInfo.InvariantCulture)));
                default: return ConversionResult.Ok(JsonValue.Create(raw.ToString()));
            }
        }

        private static ConversionResult ToBool(object raw)
        {
            switch (raw)
            {
                case bool b: return ConversionResult.Ok(JsonValue.Create(b));
                case string s:
                    switch (s.Trim().ToLowerInvariant())
                    {
                        case "true": case "1": case "s": case "sim": case "t": case "y": case "yes": case "verdadeiro":
                            return ConversionResult.Ok(JsonValue.Create(true));
                        case "false": case "0": case "n": case "nao": case "não": case "f": case "no": case "falso":
                            return ConversionResult.Ok(JsonValue.Create(false));
                        default:
                            return ConversionResult.Fail("Valor não reconhecido como booleano.");
                    }
                default:
                    var d = System.Convert.ToDecimal(raw, CultureInfo.InvariantCulture);
                    return ConversionResult.Ok(JsonValue.Create(d != 0));
            }
        }

        private static ConversionResult ToDecimal(object raw)
        {
            decimal d;
            if (raw is string s)
            {
                if (!TryParseDecimal(s, out d)) return ConversionResult.Fail("Valor não reconhecido como número decimal.");
            }
            else d = System.Convert.ToDecimal(raw, CultureInfo.InvariantCulture);
            return ConversionResult.Ok(JsonValue.Create(Normalize(d)));
        }

        private static ConversionResult ToInteger(object raw)
        {
            long l;
            if (raw is string s)
            {
                if (!TryParseDecimal(s, out var d)) return ConversionResult.Fail("Valor não reconhecido como inteiro.");
                if (d != decimal.Truncate(d)) return ConversionResult.Fail("Valor não é inteiro.");
                l = (long)d;
            }
            else
            {
                var d = System.Convert.ToDecimal(raw, CultureInfo.InvariantCulture);
                if (d != decimal.Truncate(d)) return ConversionResult.Fail("Valor não é inteiro.");
                l = (long)d;
            }
            return ConversionResult.Ok(JsonValue.Create(l));
        }

        private static ConversionResult ToDate(object raw, string? outputFormat, string? sourceFormat)
        {
            var fmt = outputFormat ?? DefaultDateFormat;
            DateTime dt;
            switch (raw)
            {
                case DateTime d: dt = d; break;
                case DateTimeOffset dto: dt = dto.DateTime; break;
                case string s:
                    var formats = new List<string>();
                    if (!string.IsNullOrWhiteSpace(sourceFormat)) formats.Add(sourceFormat);
                    formats.AddRange(DateInputFormats);
                    if (!DateTime.TryParseExact(s.Trim(), formats.ToArray(), CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out dt))
                        return ConversionResult.Fail("Valor não reconhecido como data.");
                    break;
                default:
                    return ConversionResult.Fail("Valor não reconhecido como data.");
            }
            return ConversionResult.Ok(JsonValue.Create(dt.ToString(fmt, CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// Aceita "1234.56", "1234,56", "1.234,56" e "1,234.56". O último separador é o decimal.
        /// </summary>
        public static bool TryParseDecimal(string input, out decimal value)
        {
            var s = input.Trim().Replace(" ", "").Replace("R$", "");
            int lastComma = s.LastIndexOf(','), lastDot = s.LastIndexOf('.');
            if (lastComma >= 0 && lastDot >= 0)
            {
                if (lastComma > lastDot) s = s.Replace(".", "").Replace(',', '.');
                else s = s.Replace(",", "");
            }
            else if (lastComma >= 0)
            {
                s = s.Replace(',', '.');
            }
            return decimal.TryParse(s, NumberStyles.Number | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out value);
        }

        public static decimal Normalize(decimal d) => d / 1.0000000000000000000000000000m;
    }
}
