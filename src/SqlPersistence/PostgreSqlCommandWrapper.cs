using Npgsql;

namespace NServiceBus.Persistence.Sql
{
    class PostgreSqlCommandWrapper : CommandWrapper
    {
        NpgsqlCommand typedVersionOfCommand;

        public PostgreSqlCommandWrapper(NpgsqlCommand command) : base(command)
        {
            this.typedVersionOfCommand = command;
        }

        public override void AddParameter(string name, object value)
        {
            var parameter = typedVersionOfCommand.CreateParameter();
            ParameterFiller.PostgreSqlFill(parameter, name, value);
            command.Parameters.Add(parameter);
        }
    }
}