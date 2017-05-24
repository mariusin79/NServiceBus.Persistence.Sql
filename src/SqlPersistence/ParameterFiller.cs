using System;
using System.Data;
using System.Data.Common;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;

static class ParameterFiller
{
    public static void Fill(DbParameter parameter, string paramName, object value)
    {
        parameter.ParameterName = paramName;
        parameter.Value = StringifyIfJson(value);
    }

    public static void PostgreSqlFill(Npgsql.NpgsqlParameter parameter, string paramName, object value)
    {
        parameter.ParameterName = paramName;
        parameter.Value = StringifyIfJson(value);

        if (value is JObject || value is JArray)
        {
            parameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
        }
    }

    public static void OracleFill(DbParameter parameter, string paramName, object value)
    {
        parameter.ParameterName = paramName;
        if (value is Guid)
        {
            parameter.Value = value.ToString();
        }
        else if (value is Version)
        {
            parameter.DbType = DbType.String;
            parameter.Value = value.ToString();
        }
        else
        {
            parameter.Value = StringifyIfJson(value);
        }
    }

    static object StringifyIfJson(object value)
    {
        var jObj = value as JObject;

        if (jObj != null)
        {
            return jObj.ToString(Newtonsoft.Json.Formatting.None);
        }
        else
        {
            var jArr = value as JArray;

            if (jArr != null)
            {
                return jArr.ToString(Newtonsoft.Json.Formatting.None);
            }
            else
            {
                return value;
            }
        }
    }
}