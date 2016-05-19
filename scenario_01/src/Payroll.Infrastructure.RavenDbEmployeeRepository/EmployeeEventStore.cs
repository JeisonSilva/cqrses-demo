using System;
using System.Collections.Generic;
using System.Linq;
using Payroll.Domain.Events;
using Payroll.Domain.Model;
using Raven.Abstractions.Indexing;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Imports.Newtonsoft.Json;

namespace Payroll.Infrastructure.RavenDbEmployeeRepository
{
    public partial class EmployeeEventStore :
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
                DefaultDatabase = "RegularDb",
                Conventions =
                {
                    CustomizeJsonSerializer =
                        serializer => { serializer.Converters.Add(new EmployeeIdJsonConverter()); }
                }
            };

            _store.Conventions.FindTypeTagName = t => "EmployeeEvents";

            _store.Initialize();

            InitializeIndexes();
        }

        private void InitializeIndexes()
        {
            new EventsPerEmployeeIndex().Execute(_store);
            new SalaryPerEmployeeIndex().Execute(_store);
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
        {
            HandleInternal(message);
        }

        public void Handle(EmployeeHomeAddressUpdatedEvent message)
        {
            HandleInternal(message);
        }

        public void Handle(EmployeeSalaryRaisedEvent message)
        {
            HandleInternal(message);
        }

        public void Dispose()
        {
            _store.Dispose();
        }

        private class EmployeeIdJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, $"employees/{value}");
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                JsonSerializer serializer)
            {
                var original = (string) reader.Value;
                return (EmployeeId) original.Substring(
                    original.IndexOf("/", StringComparison.Ordinal) + 1);
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof (EmployeeId);
            }
        }
    }
}
