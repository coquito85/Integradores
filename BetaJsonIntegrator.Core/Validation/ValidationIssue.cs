namespace BetaJsonIntegrator.Core.Validation;

public enum IssueSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2
}

/// <summary>
/// Ocorrência de validação com contexto. <see cref="Path"/> é o caminho do campo no layout
/// (ex.: <c>declaracoes.adquirentes.ni</c>) ou, após a geração, o caminho no JSON
/// (ex.: <c>declaracoes[0].adquirentes[0].ni</c>). A mensagem nunca contém valores dos dados.
/// </summary>
public sealed record ValidationIssue(IssueSeverity Severity, string Code, string Path, string Message)
{
    public static ValidationIssue Error(string code, string path, string message) =>
        new(IssueSeverity.Error, code, path, message);

    public static ValidationIssue Warning(string code, string path, string message) =>
        new(IssueSeverity.Warning, code, path, message);

    public static ValidationIssue Info(string code, string path, string message) =>
        new(IssueSeverity.Info, code, path, message);

    public override string ToString()
    {
        var level = Severity switch
        {
            IssueSeverity.Error => "[Erro]",
            IssueSeverity.Warning => "[Aviso]",
            _ => "[Info]"
        };
        return string.IsNullOrEmpty(Path)
            ? $"{level} {Code}: {Message}"
            : $"{level} {Code} {Path}: {Message}";
    }
}
