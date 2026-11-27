using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;

namespace ExecutorService.GeneratorJs;

/// <summary>Historico das ultimas execucoes do GeneratorJS (history.json em %AppData%).</summary>
internal sealed class HistoricoStore
{
    private const int Max = 200;
    private readonly string _arquivo;
    private readonly object _trava = new();

    public HistoricoStore(string pasta)
    {
        Directory.CreateDirectory(pasta);
        _arquivo = Path.Combine(pasta, "history.json");
    }

    public List<RunRecord> Listar()
    {
        lock (_trava) return JsonHelper.LerArquivo(_arquivo, new List<RunRecord>());
    }

    public void Adicionar(RunRecord registro)
    {
        lock (_trava)
        {
            var todos = Listar();
            todos.Insert(0, registro);
            if (todos.Count > Max) todos.RemoveRange(Max, todos.Count - Max);
            JsonHelper.GravarArquivo(_arquivo, todos, out _);
        }
    }

    public void Limpar()
    {
        lock (_trava) { if (File.Exists(_arquivo)) File.Delete(_arquivo); }
    }
}
