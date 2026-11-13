using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace BetaJsonIntegrator.Core.Validation
{
    /// <summary>
    /// Validador de JSON Schema embutido (sem dependências externas), cobrindo os recursos usados
    /// por schemas gerados a partir de DTOs (draft-07 / 2019-09 / 2020-12 / OpenAPI):
    /// $ref local, type, required, properties, additionalProperties, items, enum, const,
    /// oneOf/anyOf/allOf/not, pattern, min/maxLength, minimum/maximum (+exclusive), multipleOf,
    /// min/maxItems, uniqueItems, format (date, date-time, uuid), nullable.
    /// Palavras-chave não suportadas são listadas em uma mensagem informativa.
    /// Para conformidade integral com um draft específico, troque por uma biblioteca dedicada
    /// mantendo esta mesma interface (Validate).
    /// Mensagens nunca contêm o valor validado (evita expor dados pessoais).
    /// </summary>
    public sealed class JsonSchemaValidator : IDisposable
    {
        private static readonly HashSet<string> AnnotationKeywords = new(StringComparer.Ordinal)
        {
            "$schema", "$id", "id", "$comment", "title", "description", "default", "examples", "example",
            "deprecated", "readOnly", "writeOnly", "definitions", "$defs", "components", "x-enumNames",
            "xml", "externalDocs", "discriminator"
        };

        private static readonly HashSet<string> SupportedKeywords = new(StringComparer.Ordinal)
        {
            "$ref", "type", "required", "properties", "additionalProperties", "items", "enum", "const",
            "oneOf", "anyOf", "allOf", "not", "pattern", "minLength", "maxLength", "minimum", "maximum",
            "exclusiveMinimum", "exclusiveMaximum", "multipleOf", "minItems", "maxItems", "uniqueItems",
            "format", "nullable", "minProperties", "maxProperties", "title", "description"
        };

        private readonly JsonDocument _doc;
        private readonly HashSet<string> _unsupported = new(StringComparer.Ordinal);

        private JsonSchemaValidator(JsonDocument doc) => _doc = doc;

        public static JsonSchemaValidator FromText(string schemaText) =>
            new(JsonDocument.Parse(schemaText, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            }));

        public static JsonSchemaValidator FromFile(string path) => FromText(File.ReadAllText(path));

        public void Dispose() => _doc.Dispose();

        public List<ValidationIssue> Validate(JsonNode instance)
        {
            _unsupported.Clear();
            var issues = new List<ValidationIssue>();
            using var instDoc = JsonDocument.Parse(instance.ToJsonString());
            Eval(_doc.RootElement, instDoc.RootElement, "", issues, 0);

            if (_unsupported.Count > 0)
                issues.Add(ValidationIssue.Info("SCHEMA_KEYWORD_UNSUPPORTED", "",
                    "Palavras-chave do Schema não avaliadas por este validador: " + string.Join(", ", _unsupported.OrderBy(x => x)) + "."));
            return issues;
        }

        // -------------------------------------------------------------------------
        private void Eval(JsonElement schema, JsonElement inst, string path, List<ValidationIssue> issues, int depth)
        {
            if (depth > 64) return;
            if (schema.ValueKind == JsonValueKind.True) return;
            if (schema.ValueKind == JsonValueKind.False)
            {
                Err(issues, "SCHEMA_FALSE", path, "O Schema não permite este valor/propriedade.");
                return;
            }
            if (schema.ValueKind != JsonValueKind.Object) return;

            foreach (var prop in schema.EnumerateObject())
            {
                var name = prop.Name;
                if (!SupportedKeywords.Contains(name) && !AnnotationKeywords.Contains(name) && !name.StartsWith("x-", StringComparison.Ordinal))
                    _unsupported.Add(name);
            }

            if (schema.TryGetProperty("$ref", out var refEl) && refEl.ValueKind == JsonValueKind.String)
            {
                var target = ResolveRef(refEl.GetString()!);
                if (target.HasValue) Eval(target.Value, inst, path, issues, depth + 1);
                else _unsupported.Add("$ref(não resolvido: " + refEl.GetString() + ")");
            }

            bool nullable = schema.TryGetProperty("nullable", out var nu) && nu.ValueKind == JsonValueKind.True;
            if (inst.ValueKind == JsonValueKind.Null && nullable) return;

            if (schema.TryGetProperty("type", out var typeEl) && !TypeMatches(typeEl, inst))
            {
                Err(issues, "TYPE_MISMATCH", path, $"Tipo esperado: {TypeNames(typeEl)}; encontrado: {KindName(inst)}.");
                return; // demais verificações dependem do tipo
            }

            // enum / const
            if (schema.TryGetProperty("enum", out var en) && en.ValueKind == JsonValueKind.Array &&
                !en.EnumerateArray().Any(e => JsonEquals(e, inst)))
                Err(issues, "ENUM_MISMATCH", path, "Valor fora da lista de valores permitidos.");

            if (schema.TryGetProperty("const", out var cst) && !JsonEquals(cst, inst))
                Err(issues, "CONST_MISMATCH", path, "Valor diferente do valor constante exigido.");

            // composições
            if (schema.TryGetProperty("allOf", out var all) && all.ValueKind == JsonValueKind.Array)
                foreach (var s in all.EnumerateArray()) Eval(s, inst, path, issues, depth + 1);

            if (schema.TryGetProperty("anyOf", out var any) && any.ValueKind == JsonValueKind.Array)
            {
                if (!any.EnumerateArray().Any(s => Passes(s, inst, path, depth)))
                    Err(issues, "ANYOF_MISMATCH", path, "O valor não satisfaz nenhuma das alternativas permitidas (anyOf).");
            }

            if (schema.TryGetProperty("oneOf", out var one) && one.ValueKind == JsonValueKind.Array)
            {
                int matches = one.EnumerateArray().Count(s => Passes(s, inst, path, depth));
                if (matches == 0)
                    Err(issues, "ONEOF_MISMATCH", path, "O valor não corresponde a nenhuma das opções permitidas (oneOf).");
                else if (matches > 1)
                    Err(issues, "ONEOF_AMBIGUOUS", path, "O valor corresponde a mais de uma opção (oneOf).");
            }

            if (schema.TryGetProperty("not", out var not) && Passes(not, inst, path, depth))
                Err(issues, "NOT_MISMATCH", path, "O valor corresponde a um Schema proibido (not).");

            switch (inst.ValueKind)
            {
                case JsonValueKind.Object: EvalObject(schema, inst, path, issues, depth); break;
                case JsonValueKind.Array: EvalArray(schema, inst, path, issues, depth); break;
                case JsonValueKind.String: EvalString(schema, inst, path, issues); break;
                case JsonValueKind.Number: EvalNumber(schema, inst, path, issues); break;
            }
        }

        private bool Passes(JsonElement schema, JsonElement inst, string path, int depth)
        {
            var tmp = new List<ValidationIssue>();
            Eval(schema, inst, path, tmp, depth + 1);
            return tmp.All(i => i.Severity != IssueSeverity.Error);
        }

        private void EvalObject(JsonElement schema, JsonElement inst, string path, List<ValidationIssue> issues, int depth)
        {
            var present = new HashSet<string>(inst.EnumerateObject().Select(p => p.Name), StringComparer.Ordinal);

            if (schema.TryGetProperty("required", out var req) && req.ValueKind == JsonValueKind.Array)
                foreach (var r in req.EnumerateArray())
                    if (r.ValueKind == JsonValueKind.String && !present.Contains(r.GetString()!))
                        Err(issues, "REQUIRED_MISSING", Join(path, r.GetString()!), "Propriedade obrigatória ausente.");

            JsonElement props = default;
            bool hasProps = schema.TryGetProperty("properties", out props) && props.ValueKind == JsonValueKind.Object;

            foreach (var p in inst.EnumerateObject())
            {
                var childPath = Join(path, p.Name);
                if (hasProps && props.TryGetProperty(p.Name, out var propSchema))
                {
                    Eval(propSchema, p.Value, childPath, issues, depth + 1);
                }
                else if (schema.TryGetProperty("additionalProperties", out var ap))
                {
                    if (ap.ValueKind == JsonValueKind.False)
                        Err(issues, "ADDITIONAL_PROPERTY", childPath, "Propriedade não prevista no Schema (additionalProperties = false).");
                    else if (ap.ValueKind == JsonValueKind.Object)
                        Eval(ap, p.Value, childPath, issues, depth + 1);
                }
            }

            int count = present.Count;
            if (schema.TryGetProperty("minProperties", out var mn) && mn.TryGetInt32(out var mnv) && count < mnv)
                Err(issues, "MIN_PROPERTIES", path, $"Mínimo de {mnv} propriedade(s).");
            if (schema.TryGetProperty("maxProperties", out var mx) && mx.TryGetInt32(out var mxv) && count > mxv)
                Err(issues, "MAX_PROPERTIES", path, $"Máximo de {mxv} propriedade(s).");
        }

        private void EvalArray(JsonElement schema, JsonElement inst, string path, List<ValidationIssue> issues, int depth)
        {
            int len = inst.GetArrayLength();
            if (schema.TryGetProperty("minItems", out var mn) && mn.TryGetInt32(out var mnv) && len < mnv)
                Err(issues, "MIN_ITEMS", path, $"A lista exige no mínimo {mnv} item(ns).");
            if (schema.TryGetProperty("maxItems", out var mx) && mx.TryGetInt32(out var mxv) && len > mxv)
                Err(issues, "MAX_ITEMS", path, $"A lista admite no máximo {mxv} item(ns).");

            if (schema.TryGetProperty("uniqueItems", out var u) && u.ValueKind == JsonValueKind.True)
            {
                var seen = new HashSet<string>();
                foreach (var it in inst.EnumerateArray())
                    if (!seen.Add(it.GetRawText()))
                    {
                        Err(issues, "UNIQUE_ITEMS", path, "A lista contém itens duplicados.");
                        break;
                    }
            }

            if (schema.TryGetProperty("items", out var items) &&
                (items.ValueKind == JsonValueKind.Object || items.ValueKind == JsonValueKind.True || items.ValueKind == JsonValueKind.False))
            {
                int i = 0;
                foreach (var it in inst.EnumerateArray())
                    Eval(items, it, $"{path}[{i++}]", issues, depth + 1);
            }
        }

        private void EvalString(JsonElement schema, JsonElement inst, string path, List<ValidationIssue> issues)
        {
            var s = inst.GetString() ?? "";
            int len = new StringInfo(s).LengthInTextElements;

            if (schema.TryGetProperty("minLength", out var mn) && mn.TryGetInt32(out var mnv) && len < mnv)
                Err(issues, "MIN_LENGTH", path, $"Tamanho mínimo: {mnv} caractere(s).");
            if (schema.TryGetProperty("maxLength", out var mx) && mx.TryGetInt32(out var mxv) && len > mxv)
                Err(issues, "MAX_LENGTH", path, $"Tamanho máximo: {mxv} caractere(s).");

            if (schema.TryGetProperty("pattern", out var pat) && pat.ValueKind == JsonValueKind.String)
            {
                try
                {
                    if (!Regex.IsMatch(s, pat.GetString()!, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)))
                        Err(issues, "PATTERN_MISMATCH", path, $"O valor não atende ao padrão '{pat.GetString()}'.");
                }
                catch (Exception ex) when (ex is ArgumentException or RegexMatchTimeoutException)
                {
                    _unsupported.Add("pattern(inválido ou lento)");
                }
            }

            if (schema.TryGetProperty("format", out var fmt) && fmt.ValueKind == JsonValueKind.String)
            {
                switch (fmt.GetString())
                {
                    case "date":
                        if (!DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                            Err(issues, "FORMAT_DATE", path, "Data inválida; formato esperado: yyyy-MM-dd.");
                        break;
                    case "date-time":
                        if (!DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _))
                            Err(issues, "FORMAT_DATETIME", path, "Data/hora inválida; formato esperado: ISO 8601.");
                        break;
                    case "uuid":
                        if (!Guid.TryParse(s, out _))
                            Err(issues, "FORMAT_UUID", path, "UUID inválido.");
                        break;
                }
            }
        }

        private void EvalNumber(JsonElement schema, JsonElement inst, string path, List<ValidationIssue> issues)
        {
            if (!inst.TryGetDecimal(out var v)) return;

            decimal? min = Dec(schema, "minimum"), max = Dec(schema, "maximum");
            bool exMinBool = schema.TryGetProperty("exclusiveMinimum", out var exMin) && exMin.ValueKind == JsonValueKind.True;
            bool exMaxBool = schema.TryGetProperty("exclusiveMaximum", out var exMax) && exMax.ValueKind == JsonValueKind.True;

            if (min.HasValue && (exMinBool ? v <= min : v < min))
                Err(issues, "MINIMUM", path, $"Valor abaixo do mínimo permitido ({min}).");
            if (max.HasValue && (exMaxBool ? v >= max : v > max))
                Err(issues, "MAXIMUM", path, $"Valor acima do máximo permitido ({max}).");

            if (exMin.ValueKind == JsonValueKind.Number && exMin.TryGetDecimal(out var emn) && v <= emn)
                Err(issues, "EXCLUSIVE_MINIMUM", path, $"Valor deve ser maior que {emn}.");
            if (exMax.ValueKind == JsonValueKind.Number && exMax.TryGetDecimal(out var emx) && v >= emx)
                Err(issues, "EXCLUSIVE_MAXIMUM", path, $"Valor deve ser menor que {emx}.");

            var mult = Dec(schema, "multipleOf");
            if (mult.HasValue && mult.Value != 0 && v % mult.Value != 0)
                Err(issues, "MULTIPLE_OF", path, $"Valor deve ser múltiplo de {mult}.");
        }

        // ---------------- helpers ----------------
        private static decimal? Dec(JsonElement e, string name) =>
            e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number && p.TryGetDecimal(out var d) ? d : null;

        private bool TypeMatches(JsonElement typeEl, JsonElement inst)
        {
            if (typeEl.ValueKind == JsonValueKind.String) return TypeMatches(typeEl.GetString()!, inst);
            if (typeEl.ValueKind == JsonValueKind.Array)
                return typeEl.EnumerateArray().Any(t => t.ValueKind == JsonValueKind.String && TypeMatches(t.GetString()!, inst));
            return true;
        }

        private static bool TypeMatches(string type, JsonElement inst) => type switch
        {
            "string" => inst.ValueKind == JsonValueKind.String,
            "boolean" => inst.ValueKind is JsonValueKind.True or JsonValueKind.False,
            "number" => inst.ValueKind == JsonValueKind.Number,
            "integer" => inst.ValueKind == JsonValueKind.Number && inst.TryGetDecimal(out var d) && d == decimal.Truncate(d),
            "object" => inst.ValueKind == JsonValueKind.Object,
            "array" => inst.ValueKind == JsonValueKind.Array,
            "null" => inst.ValueKind == JsonValueKind.Null,
            _ => true
        };

        private static string TypeNames(JsonElement typeEl) =>
            typeEl.ValueKind == JsonValueKind.Array
                ? string.Join("|", typeEl.EnumerateArray().Select(t => t.ToString()))
                : typeEl.ToString();

        private static string KindName(JsonElement e) => e.ValueKind switch
        {
            JsonValueKind.String => "string",
            JsonValueKind.True or JsonValueKind.False => "boolean",
            JsonValueKind.Number => "number",
            JsonValueKind.Object => "object",
            JsonValueKind.Array => "array",
            JsonValueKind.Null => "null",
            _ => e.ValueKind.ToString()
        };

        private static bool JsonEquals(JsonElement a, JsonElement b)
        {
            if (a.ValueKind != b.ValueKind)
            {
                bool bothBool = a.ValueKind is JsonValueKind.True or JsonValueKind.False &&
                                b.ValueKind is JsonValueKind.True or JsonValueKind.False;
                return bothBool && a.ValueKind == b.ValueKind;
            }
            return a.ValueKind switch
            {
                JsonValueKind.Number => a.TryGetDecimal(out var x) && b.TryGetDecimal(out var y) && x == y,
                JsonValueKind.String => a.GetString() == b.GetString(),
                JsonValueKind.True or JsonValueKind.False or JsonValueKind.Null => true,
                _ => a.GetRawText() == b.GetRawText()
            };
        }

        private JsonElement? ResolveRef(string reference)
        {
            if (!reference.StartsWith("#", StringComparison.Ordinal)) return null;
            var pointer = reference.Substring(1);
            var cur = _doc.RootElement;
            if (pointer.Length == 0) return cur;
            if (!pointer.StartsWith("/", StringComparison.Ordinal)) return null;

            foreach (var raw in pointer.Substring(1).Split('/'))
            {
                var seg = Uri.UnescapeDataString(raw).Replace("~1", "/").Replace("~0", "~");
                if (cur.ValueKind == JsonValueKind.Object && cur.TryGetProperty(seg, out var next)) cur = next;
                else if (cur.ValueKind == JsonValueKind.Array && int.TryParse(seg, out var idx) && idx < cur.GetArrayLength())
                    cur = cur[idx];
                else return null;
            }
            return cur;
        }

        private static string Join(string path, string name) => path.Length == 0 ? name : path + "." + name;

        private static void Err(List<ValidationIssue> issues, string code, string path, string message) =>
            issues.Add(ValidationIssue.Error(code, path.Length == 0 ? "$" : path, message));
    }
}
