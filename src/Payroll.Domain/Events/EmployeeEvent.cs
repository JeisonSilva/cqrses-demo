using Payroll.Domain.Model;
using Payroll.Infrastructure;

namespace Payroll.Domain.Events
{
    public abstract class EmployeeEvent : Event
    {
        public EmployeeId Id { get; private set; }

        protected EmployeeEvent(EmployeeId id)
        {
            Id = id;
        }
    }
}
