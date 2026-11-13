using System.Text.RegularExpressions;

namespace BetaJsonIntegrator.Core.Services
{
    public interface IIntegratorLog
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message);
    }

    public sealed class NullIntegratorLog : IIntegratorLog
    {
        public static readonly NullIntegratorLog Instance = new();
        public void Info(string message) { }
        public void Warn(string message) { }
        public void Error(string message) { }
    }

    /// <summary>Log em arquivo (Logs/integrator-yyyy-MM-dd.log). CPF/CNPJ são mascarados.</summary>
    public sealed class FileIntegratorLog : IIntegratorLog
    {
        private static readonly Regex CpfCnpj = new(
            @"\b\d{3}\.?\d{3}\.?\d{3}-?\d{2}\b|\b\d{2}\.?\d{3}\.?\d{3}/?\d{4}-?\d{2}\b", RegexOptions.Compiled);

        private readonly string _folder;
        private readonly object _lock = new();

        public FileIntegratorLog(string folder)
        {
            _folder = folder;
            Directory.CreateDirectory(folder);
        }

        public static string Mask(string text) => CpfCnpj.Replace(text, "***");

        public void Info(string message) => Write("INFO", message);
        public void Warn(string message) => Write("WARN", message);
        public void Error(string message) => Write("ERROR", message);

        private void Write(string level, string message)
        {
            var file = Path.Combine(_folder, $"integrator-{DateTime.Now:yyyy-MM-dd}.log");
            var line = $"{DateTime.Now:HH:mm:ss.fff} [{level}] {Mask(message)}{Environment.NewLine}";
            lock (_lock)
            {
                try { File.AppendAllText(file, line); } catch (IOException) { /* log nunca derruba a execução */ }
            }
        }
    }
}
