using System;
using Payroll.Domain.Model;
using Raven.Imports.Newtonsoft.Json;

namespace Payroll.Infrastructure.RavenDbEmployeeRepository
{
    public class EmployeeIdJsonConverter
        : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, $"employees/{value}");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var original = (string)reader.Value;
            return (EmployeeId)original.Substring(
                original.IndexOf("/", StringComparison.Ordinal) + 1);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(EmployeeId);
        }
    }
}