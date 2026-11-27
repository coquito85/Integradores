
using System;
using System.Collections.Generic;

namespace ReferenciaStandard.Entidades
{
    public sealed class FilterCondition
    {
        public string Column { get; set; } = "";
        /// <summary>eq, ne, gt, ge, lt, le, contains, startswith, isnull, notnull</summary>
        public string Operator { get; set; } = "eq";
        public string Value { get; set; } = "";
    }

    /// <summary>Pedido de geracao que a tela envia ao ExecutorService.</summary>
    public sealed class GenerateRequest
    {
        public string ProfileId { get; set; } = "";
        public string LayoutId { get; set; } = "";
        /// <summary>Vazio = tabela do layout.</summary>
        public string Table { get; set; }
        public List<FilterCondition> Filters { get; set; } = new List<FilterCondition>();
        /// <summary>"files" (grava em disco) ou "virtual" (devolve o JSON em memoria).</summary>
        public string Mode { get; set; } = "files";
        public string OutputFolder { get; set; }
        /// <summary>Tokens: {layout} {date} {time}. Extensao .json e automatica.</summary>
        public string FileName { get; set; } = "{layout}_{date}_{time}";
        /// <summary>0 = tudo em um arquivo.</summary>
        public int MaxPerFile { get; set; }
        public bool EmptyArrayAsNull { get; set; } = true;
    }

    public sealed class PreviewResult
    {
        public string Sql { get; set; } = "";
        public List<string> Parameters { get; set; } = new List<string>();
        public List<string> Columns { get; set; } = new List<string>();
        public List<List<string>> Rows { get; set; } = new List<List<string>>();
        public string Json { get; set; } = "";
        public long? TotalRows { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>Arquivo JSON gravado numa geracao.</summary>
    public sealed class GeneratedFile
    {
        public GeneratedFile() { }

        public GeneratedFile(string path, long rows, long bytes)
        {
            Path = path;
            Rows = rows;
            Bytes = bytes;
        }

        public string Path { get; set; } = "";
        public long Rows { get; set; }
        public long Bytes { get; set; }
    }

    public sealed class GenerateResult
    {
        public bool Success { get; set; }
        public bool Cancelled { get; set; }
        public string Error { get; set; }
        public long Rows { get; set; }
        public long ElapsedMs { get; set; }
        public List<GeneratedFile> Files { get; set; } = new List<GeneratedFile>();
        public string Json { get; set; }
        public Dictionary<string, long> EmptyByColumn { get; set; } = new Dictionary<string, long>();
        public List<string> Issues { get; set; } = new List<string>();
        public long IssueCount { get; set; }
    }

    /// <summary>Linha de log emitida durante o processamento (vai para a tela em tempo real).</summary>
    public sealed class LogEntry
    {
        public LogEntry(DateTime time, string level, string message, int? percent = null)
        {
            Time = time;
            Level = level;
            Message = message;
            Percent = percent;
        }

        public DateTime Time { get; }
        public string Level { get; }
        public string Message { get; }
        public int? Percent { get; }

        public static LogEntry Info(string message, int? percent = null) { return new LogEntry(DateTime.Now, "info", message, percent); }
        public static LogEntry Warn(string message) { return new LogEntry(DateTime.Now, "warn", message); }
        public static LogEntry Error(string message) { return new LogEntry(DateTime.Now, "error", message); }
        public static LogEntry Ok(string message, int? percent = null) { return new LogEntry(DateTime.Now, "ok", message, percent); }
    }

    /// <summary>Uma linha do historico de execucoes.</summary>
    public sealed class RunRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime StartedAt { get; set; } = DateTime.Now;
        public string LayoutId { get; set; } = "";
        public string LayoutName { get; set; } = "";
        public string ProfileName { get; set; } = "";
        public string Mode { get; set; } = "files";
        /// <summary>ok | error | cancelled</summary>
        public string Status { get; set; } = "ok";
        public long Rows { get; set; }
        public long ElapsedMs { get; set; }
        public string Message { get; set; }
        public List<GeneratedFile> Files { get; set; } = new List<GeneratedFile>();
    }
}
