using System;
using Payroll.Domain.Events;

namespace Payroll.Infrastructure.RavenDbEmployeeRepository
{
    public partial class RavenDbEmployeeEventStore :
        IMessageHandler<EmployeeRegisteredEvent>,
        IMessageHandler<EmployeeHomeAddressUpdatedEvent>,
        IMessageHandler<EmployeeSalaryRaisedEvent>
    {
        public RavenDbEmployeeEventStore()
        {
            InitializeIndexes();
        }

        private void InitializeIndexes()
        {
            new EventsPerEmployeeIndex().Execute(DocumentStoreHolder.Instance);
            new SalaryPerEmployeeIndex().Execute(DocumentStoreHolder.Instance);
        }


        public void HandleInternal(Message message)
        {
            using (var session = DocumentStoreHolder.Instance.OpenSession())
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
    }
}
