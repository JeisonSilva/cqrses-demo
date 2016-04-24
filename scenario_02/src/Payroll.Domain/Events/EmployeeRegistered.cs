using Infrastructure.EventSourcing;
using Payroll.Domain.Model;

namespace Payroll.Domain.Events
{
    public class EmployeeRegistered : VersionedEvent<string>
    {
        public FullName Name { get; }
        public decimal InitialSalary { get; }

        public EmployeeRegistered(FullName name, decimal initialSalary)
        {
            Name = name;
            InitialSalary = initialSalary;
        }
    }
}