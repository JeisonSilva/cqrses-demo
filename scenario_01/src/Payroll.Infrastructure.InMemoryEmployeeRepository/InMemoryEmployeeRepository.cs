using System.Collections.Generic;
using Payroll.Domain;
using Payroll.Domain.Model;
using Payroll.Domain.Repositories;

namespace Payroll.Infrastructure.InMemoryEmployeeRepository             
{
    public class InMemoryEmployeeRepository : IEmployeeRepository
    {
        private readonly ILogger _logger;

        readonly IDictionary<EmployeeId, Employee> _data = 
            new Dictionary<EmployeeId, Employee>();

        public InMemoryEmployeeRepository(ILogger logger)
        {
            _logger = logger;
        }

        public bool IsRegistered(EmployeeId id)
        {
            return (_data.ContainsKey(id));
        }

        public Employee Load(EmployeeId id)
        {
            return _data[id];
        }

        public void CreateEmployee(EmployeeId id, FullName name, decimal initialSalary)
        {
            _logger.Trace("InMemoryRepository", $"creating in-memory record for employee ${id}");
            _data.Add(id, new Employee(id, name, Address.NotInformed, initialSalary));
        }

        public void RaiseSalary(EmployeeId id, decimal amount)
        {
            _logger.Trace("InMemoryRepository", $"updating in-memory record salary information of employee ${id}");
            var employee = _data[id];
            _data[id] = new Employee(
                employee.Id,
                employee.Name,
                employee.HomeAddress,
                employee.Salary + amount
                );
         }

        public void UpdateHomeAddress(EmployeeId id, Address homeAddress)
        {
            _logger.Trace("InMemoryRepository", $"updating in-memory record address information of employee ${id}");
            var employee = _data[id];
            _data[id] = new Employee(
                employee.Id,
                employee.Name,
                homeAddress,
                employee.Salary
                );
        }
    }
}