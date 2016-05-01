using System;
using Infrastructure.EventSourcing;

namespace Payroll.Domain.Events
{
    public class EmployeeSalaryRaisedEvent : VersionedEvent<Guid>
    {
        public decimal Amount { get; }

        public EmployeeSalaryRaisedEvent(decimal amout)
        {
            Amount = amout;
        }
    }
}
