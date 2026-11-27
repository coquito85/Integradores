using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;
using ReferenciaStandard.Repository;

namespace ReferenciaFactory.Layouts;

/// <summary>
/// Catalogo dinamico de layouts. Pede ao ReferenciaStandard (LayoutsRepository) todos os XML que existem
/// (embutidos, pasta ao lado do exe e pasta do usuario), interpreta cada um e confere as regras.
/// Para adicionar um layout basta colocar o XML numa dessas pastas: nao ha mais switch com codigos fixos.
/// </summary>
public sealed class LayoutCatalog
{
    private readonly object _lock = new();
    private List<LayoutDefinition> _items = new();

    public List<string> LoadErrors { get; private set; } = new();

    public LayoutCatalog() => Reload();

    public IReadOnlyList<LayoutDefinition> All { get { lock (_lock) return _items.ToList(); } }

    public LayoutDefinition? Find(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        lock (_lock) return _items.FirstOrDefault(l => l.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public LayoutDefinition Get(string? id) =>
        Find(id) ?? throw new GeneratorException($"Layout '{id}' nao encontrado.");

    public void Reload()
    {
        var found = new Dictionary<string, LayoutDefinition>(StringComparer.OrdinalIgnoreCase);
        var errors = new List<string>();

        // padrao primeiro, usuario depois: o importado sobrescreve o padrao de mesmo identificador
        foreach (var arquivo in LayoutsRepository.ListarPadrao().Concat(LayoutsRepository.ListarUsuario()))
        {
            if (arquivo.Xml is null)
            {
                errors.Add($"{arquivo.Nome}: {arquivo.Erro ?? "arquivo vazio"}");
                continue;
            }

            try
            {
                var layout = Interpretar(arquivo.Xml, arquivo.Caminho, arquivo.Origem);
                found[layout.Id] = layout;
            }
            catch (Exception ex)
            {
                errors.Add($"{arquivo.Nome}: {ex.Message}");
            }
        }

        lock (_lock)
        {
            _items = found.Values.OrderBy(l => l.Name, StringComparer.OrdinalIgnoreCase).ToList();
            LoadErrors = errors;
        }
    }

    /// <summary>Valida e copia um XML para a pasta do usuario.</summary>
    public LayoutDefinition Import(string sourceFile)
    {
        var xml = LayoutsRepository.LerArquivo(sourceFile);
        var parsed = Interpretar(xml, sourceFile, LayoutsRepository.OrigemImportado);
        LayoutsRepository.SalvarImportado(parsed.Id, xml);
        Reload();
        return Get(parsed.Id);
    }

    /// <summary>Le o XML (ReferenciaStandard) e confere as regras (LayoutValidator).</summary>
    private static LayoutDefinition Interpretar(string xml, string caminho, string origem)
    {
        var layout = XmlHelper.LerLayout(xml, caminho, origem);
        layout.Findings.AddRange(LayoutValidator.Validate(layout));
        return layout;
    }
}
