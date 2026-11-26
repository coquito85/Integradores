using System;

namespace ReferenciaStandard.Helpers
{
#pragma warning disable

    public class ExceptionsHelper : Exception
    {
        public int CodigoErro { get; private set; }
        public int ErrorCode { get; }
        public string Operation { get; }
        public string Query { get; }

        public ExceptionsHelper() : base() { }

        public ExceptionsHelper(string mensagem) : base(mensagem) { }

        public ExceptionsHelper(string mensagem, Exception innerException)
            : base(mensagem, innerException) { }

        public ExceptionsHelper(string mensagem, int codigoErro)
            : base(mensagem)
        {
            CodigoErro = codigoErro;
            ErrorCode = codigoErro;
        }

        public ExceptionsHelper(string mensagem, int codigoErro, Exception innerException)
            : base(mensagem, innerException)
        {
            CodigoErro = codigoErro;
            ErrorCode = codigoErro;
        }

        public ExceptionsHelper(string message, int errorCode, string operation = null, string query = null)
            : base(message)
        {
            ErrorCode = errorCode;
            CodigoErro = errorCode;
            Operation = operation;
            Query = query;
        }

        public ExceptionsHelper(string message, int errorCode, Exception innerException, string operation = null, string query = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            CodigoErro = errorCode;
            Operation = operation;
            Query = query;
        }

        public override string ToString()
        {
            string queryInfo = string.IsNullOrWhiteSpace(Query) ? "N/D" : Query;
            string operationInfo = string.IsNullOrWhiteSpace(Operation) ? "N/D" : Operation;

            var response = $"Erro: {Message}\n" +
                   $"Código: {CodigoErro}\n" +
                   $"Operação: {operationInfo}\n" +
                   $"Query: {queryInfo}\n" +
                   $"{(InnerException != null ? $"Inner: {InnerException.Message}" : string.Empty)}";

            return response;
        }


        public static string GetErrorFromDatabase(int errorCode)
        {
            switch (errorCode)
            {
                case 1000:
                    return "Erro de validação de entrada.";

                case 2000:
                    return "Erro ao inserir no banco de dados.";

                case 3000:
                    return "Erro ao validar o tipo de conexão.";

                default:
                    return "Erro não catalogado.";
            }
        }

    }

    public class MalformedLineException : Exception
    {
        public int NumeroLinha { get; private set; }
        public string LinhaConteudo { get; private set; }
        public string Query { get; private set; }

        public MalformedLineException() : base() { }

        public MalformedLineException(string mensagem) : base(mensagem) { }

        public MalformedLineException(string mensagem, Exception innerException)
            : base(mensagem, innerException) { }

        public MalformedLineException(string mensagem, int numeroLinha, string linhaConteudo, string query = null)
            : base(mensagem)
        {
            NumeroLinha = numeroLinha;
            LinhaConteudo = linhaConteudo;
            Query = query;
        }

        public MalformedLineException(string mensagem, int numeroLinha, string linhaConteudo, Exception innerException, string query = null)
            : base(mensagem, innerException)
        {
            NumeroLinha = numeroLinha;
            LinhaConteudo = linhaConteudo;
            Query = query;
        }

        public override string ToString()
        {
            string queryInfo = string.IsNullOrWhiteSpace(Query) ? "N/D" : Query;

            return $"Erro: {Message}\n" +
                   $"Linha: {NumeroLinha}\n" +
                   $"Conteúdo: \"{LinhaConteudo}\"\n" +
                   $"Query: {queryInfo}\n" +
                   $"{(InnerException != null ? $"Inner: {InnerException.Message}" : string.Empty)}";
        }
    }
}
