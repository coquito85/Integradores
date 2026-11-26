

using Newtonsoft.Json.Linq;
using ReferenciaStandard.Entidades;
using ReferenciaStandard.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ReferenciaStandard.Helpers
{
#pragma warning disable
    public class XmlHelper
    {



        XmlDocument xmlDoc;
        public Dictionary<string, object> ScraperXmlContent(string conteudoXML, string NodesSelect)
        {
            Dictionary<string, object> CollectionConfig = new Dictionary<string, object>();
            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(conteudoXML);

            XmlNodeList settingsNodes = xmlDoc.SelectNodes(NodesSelect);

            foreach (XmlNode nodexml in settingsNodes)
            {

                string settingName = nodexml.Attributes["name"].Value;
                string settingValue = nodexml.SelectSingleNode("value").InnerText;
                string serializeAs = nodexml.Attributes["serializeAs"].Value;

                if (serializeAs == "bool")
                    if (Enum.TryParse(settingValue, true, out IsBooleanType boolValue))
                        CollectionConfig.Add(settingName, boolValue);
                    else
                        CollectionConfig.Add(settingName, IsBooleanType.False);
                else if (settingName == "Generate")
                {
                    if (Enum.TryParse(settingValue, true, out IsGenerateJsFiles genValue))
                        CollectionConfig.Add(settingName, genValue);
                    else
                        CollectionConfig.Add(settingName, IsGenerateJsFiles.Arquivos);
                }
                else
                    CollectionConfig.Add(settingName, settingValue);
            }
            return CollectionConfig;
        }
        public Dictionary<string, object> ScraperXmlContent(string conteudoXML, string NodesSelect, string IsNameStruture)
        {
            Dictionary<string, object> CollectionConfig = new Dictionary<string, object>();
            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(conteudoXML);

            XmlNodeList settingsNodes = xmlDoc.SelectNodes(NodesSelect);

            foreach (XmlNode nodexml in settingsNodes)
            {
                string keyName = nodexml.ParentNode.Attributes["name"]?.Value;
                if (keyName == IsNameStruture)
                {
                    string settingName = nodexml.Attributes["name"].Value;
                    string settingValue = nodexml.SelectSingleNode("value").InnerText;
                    string serializeAs = nodexml.Attributes["serializeAs"].Value;

                    if (serializeAs == "bool")
                        if (Enum.TryParse(settingValue, true, out IsBooleanType boolValue))
                            CollectionConfig.Add(settingName, boolValue);
                        else
                            CollectionConfig.Add(settingName, IsBooleanType.Undefined);
                    else if (serializeAs == "int")
                    {
                        if (int.TryParse(settingValue, out int intValue))
                            CollectionConfig.Add(settingName, intValue);
                        else
                            CollectionConfig.Add(settingName, 0);
                    }
                    else
                        CollectionConfig.Add(settingName, settingValue);
                }
                else
                {
                    return null;
                }

            }
            return CollectionConfig;
        }

        /// <summary>
        /// Método genérico e centralizado para ler qualquer lista simples do XML (um nó por item,
        /// campos filhos com o mesmo nome das propriedades de T). Usado por catálogos como o de
        /// Novidades, evitando duplicar lógica de parsing de XML fora deste repositório.
        /// Ex.: ScraperXmlLista&lt;NovidadeItem&gt;(conteudoXml, "//Novidade")
        /// </summary>
       
        //public List<T> ScraperXmlLista<T>(string conteudoXML, string xpathNodes) where T : new()
        //{
        //    var lista = new List<T>();

        //    xmlDoc = new XmlDocument();
        //    xmlDoc.LoadXml(conteudoXML);

        //    XmlNodeList nodes = xmlDoc.SelectNodes(xpathNodes);
        //    if (nodes == null || nodes.Count == 0)
        //        return lista;

        //    foreach (XmlNode node in nodes)
        //    {
        //        var objeto = new T();
        //        PreencherObjeto(objeto, node);
        //        lista.Add(objeto);
        //    }

        //    return lista;
        //}

        //public List<T> ScraperXmlContent<T>(string conteudoXML, Entid_LayoutInfo _helper) where T : new()
        //{

        //    var listaObjetos = new List<T>();
        //    var xmlDoc = new XmlDocument();
        //    xmlDoc.LoadXml(conteudoXML);

        //    XmlNode configurationNode = xmlDoc.DocumentElement; // <schema> ou <model>
        //    if (configurationNode == null)
        //        return listaObjetos;

        //    string configName = configurationNode.Attributes?["name"]?.Value;
        //    if (string.IsNullOrEmpty(configName))
        //        return listaObjetos;

        //    if (!configName.Equals(_helper.IsNameLayout, StringComparison.OrdinalIgnoreCase))
        //        return listaObjetos;


        //    string section = _helper.IsStruture?.ToLower();

        //    XmlNodeList XmlStrturures = null;

        //    if (section.Equals("designer", StringComparison.OrdinalIgnoreCase))
        //    {
        //        XmlStrturures = configurationNode.SelectNodes($"{section}[@name='{_helper.IsNameStruture}']");
        //        if (XmlStrturures == null || XmlStrturures.Count == 0)
        //            return listaObjetos;


        //        foreach (XmlNode NodeStruture in XmlStrturures)
        //        {
        //            var objeto = new T();
        //            // XmlNode bodyNode = Nodestruture.SelectSingleNode("Body");
        //            PreencherObjeto(objeto, NodeStruture);
        //            listaObjetos.Add(objeto);
        //        }


        //    }
        //    else if (section.Equals("gallery", StringComparison.OrdinalIgnoreCase))
        //    {
        //        XmlStrturures = configurationNode.SelectNodes($"{section}[@name='{_helper.IsNameStruture}']");
        //        if (XmlStrturures == null || XmlStrturures.Count == 0)
        //            return listaObjetos;


        //        foreach (XmlNode NodeStruture in XmlStrturures)
        //        {
        //            var objeto = new T();
        //            // XmlNode bodyNode = Nodestruture.SelectSingleNode("Body");
        //            PreencherObjetoMoods(objeto, NodeStruture);
        //            listaObjetos.Add(objeto);
        //        }


        //    }
        //    else if (section.Equals("model", StringComparison.OrdinalIgnoreCase))
        //    {

        //        XmlStrturures = configurationNode.SelectNodes($"{section}/table[@name='{_helper.IsTable}']");

        //        if (XmlStrturures == null || XmlStrturures.Count == 0)
        //            return listaObjetos;
        //        foreach (XmlNode NodeStruture in XmlStrturures)
        //        {
        //            string IsNameView = string.Empty;
        //            if (!string.IsNullOrEmpty(_helper.IsOperation))
        //            {
        //                var objeto = new T();
        //                IsNameView = NodeStruture.Attributes?["displayName"]?.Value;
        //                if (_helper.IsOperation.Equals("insert", StringComparison.OrdinalIgnoreCase))
        //                    PreencherObjeto(objeto, InsertTableModelVazio(NodeStruture, _helper.IsTable, false).Trim(), IsNameView);
        //                else if (_helper.IsOperation.Equals("insertselect", StringComparison.OrdinalIgnoreCase))
        //                    PreencherObjeto(objeto, InsertTableModelVazio(NodeStruture, _helper.IsTable, true).Trim(), IsNameView);
        //                else if (_helper.IsOperation.Equals("update", StringComparison.OrdinalIgnoreCase))
        //                    PreencherObjeto(objeto, UpdateTableModelVazio(NodeStruture, _helper.IsTable).Trim(), IsNameView);

        //                listaObjetos.Add(objeto);
        //            }



        //        }
        //    }
        //    else if (section.Equals("schema", StringComparison.OrdinalIgnoreCase))
        //    {
        //        XmlStrturures = configurationNode.SelectNodes($"{section}/table[@name='{_helper.IsTable}']");
        //        if (XmlStrturures == null || XmlStrturures.Count == 0)
        //            return listaObjetos;

        //        foreach (XmlNode NodeStruture in XmlStrturures)
        //        {

        //            bool create = NodeStruture.Attributes?["Create"]?.Value == "true";
        //            bool isExists = NodeStruture.Attributes?["IsExists"]?.Value == "true";

        //            if (create)
        //            {

        //                var objeto = new T();
        //                PreencherObjeto(objeto, CreateTableStatement(NodeStruture, _helper.IsTable, isExists).Trim());
        //                listaObjetos.Add(objeto);
        //            }


        //        }
        //    }
        //    else if (section.Equals("triggers", StringComparison.OrdinalIgnoreCase))
        //    {
        //        XmlStrturures = configurationNode.SelectNodes($"{section}/TableRule[@name='{_helper.IsTable}']");
        //        if (XmlStrturures == null || XmlStrturures.Count == 0)
        //            return listaObjetos;

        //        foreach (XmlNode tableRule in XmlStrturures)
        //        {
        //            // pega todas as regras de timing (Insert / Update)
        //            XmlNodeList timings = tableRule.SelectNodes("Timing");



        //            foreach (XmlNode timing in timings)
        //            {
        //                bool create = timing.Attributes["Create"]?.Value == "true";
        //                bool isExists = timing.Attributes["IsExists"]?.Value == "true";

        //                if (create)
        //                {
        //                    var objeto = new T();

        //                    PreencherObjeto(objeto, CreateTriggerStatement(timing, tableRule, _helper.IsTable, isExists));
        //                    listaObjetos.Add(objeto);
        //                }



        //            }
        //        }


        //    }
        //    else if (section.Equals("generate", StringComparison.OrdinalIgnoreCase))
        //    {
        //        XmlStrturures = configurationNode.SelectNodes($"{section}/table[@name='{_helper.IsTable}']");
        //        if (XmlStrturures == null || XmlStrturures.Count == 0)
        //            return listaObjetos;
        //        foreach (XmlNode NodeStruture in XmlStrturures)
        //        {
        //            bool IsSelect = NodeStruture.Attributes?["generateselect"]?.Value == "true";

        //            if (IsSelect)
        //            {
        //                var objeto = new T();
        //                string IsNameView = string.Empty;

        //                if (!string.IsNullOrEmpty(_helper.IsOperation) && _helper.IsOperation.Equals("NotRelation", StringComparison.OrdinalIgnoreCase))
        //                {
        //                    IsNameView = NodeStruture.Attributes?["alias"]?.Value;
        //                    PreencherObjeto(objeto, GenerateSelectSql(NodeStruture, _helper.IsTable, false, _helper.IsScope).Trim(), IsNameView);

        //                }
        //                else
        //                {

        //                    IsNameView = NodeStruture.Attributes?["generateviewer"]?.Value;
        //                    PreencherObjeto(objeto, GenerateSelectSql(NodeStruture, _helper.IsTable, true, _helper.IsScope).Trim(), IsNameView);
        //                }
        //                listaObjetos.Add(objeto);



        //            }
        //        }


        //    }

        //    return listaObjetos;
        //}






        //public List<Entid_Query> ScraperXmlContent(string conteudoXML, Entid_LayoutInfo _helper)
        //{
        //    string typeConfig = _helper.IsStruture;
        //    string tableName = _helper.IsTable;
        //    Entid_Query IsSQL;
        //    List<Entid_Query> ListQuerys = new List<Entid_Query>();

        //    xmlDoc = new XmlDocument();
        //    xmlDoc.LoadXml(conteudoXML);

        //    XmlNode configurationNode = xmlDoc.DocumentElement; // <schema> ou <model>
        //    if (configurationNode == null)
        //        return ListQuerys;


        //    string configuracaoNome = configurationNode.Attributes?["name"]?.Value;
        //    if (string.IsNullOrEmpty(configuracaoNome))
        //        return ListQuerys;

        //    if (configuracaoNome.Equals(_helper.IsNameLayout))
        //    {

        //        string section = typeConfig?.ToLower();
        //        if (section != "generate" && section != "model" && section != "schema")
        //            return ListQuerys;


        //        // CORREÇÃO: Use diferentes estratégias de XPath
        //        XmlNodeList tabelas = configurationNode.SelectNodes($"{section}/table[@name='{tableName}']");
        //        if (tabelas == null || tabelas.Count == 0)
        //            return ListQuerys;

        //        if (section.Equals("model", StringComparison.OrdinalIgnoreCase))
        //        {
        //            foreach (XmlNode tabela in tabelas)
        //            {


        //                if (!string.IsNullOrEmpty(_helper.IsOperation))
        //                {
        //                    IsSQL = new Entid_Query();
        //                    if (_helper.IsOperation.Equals("insert", StringComparison.OrdinalIgnoreCase))
        //                        IsSQL.IsQuery = InsertTableModelVazio(tabela, tableName, false).Trim();
        //                    else if (_helper.IsOperation.Equals("insertselect", StringComparison.OrdinalIgnoreCase))
        //                        IsSQL.IsQuery = InsertTableModelVazio(tabela, tableName, true).Trim();

        //                    else if (_helper.IsOperation.Equals("update", StringComparison.OrdinalIgnoreCase))
        //                        IsSQL.IsQuery = UpdateTableModelVazio(tabela, tableName);

        //                    if (!string.IsNullOrEmpty(IsSQL.IsQuery))
        //                        ListQuerys.Add(IsSQL);
        //                }



        //            }
        //        }
        //        else if (section.Equals("schema", StringComparison.OrdinalIgnoreCase))
        //        {
        //            foreach (XmlNode tabela in tabelas)
        //            {

        //                bool create = tabela.Attributes?["Create"]?.Value == "true";
        //                bool isExists = tabela.Attributes?["IsExists"]?.Value == "true";

        //                if (create)
        //                {

        //                    IsSQL = new Entid_Query();
        //                    IsSQL.IsQuery = CreateTableStatement(tabela, tableName, isExists);
        //                    if (!string.IsNullOrEmpty(IsSQL.IsQuery))
        //                        ListQuerys.Add(IsSQL);
        //                }


        //            }
        //        }

        //        else if (section.Equals("generate", StringComparison.OrdinalIgnoreCase))
        //        {
        //            foreach (XmlNode tabela in tabelas)
        //            {
        //                bool IsSelect = tabela.Attributes?["generateselect"]?.Value == "true";

        //                if (IsSelect)
        //                {
        //                    IsSQL = new Entid_Query();

        //                    if (!string.IsNullOrEmpty(_helper.IsOperation) && _helper.IsOperation.Equals("NotRelation", StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        IsSQL.IsNameView = tabela.Attributes?["alias"]?.Value;
        //                        IsSQL.IsQuery = GenerateSelectSql(tabela, tableName, false, _helper.IsScope).Trim();
        //                    }
        //                    else
        //                    {

        //                        IsSQL.IsNameView = tabela.Attributes?["generateviewer"]?.Value;
        //                        IsSQL.IsQuery = GenerateSelectSql(tabela, tableName, true, _helper.IsScope).Trim();
        //                    }


        //                    if (!string.IsNullOrEmpty(IsSQL.IsQuery))
        //                        ListQuerys.Add(IsSQL);

        //                }

        //            }
        //        }

        //    }


        //    return ListQuerys;
        //}
        public string ScraperXmlSelectQuery(string conteudoXML, string Provider)
        {
            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(conteudoXML);

            XmlNode tableNode = xmlDoc.SelectSingleNode("//table");

            string tableName = tableNode.Attributes["name"]?.Value;
            string alias = tableNode.Attributes["alias"]?.Value;

            XmlNodeList columns = xmlDoc.SelectNodes("//selection/Column");

            List<string> selectFields = new List<string>();
            List<string> orderByFields = new List<string>();

            foreach (XmlNode nodecol in columns)
            {
                string colName = nodecol.Attributes["name"]?.Value;
                string orderBy = nodecol.Attributes["orderby"]?.Value;
                // 

                string field = $"{alias}.{colName}";
                string fieldSql = ApplySqlCast(nodecol, Provider);

                selectFields.Add(fieldSql);

                if (orderBy == "true")
                    orderByFields.Add(field);

                //if (!string.IsNullOrEmpty(colName))
                //{
                //    string field = string.IsNullOrEmpty(alias) ? colName : $"{alias}.{colName}";

                //    selectFields.Add(field);

                //    if (orderBy == "true")
                //        orderByFields.Add(field);
                //}
            }

            string select = "SELECT " + string.Join(", ", selectFields);
            string from = $"FROM {tableName} {alias}";

            string orderByClause = orderByFields.Count > 0 ? " ORDER BY " + string.Join(", ", orderByFields) : "";

            return $"{select} {from}{orderByClause}";
        }
        public string ScraperXmlFielsJson(string conteudoXML, DataTable tabela)
        {
            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(conteudoXML);

            XmlNode tableNode = xmlDoc.SelectSingleNode("//table");
            string aliastable = tableNode?.Attributes["alias"]?.Value;

            XmlNode refealias = xmlDoc.SelectSingleNode($"//Refealias[@name='{aliastable}']");

            if (refealias == null)
                return null;

            JArray declaracoesArray = new JArray();

            XmlNode declaracoesNode = refealias.SelectSingleNode("field[@name='declaracoes']");

            foreach (DataRow row in tabela.Rows)
            {
                JObject declaracaoObj = new JObject();

                foreach (XmlNode field in declaracoesNode.SelectNodes("field"))
                {
                    string fieldName = field.Attributes["name"]?.Value;
                    declaracaoObj[fieldName] = JsonHelper.ProcessarFieldEstrutura(field, row);
                }

                declaracoesArray.Add(declaracaoObj);
            }

            JObject root = new JObject();
            root["declaracoes"] = declaracoesArray;

            return root.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        public Dictionary<string, dynamic> ScraperXmlFielsValidate(string conteudoXML)
        {
            var mapeamento = new Dictionary<string, dynamic>();
            var collectionCampos = new List<dynamic>();
            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(conteudoXML);

            XmlNode tableNode = xmlDoc.SelectSingleNode("//table");
            string aliastable = tableNode.Attributes["alias"]?.Value;
            XmlNode refealiasNode = xmlDoc.SelectSingleNode($"//Refealias[@name='{aliastable}']");


            if (refealiasNode == null)
                return mapeamento;


            XmlNodeList ListNodesfields = refealiasNode.SelectNodes(".//field[@Columnrefe]");
            foreach (XmlNode Nodefield in ListNodesfields)
            {

                string columnRef = Nodefield.Attributes["Columnrefe"]?.Value;
                string fieldName = Nodefield.Attributes["name"]?.Value;
                string fieldType = Nodefield.Attributes["type"]?.Value;
                string fieldFormat = Nodefield.Attributes["formate"]?.Value;
                string fieldValue = Nodefield.Attributes["value"]?.Value;

                string caminho = GetCaminhoCompleto(Nodefield);

                var campoInfo = new
                {
                    ColumnRef = columnRef,
                    NomeXml = fieldName,
                    Tipo = fieldType,
                    Formato = fieldFormat,
                    ValorFixo = fieldValue,
                    Caminho = caminho,
                    Nivel = GetNivelHierarquico(Nodefield),
                    TemFilhos = Nodefield.SelectNodes("field").Count > 0,
                    XmlOriginal = Nodefield.OuterXml
                };

                collectionCampos.Add(campoInfo);

                if (!string.IsNullOrEmpty(columnRef))
                {
                    if (mapeamento.ContainsKey(columnRef))
                    {
                        var existente = mapeamento[columnRef];

                        if (existente.Caminho != caminho)
                            throw new Exception($"Coluna '{columnRef}' mapeada em dois caminhos diferentes:\n" + $"1) {existente.Caminho}\n" + $"2) {caminho}");

                    }
                    else
                    {
                        mapeamento.Add(columnRef, campoInfo);
                    }
                }
            }

            return mapeamento;




        }

        /// <summary>
        /// Obtém o caminho completo do campo no XML
        /// </summary>
        private string GetCaminhoCompleto(XmlNode node)
        {
            List<string> caminho = new List<string>();
            XmlNode current = node;

            while (current != null && current.Name != "Refealias")
            {
                if (current.Attributes?["name"] != null)
                    caminho.Insert(0, current.Attributes["name"].Value);

                current = current.ParentNode;
            }

            return string.Join(" → ", caminho);
        }

        /// <summary>
        /// Obtém o nível hierárquico do campo (0 = raiz, 1 = primeiro nível, etc)
        /// </summary>
        private int GetNivelHierarquico(XmlNode node)
        {
            int nivel = 0;
            var current = node.ParentNode;

            while (current != null && current.Name != "Refealias" && current.Name != "generatefields")
            {
                nivel++;
                current = current.ParentNode;
            }

            return nivel;
        }


        private string ApplySqlCast(XmlNode nodecol, string ProviderName)
        {
            string colName = nodecol.Attributes["name"]?.Value;
            string type = nodecol.Attributes["type"]?.Value ?? "string";
            string format = nodecol.Attributes["formate"]?.Value; // Novo atributo format
                                                                  // string field = $"{alias}.{colName}";

            bool isAccess = ProviderName.Equals("System.Data.OleDb", StringComparison.OrdinalIgnoreCase) ||
                ProviderName.IndexOf("Microsoft.Jet.OLEDB", StringComparison.OrdinalIgnoreCase) >= 0 ||
                ProviderName.IndexOf("Microsoft.ACE.OLEDB", StringComparison.OrdinalIgnoreCase) >= 0;

            // Função auxiliar para aplicar formato em datas
            string ApplyDateFormat(string fieldExpr, string formatSpec, string aliasName)
            {
                if (string.IsNullOrEmpty(formatSpec))
                    return fieldExpr;

                if (isAccess)
                {
                    // Access: usa Format com máscara específica
                    string accessFormat = ConvertToAccessFormat(formatSpec);
                    return $"Format({fieldExpr}, '{accessFormat}')";
                }
                else
                {
                    // SQL Server: usa FORMAT (ou CONVERT como fallback)
                    return $"FORMAT({fieldExpr}, '{formatSpec}')";
                }
            }

            switch (type.ToLower())
            {
                case "int":
                case "integer":
                    if (isAccess)
                        return $"CLng({colName}) AS [{colName}]";
                    else
                        return $"CAST({colName} AS INT) AS [{colName}]";

                case "decimal":
                    if (isAccess)
                        return $"CDec({colName}) AS [{colName}]";
                    else
                        return $"CAST({colName} AS DECIMAL(18,2)) AS [{colName}]";

                case "bool":
                case "boolean":
                    if (isAccess)
                        return $"CBool({colName}) AS [{colName}]";
                    else
                        return $"CAST({colName} AS BIT) AS [{colName}]";

                case "datetime":

                    {
                        string expr;
                        if (isAccess)
                            expr = $"CDate({colName})";
                        else
                            expr = $"CAST({colName} AS DATETIME)";

                        // Aplica formatação se especificada
                        if (!string.IsNullOrEmpty(format))
                            expr = ApplyDateFormat(expr, format, $"{colName}");

                        return $"{expr} AS [{colName}]";
                    }

                case "time":
                    {
                        string expr;
                        if (isAccess)
                        {
                            expr = $"TimeValue({colName})";
                            if (!string.IsNullOrEmpty(format))
                            {
                                string accessFormat = ConvertToAccessFormat(format);
                                expr = $"Format({expr}, '{accessFormat}')";
                            }
                        }
                        else
                        {
                            expr = $"CAST({colName} AS TIME)";
                            if (!string.IsNullOrEmpty(format))
                                expr = $"FORMAT({expr}, '{format}')";
                        }
                        return $"{expr} AS [{colName}]";
                    }

                case "double":
                    {
                        if (isAccess)
                            return $"CDbl({colName}) AS [{colName}]";
                        else
                            return $"CAST({colName} AS FLOAT) AS [{colName}]";
                    }

                case "float":
                    {
                        if (isAccess)
                            return $"CSng({colName}) AS [{colName}]";
                        else
                            return $"CAST({colName} AS FLOAT) AS [{colName}]";
                    }

                case "money":
                case "currency":
                    {
                        string expr;
                        if (isAccess)
                        {
                            expr = $"CCur({colName})";

                            // Aplica formatação de moeda se especificada - MODO TRADICIONAL
                            if (!string.IsNullOrEmpty(format))
                            {
                                string accessFormat;

                                // MODO TRADICIONAL: if/else em vez de switch expression
                                if (format == "R$")
                                    accessFormat = "R$ #.##0,00";
                                else if (format == "US$")
                                    accessFormat = "$#,##0.00";
                                else if (format == "EUR")
                                    accessFormat = "#.##0,00 €";
                                else
                                    accessFormat = format;

                                expr = $"Format({expr}, '{accessFormat}')";
                            }
                        }
                        else
                        {
                            expr = $"CAST({colName} AS MONEY)";
                            if (!string.IsNullOrEmpty(format))
                                expr = $"FORMAT({expr}, '{format}')";
                        }
                        return $"{expr} AS [{colName}]";
                    }

                case "string":
                case "text":
                case "varchar":
                    {
                        if (isAccess)
                            return $"{colName} AS [{colName}]";
                        else
                            return $"CAST({colName} AS NVARCHAR(MAX)) AS [{colName}]";
                    }

                case "byte":
                    {
                        if (isAccess)
                            return $"CByte({colName}) AS [{colName}]";
                        else
                            return $"CAST({colName} AS TINYINT) AS [{colName}]";
                    }

                case "long":
                    {
                        if (isAccess)
                            return $"CLng({colName}) AS [{colName}]";
                        else
                            return $"CAST({colName} AS BIGINT) AS [{colName}]";
                    }

                default:
                    return $"{colName} AS [{colName}]";
            }
        }

        // Método auxiliar para converter formatos .NET/Access
        private string ConvertToAccessFormat(string formatSpec)
        {
            // Mapeamento de formatos comuns .NET para Access
            if (string.IsNullOrWhiteSpace(formatSpec))
                return formatSpec;

            var formatMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Datas
                { "yyyy-MM-dd", "yyyy-mm-dd" },
                { "dd/MM/yyyy", "dd/mm/yyyy" },
                { "MM/dd/yyyy", "mm/dd/yyyy" },
                { "dd/MM/yy",   "dd/mm/yy" },
                { "dd MMM yyyy", "dd mmm yyyy" },
                { "dd MMMM yyyy", "dd mmmm yyyy" },
                { "dddd, dd MMMM yyyy", "dddd, dd mmmm yyyy" },

        // Horas
                { "HH:mm:ss", "hh:nn:ss" },
                { "HH:mm", "hh:nn" },
                { "hh:mm tt", "hh:nn AM/PM" },
                { "HH:mm:ss.fff", "hh:nn:ss.000" },

        // Data + hora
                { "yyyy-MM-dd HH:mm:ss", "yyyy-mm-dd hh:nn:ss" },
                { "dd/MM/yyyy HH:mm:ss", "dd/mm/yyyy hh:nn:ss" },

        // Moeda
                { "C", "Currency" },
                { "R$ #,##0.00", "R$ #.##0,00" },
                { "$#,##0.00", "$#,##0.00" },

        // Numéricos
                { "N0", "0" },
                { "N2", "#,##0.00" },
                { "P0", "Percent" },
                { "P2", "0.00%" }
            };

            return formatMap.TryGetValue(formatSpec, out var accessFormat) ? accessFormat : formatSpec;
        }
        //public string SelectXmlLayout(Entid_LayoutInfo collectionLayouts)
        //{
        //    var XmlSection = string.Empty;
        //    try
        //    {


        //        if (collectionLayouts.IsStruture.Equals("DatabaseSettings"))
        //            XmlSection = $"//{collectionLayouts.IsStruture}/setting";
        //        else if (collectionLayouts.IsStruture.Equals("DatabaseSettingsGeneratorJS"))
        //            XmlSection = $"//{collectionLayouts.IsStruture}/setting";
        //        else if (collectionLayouts.IsStruture.Equals("MariadbConfig"))
        //            XmlSection = $"//{collectionLayouts.IsStruture}/setting";
        //        else if (collectionLayouts.IsStruture.Equals("SqliteConfig"))
        //            XmlSection = $"//{collectionLayouts.IsStruture}/setting";
        //        else
        //            return null;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //    return XmlSection;

        //}
        public string CheckCodeLayoutXML(string conteudoXML)
        {
            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(conteudoXML);

            XmlNode layoutNode = xmlDoc.SelectSingleNode("//Layout");

            string identifier = layoutNode?.Attributes["identifier"]?.Value;

            return identifier;

        }

        //private Dictionary<Type, Dictionary<string, string>> _typeMappings = new Dictionary<Type, Dictionary<string, string>>()
        //{
        //    {
        //        typeof(Entid_LayoutMail), new Dictionary<string, string>
        //        {
        //            { "IsTemplate", "Body" },
        //            { "IsSubject", "Subject" }
        //        }
        //    }
        //};
        //private void PreencherObjeto<T>(T obj, XmlNode node)
        //{
        //    var properties = typeof(T).GetProperties();
        //    var mappings = _typeMappings.ContainsKey(typeof(T)) ? _typeMappings[typeof(T)] : null;

        //    foreach (var property in properties)
        //    {
        //        // Propriedades somente-leitura (ex.: calculadas, sem setter) não podem ser preenchidas via reflexão.
        //        if (!property.CanWrite)
        //            continue;

        //        string nodeName = mappings != null && mappings.ContainsKey(property.Name) ? mappings[property.Name] : property.Name;

        //        XmlNode valueNode = node.SelectSingleNode(nodeName);

        //        if (valueNode != null && !string.IsNullOrEmpty(valueNode.InnerText))
        //        {
        //            try
        //            {
        //                // Convert.ChangeType não converte diretamente para Nullable<T> (ex.: int?),
        //                // então usa o tipo subjacente quando a propriedade é nullable.
        //                var tipoDestino = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        //                var value = Convert.ChangeType(valueNode.InnerText, tipoDestino);
        //                property.SetValue(obj, value);
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"Erro ao converter propriedade {property.Name}: {ex.Message}");
        //            }
        //        }
        //    }
        //}
        private void PreencherObjetoMoods<T>(T obj, XmlNode node)
        {
            var tipo = typeof(T);
            var props = tipo.GetProperties();

            foreach (var p in props)
            {
                // --- 1) IsNameMood vem do atributo do <gallery>
                if (p.Name == "IsNameMood")
                {
                    var mood = node.Attributes?["name"]?.Value;
                    if (mood != null)
                        p.SetValue(obj, mood);
                }

                // --- 2) Arrays: options e values
                if (p.Name == "isOption") // int[]
                {
                    var options = node.SelectNodes("option");
                    if (options != null)
                    {
                        int[] arr = new int[options.Count];
                        for (int i = 0; i < options.Count; i++)
                        {
                            var attr = options[i].Attributes?["name"]?.Value;
                            arr[i] = int.Parse(attr);
                        }
                        p.SetValue(obj, arr);
                    }
                }

                if (p.Name == "IsNameValue") // string[]
                {
                    var values = node.SelectNodes("option/value");
                    if (values != null)
                    {
                        string[] arr = new string[values.Count];
                        for (int i = 0; i < values.Count; i++)
                            arr[i] = values[i].InnerText.Trim();

                        p.SetValue(obj, arr);
                    }
                }
            }
        }
        // Método auxiliar para preencher o objeto T com a query
        private void PreencherObjeto<T>(T obj, string query, string viewName = null)
        {
            if (string.IsNullOrEmpty(query)) return;

            var properties = typeof(T).GetProperties();

            // Procura por uma propriedade que possa armazenar a query
            foreach (var property in properties)
            {
                // Supondo que tenha uma propriedade como "IsQuery" ou similar
                if (property.PropertyType == typeof(string) && property.Name.Equals("IsQuery", StringComparison.OrdinalIgnoreCase))
                    property.SetValue(obj, query);

                if (!string.IsNullOrEmpty(viewName) && property.Name.Equals("IsNameView", StringComparison.OrdinalIgnoreCase))
                    property.SetValue(obj, viewName);
            }
        }
        private static string RemovePrefixIs(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;
            if ((name.StartsWith("Is") || name.StartsWith("is")) && name.Length > 2)
                return name.Substring(2);
            return name;
        }
        private string CreateTableStatement(XmlNode tabela, string nomeTabela, bool isExists)
        {
            var columns = new List<string>();
            var primaryKeys = new List<string>();
            var foreignKeys = new List<string>();

            foreach (XmlNode col in tabela.SelectNodes("Column"))
            {
                var columnDefinition = ProcessColumnDefinition(col, primaryKeys, foreignKeys);
                if (!string.IsNullOrEmpty(columnDefinition))
                    columns.Add(columnDefinition);
            }

            return BuildFinalSqlStatement(nomeTabela, columns, primaryKeys, foreignKeys, isExists);
        }
        private string CreateTriggerStatement(XmlNode Xmltiming, XmlNode tableRuleNode, string nomeTabela, bool isExists)
        {



            string eventType = Xmltiming.Attributes["event"]?.Value;   // Insert / Update
            string whenType = Xmltiming.Attributes["when"]?.Value;     // Before
            string caption = Xmltiming.Attributes["Caption"]?.Value;

            XmlNode conditionNode = tableRuleNode.SelectSingleNode("Condition");
            XmlNodeList columns = conditionNode.SelectNodes("Column");

            XmlNode actionNode = Xmltiming.SelectSingleNode("Action");
            string actionType = actionNode?.Attributes["type"]?.Value;


            string primaryKey = null;
            List<string> compareColumns = new List<string>();

            foreach (XmlNode col in columns)
            {
                string colName = col.Attributes["name"]?.Value;
                string keyType = col.Attributes["key"]?.Value;

                if (col.Attributes["IsPrimarykey"]?.Value == "true")
                    primaryKey = colName;
                else
                    compareColumns.Add(colName);

            }


            StringBuilder sb = new StringBuilder();


            sb.Append("CREATE TRIGGER ");
            if (isExists)
                sb.Append("IF NOT EXISTS ");

            sb.AppendLine(caption);
            sb.AppendLine($"{whenType.ToUpper()} {eventType.ToUpper()} ON {nomeTabela}");
            sb.AppendLine("FOR EACH ROW");
            sb.AppendLine("BEGIN");

            if (actionType == "DeleteOld")
            {
                sb.AppendLine($"DELETE FROM {nomeTabela}");
                sb.AppendLine("WHERE");

                for (int i = 0; i < compareColumns.Count; i++)
                {
                    string col = compareColumns[i];
                    sb.Append($"    {col} = NEW.{col}");

                    if (i < compareColumns.Count - 1)
                        sb.AppendLine(" AND");
                    else
                        sb.AppendLine();
                }

                // só no UPDATE: evita apagar o próprio registro
                if (eventType.Equals("Update", StringComparison.OrdinalIgnoreCase) && primaryKey != null)
                {
                    sb.AppendLine($"    AND {primaryKey} <> OLD.{primaryKey}");
                }

                sb.AppendLine(";");
            }

            sb.AppendLine("END;");



            return sb.ToString();
        }
        string GetPrimaryKey(string tableName, XmlDocument xmlDoc)
        {
            XmlNode pkNode = xmlDoc.SelectSingleNode(
                $"//schema/table[@name='{tableName}']/Column[@key='Primary']");

            return pkNode?.Attributes["name"]?.Value;
        }
        private string GenerateSelectSql(XmlNode tableNode, string tableName, bool hasNotRelations, int[] scopes)
        {
            if (tableNode == null) return string.Empty;

            StringBuilder sqlBuilder = new StringBuilder();
            string tableAlias = tableNode.Attributes?["alias"]?.Value?.Trim() ?? tableName;

            XmlNode generateNode = tableNode.SelectSingleNode("ancestor::generate")
                                  ?? tableNode.OwnerDocument.SelectSingleNode("//generate");
            if (generateNode == null) return string.Empty;

            var tableMap = generateNode.SelectNodes("./table")
                .Cast<XmlNode>()
                .Where(t => t?.Attributes?["name"] != null)
                .ToDictionary(
                    t => t.Attributes["name"].Value.Trim(),
                    t => t,
                    StringComparer.OrdinalIgnoreCase
                );

            var selectNodes = tableNode.SelectNodes("selection");
            if (selectNodes == null || selectNodes.Count == 0)
                return string.Empty;


            foreach (XmlNode selection in selectNodes)
            {
                var columns = ExtractColumns(selection, tableAlias);
                var joins = new StringBuilder();
                var joinedTables = new Dictionary<string, string>();

                if (hasNotRelations)
                    ProcessRelations(tableNode, tableMap, columns, joins, joinedTables);



                var whereConditions = ExtractWhereConditions(tableNode, tableAlias, hasNotRelations, joinedTables, scopes);
                if (whereConditions.Count == 0)
                    sqlBuilder.AppendLine(BuildSql(columns, tableName, tableAlias, joins.ToString(), null));
                else
                {
                    foreach (string whereSql in whereConditions)
                        sqlBuilder.AppendLine(BuildSql(columns, tableName, tableAlias, joins.ToString(), whereSql));
                }
            }

            return sqlBuilder.ToString();
        }
        private List<Entid_ColumnInfo> ExtractColumns(XmlNode selection, string alias)
        {
            var list = new List<Entid_ColumnInfo>();
            Entid_ColumnInfo parameters;
            foreach (XmlNode col in selection.SelectNodes("Column"))
            {

                string name = col.Attributes?["name"]?.Value?.Trim();
                if (string.IsNullOrEmpty(name)) continue;

                bool groupBy = col.Attributes?["groupby"]?.Value?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
                bool orderBy = col.Attributes?["orderby"]?.Value?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
                int ordinate = int.TryParse(col.Attributes?["ordinate"]?.Value, out int ord) ? ord : 0;

                parameters = new Entid_ColumnInfo()
                {
                    Alias = alias,
                    Nome = name,
                    GroupBy = groupBy,
                    OrderBy = orderBy,
                    Ordinate = ordinate
                };

                list.Add(parameters);
            }
            return list;
        }

        /* FOI DESATIVADO PARA DAR MELHORIA NO PROCESSO AO MOMENTO DE CRIAR O SELECT
        private void ProcessRelations(XmlNode tableNode,Dictionary<string, XmlNode> tableMap, List<HelperColumnInfo> columns,StringBuilder joins,Dictionary<string, string> joinedTables)
        {
            foreach (XmlNode rel in tableNode.SelectNodes("relations/relation"))
            {
                string parentTable = rel.Attributes?["tableparent"]?.Value?.Trim();
                string joinType = rel.Attributes?["joinparent"]?.Value?.Trim()?.ToUpper() ?? "INNER";
                string columnParent = rel.Attributes?["columnparent"]?.Value?.Trim();
                if (string.IsNullOrEmpty(parentTable) || string.IsNullOrEmpty(columnParent)) continue;

                if (!tableMap.TryGetValue(parentTable, out XmlNode parentNode)) continue;
                if (parentNode.Attributes?["generateselect"]?.Value?.Equals("true", StringComparison.OrdinalIgnoreCase) != true)
                    continue;

                string parentAlias = rel.Attributes?["alias"]?.Value?.Trim() ?? parentNode.Attributes?["alias"]?.Value?.Trim() ?? parentTable;

                joinedTables[parentTable] = parentAlias;

                joins.Append($" {joinType} JOIN {parentTable} AS {parentAlias} ON {tableNode.Attributes?["alias"]?.Value ?? tableNode.Name}.{columnParent} = {parentAlias}.{columnParent}");

                foreach (XmlNode col in parentNode.SelectNodes("./selection/Column"))
                {
                    string name = col.Attributes?["name"]?.Value?.Trim();
                    if (string.IsNullOrEmpty(name)) continue;

                    if (!columns.Any(c => c.Nome.Equals(name, StringComparison.OrdinalIgnoreCase) && c.Alias == parentAlias))
                    {
                        bool groupBy = col.Attributes?["groupby"]?.Value?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
                        bool orderBy = col.Attributes?["orderby"]?.Value?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

                        var parameters = new HelperColumnInfo { Nome = name, Alias = parentAlias, GroupBy = groupBy, OrderBy = orderBy, Ordinate = columns.Count + 1 };
                        columns.Add(parameters);
                    }
                }
            }
        }
       */
        private void ProcessRelations(XmlNode tableNode, Dictionary<string, XmlNode> tableMap, List<Entid_ColumnInfo> columns, StringBuilder joins, Dictionary<string, string> joinedTables)
        {
            string mainAlias = tableNode.Attributes?["alias"]?.Value ?? tableNode.Name;

            foreach (XmlNode rel in tableNode.SelectNodes("relations/relation"))
            {
                string parentTable = rel.Attributes?["tableparent"]?.Value?.Trim();
                string joinType = rel.Attributes?["joinparent"]?.Value?.Trim()?.ToUpper() ?? "INNER";
                string columnParent = rel.Attributes?["columnparent"]?.Value?.Trim();
                if (string.IsNullOrEmpty(parentTable) || string.IsNullOrEmpty(columnParent)) continue;

                if (!tableMap.TryGetValue(parentTable, out XmlNode parentNode)) continue;
                if (parentNode.Attributes?["generateselect"]?.Value?.Equals("true", StringComparison.OrdinalIgnoreCase) != true)
                    continue;

                string parentAlias = rel.Attributes?["alias"]?.Value?.Trim()
                                   ?? parentNode.Attributes?["alias"]?.Value?.Trim()
                                   ?? parentTable;

                joinedTables[parentTable] = parentAlias;

                joins.Append($" {joinType} JOIN {parentTable} AS {parentAlias} ON {mainAlias}.{columnParent} = {parentAlias}.{columnParent}");

                foreach (XmlNode col in parentNode.SelectNodes("./selection/Column"))
                {
                    string name = col.Attributes?["name"]?.Value?.Trim();
                    if (string.IsNullOrEmpty(name)) continue;


                    var duplicado = columns.FirstOrDefault(c => c.Nome.Equals(name, StringComparison.OrdinalIgnoreCase));

                    if (duplicado != null)
                        columns.Remove(duplicado);


                    bool groupBy = col.Attributes?["groupby"]?.Value?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
                    bool orderBy = col.Attributes?["orderby"]?.Value?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

                    var parameters = new Entid_ColumnInfo
                    {
                        Nome = name,
                        Alias = parentAlias,
                        GroupBy = groupBy,
                        OrderBy = orderBy,
                        Ordinate = columns.Count + 1
                    };

                    columns.Add(parameters);
                }
            }
        }

        private List<string> ExtractWhereConditions(XmlNode tableNode, string tableAlias, bool hasNotRelations, Dictionary<string, string> joinedTables, int[] scopes)
        {
            var list = new List<string>();
            if (scopes == null) return list;

            string xpath = hasNotRelations ? "relations/clauses/scope" : "clauses/scope";
            foreach (XmlNode scopeNode in tableNode.SelectNodes(xpath))
            {
                string idStr = scopeNode.Attributes?["id"]?.Value;
                if (!int.TryParse(idStr, out int scopeId) || !scopes.Contains(scopeId))
                    continue;

                XmlNode whereNode = scopeNode.SelectSingleNode("where");
                if (whereNode == null) continue;

                string whereSql = hasNotRelations ? ProcessRelationWhereNode(whereNode, tableAlias, joinedTables) : ProcessWhereNode(whereNode, tableAlias);

                if (!string.IsNullOrEmpty(whereSql))
                    list.Add(whereSql.StartsWith("1=1") ? whereSql : $"1=1 AND {whereSql}");
            }
            return list;
        }
        private string BuildSql(List<Entid_ColumnInfo> columns, string tableName, string alias, string joins, string where)
        {
            var selectCols = string.Join(", ", columns.Select(c => $"{c.Alias}.{c.Nome}"));
            var groupByCols = columns.Where(c => c.GroupBy).Select(c => $"{c.Alias}.{c.Nome}");
            var orderByCols = columns.Where(c => c.OrderBy).Select(c => $"{c.Alias}.{c.Nome}");

            StringBuilder sql = new StringBuilder($"SELECT {selectCols} FROM {tableName} AS {alias}{joins}");

            if (!string.IsNullOrEmpty(where))
                sql.Append($" WHERE {where}");

            if (groupByCols.Any())
                sql.Append($" GROUP BY {string.Join(", ", groupByCols)}");

            if (orderByCols.Any())
                sql.Append($" ORDER BY {string.Join(", ", orderByCols)}");

            sql.Append(";");
            return sql.ToString();
        }
        private string ProcessRelationWhereNode(XmlNode whereNode, string mainAlias, Dictionary<string, string> joinedTables)
        {
            if (whereNode == null) return string.Empty;

            var conditions = new List<string>();

            foreach (XmlNode child in whereNode.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;

                switch (child.Name.ToLower())
                {
                    case "condition":
                        string conditionStr = ProcessRelationCondition(child, mainAlias, joinedTables);
                        if (!string.IsNullOrEmpty(conditionStr))
                            conditions.Add(conditionStr);
                        break;

                    case "group":
                        string groupStr = ProcessRelationGroupNode(child, mainAlias, joinedTables, true);
                        if (!string.IsNullOrEmpty(groupStr))
                            conditions.Add(groupStr);
                        break;
                }
            }

            return string.Join(" AND ", conditions);
        }
        private string ProcessRelationCondition(XmlNode conditionNode, string mainAlias, Dictionary<string, string> joinedTables)
        {
            string raw = conditionNode.Attributes?["raw"]?.Value?.Trim();
            string col = conditionNode.Attributes?["column"]?.Value?.Trim();
            string op = conditionNode.Attributes?["operator"]?.Value?.Trim();
            string val = conditionNode.Attributes?["value"]?.Value?.Trim();
            string refealias = conditionNode.Attributes?["refealias"]?.Value?.Trim();


            if (!string.IsNullOrEmpty(refealias))
            {
                if (!string.IsNullOrEmpty(raw))
                    return raw;

                if (!string.IsNullOrEmpty(col) && !string.IsNullOrEmpty(op))
                {
                    string valuePart = !string.IsNullOrEmpty(val) ? $" {val}" : "";
                    return $"{refealias}.{col} {op}{valuePart}";
                }
            }
            else
            {

                string correctAlias = FindCorrectAliasForColumn(col, mainAlias, joinedTables, conditionNode);

                if (!string.IsNullOrEmpty(raw))
                    return raw;

                if (!string.IsNullOrEmpty(col) && !string.IsNullOrEmpty(op))
                {
                    string valuePart = !string.IsNullOrEmpty(val) ? $" {val}" : "";
                    return $"{correctAlias}.{col} {op}{valuePart}";
                }
            }

            return string.Empty;
        }
        private string FindCorrectAliasForColumn(string columnName, string mainAlias, Dictionary<string, string> joinedTables, XmlNode generateNode)
        {
            if (string.IsNullOrEmpty(columnName) || generateNode == null)
                return mainAlias;

            // Primeiro verifica se a coluna existe na tabela principal
            if (HasColumnInSelection(generateNode, mainAlias, columnName))
                return mainAlias;

            // Depois verifica nas tabelas relacionadas (joinedTables)
            foreach (var joinedTable in joinedTables)
            {
                string tableName = joinedTable.Key;
                string tableAlias = joinedTable.Value;

                if (HasColumnInSelection(generateNode, tableName, columnName))
                    return tableAlias;
            }

            // Se não encontrou em nenhuma, retorna o alias principal
            return mainAlias;
        }
        private bool HasColumnInSelection(XmlNode generateNode, string tableNameOrAlias, string columnName)
        {
            // Encontra o nó da tabela pelo nome ou alias
            XmlNode tableNode = FindTableNode(generateNode, tableNameOrAlias);
            if (tableNode == null) return false;

            // Verifica se a coluna existe na seleção desta tabela
            XmlNode columnNode = tableNode.SelectSingleNode($"selection/Column[@name='{columnName}']");
            return columnNode != null;
        }
        private XmlNode FindTableNode(XmlNode generateNode, string tableNameOrAlias)
        {
            foreach (XmlNode tableNode in generateNode.SelectNodes("./table"))
            {
                string name = tableNode.Attributes?["name"]?.Value;
                string alias = tableNode.Attributes?["alias"]?.Value;

                // Compara com o nome da tabela
                if (name != null && name.Equals(tableNameOrAlias, StringComparison.OrdinalIgnoreCase))
                    return tableNode;

                // Compara com o alias da tabela
                if (alias != null && alias.Equals(tableNameOrAlias, StringComparison.OrdinalIgnoreCase))
                    return tableNode;
            }
            return null;
        }
        private string ProcessRelationGroupNode(XmlNode groupNode, string mainAlias, Dictionary<string, string> joinedTables, bool isRootGroup = false)
        {
            var groupConditions = new List<string>();
            string groupLogic = groupNode.Attributes?["logic"]?.Value?.Trim()?.ToUpper() ?? "AND";

            foreach (XmlNode child in groupNode.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;

                switch (child.Name.ToLower())
                {
                    case "condition":
                        string conditionStr = ProcessRelationCondition(child, mainAlias, joinedTables);
                        if (!string.IsNullOrEmpty(conditionStr))
                            groupConditions.Add(conditionStr);
                        break;

                    case "group":
                        string subGroupStr = ProcessRelationGroupNode(child, mainAlias, joinedTables, false);
                        if (!string.IsNullOrEmpty(subGroupStr))
                            groupConditions.Add(subGroupStr);
                        break;
                }
            }

            if (groupConditions.Count == 0) return string.Empty;

            string result = string.Join($" {groupLogic} ", groupConditions);
            bool needsParentheses = !isRootGroup && (groupConditions.Count > 1 || groupLogic == "OR");

            return needsParentheses ? $"({result})" : result;
        }
        private string ProcessWhereNode(XmlNode whereNode, string alias)
        {
            if (whereNode == null) return string.Empty;

            var conditions = new List<string>();

            foreach (XmlNode child in whereNode.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;

                switch (child.Name.ToLower())
                {
                    case "condition":
                        string conditionStr = ProcessCondition(child, alias);
                        if (!string.IsNullOrEmpty(conditionStr))
                            conditions.Add(conditionStr);
                        break;

                    case "group":
                        string groupStr = ProcessGroupNode(child, alias, true); // true = é grupo raiz
                        if (!string.IsNullOrEmpty(groupStr))
                            conditions.Add(groupStr);
                        break;
                }
            }

            return string.Join(" ", conditions);
        }
        private string ProcessGroupNode(XmlNode groupNode, string alias, bool isRootGroup = false)
        {
            var groupConditions = new List<string>();
            string groupLogic = groupNode.Attributes?["logic"]?.Value?.Trim()?.ToUpper() ?? "AND";

            foreach (XmlNode child in groupNode.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;

                switch (child.Name.ToLower())
                {
                    case "condition":
                        string conditionStr = ProcessCondition(child, alias);
                        if (!string.IsNullOrEmpty(conditionStr))
                            groupConditions.Add(conditionStr);
                        break;

                    case "group":
                        string subGroupStr = ProcessGroupNode(child, alias, false);
                        if (!string.IsNullOrEmpty(subGroupStr))
                            groupConditions.Add(subGroupStr);
                        break;
                }
            }

            if (groupConditions.Count == 0) return string.Empty;

            string result = string.Join($" {groupLogic} ", groupConditions);
            bool needsParentheses = !isRootGroup && (groupConditions.Count > 1 || groupLogic == "OR");

            return needsParentheses ? $"({result})" : result;
        }
        private string ProcessCondition(XmlNode conditionNode, string alias)
        {
            string logic = conditionNode.Attributes?["logic"]?.Value?.Trim();
            string raw = conditionNode.Attributes?["raw"]?.Value?.Trim();
            string col = conditionNode.Attributes?["column"]?.Value?.Trim();
            string op = conditionNode.Attributes?["operator"]?.Value?.Trim();
            string val = conditionNode.Attributes?["value"]?.Value?.Trim();
            string refealias = conditionNode.Attributes?["refealias"]?.Value?.Trim();

            string tablePrefix = string.IsNullOrEmpty(refealias) ? alias : refealias;

            if (!string.IsNullOrEmpty(raw))
            {
                var ConditionRaw = $"{raw} {logic}";
                return ConditionRaw;
            }
            else if (!string.IsNullOrEmpty(col) && !string.IsNullOrEmpty(op))
            {
                string valuePart = !string.IsNullOrEmpty(val) ? $" {val}" : "";
                return $"{tablePrefix}.{col} {op}{valuePart}";
            }

            return string.Empty;
        }
        private string InsertTableModelVazio(XmlNode tabela, string tableName, bool comVerificacaoDuplicidade = false)
        {
            if (tabela == null) return string.Empty;

            XmlNodeList colunas = tabela.SelectNodes("Column");
            if (colunas == null || colunas.Count == 0) return string.Empty;

            List<string> listaCampos = new List<string>();
            List<string> listaValores = new List<string>();
            List<string> listaValidacao = new List<string>(); // para WHERE NOT EXISTS

            foreach (XmlNode coluna in colunas)
            {
                string nome = coluna.Attributes?["name"]?.Value?.Trim();
                if (string.IsNullOrEmpty(nome))
                    continue;


                bool isAuto = coluna.Attributes?["Autoincrement"]?.Value?.Trim()?.ToLower() == "true";
                if (isAuto)
                    continue;

                listaCampos.Add(nome);
                listaValores.Add($"@{nome}");


                bool Isvalidate = coluna.Attributes?["validate"]?.Value?.Trim()?.ToLower() == "true";
                if (Isvalidate)
                    listaValidacao.Add(nome);
            }

            if (listaCampos.Count == 0)
                return string.Empty;

            string campos = string.Join(", ", listaCampos);
            string valores = string.Join(", ", listaValores);

            // Inserção sem verificação de duplicidade
            if (!comVerificacaoDuplicidade || listaValidacao.Count == 0)
                return $"INSERT INTO {tableName} ({campos}) VALUES ({valores});";

            // Construindo a condição WHERE apenas com colunas de validação
            string whereCondition = string.Join(" AND ", listaValidacao.Select(k => $"{k} = @{k}"));
            string sqlStr = $@" INSERT INTO {tableName} ({campos}) SELECT {valores} WHERE NOT EXISTS (SELECT 1 FROM {tableName} WHERE {whereCondition});";

            return sqlStr;
        }
        private string UpdateTableModelVazio(XmlNode tabela, string tableName)
        {
            if (tabela == null) return string.Empty;

            XmlNodeList colunas = tabela.SelectNodes("Column");
            if (colunas == null || colunas.Count == 0) return string.Empty;

            string primaryKey = string.Empty;
            List<string> listaSets = new List<string>();

            foreach (XmlNode coluna in colunas)
            {
                string nome = coluna.Attributes?["name"]?.Value?.Trim();
                if (string.IsNullOrEmpty(nome))
                    continue;

                string autoIncrement = coluna.Attributes?["Autoincrement"]?.Value?.Trim()?.ToLower();
                bool isAuto = autoIncrement == "true";

                if (isAuto)
                {
                    primaryKey = nome;
                    continue;
                }
                listaSets.Add($"{nome} = @{nome}");
            }

            if (listaSets.Count == 0 || string.IsNullOrEmpty(primaryKey))
                return string.Empty;

            string sets = string.Join(", ", listaSets);
            string sql = $"UPDATE {tableName} SET {sets} WHERE {primaryKey} = @{primaryKey};";
            return sql;
        }
        private string UpdateInsertTableModelVazio(XmlNode tabela, string tableName)
        {
            if (tabela == null) return string.Empty;

            XmlNodeList colunas = tabela.SelectNodes("Column");
            if (colunas == null || colunas.Count == 0) return string.Empty;

            string primaryKey = string.Empty;
            List<string> campos = new List<string>();
            List<string> valores = new List<string>();
            List<string> updateSets = new List<string>();

            foreach (XmlNode coluna in colunas)
            {
                string nome = coluna.Attributes?["name"]?.Value?.Trim();
                if (string.IsNullOrEmpty(nome))
                    continue;

                string autoIncrement = coluna.Attributes?["Autoincrement"]?.Value?.Trim()?.ToLower();
                bool isAuto = autoIncrement == "true";

                if (isAuto)
                {
                    primaryKey = nome;
                }

                campos.Add(nome);
                valores.Add($"@{nome}");

                if (!isAuto)
                    updateSets.Add($"{nome} = excluded.{nome}");
            }

            if (string.IsNullOrEmpty(primaryKey))
                return string.Empty;

            string camposStr = string.Join(", ", campos);
            string valoresStr = string.Join(", ", valores);
            string updateSetsStr = string.Join(", ", updateSets);

            string sqlStr = $@" INSERT INTO {tableName} ({camposStr}) VALUES ({valoresStr}) ON CONFLICT({primaryKey}) DO UPDATE SET {updateSetsStr};";

            return sqlStr;
        }
        private string ProcessColumnDefinition(XmlNode col, List<string> primaryKeys, List<string> foreignKeys)
        {
            string nome = col.Attributes?["name"]?.Value;
            if (string.IsNullOrEmpty(nome)) return null;

            string tipo = col.Attributes?["type"]?.Value?.ToUpper();
            string notNull = col.Attributes?["NotNull"]?.Value;
            string key = col.Attributes?["key"]?.Value;
            string autoInc = col.Attributes?["Autoincrement"]?.Value;
            string enumRange = col.Attributes?["enumrange"]?.Value;
            string maxLength = col.Attributes?["maxlenght"]?.Value;
            string comment = col.Attributes?["comment"]?.Value;
            string referencesFlag = col.Attributes?["references"]?.Value;
            string referencedTable = col.Attributes?["tablename"]?.Value;

            var definicao = new StringBuilder();
            // Nome da coluna
            definicao.Append(nome);

            // Tipo de dados
            definicao.Append(" " + GetSqlDataType(nome, tipo, maxLength, enumRange));

            // Primary Key (tratada separadamente)
            // && 
            if (!string.IsNullOrEmpty(key))
            {
                if (key.Equals("Primary", StringComparison.OrdinalIgnoreCase))
                {
                    primaryKeys.Add(nome);

                    if (autoInc == "true" && (tipo == "INTEGER" || tipo == "INT"))
                        definicao.Append(" PRIMARY KEY AUTOINCREMENT");// Para SQLite: PRIMARY KEY AUTOINCREMENT deve vir junto
                    else
                        definicao.Append(" PRIMARY KEY");// Primary key normal - será tratada no final

                }
                else if (key.Equals("Foreign", StringComparison.OrdinalIgnoreCase))
                {
                    if (referencesFlag == "true" && !string.IsNullOrEmpty(referencedTable))
                    {
                        string referencedColumn = GetReferencedColumn(referencedTable);
                        if (!string.IsNullOrEmpty(referencedColumn))
                            definicao.Append($" REFERENCES {referencedTable}({referencedColumn}) ON DELETE CASCADE ON UPDATE CASCADE");
                    }
                }
                else if (key.Equals("Unique", StringComparison.OrdinalIgnoreCase))
                    definicao.Append(" UNIQUE");



            }
            else


            {
                if (autoInc == "true")
                    definicao.Append(" AUTOINCREMENT");

                if (notNull == "true")
                    definicao.Append(" NOT NULL");



            }

            return definicao.ToString();
        }
        private string BuildFinalSqlStatement(string nomeTabela, List<string> columns, List<string> primaryKeys, List<string> foreignKeys, bool isExists)
        {
            if (columns.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            sb.Append($"CREATE TABLE {(isExists ? "IF NOT EXISTS " : "")}{nomeTabela}(");
            sb.AppendLine();

            // Constrói todas as partes da definição da tabela
            var tableParts = new List<string>();

            // Adiciona colunas
            tableParts.AddRange(columns);

            // Adiciona primary key se existir
            if (primaryKeys.Count > 0 && !HasAutoIncrementPrimaryKey(columns, primaryKeys))
                tableParts.Add($"PRIMARY KEY ({string.Join(", ", primaryKeys)})");

            if (foreignKeys.Count > 0)
                tableParts.AddRange(foreignKeys);

            sb.AppendLine("  " + string.Join(",\n  ", tableParts));
            sb.AppendLine(");");
            return sb.ToString();
        }
        private bool HasAutoIncrementPrimaryKey(List<string> columns, List<string> primaryKeys)
        {
            // Verifica se alguma das primary keys já tem "PRIMARY KEY AUTOINCREMENT" na definição da coluna
            foreach (var pk in primaryKeys)
            {
                var pkColumn = columns.FirstOrDefault(c => c.StartsWith(pk + " "));
                if (pkColumn != null && pkColumn.Contains("PRIMARY KEY AUTOINCREMENT"))
                    return true;
            }
            return false;
        }

        private string GetSqlDataType(string columnName, string tipo, string maxLength, string enumRange)
        {
            // Converte para maiúsculas para comparação case-insensitive
            string tipoUpper = tipo?.ToUpper();

            switch (tipoUpper)
            {
                case "INTEGER":
                    return "INTEGER";
                case "ENUM":
                    if (!string.IsNullOrEmpty(enumRange))
                        return $"TEXT CHECK({columnName} IN {enumRange})";
                    return "TEXT"; // fallback para ENUM sem range
                case "VARCHAR":
                    if (!string.IsNullOrEmpty(maxLength) && int.TryParse(maxLength, out int length))
                        return $"VARCHAR({length})";
                    return "VARCHAR(255)";
                case "BOOLEAN":
                    return "BOOLEAN";
                case "DATE":
                    return "DATE";
                case "DATETIME":
                    return "DATETIME";
                case "REAL":
                    return "REAL";
                case "NUMERIC":
                    return "NUMERIC";
                case "BLOB":
                    return "BLOB";
                default:
                    // fallback para qualquer outro tipo não reconhecido
                    return "TEXT";
            }
        }
        private string GetReferencedColumn(string referencedTable)
        {
            // Procura a tabela no XML
            var tableNode = xmlDoc.SelectSingleNode($"//table[@name='{referencedTable}']");
            if (tableNode == null) return null;

            // Procura a coluna marcada como Primary
            var pkColumn = tableNode.SelectSingleNode("Column[@key='Primary']");
            if (pkColumn?.Attributes?["name"] != null)
                return pkColumn.Attributes["name"].Value;

            return null;
        }
        public static string FormatStruturaXml(XmlDocument doc)
        {
            if (doc == null)
                return string.Empty;

            using (var stringWriter = new StringWriter())
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.Replace
                };

                using (var xmlTextWriter = XmlWriter.Create(stringWriter, settings))
                {
                    doc.Save(xmlTextWriter);
                }

                return stringWriter.ToString();
            }
        }



        public void CriarFileXML(string LeituraFileDestino, string xmlContent = null)
        {
            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);
            File.WriteAllText(LeituraFileDestino, FormatStruturaXml(xmlDoc));

        }
        public static string LerXmlFile(string localfile)
        {
            string textoDesencriptado = File.ReadAllText(localfile);
            XElement xmlElement = XElement.Parse(textoDesencriptado);
            textoDesencriptado = xmlElement.ToString();
            return textoDesencriptado;
        }
        public string LerLayoutXml(string xmlContent = null) // pega o arquivo do xml definido
        {
            if (string.IsNullOrWhiteSpace(xmlContent))
                return null; // retorna null se não tiver conteúdo

            try
            {
                xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlContent); // tenta carregar o XML
                var xml_doc = FormatStruturaXml(xmlDoc);
                return xml_doc; // já retorna null se FormatStruturaXml retornar null
            }
            catch (XmlException)
            {
                // Se o xmlContent estiver mal formado, retorna null
                return null;
            }
        }






        public void GravarUpdateFileXML(string LocalFile, string xmlContent)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlContent);
            File.WriteAllText(LocalFile, FormatStruturaXml(doc));

        }


        public static bool CompareXmlStructure(string xml1, string xml2)
        {
            // Carregar os dois XMLs
            XElement xmlElement1 = XElement.Parse(xml1);
            XElement xmlElement2 = XElement.Parse(xml2);

            // Comparar as duas estruturas de XML
            return AreXmlElementsEqual(xmlElement1, xmlElement2);
        }

        private static bool AreXmlElementsEqual(XElement element1, XElement element2)
        {
            // Verifica se o nome do elemento é igual
            if (element1.Name != element2.Name)
                return false;

            // Compara os atributos (sem comparar os valores dos elementos)
            var attributes1 = element1.Attributes();
            var attributes2 = element2.Attributes();
            if (attributes1.Count() != attributes2.Count())
                return false;

            foreach (var attribute1 in attributes1)
            {
                var attribute2 = attributes2.FirstOrDefault(a => a.Name == attribute1.Name);
                if (attribute2 == null)
                    return false; // Se um atributo estiver faltando
            }

            // Compara os filhos (sub-elementos) sem considerar os valores, mas considerando a quantidade e o nome
            var children1 = element1.Elements("setting").GroupBy(e => e.Attribute("name")?.Value).ToList();
            var children2 = element2.Elements("setting").GroupBy(e => e.Attribute("name")?.Value).ToList();

            // Verifica se o número de grupos de elementos é o mesmo
            if (children1.Count != children2.Count)
                return false;

            // Compara cada grupo de "setting" com o mesmo nome
            foreach (var group1 in children1)
            {
                var group2 = children2.FirstOrDefault(g => g.Key == group1.Key);
                if (group2 == null || group1.Count() != group2.Count())
                    return false;

                // Para cada "setting" no grupo, verifica se a estrutura é a mesma
                foreach (var setting1 in group1)
                {
                    var setting2 = group2.FirstOrDefault(s => s.Attribute("name")?.Value == setting1.Attribute("name")?.Value);
                    if (setting2 == null || !AreXmlElementsEqual(setting1, setting2))
                        return false;
                }
            }

            return true;
        }

        public string AtualizarCamposAnalytics(string conteudoXML, Dictionary<string, Dictionary<string, string>> dadosAtualizados)
        {
            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(conteudoXML);

            foreach (var secao in dadosAtualizados)
            {
                XmlNode sectionNode = xmlDoc.SelectSingleNode($"//{secao.Key}");
                if (sectionNode == null)
                    continue;

                foreach (var campo in secao.Value)
                {
                    XmlNode campoNode = sectionNode.SelectSingleNode(campo.Key);
                    if (campoNode != null)
                        campoNode.InnerText = campo.Value ?? string.Empty;
                }
            }

            return FormatStruturaXml(xmlDoc);
        }

        /// <summary>
        /// Lê uma seção específica do XML de analytics (ex: "ComputerData", "UserData")
        /// e retorna os campos filhos como Dictionary<NomeCampo, Valor>.
        /// </summary>
        public Dictionary<string, string> ScraperXmlAnalyticsSection(string conteudoXML, string sectionName)
        {
            var CollectionCampos = new Dictionary<string, string>();

            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(conteudoXML);

            XmlNode sectionNode = xmlDoc.SelectSingleNode($"//{sectionName}");
            if (sectionNode == null)
                return CollectionCampos;

            foreach (XmlNode campo in sectionNode.ChildNodes)
            {
                if (campo.NodeType != XmlNodeType.Element)
                    continue;

                string nomeCampo = campo.Name;
                string valorCampo = campo.InnerText;

                if (CollectionCampos.ContainsKey(nomeCampo))
                    CollectionCampos[nomeCampo] = valorCampo;
                else
                    CollectionCampos.Add(nomeCampo, valorCampo);
            }

            return CollectionCampos;
        }


        /// <summary>
        /// Lê o XML de analytics completo (ex: betaanalytics) e retorna todas as seções
        /// (ComputerData, UserData, etc.) já mapeadas: Dictionary<NomeSecao, Dictionary<NomeCampo, Valor>>.
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> ScraperXmlAnalyticsContent(string conteudoXML)
        {
            var resultado = new Dictionary<string, Dictionary<string, string>>();

            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(conteudoXML);

            XmlNode rootNode = xmlDoc.DocumentElement; // <betaanalytics>
            if (rootNode == null)
                return resultado;

            foreach (XmlNode sectionNode in rootNode.ChildNodes)
            {
                if (sectionNode.NodeType != XmlNodeType.Element)
                    continue;

                string nomeSecao = sectionNode.Name; // ComputerData, UserData...
                var camposSecao = new Dictionary<string, string>();

                foreach (XmlNode campo in sectionNode.ChildNodes)
                {
                    if (campo.NodeType != XmlNodeType.Element)
                        continue;

                    string nomeCampo = campo.Name;
                    string valorCampo = campo.InnerText;

                    if (camposSecao.ContainsKey(nomeCampo))
                        camposSecao[nomeCampo] = valorCampo;
                    else
                        camposSecao.Add(nomeCampo, valorCampo);
                }

                if (resultado.ContainsKey(nomeSecao))
                    resultado[nomeSecao] = camposSecao;
                else
                    resultado.Add(nomeSecao, camposSecao);
            }

            return resultado;
        }


    }
}
