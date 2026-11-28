using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;
using ReferenciaStandard.Repository;

namespace ReferenciaFactory.Layouts
{

    /// <summary>
    /// Catalogo dinamico de layouts. Pede ao ReferenciaStandard (LayoutsRepository) todos os XML que existem
    /// (embutidos, pasta ao lado do exe e pasta do usuario), interpreta cada um e confere as regras.
    /// Para adicionar um layout basta colocar o XML numa dessas pastas: nao ha mais switch com codigos fixos.
    /// Quem pode USAR cada layout e decidido pela licenca do cartorio (CartorioStore): o que nao consta nela
    /// nao aparece em All, nao e achado por Find/Get e portanto nao roda.
    /// </summary>
    public sealed class LayoutCatalog
    {
        private readonly object _lock = new();
        private readonly CartorioStore? _cartorio;
        private List<LayoutDefinition> _items = new();

        public List<string> LoadErrors { get; private set; } = new();

        /// <param name="cartorio">Licenca do cartorio. Nulo = sem filtro (todos os layouts).</param>
        public LayoutCatalog(CartorioStore? cartorio = null)
        {
            _cartorio = cartorio;
            Reload();
        }

        private bool Permitido(string? id) => _cartorio is null || _cartorio.Status.Permite(id);

        public IReadOnlyList<LayoutDefinition> All
        {
            get { lock (_lock) return _items.Where(l => Permitido(l.Id)).ToList(); }
        }

        public LayoutDefinition? Find(string? id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Permitido(id)) return null;
            lock (_lock) return _items.FirstOrDefault(l => l.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        public LayoutDefinition Get(string? id) =>
            Find(id) ?? throw new GeneratorException(
                Permitido(id) ? $"Layout '{id}' nao encontrado." : $"O layout '{id}' nao consta na licenca deste cartorio.");

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

        /// <summary>Valida e copia um XML para a pasta do usuario (so se o layout constar na licenca).</summary>
        public LayoutDefinition Import(string sourceFile)
        {
            var xml = LayoutsRepository.LerArquivo(sourceFile);
            var parsed = Interpretar(xml, sourceFile, LayoutsRepository.OrigemImportado);
            if (!Permitido(parsed.Id))
                throw new GeneratorException($"O layout '{parsed.Id}' nao consta na licenca deste cartorio. Peca ao fornecedor uma licenca atualizada.");
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
}