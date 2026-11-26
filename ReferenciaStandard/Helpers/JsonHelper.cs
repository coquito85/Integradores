using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
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
            JsonObject json = JsonNode.Parse(jsonStr).AsObject();

            // Garante que existe a seção Etapas
            if (!json.ContainsKey("Etapas"))
                json["Etapas"] = new JsonObject();

            JsonObject etapas = json["Etapas"].AsObject();

            //Atualiza a etapa
            etapas[nomeEtapa] = valor;

            // Retorna string do JSON atualizado
            return json.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        public static bool LeituraJsonEtapa(string jsonStr, string nomeEtapa)
        {
            if (jsonStr != null)
            {
                JsonObject json = JsonNode.Parse(jsonStr).AsObject();

                if (!json.ContainsKey("Etapas"))
                    return false;

                JsonObject etapas = json["Etapas"].AsObject();

                if (!etapas.ContainsKey(nomeEtapa))
                    return false;

                return etapas[nomeEtapa]?.GetValue<bool>() ?? false;
            }
            else
                return false;

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


        public static string IsSerializeJson<T>(T obj)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            return JsonConvert.SerializeObject(obj, settings);
        }

        public static T IsDeserializeObjectJson<T>(string json)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            return JsonConvert.DeserializeObject<T>(json, settings);
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
