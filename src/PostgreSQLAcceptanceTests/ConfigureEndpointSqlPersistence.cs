using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Support;
using NServiceBus.Persistence.Sql;
using NServiceBus.Persistence.Sql.ScriptBuilder;
using TestHelper;

public class ConfigureEndpointSqlPersistence : IConfigureEndpointTestExecution
{
    ConfigureEndpointHelper endpointHelper;

    public Task Configure(string endpointName, EndpointConfiguration configuration, RunSettings settings, PublisherMetadata publisherMetadata)
    {
        if (configuration.IsSendOnly())
        {
            return Task.FromResult(0);
        }
        var tablePrefix = TableNameCleaner.Clean(endpointName);
        endpointHelper = new ConfigureEndpointHelper(configuration, tablePrefix, PostgreSqlConnectionBuilder.Build, BuildSqlVariant.PostgreSql, FilterTableExists);
        var persistence = configuration.UsePersistence<SqlPersistence>();
        persistence.ConnectionBuilder(PostgreSqlConnectionBuilder.Build);
        persistence.SqlVariant(SqlVariant.PostgreSql);
        persistence.TablePrefix($"{tablePrefix}_");
        var subscriptions = persistence.SubscriptionSettings();
        subscriptions.DisableCache();
        persistence.DisableInstaller();
        return Task.FromResult(0);
    }

    bool FilterTableExists(Exception exception)
    {
        return exception.Message.Contains("Cannot drop the table");
    }

    public Task Cleanup()
    {
        return endpointHelper?.Cleanup();
    }
}