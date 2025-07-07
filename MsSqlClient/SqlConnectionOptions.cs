using Microsoft.Data.SqlClient;
using System;

namespace MsSqlClient
{
    /// <summary>
    /// Holds configuration options for connecting to a SQL Server database and builds a connection string.
    /// </summary>
    public class SqlConnectionOptions
    {
        /// <summary>Hostname or IP of the SQL Server instance (e.g. "localhost" or "192.168.1.100").</summary>
        public string? Server { get; set; }

        /// <summary>Named instance on the server (leave null if using default instance).</summary>
        public string? InstanceName { get; set; }

        /// <summary>Port number (e.g. 1433). Overrides InstanceName if set.</summary>
        public int? Port { get; set; }

        /// <summary>Name of the database to connect to.</summary>
        public string? Database { get; set; }

        /// <summary>Use Windows Authentication if true; otherwise SQL Authentication.</summary>
        public bool IntegratedSecurity { get; set; } = false;

        /// <summary>SQL login user ID (ignored if IntegratedSecurity is true).</summary>
        public string? UserId { get; set; }

        /// <summary>SQL login password (ignored if IntegratedSecurity is true).</summary>
        public string? Password { get; set; }

        /// <summary>Encrypt the connection to the server.</summary>
        public bool Encrypt { get; set; } = true;

        /// <summary>Trust the server certificate without validation.</summary>
        public bool TrustServerCertificate { get; set; } = false;

        /// <summary>Time (in seconds) to wait for a connection to open.</summary>
        public int ConnectTimeout { get; set; } = 30;

        /// <summary>Allow multiple active result sets.</summary>
        public bool MultipleActiveResultSets { get; set; } = false;

        /// <summary>
        /// Builds and returns a SQL Server connection string based on the current options.
        /// </summary>
        public string GetConnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = BuildDataSource(),
                InitialCatalog = Database,
                IntegratedSecurity = IntegratedSecurity,
                Encrypt = Encrypt,
                TrustServerCertificate = TrustServerCertificate,
                ConnectTimeout = ConnectTimeout,
                MultipleActiveResultSets = MultipleActiveResultSets
            };

            if (!IntegratedSecurity)
            {
                builder.UserID = UserId;
                builder.Password = Password;
            }

            return builder.ConnectionString;
        }

        /// <summary>
        /// Constructs the DataSource value ("server[\instance][,port]").
        /// </summary>
        private string BuildDataSource()
        {
            if (string.IsNullOrEmpty(Server))
                throw new InvalidOperationException("Server property must be set.");

            if (Port.HasValue)
                return $"{Server},{Port.Value}";
            if (!string.IsNullOrEmpty(InstanceName))
                return $"{Server}\\{InstanceName}";
            return Server;
        }
    }
}
