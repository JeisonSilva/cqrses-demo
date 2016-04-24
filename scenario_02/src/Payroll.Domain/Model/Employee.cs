using Infrastructure.EventSourcing;
using Payroll.Domain.Events;

namespace Payroll.Domain.Model
{
    public sealed class Employee : EventSourced<string>
    {
        public FullName Name { get; private set; }
        public decimal Salary { get; private set; }
        public Address HomeAddress { get; set; }

        private Employee(string id) : base(id)
        {
            Handles<EmployeeRegistered>(OnEmployeeRegistered);
            Handles<EmployeeSalaryRaised>(OnEmployeeSalaryRaised);
            Handles<EmployeeHomeAddressChanged>(OnEmployeeHomeAddressChanged);    
        }

        public Employee(string id, FullName name, decimal initialSalary) 
            : this(id)
        {
            Throw.IfArgumentIsNull(name, nameof(name));
            Throw.IfArgumentIsNegative(initialSalary, nameof(initialSalary));

            Update(new EmployeeRegistered(name, initialSalary));
        }

        public void RaiseSalary(decimal amount)
        {
            Throw.IfArgumentIsNegative(amount, nameof(amount));
            Update(new EmployeeSalaryRaised(amount));
        }

        public void ChangeHomeAddress(Address address)
        {
            Throw.IfArgumentIsNull(address, nameof(address));
            Update(new EmployeeHomeAddressChanged(address));
        }

        private void OnEmployeeRegistered(EmployeeRegistered @event)
        {
            Name = @event.Name;
            Salary = @event.InitialSalary;
        }

        private void OnEmployeeSalaryRaised(EmployeeSalaryRaised @event)
        {
            Salary += @event.Amout;
        }

        private void OnEmployeeHomeAddressChanged(EmployeeHomeAddressChanged @event)
        {
            HomeAddress = @event.NewAddress;
        }
    }
}
