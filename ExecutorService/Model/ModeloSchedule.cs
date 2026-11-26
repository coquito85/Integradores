using ReferenciaFactory.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExecutorService.Model
{
    public sealed class ModeloSchedule
    {
        public string FilePath { get; set; } = "";
        public int Ordem { get; set; }
        public bool Agendado { get; set; }
        public DateTime? DataExecucao { get; set; }
        public DateTime? DataFim { get; set; }

        public DateTime? UltimoInicio { get; set; }
        public DateTime? UltimoFim { get; set; }
        public IsStatusExecute UltimoStatus { get; set; } = IsStatusExecute.Nenhum;
        public string? UltimoDetalhe { get; set; }
        public DateTime? IgnoradoEm { get; set; }

        public ModeloSchedule Clone() => (ModeloSchedule)MemberwiseClone();

        [JsonIgnore]
        public int DuracaoUltimaSegundos =>
            UltimoInicio.HasValue && UltimoFim.HasValue ? (int)Math.Max(0, (UltimoFim.Value - UltimoInicio.Value).TotalSeconds) : 0;
    }

    internal static class Pastas
    {
        public static readonly string Raiz = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OrionAutomator");

        public static readonly JsonSerializerOptions Json = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <summary>Persistencia dos agendamentos (JSON em %AppData%\OrionAutomator\agendamentos.json).</summary>
    public sealed class AgendamentoStore
    {
        public static AgendamentoStore Instance { get; } = new();

        private readonly object _lock = new();
        private readonly string _arquivo = Path.Combine(Pastas.Raiz, "agendamentos.json");
        private Dictionary<string, ModeloSchedule> _dados = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Disparado (em qualquer thread) quando um agendamento muda; o argumento e o caminho do fluxo.</summary>
        public event Action<string>? Atualizado;

        private AgendamentoStore() { Carregar(); }

        private void Carregar()
        {
            try
            {
                if (!File.Exists(_arquivo)) return;
                var lista = JsonSerializer.Deserialize<List<ModeloSchedule>>(File.ReadAllText(_arquivo), Pastas.Json) ?? new();
                _dados = lista.Where(s => !string.IsNullOrWhiteSpace(s.FilePath))
                              .ToDictionary(s => s.FilePath, StringComparer.OrdinalIgnoreCase);
            }
            catch { _dados = new(StringComparer.OrdinalIgnoreCase); }
        }

        private void Gravar()
        {
            Directory.CreateDirectory(Pastas.Raiz);
            File.WriteAllText(_arquivo, JsonSerializer.Serialize(_dados.Values.ToList(), Pastas.Json));
        }

        public Dictionary<string, ModeloSchedule> Snapshot()
        {
            lock (_lock) return new Dictionary<string, ModeloSchedule>(_dados, StringComparer.OrdinalIgnoreCase);
        }

        public ModeloSchedule? Obter(string path)
        {
            lock (_lock) return _dados.TryGetValue(path, out var s) ? s : null;
        }

        public int ProximaOrdem()
        {
            lock (_lock) return _dados.Count == 0 ? 1 : _dados.Values.Max(s => s.Ordem) + 1;
        }

        public (bool ok, string? erro) Salvar(ModeloSchedule novo)
        {
            lock (_lock)
            {
                if (novo.Agendado)
                {
                    if (!novo.DataExecucao.HasValue || !novo.DataFim.HasValue)
                        return (false, "Informe data, hora inicial e hora final.");
                    if (novo.DataFim <= novo.DataExecucao)
                        return (false, "A hora final deve ser maior que a hora inicial.");

                    var conflito = _dados.Values.FirstOrDefault(o =>
                        !string.Equals(o.FilePath, novo.FilePath, StringComparison.OrdinalIgnoreCase) &&
                        o.Agendado && o.DataExecucao.HasValue && o.DataFim.HasValue &&
                        novo.DataExecucao < o.DataFim && o.DataExecucao < novo.DataFim);

                    if (conflito != null)
                        return (false, $"Conflito com \"{Path.GetFileNameWithoutExtension(conflito.FilePath)}\" " +
                                       $"({conflito.DataExecucao:dd/MM/yyyy HH:mm} → {conflito.DataFim:HH:mm}).");
                }

                if (novo.Ordem <= 0) novo.Ordem = _dados.Count == 0 ? 1 : _dados.Values.Max(s => s.Ordem) + 1;

                _dados[novo.FilePath] = novo;
                try { Gravar(); }
                catch (Exception ex) { return (false, "Não foi possível gravar: " + ex.Message); }
            }

            Atualizado?.Invoke(novo.FilePath);
            return (true, null);
        }
    }

    /// <summary>Lista de arquivos recentes (JSON em %AppData%\OrionAutomator\recent.json).</summary>
    public static class RecentesStore
    {
        private static readonly string Arquivo = Path.Combine(Pastas.Raiz, "recent.json");
        private const int Maximo = 20;

        public static List<string> Ler()
        {
            try
            {
                if (!File.Exists(Arquivo)) return new();
                var l = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(Arquivo)) ?? new();
                return l.Where(File.Exists).ToList();
            }
            catch { return new(); }
        }

        public static void Gravar(IEnumerable<string> caminhos)
        {
            try
            {
                Directory.CreateDirectory(Pastas.Raiz);
                File.WriteAllText(Arquivo, JsonSerializer.Serialize(caminhos.Take(Maximo).ToList(), Pastas.Json));
            }
            catch { }
        }

        public static void Adicionar(string path)
        {
            var l = Ler();
            l.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
            l.Insert(0, path);
            Gravar(l);
        }
    }

}
