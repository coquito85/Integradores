using System;
using System.Collections.Generic;

namespace ReferenciaStandard.Entidades
{
    public sealed class MapOptions
    {
        /// <summary>true = comportamento do Extractor original: &lt;field type="array"/&gt; sem filhos vira null. false = [].</summary>
        public bool EmptyArrayAsNull { get; set; } = true;
    }

    /// <summary>Estatisticas da conversao (colunas vazias e falhas de conversao) para o relatorio final.</summary>
    public sealed class MapStats
    {
        public Dictionary<string, long> EmptyByColumn { get; } = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        public List<string> Issues { get; } = new List<string>();
        public long IssueCount { get; private set; }

        public void Issue(string message)
        {
            IssueCount++;
            if (Issues.Count < 50) Issues.Add(message);
        }

        public void Empty(string column)
        {
            long n;
            EmptyByColumn[column] = EmptyByColumn.TryGetValue(column, out n) ? n + 1 : 1;
        }
    }
}