namespace TestHelper
{
    public class PostgreSqlConnectionBuilder
    {
        public const string ConnectionString = "host=172.26.184.120;port=5432;database=foss;username=postgres";

        public static Npgsql.NpgsqlConnection Build()
        {
            return new Npgsql.NpgsqlConnection(ConnectionString);
        }
    }
}