using System;
using Infrastructure.EventSourcing;

namespace Payroll.Domain.Events
{
    public class EmployeeSalaryRaisedEvent : VersionedEvent<Guid>
    {
        public decimal Amout { get; }

        public EmployeeSalaryRaisedEvent(decimal amout)
        {
            Amout = amout;
        }
    }
}
