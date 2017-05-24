﻿using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
#pragma warning disable 618

class RuntimeSagaInfo
{
    Type sagaDataType;
    RetrieveVersionSpecificJsonSettings versionSpecificSettings;
    public Type SagaType;
    JsonSerializer jsonSerializer;
    Func<TextReader, JsonReader> readerCreator;
    // ReSharper disable once NotAccessedField.Local
    Func<TextWriter, JsonWriter> writerCreator;
    ConcurrentDictionary<Version, JsonSerializer> deserializers;
    CommandBuilder commandBuilder;
    public readonly Version CurrentVersion;
    public readonly string CompleteCommand;
    public readonly string SelectFromCommand;
    public readonly string GetBySagaIdCommand;
    public readonly string SaveCommand;
    public readonly string UpdateCommand;
    public readonly Func<IContainSagaData, object> TransitionalAccessor;
    public readonly bool HasCorrelationProperty;
    public readonly bool HasTransitionalCorrelationProperty;
    public readonly string CorrelationProperty;
    public readonly string TransitionalCorrelationProperty;
    public readonly string GetByCorrelationPropertyCommand;
    public readonly string TableName;
    public readonly Action<DbParameter, string, object> FillParameter;

    public RuntimeSagaInfo(
        SagaCommandBuilder commandBuilder,
        Type sagaDataType,
        RetrieveVersionSpecificJsonSettings versionSpecificSettings,
        Type sagaType,
        JsonSerializer jsonSerializer,
        Func<TextReader, JsonReader> readerCreator,
        Func<TextWriter, JsonWriter> writerCreator,
        string tablePrefix,
        string schema,
        SqlVariant sqlVariant,
        Func<string, string> nameFilter)
    {
        this.sagaDataType = sagaDataType;
        if (versionSpecificSettings != null)
        {
            deserializers = new ConcurrentDictionary<Version, JsonSerializer>();
        }
        this.versionSpecificSettings = versionSpecificSettings;
        SagaType = sagaType;
        this.jsonSerializer = jsonSerializer;
        this.readerCreator = readerCreator;
        this.writerCreator = writerCreator;
        this.commandBuilder = new CommandBuilder(sqlVariant);
        CurrentVersion = sagaDataType.Assembly.GetFileVersion();
        ValidateIsSqlSaga();
        var sqlSagaAttributeData = SqlSagaTypeDataReader.GetTypeData(sagaType);
        var tableSuffix = nameFilter(sqlSagaAttributeData.TableSuffix);

        switch (sqlVariant)
        {
            case SqlVariant.MsSqlServer:
                TableName = $"[{schema}].[{tablePrefix}{tableSuffix}]";
                FillParameter = ParameterFiller.Fill;
                break;
            case SqlVariant.MySql:
                TableName = $"`{tablePrefix}{tableSuffix}`";
                FillParameter = ParameterFiller.Fill;
                break;
            case SqlVariant.PostgreSql:
                var tableName = $"{tablePrefix}{tableSuffix}";
                TableName = tableName.Length <= 63 ? tableName : $"{tableName.Substring(tableName.Length - 63)}";
                FillParameter = ParameterFiller.Fill;
                break;
            case SqlVariant.Oracle:
                if (tableSuffix.Length > 27)
                {
                    throw new Exception($"Saga '{tableSuffix}' contains more than 27 characters, which is not supported by SQL persistence using Oracle. Either disable Oracle script generation using the SqlPersistenceSettings assembly attribute, shorten the name of the saga, or specify an alternate table name by overriding the SqlSaga's TableSuffix property.");
                }
                if (Encoding.UTF8.GetBytes(tableSuffix).Length != tableSuffix.Length)
                {
                    throw new Exception($"Saga '{tableSuffix}' contains non-ASCII characters, which is not supported by SQL persistence using Oracle. Either disable Oracle script generation using the SqlPersistenceSettings assembly attribute, change the name of the saga, or specify an alternate table name by overriding the SqlSaga's TableSuffix property.");
                }
                TableName = tableSuffix.ToUpper();
                FillParameter = ParameterFiller.OracleFill;
                break;
            default:
                throw new Exception($"Unknown SqlVariant: {sqlVariant}.");
        }

        CompleteCommand = commandBuilder.BuildCompleteCommand(TableName);
        SelectFromCommand = commandBuilder.BuildSelectFromCommand(TableName);
        GetBySagaIdCommand = commandBuilder.BuildGetBySagaIdCommand(TableName);
        SaveCommand = commandBuilder.BuildSaveCommand(sqlSagaAttributeData.CorrelationProperty, sqlSagaAttributeData.TransitionalCorrelationProperty, TableName);
        UpdateCommand = commandBuilder.BuildUpdateCommand(sqlSagaAttributeData.TransitionalCorrelationProperty, TableName);

        CorrelationProperty = sqlSagaAttributeData.CorrelationProperty;
        HasCorrelationProperty = CorrelationProperty != null;
        if (HasCorrelationProperty)
        {
            GetByCorrelationPropertyCommand = commandBuilder.BuildGetByPropertyCommand(sqlSagaAttributeData.CorrelationProperty, TableName);
        }

        TransitionalCorrelationProperty = sqlSagaAttributeData.TransitionalCorrelationProperty;
        HasTransitionalCorrelationProperty = TransitionalCorrelationProperty != null;
        if (HasTransitionalCorrelationProperty)
        {
            TransitionalAccessor = sagaDataType.GetPropertyAccessor<IContainSagaData>(TransitionalCorrelationProperty);
        }
    }

    void ValidateIsSqlSaga()
    {
        if (!SagaType.IsSubclassOfRawGeneric(typeof(SqlSaga<>)))
        {
            throw new Exception($"Type '{SagaType.FullName}' does not inherit from SqlSaga<T>. Change the type to inherit from SqlSaga<T>.");
        }
    }

    public CommandWrapper CreateCommand(DbConnection connection)
    {
        return commandBuilder.CreateCommand(connection);
    }

    public JObject ToJson(IContainSagaData sagaData)
    {
        var originalMessageId = sagaData.OriginalMessageId;
        var originator = sagaData.Originator;
        var id = sagaData.Id;
        sagaData.OriginalMessageId = null;
        sagaData.Originator = null;
        sagaData.Id = Guid.Empty;

        try
        {
            try
            {
                return JObject.FromObject(sagaData, jsonSerializer);
            }
            catch (Exception exception)
            {
                throw new SerializationException(exception);
            }
        }
        finally
        {
            sagaData.OriginalMessageId = originalMessageId;
            sagaData.Originator = originator;
            sagaData.Id = id;
        }
    }


    public TSagaData FromString<TSagaData>(TextReader textReader, Version storedSagaTypeVersion)
        where TSagaData : IContainSagaData
    {
        var serializer = GetDeserialize(storedSagaTypeVersion);
        using (var jsonReader = readerCreator(textReader))
        {
            try
            {
                return serializer.Deserialize<TSagaData>(jsonReader);
            }
            catch (Exception exception)
            {
                throw new SerializationException(exception);
            }
        }
    }


    JsonSerializer GetDeserialize(Version storedSagaTypeVersion)
    {
        if (versionSpecificSettings == null)
        {
            return jsonSerializer;
        }
        return deserializers.GetOrAdd(storedSagaTypeVersion, _ =>
        {
            var settings = versionSpecificSettings(sagaDataType, storedSagaTypeVersion);
            if (settings != null)
            {
                return JsonSerializer.Create(settings);
            }
            return jsonSerializer;
        });
    }

}