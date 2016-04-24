using Infrastructure.EventSourcing;

namespace Payroll.Domain.Events
{
    public class EmployeeSalaryRaised : VersionedEvent<string>
    {
        public decimal Amout { get; }

        public EmployeeSalaryRaised(decimal amout)
        {
            Amout = amout;
        }
    }
}
