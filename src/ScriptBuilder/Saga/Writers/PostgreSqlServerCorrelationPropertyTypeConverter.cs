using System;

namespace NServiceBus.Persistence.Sql.ScriptBuilder
{
    /// <summary>
    /// Not for public use.
    /// </summary>
    public static class PostgreSqlServerCorrelationPropertyTypeConverter
    {
        public static string GetColumnType(CorrelationPropertyType propertyType)
        {
            switch (propertyType)
            {
                case CorrelationPropertyType.DateTime:
                    return "timestamp";
                case CorrelationPropertyType.DateTimeOffset:
                    return "timestamp";
                case CorrelationPropertyType.String:
                    return "varchar(200)";
                case CorrelationPropertyType.Int:
                    return "bigint";
                case CorrelationPropertyType.Guid:
                    return "uuid";
            }
            throw new Exception($"Could not convert {propertyType}.");
        }
    }
}