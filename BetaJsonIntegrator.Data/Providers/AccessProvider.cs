using System.Data.Common;

namespace BetaJsonIntegrator.Data.Providers;

/// <summary>
/// Microsoft Access (.mdb/.accdb) via OleDb. Passe <c>OleDbFactory.Instance</c>. Identificadores entre colchetes
/// e parâmetros posicionais (?). Exige o Access Database Engine (ACE) instalado, com a mesma arquitetura do app.
/// </summary>
public sealed class AccessProvider : DbQueryProvider
{
    public AccessProvider(DbProviderFactory factory, string connectionString)
        : base(factory, connectionString) { }

    public override string Name => "Access";
    protected override char OpenQuote => '[';
    protected override char CloseQuote => ']';
    protected override string ParameterMarker(int index) => "?";
}
