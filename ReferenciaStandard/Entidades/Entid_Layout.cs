
using ReferenciaStandard.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReferenciaStandard.Entidades
{
  

    /// <summary>Resultado de uma conferencia do layout (erro, aviso ou informacao).</summary>
    public sealed class LayoutFinding
    {
        public LayoutFinding(IsFindingLevel level, string code, string message, string path = null)
        {
            Level = level;
            Code = code;
            Message = message;
            Path = path;
        }

        public IsFindingLevel Level { get; }
        public string Code { get; }
        public string Message { get; }
        public string Path { get; }
    }

    /// <summary>Coluna da secao &lt;selection&gt; do layout (o "modelo" da tabela de origem).</summary>
    public sealed class LayoutColumn
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "string";
        public string Format { get; set; }
        public string Description { get; set; }
        public bool OrderBy { get; set; }
    }

    /// <summary>No da arvore &lt;field&gt; do bloco Jsonfields (estrutura do JSON de saida).</summary>
    public sealed class LayoutField
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "string";
        /// <summary>Valor fixo (atributo value).</summary>
        public string Value { get; set; }
        /// <summary>Coluna de origem (atributo Columnrefe).</summary>
        public string ColumnRef { get; set; }
        public string Format { get; set; }
        public List<LayoutField> Children { get; } = new List<LayoutField>();
    }

    public sealed class LayoutDefinition
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string GenerateName { get; set; }
        public string FilePath { get; set; } = "";
        /// <summary>"padrao" (embutido ou pasta do aplicativo) ou "importado" (pasta do usuario).</summary>
        public string Source { get; set; } = "padrao";
        public string TableName { get; set; } = "";
        public string TableAlias { get; set; } = "t";
        public List<LayoutColumn> Columns { get; } = new List<LayoutColumn>();
        /// <summary>Campo array de nivel raiz: cada linha do banco vira um item dele (ex.: "declaracoes").</summary>
        public LayoutField RootField { get; set; }
        public List<LayoutFinding> Findings { get; } = new List<LayoutFinding>();
        public string Xml { get; set; } = "";

        public LayoutColumn FindColumn(string name)
        {
            if (name == null) return null;
            return Columns.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public int ErrorCount { get { return Findings.Count(f => f.Level == IsFindingLevel.Error); } }
        public int WarningCount { get { return Findings.Count(f => f.Level == IsFindingLevel.Warning); } }
    }

    /// <summary>Um XML de layout encontrado por LayoutsRepository (ainda sem interpretar).</summary>
    public sealed class LayoutArquivo
    {
        public string Nome { get; set; } = "";
        /// <summary>Caminho no disco, ou "(embutido) Nome.xml" quando vem de dentro da DLL.</summary>
        public string Caminho { get; set; } = "";
        /// <summary>"padrao" ou "importado".</summary>
        public string Origem { get; set; } = "padrao";
        public string Xml { get; set; }
        /// <summary>Preenchido quando o arquivo nao pode ser lido.</summary>
        public string Erro { get; set; }
    }
}
