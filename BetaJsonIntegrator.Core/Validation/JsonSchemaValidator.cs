using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BetaJsonIntegrator.Core.Validation;

public sealed class SchemaValidationResult
{
    public SchemaValidationResult(IReadOnlyList<ValidationIssue> issues, IReadOnlyList<string> unsupportedKeywords)
    {
        Issues = issues;
        UnsupportedKeywords = unsupportedKeywords;
    }

    public IReadOnlyList<ValidationIssue> Issues { get; }

    /// <summary>Palavras-chave presentes no Schema que este validador não avalia.</summary>
    public IReadOnlyList<string> UnsupportedKeywords { get; }
}

/// <summary>
/// Validador de JSON Schema (subconjunto draft-07 usado pelo Schema DOI): $ref interno, type, required,
/// properties, additionalProperties, items, enum, const, oneOf/anyOf/allOf/not, limites de tamanho/valor,
/// pattern e format date/date-time. Palavras-chave não avaliadas são reportadas em
/// <see cref="SchemaValidationResult.UnsupportedKeywords"/>. Os erros trazem o caminho
/// (ex.: <c>declaracoes[0].adquirentes[0].ni</c>) e nunca o valor inválido.
/// Para validação integral do draft, a interface pode ser trocada por uma biblioteca dedicada.
/// </summary>
public static class JsonSchemaValidator
{
    private static readonly JsonDocumentOptions Options = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly HashSet<string> Unsupported = new(StringComparer.Ordinal)
    {
        "if", "then", "else", "patternProperties", "propertyNames", "dependencies", "dependentSchemas",
        "dependentRequired", "contains", "uniqueItems", "multipleOf", "prefixItems", "additionalItems",
        "minProperties", "maxProperties", "unevaluatedProperties", "unevaluatedItems"
    };

    public static SchemaValidationResult Validate(string instanceJson, string schemaText, int maxIssues = 1000)
    {
        var issues = new List<ValidationIssue>();

        JsonDocument schemaDoc;
        try
        {
            schemaDoc = JsonDocument.Parse(schemaText, Options);
        }
        catch (JsonException ex)
        {
            issues.Add(ValidationIssue.Error("SCHEMA_INVALID", "schema.json", "JSON Schema inválido: " + ex.Message));
            return new SchemaValidationResult(issues, Array.Empty<string>());
        }

        using (schemaDoc)
        using (var instance = JsonDocument.Parse(instanceJson))
        {
            var walker = new Walker(schemaDoc.RootElement, maxIssues);
            walker.Eval(instance.RootElement, schemaDoc.RootElement, "", issues, 0);
            if (walker.Truncated)
                issues.Add(ValidationIssue.Info("SCHEMA_TRUNCATED", "", $"Listagem limitada a {maxIssues} erros de Schema."));
            return new SchemaValidationResult(issues, walker.UnsupportedFound.OrderBy(k => k, StringComparer.Ordinal).ToList());
        }
    }

    private sealed class Walker
    {
        private readonly JsonElement _root;
        private readonly int _max;

        public Walker(JsonElement root, int max)
        {
            _root = root;
            _max = max;
        }

        public bool Truncated { get; private set; }
        public HashSet<string> UnsupportedFound { get; } = new(StringComparer.Ordinal);

        private void Err(List<ValidationIssue> errs, string code, string path, string message)
        {
            if (errs.Count >= _max) { Truncated = true; return; }
            errs.Add(ValidationIssue.Error(code, path, message));
        }

        private static string Join(string path, string name) => path.Length == 0 ? name : path + "." + name;

