namespace BetaJsonIntegrator.Core.Validation
{
    public enum IssueSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    /// <summary>
    /// Mensagem de validação/pendência. Nunca deve carregar valores de dados pessoais:
    /// apenas nomes de campos, caminhos JSON e números de linha.
    /// </summary>
    public sealed class ValidationIssue
    {
        public IssueSeverity Severity { get; init; }
        public string Code { get; init; } = "";
        public string Path { get; init; } = "";
        public string Message { get; init; } = "";

        public static ValidationIssue Info(string code, string path, string message) =>
            new() { Severity = IssueSeverity.Info, Code = code, Path = path, Message = message };

        public static ValidationIssue Warning(string code, string path, string message) =>
            new() { Severity = IssueSeverity.Warning, Code = code, Path = path, Message = message };

        public static ValidationIssue Error(string code, string path, string message) =>
            new() { Severity = IssueSeverity.Error, Code = code, Path = path, Message = message };

        public override string ToString() =>
            $"[{Severity}] {Code}" + (string.IsNullOrEmpty(Path) ? "" : $" @ {Path}") + $": {Message}";
    }
}
