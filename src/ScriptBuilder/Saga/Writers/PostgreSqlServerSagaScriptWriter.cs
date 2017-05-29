using System.IO;
using System.Text;


namespace NServiceBus.Persistence.Sql.ScriptBuilder
{
    public class PostgreSqlServerSagaScriptWriter : ISagaScriptWriter
    {
        TextWriter writer;
        SagaDefinition saga;
        string tableName;

        public PostgreSqlServerSagaScriptWriter(TextWriter textWriter, SagaDefinition saga)
        {
            writer = textWriter;
            this.saga = saga;
            tableName = saga.TableSuffix.ToLower();
        }

        public void Initialize()
        {
            writer.WriteLine($@"
create or replace function sqlpersistence_raiseerror(message varchar(256)) returns void as $$
begin
    raise exception '%', message;
end;
$$ language plpgsql;

DO $$
DECLARE tableNameNonQuoted text; sqlStatement text; dataType text; row RECORD;
BEGIN
    tableNameNonQuoted = right(concat(@tablePrefix, '{tableName}'), 63);
");
        }

        public void WriteTableNameVariable()
        {
        }

        public void CreateComplete()
        {
            writer.WriteLine(@"
END;
$$;");
        }

        public void AddProperty(CorrelationProperty correlationProperty)
        {
            var columnType = PostgreSqlServerCorrelationPropertyTypeConverter.GetColumnType(correlationProperty.Type);
            var name = correlationProperty.Name.ToLower();

            writer.Write($@"
if not exists
(
  select column_name from information_schema.columns
  where column_name = 'correlation_{name}' and
      table_name = tableNameNonQuoted
)
then
    sqlStatement = concat('alter table ', tableNameNonQuoted, ' add column correlation_{name} {columnType}');
    execute sqlStatement;
end if;
");
        }

        public void VerifyColumnType(CorrelationProperty correlationProperty)
        {
            var columnType = PostgreSqlServerCorrelationPropertyTypeConverter.GetColumnType(correlationProperty.Type);
            var name = correlationProperty.Name.ToLower();

            writer.Write($@"
if not exists (
    select * from information_schema.columns
    where column_name = 'correlation_{name}' and
        table_name = tableNameNonQuoted
)
then
    perform sqlpersistence_raiseerror(concat('Incorrect data type for correlation_{name}. Expected {columnType}. ', dataType));
end if;
");
        }

        public void WriteCreateIndex(CorrelationProperty correlationProperty)
        {
            var name = correlationProperty.Name.ToLower();

            writer.Write($@"
sqlStatement = concat('create unique index if not exists index_correlation_{name}_{tableName} on ', tableNameNonQuoted, '(correlation_{name})');
execute sqlStatement;
");
        }

        public void WritePurgeObsoleteIndex()
        {
            var builder = new StringBuilder();

            var correlation = saga.CorrelationProperty;
            if (correlation != null)
            {
                builder.Append($" and indexname <> 'index_correlation_{correlation.Name.ToLower()}_{tableName}'");
            }
            var transitional = saga.TransitionalCorrelationProperty;
            if (transitional != null)
            {
                builder.Append($" and indexname <> 'index_correlation_{transitional.Name.ToLower()}_{tableName}'");
            }

            writer.Write($@"
for row in 
(
    select * from pg_indexes 
    where 
        tablename = tableNameNonQuoted and
        indexname like 'index_correlation_%'{builder}
)
loop
    sqlStatement = concat('drop index ', row.indexname);
    execute sqlStatement;
end loop;
");
        }

        public void WritePurgeObsoleteProperties()
        {
            var builder = new StringBuilder();

            var correlation = saga.CorrelationProperty;
            if (correlation != null)
            {
                builder.Append($" and column_name <> 'correlation_{correlation.Name.ToLower()}'");
            }
            var transitional = saga.TransitionalCorrelationProperty;
            if (transitional != null)
            {
                builder.Append($" and column_name <> 'correlation_{transitional.Name.ToLower()}'");
            }
            writer.Write($@"
for row in 
(
    select * from information_schema.columns
    where 
        table_name = tableNameNonQuoted and
        column_name like 'correlation_%'{builder}
)
loop
    sqlStatement = concat('alter table ', tableNameNonQuoted, ' drop column ', row.column_name);
    execute sqlStatement;
end loop;
");
        }

        public void WriteCreateTable()
        {
            writer.Write(@"
sqlStatement = concat('
    create table if not exists ', tableNameNonQuoted, '(
        Id uuid not null,
        Metadata jsonb not null,
        Data jsonb not null,
        PersistenceVersion varchar(23) not null,
        SagaTypeVersion varchar(23) not null,
        Concurrency int not null,
        primary key (Id)
    )
');
execute sqlStatement;
");
        }

        public void WriteDropTable()
        {
            writer.Write($@"
DO $$
DECLARE tablename text; sqlStatement text;
BEGIN
    tablename = right(concat(@tablePrefix, '{tableName}'), 63);
    sqlStatement = concat('drop table if exists ', tablename);
    execute sqlStatement;
END;
$$;");
        }
    }
}