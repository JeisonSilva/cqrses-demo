using System;
using Payroll.Domain.Events;
using Payroll.Domain.Model;
using Raven.Client.Document;
using Raven.Imports.Newtonsoft.Json;

namespace Payroll.Infrastructure.RavenDbEmployeeRepository
{
    public class EmployeeEventStore :
        IMessageHandler<EmployeeRegisteredEvent>,
        IMessageHandler<EmployeeHomeAddressUpdatedEvent>,
        IMessageHandler<EmployeeSalaryRaisedEvent>,
        IDisposable
    {
        private readonly DocumentStore _store;

        public EmployeeEventStore()
        {
            _store = new DocumentStore
            {
                Url = "http://localhost:8080/", // server URL
                DefaultDatabase = "RegularDb"
            };

            _store.Conventions.CustomizeJsonSerializer = serializer =>
            {
                serializer.Converters.Add(new EmployeeIdJsonConverter());
            };

            _store.Initialize();
            _store.Conventions.FindTypeTagName = t => "EmployeeEvents";
        }

        public void HandleInternal(Message message)
        {
            using (var session = _store.OpenSession())
            {
                session.Store(message);
                session.SaveChanges();
            }
        }

        public void Handle(EmployeeRegisteredEvent message)
        { HandleInternal(message); }

        public void Handle(EmployeeHomeAddressUpdatedEvent message)
        { HandleInternal(message); }

        public void Handle(EmployeeSalaryRaisedEvent message)
        { HandleInternal(message); }

        public void Dispose()
        {
            _store.Dispose();
        }
    }

    public class EmployeeIdJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, $"employees/{value}");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (EmployeeId);
        }
    }
}
