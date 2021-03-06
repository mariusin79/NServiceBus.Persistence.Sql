﻿using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

class CommandWrapper : IDisposable
{
    protected DbCommand command;
    int disposeSignaled;

    public CommandWrapper(DbCommand command)
    {
        this.command = command;
    }

    public DbCommand InnerCommand => command;

    public string CommandText
    {
        get { return command.CommandText; }
        set { command.CommandText = value; }
    }

    public DbTransaction Transaction
    {
        get { return command.Transaction; }
        set { command.Transaction = value; }
    }

    public virtual void AddParameter(string name, object value)
    {
        var parameter = command.CreateParameter();
        ParameterFiller.Fill(parameter, name, value);
        command.Parameters.Add(parameter);
    }

    public void AddParameter(string name, Version value)
    {
        AddParameter(name, value.ToString());
    }

    public Task ExecuteNonQueryEx()
    {
        return command.ExecuteNonQueryEx();
    }

    public Task<int> ExecuteNonQueryAsync()
    {
        return command.ExecuteNonQueryAsync();
    }

    public Task<DbDataReader> ExecuteReaderAsync()
    {
        return command.ExecuteReaderAsync();
    }

    public Task<DbDataReader> ExecuteReaderAsync(CommandBehavior behavior)
    {
        return command.ExecuteReaderAsync(behavior);
    }

    public Task<int> ExecuteNonQueryEx(CancellationToken cancellationToken)
    {
        return command.ExecuteNonQueryEx(cancellationToken);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposeSignaled, 1) != 0)
        {
            return;
        }
        var temp = Interlocked.Exchange(ref command, null);
        temp?.Dispose();
    }
}