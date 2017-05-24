using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NServiceBus.Persistence.Sql;

static class Serializer
{
    public static JsonSerializer JsonSerializer;

    static Serializer()
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };
        JsonSerializer = JsonSerializer.Create(settings);
    }

    public static T Deserialize<T>(TextReader reader)
    {
        using (var jsonReader = new JsonTextReader(reader))
        {
            try
            {
                return JsonSerializer.Deserialize<T>(jsonReader);
            }
            catch (Exception exception)
            {
                throw new SerializationException(exception);
            }
        }
    }

    public static object Serialize(object target)
    {
        if (target is IEnumerable)
        {
            return JArray.FromObject(target, JsonSerializer);
        }
        else
        {
            return JObject.FromObject(target, JsonSerializer);
        }

        //var stringBuilder = new StringBuilder();
        //var stringWriter = new StringWriter(stringBuilder);
        //using (var jsonWriter = new JsonTextWriter(stringWriter))
        //{
        //    try
        //    {
        //        JsonSerializer.Serialize(jsonWriter, target);
        //    }
        //    catch (Exception exception)
        //    {
        //        throw new SerializationException(exception);
        //    }
        //}
        //return stringBuilder.ToString();
    }
}