        public void Eval(JsonElement value, JsonElement schema, string path, List<ValidationIssue> errs, int depth)
        {
            if (depth > 64) return;
            if (schema.ValueKind == JsonValueKind.False) { Err(errs, "SCHEMA_FALSE", path, "Valor não permitido pelo Schema."); return; }
            if (schema.ValueKind != JsonValueKind.Object) return;

            foreach (var prop in schema.EnumerateObject())
            {
                switch (prop.Name)
                {
                    case "$ref":
                        if (prop.Value.ValueKind == JsonValueKind.String && TryResolve(prop.Value.GetString()!, out var target))
                            Eval(value, target, path, errs, depth + 1);
                        else
                            UnsupportedFound.Add("$ref (externo ou não resolvido)");
                        break;

                    case "type": CheckType(value, schema, prop.Value, path, errs); break;
                    case "enum": CheckEnum(value, prop.Value, path, errs); break;
                    case "const":
                        if (!JsonEquals(value, prop.Value)) Err(errs, "SCHEMA_CONST", path, "Valor diferente do valor constante exigido pelo Schema.");
                        break;

                    case "required":
                        if (value.ValueKind == JsonValueKind.Object && prop.Value.ValueKind == JsonValueKind.Array)
                            foreach (var req in prop.Value.EnumerateArray())
                            {
                                var name = req.GetString() ?? "";
                                if (!value.TryGetProperty(name, out _))
                                    Err(errs, "SCHEMA_REQUIRED", Join(path, name), "Propriedade obrigatória ausente.");
                            }
                        break;

                    case "properties":
                        if (value.ValueKind == JsonValueKind.Object && prop.Value.ValueKind == JsonValueKind.Object)
                            foreach (var p in prop.Value.EnumerateObject())
                                if (value.TryGetProperty(p.Name, out var child))
                                    Eval(child, p.Value, Join(path, p.Name), errs, depth + 1);
                        break;

                    case "additionalProperties":
                        if (value.ValueKind == JsonValueKind.Object) CheckAdditional(value, schema, prop.Value, path, errs, depth);
                        break;

                    case "items":
                        if (value.ValueKind == JsonValueKind.Array)
                        {
                            if (prop.Value.ValueKind == JsonValueKind.Array) { UnsupportedFound.Add("items (lista)"); break; }
                            int i = 0;
                            foreach (var item in value.EnumerateArray())
                                Eval(item, prop.Value, $"{path}[{i++}]", errs, depth + 1);
                        }
                        break;

                    case "minItems":
                        if (value.ValueKind == JsonValueKind.Array && value.GetArrayLength() < prop.Value.GetInt32())
                            Err(errs, "SCHEMA_MIN_ITEMS", path, $"A lista deve ter ao menos {prop.Value.GetInt32()} item(ns).");
                        break;
                    case "maxItems":
                        if (value.ValueKind == JsonValueKind.Array && value.GetArrayLength() > prop.Value.GetInt32())
                            Err(errs, "SCHEMA_MAX_ITEMS", path, $"A lista deve ter no máximo {prop.Value.GetInt32()} item(ns).");
                        break;

                    case "minLength":
                        if (value.ValueKind == JsonValueKind.String && value.GetString()!.Length < prop.Value.GetInt32())
                            Err(errs, "SCHEMA_MIN_LENGTH", path, $"O texto deve ter ao menos {prop.Value.GetInt32()} caractere(s).");
                        break;
                    case "maxLength":
                        if (value.ValueKind == JsonValueKind.String && value.GetString()!.Length > prop.Value.GetInt32())
                            Err(errs, "SCHEMA_MAX_LENGTH", path, $"O texto deve ter no máximo {prop.Value.GetInt32()} caractere(s).");
                        break;

                    case "pattern":
                        if (value.ValueKind == JsonValueKind.String) CheckPattern(value.GetString()!, prop.Value.GetString() ?? "", path, errs);
                        break;

                    case "minimum": CheckRange(value, prop.Value, path, errs, "SCHEMA_MINIMUM", (v, l) => v >= l, "maior ou igual a"); break;
                    case "maximum": CheckRange(value, prop.Value, path, errs, "SCHEMA_MAXIMUM", (v, l) => v <= l, "menor ou igual a"); break;
                    case "exclusiveMinimum": CheckRange(value, prop.Value, path, errs, "SCHEMA_EXCL_MIN", (v, l) => v > l, "maior que"); break;
                    case "exclusiveMaximum": CheckRange(value, prop.Value, path, errs, "SCHEMA_EXCL_MAX", (v, l) => v < l, "menor que"); break;

                    case "format":
                        if (value.ValueKind == JsonValueKind.String) CheckFormat(value.GetString()!, prop.Value.GetString(), path, errs);
                        break;

                    case "allOf":
                        foreach (var sub in prop.Value.EnumerateArray()) Eval(value, sub, path, errs, depth + 1);
                        break;

                    case "anyOf":
                        {
                            bool any = false;
                            foreach (var sub in prop.Value.EnumerateArray())
                            {
                                var tmp = new List<ValidationIssue>();
                                Eval(value, sub, path, tmp, depth + 1);
                                if (tmp.Count == 0) { any = true; break; }
                            }
                            if (!any) Err(errs, "SCHEMA_ANYOF", path, "O valor não atende a nenhuma das alternativas (anyOf) do Schema.");
                            break;
                        }

                    case "oneOf":
                        {
                            int matches = 0;
                            foreach (var sub in prop.Value.EnumerateArray())
                            {
                                var tmp = new List<ValidationIssue>();
                                Eval(value, sub, path, tmp, depth + 1);
                                if (tmp.Count == 0) matches++;
                            }
                            if (matches == 0) Err(errs, "SCHEMA_ONEOF", path, DescribeOneOf(prop.Value));
                            else if (matches > 1) Err(errs, "SCHEMA_ONEOF_MULTIPLE", path, "O valor corresponde a mais de uma opção (oneOf) do Schema.");
                            break;
                        }

                    case "not":
                        {
                            var tmp = new List<ValidationIssue>();
                            Eval(value, prop.Value, path, tmp, depth + 1);
                            if (tmp.Count == 0) Err(errs, "SCHEMA_NOT", path, "O valor é proibido pelo Schema (not).");
                            break;
                        }

                    default:
                        if (Unsupported.Contains(prop.Name)) UnsupportedFound.Add(prop.Name);
                        break;
                }
            }
        }

