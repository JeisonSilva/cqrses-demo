using System;
using Payroll.Domain.Events;
using Raven.Client.Document;

namespace Payroll.Infrastructure.RavenDbEmployeeRepository
{
    public partial class RavenDbEmployeeEventStore :
        IMessageHandler<EmployeeRegisteredEvent>,
        IMessageHandler<EmployeeHomeAddressUpdatedEvent>,
        IMessageHandler<EmployeeSalaryRaisedEvent>,
        IDisposable
    {
        private readonly DocumentStore _store;

        public RavenDbEmployeeEventStore()
        {
            _store = new DocumentStore
            {
                Url = "http://localhost:8080/",
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
    }
}
