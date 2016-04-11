using Payroll.Domain.Model;
using Payroll.Infrastructure;

namespace Payroll.Domain.Events
{
    public sealed class EmployeeSalaryRaisedEvent
        : EmployeeEvent
    {
        public decimal Amount { get; }

        public EmployeeSalaryRaisedEvent(
            EmployeeId id, 
            decimal amount
            ) : base(id)
        {
            Amount = amount;
        }
    }
}
