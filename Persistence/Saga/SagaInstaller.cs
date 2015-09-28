﻿using System.IO;
using System.Text;
using NServiceBus;
using NServiceBus.Installation;
using NServiceBus.SqlPersistence;

class SagaInstaller : INeedToInstallSomething
{

    public void Install(string identity, Configure config)
    {
        var settings = config.Settings;
        if (!settings.IsSagaEnabled())
        {
            return;
        }
        var typesToScan = config.TypesToScan;
        var sagaDefinitions = SagaMetaDataReader.GetSagaDefinitions(typesToScan);
        var connectionString = settings.GetConnectionString();
        var endpointName = config.Settings.EndpointName();
        var builder = new StringBuilder();
        using (var writer = new StringWriter(builder))
        {
            SagaScriptBuilder.BuildCreateScript("dbo", endpointName, sagaDefinitions, s => writer);
        }
        SqlHelpers.Execute(connectionString, builder.ToString());
    }
}