using Newtonsoft.Json;
using ReferenciaFactory.Collections;
using ReferenciaStandard.Helpers;
using ReferenciaStandard.Repository;

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
        public static string Raiz => AppPaths.Raiz;
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
            var lista = JsonHelper.LerArquivo(_arquivo, new List<ModeloSchedule>()) ?? new List<ModeloSchedule>();
            _dados = lista.Where(s => !string.IsNullOrWhiteSpace(s.FilePath))
                          .GroupBy(s => s.FilePath, StringComparer.OrdinalIgnoreCase)
                          .ToDictionary(g => g.Key, g => g.Last(), StringComparer.OrdinalIgnoreCase);
        }

        private void Gravar()
        {
            if (!JsonHelper.GravarArquivo(_arquivo, _dados.Values.ToList(), out var erro))
                throw new IOException(erro);
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

                _dados.TryGetValue(novo.FilePath, out var anterior);
                _dados[novo.FilePath] = novo;
                try { Gravar(); }
                catch (Exception ex)
                {
                    if (anterior != null) _dados[novo.FilePath] = anterior; else _dados.Remove(novo.FilePath);
                    return (false, "Não foi possível gravar: " + ex.Message);
                }
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

        /// <summary>
        /// Devolve a lista salva, inclusive arquivos que nao existem mais: a janela mostra esses itens em
        /// vermelho para o usuario remover. Filtrar aqui apagaria da lista, na proxima gravacao,
        /// arquivos de uma unidade de rede que estivesse apenas offline.
        /// </summary>
        public static List<string> Ler() =>
            JsonHelper.LerArquivo(Arquivo, new List<string>()) ?? new List<string>();

        public static void Gravar(IEnumerable<string> caminhos) =>
            JsonHelper.GravarArquivo(Arquivo, caminhos.Take(Maximo).ToList(), out _);

        public static void Adicionar(string path)
        {
            var l = Ler();
            l.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
            l.Insert(0, path);
            Gravar(l);
        }
    }

}
