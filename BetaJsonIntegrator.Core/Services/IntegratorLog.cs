using System.Text;

namespace BetaJsonIntegrator.Core.Services;

/// <summary>Log técnico. Nunca receba CPF/CNPJ ou dados pessoais nas mensagens (spec. seção 14).</summary>
public interface IIntegratorLog
{
    void Info(string message);
    void Warning(string message);
    void Error(string message);
}

public sealed class NullIntegratorLog : IIntegratorLog
{
    public void Info(string message) { }
    public void Warning(string message) { }
    public void Error(string message) { }
}

/// <summary>Grava em <c>Logs/integrator-yyyy-MM-dd.log</c> (spec. seção 13). Falha de gravação nunca derruba a execução.</summary>
public sealed class FileIntegratorLog : IIntegratorLog
{
    private readonly string _directory;
    private readonly object _lock = new();

    public FileIntegratorLog(string directory)
    {
        _directory = directory;
    }

    public void Info(string message) => Write("INFO", message);
    public void Warning(string message) => Write("WARN", message);
    public void Error(string message) => Write("ERROR", message);

    private void Write(string level, string message)
    {
        try
        {
            lock (_lock)
            {
                Directory.CreateDirectory(_directory);
                var file = Path.Combine(_directory, $"integrator-{DateTime.Now:yyyy-MM-dd}.log");
                var text = message.Replace('\r', ' ').Replace('\n', ' ');
                File.AppendAllText(file, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {text}{Environment.NewLine}",
                    new UTF8Encoding(false));
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // o log é auxiliar: não interrompe o processamento
        }
    }
}
