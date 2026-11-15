using System.Data;
using System.Text;

namespace BetaJsonIntegrator.Data.Providers;

/// <summary>Opções do leitor CSV (spec. seção 3: delimitador, encoding e cabeçalho configuráveis).</summary>
public sealed class CsvOptions
{
    public char Delimiter { get; init; } = ';';
    public Encoding Encoding { get; init; } = new UTF8Encoding(false);
    public bool HasHeader { get; init; } = true;
}

/// <summary>
/// Lê um CSV (RFC 4180: campos entre aspas, delimitador e quebras de linha dentro de aspas).
/// Todas as colunas chegam como texto; a conversão para o tipo JSON é feita pelo Core.
/// Células vazias viram <see cref="DBNull"/>.
/// </summary>
public sealed class CsvProvider : IDataProvider
{
    private readonly string _path;
    private readonly CsvOptions _options;

    public CsvProvider(string path, CsvOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Informe o caminho do arquivo CSV.", nameof(path));
        _path = path;
        _options = options ?? new CsvOptions();
    }

    public string Name => "CSV: " + Path.GetFileName(_path);

    public DataTable Load(DataRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream, _options.Encoding, detectEncodingFromByteOrderMarks: true);
        using var records = ReadRecords(reader, _options.Delimiter).GetEnumerator();

        List<string> header;
        if (_options.HasHeader)
        {
            if (!records.MoveNext())
                throw new InvalidDataException("O arquivo CSV está vazio.");

            header = records.Current
                .Select((h, i) =>
                {
                    var name = h.Trim().TrimStart('\uFEFF');
                    return name.Length == 0 ? "Coluna" + (i + 1) : name;
                })
                .ToList();
        }
        else
        {
            if (request.Columns.Count == 0)
                throw new InvalidDataException("CSV sem cabeçalho exige que o layout declare as colunas.");
            header = request.Columns.Select(c => c.Name).ToList();
        }

        var duplicated = header.GroupBy(h => h, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
        if (duplicated != null)
            throw new InvalidDataException($"O cabeçalho do CSV possui a coluna '{duplicated.Key}' repetida.");

        var table = new DataTable("Source");
        foreach (var name in header) table.Columns.Add(name, typeof(string));

        while (records.MoveNext())
        {
            var rec = records.Current;
            if (rec.All(string.IsNullOrWhiteSpace)) continue; // linha vazia

            var row = table.NewRow();
            for (int i = 0; i < header.Count; i++)
            {
                var text = i < rec.Count ? rec[i] : "";
                row[i] = text.Length == 0 ? DBNull.Value : text;
            }
            table.Rows.Add(row);
        }

        var filtered = DataOrdering.Filter(table, request.Filters);
        return DataOrdering.Apply(filtered, request.Columns);
    }

    private static IEnumerable<List<string>> ReadRecords(TextReader reader, char delimiter)
    {
        var field = new StringBuilder();
        var record = new List<string>();
        bool inQuotes = false;
        bool started = false;
        int c;

        while ((c = reader.Read()) >= 0)
        {
            char ch = (char)c;

            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (reader.Peek() == '"') { reader.Read(); field.Append('"'); }
                    else inQuotes = false;
                }
                else field.Append(ch);
                continue;
            }

            if (ch == '"' && field.Length == 0) { inQuotes = true; started = true; continue; }

            if (ch == delimiter)
            {
                record.Add(field.ToString());
                field.Clear();
                started = true;
                continue;
            }

            if (ch == '\r')
            {
                if (reader.Peek() == '\n') reader.Read();
                ch = '\n';
            }

            if (ch == '\n')
            {
                if (started || field.Length > 0 || record.Count > 0)
                {
                    record.Add(field.ToString());
                    yield return record;
                }
                record = new List<string>();
                field.Clear();
                started = false;
                continue;
            }

            field.Append(ch);
            started = true;
        }

        if (started || field.Length > 0 || record.Count > 0)
        {
            record.Add(field.ToString());
            yield return record;
        }
    }
}
