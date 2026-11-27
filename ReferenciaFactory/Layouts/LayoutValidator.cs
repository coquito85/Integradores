using ReferenciaStandard.Entidades;
using ReferenciaStandard.Repository;


namespace ReferenciaFactory.Layouts
{

    /// <summary>
    /// Validacao estatica do layout (sem banco). Pega cedo o que antes so aparecia como "campo null" no JSON.
    /// </summary>
    public static class LayoutValidator
    {
        private static readonly HashSet<string> KnownTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "string", "text", "varchar", "bool", "boolean", "decimal", "money", "currency",
        "int", "integer", "long", "double", "float", "datetime", "date", "array", "object"
    };

        public static List<LayoutFinding> Validate(LayoutDefinition l)
        {
            var f = new List<LayoutFinding>();

            if (string.IsNullOrWhiteSpace(l.TableName))
                f.Add(new(IsFindingLevel.Error, "SEM_TABELA", "A <table> do layout nao tem o atributo name."));
            if (l.Columns.Count == 0)
                f.Add(new(IsFindingLevel.Error, "SEM_COLUNAS", "Nenhuma <Column> em <selection>."));

            foreach (var dup in l.Columns.GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
                f.Add(new(IsFindingLevel.Error, "COLUNA_DUPLICADA", $"A coluna '{dup.Key}' aparece {dup.Count()}x em <selection>."));

            foreach (var c in l.Columns.Where(c => !KnownTypes.Contains(c.Type)))
                f.Add(new(IsFindingLevel.Warning, "TIPO_DESCONHECIDO", $"Coluna '{c.Name}': tipo '{c.Type}' desconhecido (sera tratado como string).", c.Name));

            if (l.RootField is null)
            {
                f.Add(new(IsFindingLevel.Error, "SEM_RAIZ_JSON", "Nao ha <Jsonfields><Refealias><field> com a estrutura do JSON (ou o alias nao confere com o da tabela)."));
                return f;
            }

            var names = new HashSet<string>(l.Columns.Select(c => c.Name), StringComparer.OrdinalIgnoreCase);
            var used = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            void Walk(LayoutField field, string path)
            {
                var here = path.Length == 0 ? field.Name : path + " → " + field.Name;

                if (!KnownTypes.Contains(field.Type))
                    f.Add(new(IsFindingLevel.Warning, "TIPO_DESCONHECIDO", $"Campo '{here}': tipo '{field.Type}' desconhecido (sera tratado como string).", here));

                if (field.ColumnRef is not null)
                {
                    if (!names.Contains(field.ColumnRef))
                        f.Add(new(IsFindingLevel.Error, "COLUNA_INEXISTENTE", $"Campo '{here}' aponta para a coluna '{field.ColumnRef}', que nao existe em <selection>.", here));
                    else
                    {
                        if (!used.TryGetValue(field.ColumnRef, out var list)) used[field.ColumnRef] = list = new();
                        list.Add(here);
                    }
                }
                else if (field.Children.Count == 0 && field.Value is not null && names.Contains(field.Value))
                {
                    f.Add(new(IsFindingLevel.Warning, "VALOR_PARECE_COLUNA",
                        $"Campo '{here}' tem value=\"{field.Value}\" (texto fixo), mas '{field.Value}' e uma coluna da selecao. Se a intencao era ler do banco, use Columnrefe=\"{field.Value}\".", here));
                }
                else if (field.Children.Count == 0 && field.Value is null && !field.Type.Equals("array", StringComparison.OrdinalIgnoreCase))
                {
                    f.Add(new(IsFindingLevel.Info, "CAMPO_SEM_ORIGEM", $"Campo '{here}' nao tem Columnrefe nem value: sera sempre null.", here));
                }

                if (field.Type == "datetime" && field.Format is { } fmt && fmt.Contains("mm") && !fmt.Contains("MM") && !fmt.Contains('h', StringComparison.OrdinalIgnoreCase))
                    f.Add(new(IsFindingLevel.Info, "FORMATO_MES", $"Campo '{here}': formato \"{fmt}\" interpretado com mm = MES (sem hora no formato).", here));

                foreach (var c in field.Children) Walk(c, here);
            }

            Walk(l.RootField, "");

            foreach (var kv in used.Where(k => k.Value.Count > 1))
                f.Add(new(IsFindingLevel.Warning, "COLUNA_EM_VARIOS_CAMINHOS", $"A coluna '{kv.Key}' alimenta {kv.Value.Count} campos: {string.Join("; ", kv.Value)}."));

            var notUsed = l.Columns.Where(c => !used.ContainsKey(c.Name)).Select(c => c.Name).ToList();
            if (notUsed.Count > 0)
                f.Add(new(IsFindingLevel.Info, "COLUNA_NAO_USADA", $"{notUsed.Count} coluna(s) da selecao nao alimentam nenhum campo do JSON: {string.Join(", ", notUsed)}."));

            return f;
        }
    }
}
