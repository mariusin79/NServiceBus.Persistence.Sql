using System;
using System.Data.Common;
using Npgsql;
using NServiceBus.Persistence.Sql;

class CommandBuilder
{
    readonly SqlVariant sqlVariant;

    public CommandBuilder(SqlVariant sqlVariant)
    {
        this.sqlVariant = sqlVariant;
    }

    public CommandWrapper CreateCommand(DbConnection connection)
    {
        var command = connection.CreateCommand();

        switch (sqlVariant)
        {
            case SqlVariant.MsSqlServer:
            case SqlVariant.MySql:
                return new CommandWrapper(command);
            case SqlVariant.PostgreSql:
                return new PostgreSqlCommandWrapper(command as NpgsqlCommand);
            case SqlVariant.Oracle:
                return new OracleCommandWrapper(command);
            default:
                throw new Exception($"Unknown SqlVariant: {sqlVariant}.");
        }
    }
}
