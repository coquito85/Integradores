
using System.Data;
using System.Data.Common;

namespace ReferenciaFactory
{ #pragma warning disable
    public class DatabaseConnection : IDisposable
    {
        private DbConnection ConnectionFactory;
        private DbProviderFactory ProviderFactory;
        private DbCommand CommandFactory;
        private bool IsDisposed;

        private readonly string ConnectionStr = "";

        /// <summary>
        /// Factory padrão usada pelo construtor com apenas a string de conexão.
        /// Defina uma vez na inicialização da aplicação, por exemplo:
        /// DatabaseConnection.DefaultFactory = Microsoft.Data.SqlClient.SqlClientFactory.Instance;
        /// </summary>
        public static DbProviderFactory DefaultFactory { get; set; }

        // Construtor principal com string de conexão
        public DatabaseConnection(string connectionString)
            : this(connectionString, DefaultFactory)
        {
        }

        // Construtor com string de conexão e provider explícito
        public DatabaseConnection(string connectionString, DbProviderFactory factory)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("A string de conexão não pode ser vazia.", nameof(connectionString));

            if (factory == null)
                throw new InvalidOperationException(
                    "Nenhum DbProviderFactory informado. Passe o factory no construtor ou defina DatabaseConnection.DefaultFactory.");

            ConnectionStr = connectionString;
            ProviderFactory = factory;
            ConnectionFactory = ProviderFactory.CreateConnection();
            ConnectionFactory.ConnectionString = ConnectionStr;
        }

        public DbConnection Connection
        {
            get
            {
                ThrowIfDisposed();
                return ConnectionFactory;
            }
        }

        public void Open()
        {
            ThrowIfDisposed();
            if (ConnectionFactory.State != ConnectionState.Open)
                ConnectionFactory.Open();
        }

        public void Close()
        {
            if (ConnectionFactory != null && ConnectionFactory.State != ConnectionState.Closed)
                ConnectionFactory.Close();
        }

        public DbCommand CreateCommand(string sql, CommandType commandType = CommandType.Text)
        {
            ThrowIfDisposed();
            Open();

            CommandFactory?.Dispose();
            CommandFactory = ConnectionFactory.CreateCommand();
            CommandFactory.CommandText = sql;
            CommandFactory.CommandType = commandType;
            return CommandFactory;
        }

        public DbParameter AddParameter(DbCommand command, string name, object value)
        {
            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
            return parameter;
        }

        public int ExecuteNonQuery(string sql)
        {
            return CreateCommand(sql).ExecuteNonQuery();
        }

        public object ExecuteScalar(string sql)
        {
            return CreateCommand(sql).ExecuteScalar();
        }

        public DbDataReader ExecuteReader(string sql)
        {
            return CreateCommand(sql).ExecuteReader();
        }

        public DbTransaction BeginTransaction()
        {
            ThrowIfDisposed();
            Open();
            return ConnectionFactory.BeginTransaction();
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ReferenciaFactory.DatabaseConnection));
        }

        // Implementação de IDisposable (corrige o erro CS0535)
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                CommandFactory?.Dispose();
                CommandFactory = null;

                if (ConnectionFactory != null)
                {
                    if (ConnectionFactory.State != ConnectionState.Closed)
                        ConnectionFactory.Close();

                    ConnectionFactory.Dispose();
                    ConnectionFactory = null;
                }

                ProviderFactory = null;
            }

            IsDisposed = true;
        }
    }

    public static class DataReaderExtensions
    {
        public static bool HasColumn(this IDataRecord reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }



}


 