using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace ReferenciaStandard.Helpers
{
    public class JsonHelper
    {

        private static readonly Dictionary<string, string> Coletor = new Dictionary<string, string>()
        {
            { "Todososregistros", "Todos os registros" },
            { "ValorAtual", "Valor Atual" },
             { "ValorNovo", "Valor Novo" },
            { "Localizarem", "Localizar em" },
            { "Substituirem", "Substituir em" },
            { "Operador", "Operador" }
        };

        public static string EscapeSqlLiteral(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            //  trata aspa simples: ' -> ''
            value = value.Replace("'", "''");

            value = Regex.Replace(value, @"\\+", m =>
            {
                int len = m.Value.Length;

                if (len == 2)
                    return "\\\\\\\\";   // vira 4

                if (len > 4)
                    return "\\\\\\" + "\\"; // normaliza para 4

                return m.Value; // se já for 4, mantém
            });

            return value;
        }
        public static string AtualizarJsonEtapa(string jsonStr, string nomeEtapa, bool valor)
        {
            JObject json = string.IsNullOrWhiteSpace(jsonStr) ? new JObject() : JObject.Parse(jsonStr);

            // Garante que existe a secao Etapas
            JObject etapas = json["Etapas"] as JObject;
            if (etapas == null)
            {
                etapas = new JObject();
                json["Etapas"] = etapas;
            }

            etapas[nomeEtapa] = valor;

            return json.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        public static bool LeituraJsonEtapa(string jsonStr, string nomeEtapa)
        {
            if (string.IsNullOrWhiteSpace(jsonStr))
                return false;

            JObject etapas = JObject.Parse(jsonStr)["Etapas"] as JObject;
            JToken valor = etapas?[nomeEtapa];

            return valor != null && valor.Type == JTokenType.Boolean && valor.Value<bool>();
        }



        public static string HeaderValuesColletionJson(string verificador)
        {
            if (string.IsNullOrWhiteSpace(verificador))
                return string.Empty;

            return Coletor.TryGetValue(verificador, out var value) ? value : string.Empty;
        }


        public static bool IsHeaderValueJsonExiste(string verificador)
        {
            if (string.IsNullOrWhiteSpace(verificador))
                return false;

            return Coletor.Any(p => string.Equals(p.Key, verificador, StringComparison.OrdinalIgnoreCase));
        }



        public static string HeaderValuesRemoveSpaceColletionJson(string verificador)
        {
            if (string.IsNullOrWhiteSpace(verificador))
                return string.Empty;

            return Coletor.TryGetValue(verificador, out var value) ? value : string.Empty;
        }
        public static string HeaderKeysColletionJson(string verificador)
        {
            if (string.IsNullOrWhiteSpace(verificador))
                return string.Empty;

            //Usa LINQ para inverter a busca
            return Coletor.FirstOrDefault(p => string.Equals(p.Value, verificador, StringComparison.OrdinalIgnoreCase)).Key ?? string.Empty;
        }


        // ── Configuracao central de JSON (um unico lugar para mudar o comportamento de todo o sistema) ──
        private static JsonSerializerSettings CriarConfiguracao()
        {
            return new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DateParseHandling = DateParseHandling.DateTime,
                Converters = { new StringEnumConverter() }
            };
        }

        // ── Configuracao da ponte com a tela (JavaScript/WebView2) ──
        // Nomes em camelCase, enums em camelCase, nulos mantidos, uma linha so. Chaves de dicionario NAO mudam
        // (nomes de coluna e ids de campo precisam chegar na tela exatamente como estao).
        private static readonly JsonSerializerSettings ConfiguracaoTela = new JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.None,
            NullValueHandling = NullValueHandling.Include,
            DateParseHandling = DateParseHandling.None,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = false, OverrideSpecifiedNames = true }
            },
            Converters = { new StringEnumConverter(new CamelCaseNamingStrategy()) }
        };

        /// <summary>Objeto -> JSON para enviar a tela (camelCase).</summary>
        public static string SerializarTela<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, ConfiguracaoTela);
        }

        /// <summary>JSON recebido da tela -> objeto (nomes sem diferenciar maiusculas/minusculas).</summary>
        public static T DesserializarTela<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default(T);

            return JsonConvert.DeserializeObject<T>(json, ConfiguracaoTela);
        }

        public static string Serializar<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, CriarConfiguracao());
        }

        public static T Desserializar<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default(T);

            return JsonConvert.DeserializeObject<T>(json, CriarConfiguracao());
        }

        public static string IsSerializeJson<T>(T obj)
        {
            return Serializar(obj);
        }

        public static T IsDeserializeObjectJson<T>(string json)
        {
            return Desserializar<T>(json);
        }

        /// <summary>
        /// Le um arquivo JSON para o tipo T. Arquivo inexistente, vazio ou corrompido devolve "padrao"
        /// (nunca lanca excecao: quem chama decide o que fazer com o valor padrao).
        /// </summary>
        public static T LerArquivo<T>(string caminho, T padrao = default(T))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(caminho) || !File.Exists(caminho))
                    return padrao;

                string conteudo = File.ReadAllText(caminho);
                if (string.IsNullOrWhiteSpace(conteudo))
                    return padrao;

                T resultado = Desserializar<T>(conteudo);
                return resultado == null ? padrao : resultado;
            }
            catch
            {
                return padrao;
            }
        }

        /// <summary>
        /// Grava o objeto em JSON de forma atomica (escreve num arquivo temporario e troca no final),
        /// assim uma queda de energia no meio da gravacao nao corrompe o arquivo existente.
        /// </summary>
        public static bool GravarArquivo<T>(string caminho, T obj, out string erro)
        {
            erro = null;
            string temporario = null;
            try
            {
                string pasta = Path.GetDirectoryName(caminho);
                if (!string.IsNullOrEmpty(pasta))
                    Directory.CreateDirectory(pasta);

                temporario = caminho + ".tmp";
                File.WriteAllText(temporario, Serializar(obj), new UTF8Encoding(false));

                if (File.Exists(caminho))
                    File.Replace(temporario, caminho, null);
                else
                    File.Move(temporario, caminho);

                return true;
            }
            catch (Exception ex)
            {
                erro = ex.Message;
                try { if (temporario != null && File.Exists(temporario)) File.Delete(temporario); } catch { }
                return false;
            }
        }

        public static async Task<string> ObterGeneroAsync(string nome)
        {
            using (var client = new HttpClient())
            {
                var resultResponse = await client.GetStringAsync($"https://api.genderize.io/?name={nome}");
                //   var resultResponse = JsonConvert.DeserializeObject(response);
                var response = JObject.Parse(resultResponse);

                var GenderStr = response["gender"]?.ToString() ?? "unknown";
                return GenderStr;
            }
            ;
        }



        public static JToken ProcessarFieldEstrutura(XmlNode field, DataRow row)
        {
            string type = field.Attributes["type"]?.Value;
            string valueAttr = field.Attributes["value"]?.Value;
            string columnRef = field.Attributes["Columnrefe"]?.Value;
            string format = field.Attributes["formate"]?.Value;

            XmlNodeList children = field.SelectNodes("field");

            // 👉 Se tem filhos (array ou objeto)
            if (children.Count > 0)
            {
                if (type == "array")
                {
                    JArray arr = new JArray();
                    JObject obj = new JObject();

                    foreach (XmlNode child in children)
                    {
                        string childName = child.Attributes["name"]?.Value;
                        obj[childName] = ProcessarFieldEstrutura(child, row);
                    }

                    arr.Add(obj);
                    return arr;
                }
                else
                {
                    JObject obj = new JObject();

                    foreach (XmlNode child in children)
                    {
                        string childName = child.Attributes["name"]?.Value;
                        obj[childName] = ProcessarFieldEstrutura(child, row);
                    }

                    return obj;
                }
            }

            // 👉 Campo simples
            object valor = null;

            // 1️⃣ se tem Columnrefe → pega do DataRow
            if (!string.IsNullOrEmpty(columnRef))
            {
                if (row.Table.Columns.Contains(columnRef))
                    valor = row[columnRef];
            }
            // 2️⃣ se tem value fixo
            else if (!string.IsNullOrEmpty(valueAttr))
            {
                valor = valueAttr;
            }

            return ConverterValue(type, valor, format);
        }
        public static JToken ConverterValue(string type, object value, string format = null)
        {
            if (value == null || value == DBNull.Value)
                return JValue.CreateNull();

            switch (type)
            {
                case "string":
                    return new JValue(value.ToString());

                case "bool":
                    return new JValue(bool.Parse(value.ToString()));

                case "decimal":
                    return new JValue(Convert.ToDecimal(value));

                case "datetime":
                    DateTime dt = Convert.ToDateTime(value);
                    if (!string.IsNullOrEmpty(format))
                        return new JValue(dt.ToString(format));
                    return new JValue(dt);

                default:
                    return new JValue(value.ToString());
            }
        }
    }
}