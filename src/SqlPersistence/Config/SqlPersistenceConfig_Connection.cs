using System;
using System.Data.Common;
using NServiceBus.Configuration.AdvanceExtensibility;
using NServiceBus.Persistence.Sql;
using NServiceBus.Settings;

namespace NServiceBus
{
    public static partial class SqlPersistenceConfig
    {
        /// <summary>
        /// Configures how <see cref="DbConnection"/>s are constructed.
        /// </summary>
        public static void ConnectionBuilder(this PersistenceExtensions<SqlPersistence> configuration, Func<DbConnection> connectionBuilder)
        {
            Guard.AgainstNull(nameof(configuration), configuration);
            Guard.AgainstNull(nameof(connectionBuilder), connectionBuilder);
            configuration.GetSettings()
                .Set("SqlPersistence.ConnectionBuilder", connectionBuilder);
        }

        internal static Func<DbConnection> GetConnectionBuilder(this ReadOnlySettings settings)
        {
            Func<DbConnection> value;
            if (settings.TryGet("SqlPersistence.ConnectionBuilder", out value))
            {
                return value;
            }
            throw new Exception("ConnectionBuilder must be defined.");
        }

    }
}