        private void CheckType(JsonElement value, JsonElement schema, JsonElement typeEl, string path, List<ValidationIssue> errs)
        {
            if (value.ValueKind == JsonValueKind.Null &&
                schema.TryGetProperty("nullable", out var nullable) && nullable.ValueKind == JsonValueKind.True)
                return;

            var allowed = new List<string>();
            if (typeEl.ValueKind == JsonValueKind.String) allowed.Add(typeEl.GetString()!);
            else if (typeEl.ValueKind == JsonValueKind.Array)
                foreach (var t in typeEl.EnumerateArray()) allowed.Add(t.GetString() ?? "");

            if (allowed.Any(t => Matches(value, t))) return;
            Err(errs, "SCHEMA_TYPE", path, $"Tipo esperado: {string.Join(" ou ", allowed)}; encontrado: {KindName(value)}.");
        }

        private static bool Matches(JsonElement v, string type) => type switch
        {
            "string" => v.ValueKind == JsonValueKind.String,
            "number" => v.ValueKind == JsonValueKind.Number,
            "integer" => v.ValueKind == JsonValueKind.Number && IsIntegral(v),
            "boolean" => v.ValueKind is JsonValueKind.True or JsonValueKind.False,
            "object" => v.ValueKind == JsonValueKind.Object,
            "array" => v.ValueKind == JsonValueKind.Array,
            "null" => v.ValueKind == JsonValueKind.Null,
            _ => false
        };

        private static bool IsIntegral(JsonElement v)
        {
            if (v.TryGetDecimal(out var d)) return d == decimal.Truncate(d);
            var dbl = v.GetDouble();
            return dbl == Math.Floor(dbl);
        }

        private static string KindName(JsonElement v) => v.ValueKind switch
        {
            JsonValueKind.String => "string",
            JsonValueKind.Number => IsIntegral(v) ? "integer" : "number",
            JsonValueKind.True or JsonValueKind.False => "boolean",
            JsonValueKind.Object => "object",
            JsonValueKind.Array => "array",
            JsonValueKind.Null => "null",
            _ => "indefinido"
        };

        private void CheckEnum(JsonElement value, JsonElement list, string path, List<ValidationIssue> errs)
        {
            if (list.ValueKind != JsonValueKind.Array) return;
            foreach (var option in list.EnumerateArray())
                if (JsonEquals(value, option)) return;

            var shown = list.EnumerateArray().Take(12).Select(o => o.ToString());
            Err(errs, "SCHEMA_ENUM", path, "Valor não permitido. Valores aceitos: " + string.Join(", ", shown) +
                (list.GetArrayLength() > 12 ? ", ..." : "") + ".");
        }

