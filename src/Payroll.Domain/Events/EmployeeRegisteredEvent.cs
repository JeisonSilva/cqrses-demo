using Payroll.Domain.Model;
using Payroll.Infrastructure;

namespace Payroll.Domain.Events
{
    public sealed class EmployeeRegisteredEvent : EmployeeEvent
    {
        public FullName Name { get; private set; }
        public decimal InitialSalary { get; private set; }

        public EmployeeRegisteredEvent(
            EmployeeId id,
            FullName name, 
            decimal initialSalary
            ) : base(id)
        {
            Name = name;
            InitialSalary = initialSalary;
        }
    }
}
