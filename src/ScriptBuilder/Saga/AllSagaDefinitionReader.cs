﻿using System;
using System.Collections.Generic;
using Mono.Cecil;
using NServiceBus.Persistence.Sql;
using NServiceBus.Persistence.Sql.ScriptBuilder;

class AllSagaDefinitionReader
{
    ModuleDefinition module;

    public AllSagaDefinitionReader(ModuleDefinition module)
    {
        this.module = module;
    }

    public IEnumerable<SagaDefinition> GetSagas(Action<ErrorsException, TypeDefinition> logError)
    {
        var sagas = new List<SagaDefinition>();
        foreach (var type in module.AllClasses())
        {
            try
            {
                SagaDefinition definition;
                if (SagaDefinitionReader.TryGetSqlSagaDefinition(type, out definition))
                {
                    sagas.Add(definition);
                }
            }
            catch (ErrorsException exception)
            {
                logError(exception, type);
            }
        }
        return sagas;
    }



}