using Infrastructure.EventSourcing;

namespace Payroll.Domain
{
    public sealed class Employee : EventSourced<string>
    {
        public FullName Name { get; private set; }
        public decimal Salary { get; private set; }
        private Employee(string id) : base(id)
        {
            Handles<EmployeeRegistered>(OnEmployeeRegistered);    
        }

        public Employee(string id, FullName name, decimal initialSalary) 
            : this(id)
        {
            Update(new EmployeeRegistered(name, initialSalary));
        }

        protected void OnEmployeeRegistered(EmployeeRegistered @event)
        {
            Name = @event.Name;
            Salary = @event.InitialSalary;
        }
    }
}
