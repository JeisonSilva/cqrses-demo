using System;
using Payroll.Domain.Events;

namespace Payroll.Infrastructure.RavenDbEmployeeRepository
{
    public partial class RavenDbEmployeeEventStore :
        IMessageHandler<EmployeeRegisteredEvent>,
        IMessageHandler<EmployeeHomeAddressUpdatedEvent>,
        IMessageHandler<EmployeeSalaryRaisedEvent>
    {
        private readonly ILogger _logger;

        public RavenDbEmployeeEventStore(ILogger logger)
        {
            _logger = logger;
            InitializeIndexes();
        }

        private void InitializeIndexes()
        {
            _logger.Trace("RavenDBEventStore", "initializing indexes");
            new EventsPerEmployeeIndex().Execute(DocumentStoreHolder.Instance);
            new SalaryPerEmployeeIndex().Execute(DocumentStoreHolder.Instance);
        }


        public void HandleInternal(Message message)
        {
            _logger.Trace("RavenDBEventStore", $"handling ${message.MessageType}");
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
