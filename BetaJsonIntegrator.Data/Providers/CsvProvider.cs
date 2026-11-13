using System.Data;
using System.Text;
using BetaJsonIntegrator.Core;
using BetaJsonIntegrator.Core.Configuration;

namespace BetaJsonIntegrator.Data.Providers
{
    public sealed class CsvOptions
    {
        public char Delimiter { get; set; } = ';';
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public bool HasHeader { get; set; } = true;
    }

    /// <summary>
    /// Leitor CSV (RFC 4180: campos entre aspas, aspas duplicadas, quebras de linha dentro de aspas).
    /// Todas as colunas chegam como texto; campos vazios viram DBNull. O motor converte conforme o XML.
    /// Sem cabeçalho, as colunas são nomeadas pela ordem de &lt;selection&gt; (ou Col1, Col2...).
    /// </summary>
    public sealed class CsvProvider : IDataProvider
    {
        private readonly Dictionary<string, string> _files;
        private readonly string? _single;
        private readonly CsvOptions _options;

        /// <summary>Um único arquivo para qualquer tabela do layout.</summary>
        public CsvProvider(string filePath, CsvOptions? options = null)
        {
            _single = filePath;
            _files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _options = options ?? new CsvOptions();
        }

        /// <summary>Um arquivo por alias de tabela.</summary>
        public CsvProvider(IDictionary<string, string> filesByAlias, CsvOptions? options = null)
        {
            _files = new Dictionary<string, string>(filesByAlias, StringComparer.OrdinalIgnoreCase);
            _options = options ?? new CsvOptions();
        }

        public string DisplayName => "CSV";

        public DataTable Load(TableConfig table)
        {
            var path = _single ?? (_files.TryGetValue(table.Alias, out var p) ? p
                : throw new KeyNotFoundException($"Nenhum arquivo CSV informado para o alias '{table.Alias}'."));
            if (!File.Exists(path)) throw new FileNotFoundException("Arquivo CSV não encontrado.", path);

            using var reader = new StreamReader(path, _options.Encoding, detectEncodingFromByteOrderMarks: true);
            var rows = ReadRecords(reader, _options.Delimiter).ToList();

            var dt = new DataTable(table.Alias);
            if (rows.Count == 0) return dt;

            List<string> headers;
            int start;
            if (_options.HasHeader)
            {
                headers = rows[0].Select(h => h.Trim().TrimStart('\uFEFF')).ToList();
                start = 1;
            }
            else
            {
                headers = table.Columns.Count > 0
                    ? table.Columns.Select(c => c.Name).ToList()
                    : Enumerable.Range(1, rows[0].Count).Select(i => "Col" + i).ToList();
                start = 0;
            }

            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Count; i++)
            {
                var name = string.IsNullOrWhiteSpace(headers[i]) ? "Col" + (i + 1) : headers[i];
                var unique = name; int n = 2;
                while (!used.Add(unique)) unique = name + "_" + n++;
                dt.Columns.Add(unique, typeof(string));
            }

            for (int r = start; r < rows.Count; r++)
            {
                var rec = rows[r];
                if (rec.Count == 1 && rec[0].Length == 0) continue; // linha vazia
                var dr = dt.NewRow();
                for (int c = 0; c < dt.Columns.Count; c++)
                {
                    var val = c < rec.Count ? rec[c] : "";
                    dr[c] = val.Length == 0 ? DBNull.Value : val;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        internal static IEnumerable<List<string>> ReadRecords(TextReader reader, char delimiter)
        {
            var field = new StringBuilder();
            var record = new List<string>();
            bool inQuotes = false, any = false;
            int ch;

            while ((ch = reader.Read()) != -1)
            {
                var c = (char)ch;
                any = true;

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (reader.Peek() == '"') { reader.Read(); field.Append('"'); }
                        else inQuotes = false;
                    }
                    else field.Append(c);
                    continue;
                }

                if (c == '"' && field.Length == 0) { inQuotes = true; continue; }
                if (c == delimiter) { record.Add(field.ToString()); field.Clear(); continue; }
                if (c == '\r') continue;
                if (c == '\n')
                {
                    record.Add(field.ToString()); field.Clear();
                    yield return record;
                    record = new List<string>();
                    any = false;
                    continue;
                }
                field.Append(c);
            }

            if (any || field.Length > 0 || record.Count > 0)
            {
                record.Add(field.ToString());
                yield return record;
            }
        }
    }
}