        private static string DescribeOneOf(JsonElement options)
        {
            var consts = new List<string>();
            foreach (var o in options.EnumerateArray())
                if (o.ValueKind == JsonValueKind.Object && o.TryGetProperty("const", out var c)) consts.Add(c.ToString());

            return consts.Count > 0 && consts.Count == options.GetArrayLength()
                ? "Valor não permitido. Valores aceitos: " + string.Join(", ", consts.Take(12)) + (consts.Count > 12 ? ", ..." : "") + "."
                : "O valor não corresponde a nenhuma das opções (oneOf) do Schema.";
        }

        private void CheckAdditional(JsonElement value, JsonElement schema, JsonElement rule, string path, List<ValidationIssue> errs, int depth)
        {
            if (rule.ValueKind == JsonValueKind.True) return;

            var declared = new HashSet<string>(StringComparer.Ordinal);
            if (schema.TryGetProperty("properties", out var props) && props.ValueKind == JsonValueKind.Object)
                foreach (var p in props.EnumerateObject()) declared.Add(p.Name);

            foreach (var p in value.EnumerateObject())
            {
                if (declared.Contains(p.Name)) continue;

                if (rule.ValueKind == JsonValueKind.False)
                {
                    var hint = declared.FirstOrDefault(d => string.Equals(d, p.Name, StringComparison.OrdinalIgnoreCase));
                    Err(errs, "SCHEMA_ADDITIONAL", Join(path, p.Name),
                        "Propriedade não permitida pelo Schema" + (hint != null ? $" (grafia esperada: '{hint}')." : "."));
                }
                else if (rule.ValueKind == JsonValueKind.Object)
                {
                    Eval(p.Value, rule, Join(path, p.Name), errs, depth + 1);
                }
            }
        }

        private void CheckPattern(string text, string pattern, string path, List<ValidationIssue> errs)
        {
            try
            {
                if (!Regex.IsMatch(text, pattern, RegexOptions.None, TimeSpan.FromSeconds(1)))
                    Err(errs, "SCHEMA_PATTERN", path, $"O texto não atende ao padrão exigido: {pattern}");
            }
            catch (ArgumentException) { UnsupportedFound.Add("pattern (expressão inválida)"); }
            catch (RegexMatchTimeoutException) { UnsupportedFound.Add("pattern (tempo excedido)"); }
        }

        private void CheckRange(JsonElement value, JsonElement limit, string path, List<ValidationIssue> errs,
            string code, Func<decimal, decimal, bool> ok, string text)
        {
            if (value.ValueKind != JsonValueKind.Number || limit.ValueKind != JsonValueKind.Number) return;
            if (!value.TryGetDecimal(out var v) || !limit.TryGetDecimal(out var l)) return;
            if (!ok(v, l)) Err(errs, code, path, $"O valor deve ser {text} {l.ToString(CultureInfo.InvariantCulture)}.");
        }

        private void CheckFormat(string text, string? format, string path, List<ValidationIssue> errs)
        {
            if (format == "date" &&
                !DateTime.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                Err(errs, "SCHEMA_FORMAT", path, "Formato esperado: date (yyyy-MM-dd).");
            else if (format == "date-time" &&
                     !DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _))
                Err(errs, "SCHEMA_FORMAT", path, "Formato esperado: date-time (ISO 8601).");
        }

        private bool TryResolve(string reference, out JsonElement target)
        {
            target = _root;
            if (reference == "#") return true;
            if (!reference.StartsWith("#/", StringComparison.Ordinal)) return false;

            var current = _root;
            foreach (var raw in reference[2..].Split('/'))
            {
                var token = Uri.UnescapeDataString(raw).Replace("~1", "/").Replace("~0", "~");
                if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(token, out var next)) return false;
                current = next;
            }
            target = current;
            return true;
        }

        private static bool JsonEquals(JsonElement a, JsonElement b)
        {
            if (a.ValueKind != b.ValueKind) return false;

            switch (a.ValueKind)
            {
                case JsonValueKind.String: return a.GetString() == b.GetString();
                case JsonValueKind.Number:
                    return a.TryGetDecimal(out var x) && b.TryGetDecimal(out var y) ? x == y : a.GetDouble() == b.GetDouble();
                case JsonValueKind.Object:
                case JsonValueKind.Array: return a.GetRawText() == b.GetRawText();
                default: return true; // true/true, false/false, null/null
            }
        }
    }
}
