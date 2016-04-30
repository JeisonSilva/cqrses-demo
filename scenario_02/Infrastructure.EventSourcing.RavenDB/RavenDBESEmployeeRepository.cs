using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Messaging;
using Payroll.Domain.Model;
using Payroll.Domain.Repositories;
using Raven.Abstractions.Data;
using Raven.Client.Converters;
using Raven.Client.Document;
using Raven.Imports.Newtonsoft.Json;
using Raven.Json.Linq;

namespace Infrastructure.EventSourcing.RavenDB
{
    public class RavenDBESEmployeeRepository :
        IEmployeeRepository
    {
        public IBus Bus { get; }
        private DocumentStore _store;
        private JsonSerializer _serializer;

        public RavenDBESEmployeeRepository(IBus bus)
        {
            Bus = bus;
            _store = new DocumentStore
            {
                Url = "http://localhost:8080/", // server URL
                DefaultDatabase = "EmployeeES"
            };
            
            _store.Initialize();

            _serializer = _store.Conventions.CreateSerializer();
            _serializer.TypeNameHandling = TypeNameHandling.All;

            _store.Conventions.IdentityTypeConvertors = new List<ITypeConverter>
            {
                new GuidConverter()
            };

            _store.Conventions.FindTypeTagName = t => "Employees";

        }

        public bool Exists(Guid id)
        {
            string localId = $"employees/{id}";
            return _store.DatabaseCommands.Head(localId) != null;
        }

        public void Save(Employee employee)
        {
            if (!Exists(employee.Id))
            {
                SaveNewEmployee(employee);
            }
            else
            {
                SaveEmployeeEvents(employee);
            }
        }

        private void SaveEmployeeEvents(Employee employee)
        {
            var requests = new List<PatchRequest>();
            foreach (var evt in employee.PendingEvents)
            {
                requests.Add(new PatchRequest
                {
                    Type = PatchCommandType.Add,
                    Name = "Events",
                    Value = RavenJObject.FromObject(evt, _serializer)
                });
                Bus.RaiseEvent(evt);
            }

            _store.DatabaseCommands.Patch(
                $"employees/{employee.Id}",
                requests.ToArray()
                );
        }

        private void SaveNewEmployee(Employee employee)
        {
            using (var session = _store.OpenSession())
            {
                var document = new EmployeeEvents(
                    employee.Id,
                    employee.PendingEvents
                    );
                session.Store(document);
                session.SaveChanges();
            }
        }

        public Employee Load(Guid id)
        {
            EmployeeEvents data;
            using (var session = _store.OpenSession())
            {
                data = session.Load<EmployeeEvents>(id);
            }
            return new Employee(id, data.Events);
        }

        class EmployeeEvents
        {
            public EmployeeEvents(Guid id, IEnumerable<IVersionedEvent<Guid>> events)
            {
                Id = id;
                Events = events.ToArray();
            }

            public Guid Id { get; private set; }
            public IVersionedEvent<Guid>[] Events { get; private set; }
        }
    }
}
