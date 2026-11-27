using System;

namespace ReferenciaStandard.Helpers
{
    /// <summary>Erro "esperado" (configuracao, layout, banco): a mensagem ja e amigavel para exibir ao usuario.</summary>
    public sealed class GeneratorException : Exception
    {
        public GeneratorException(string message) : base(message) { }
        public GeneratorException(string message, Exception inner) : base(message, inner) { }
    }
}
