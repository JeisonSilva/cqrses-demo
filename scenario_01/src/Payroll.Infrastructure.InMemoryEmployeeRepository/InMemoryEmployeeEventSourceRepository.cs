using System;
using System.Collections.Generic;
using System.Linq;
using Payroll.Domain;
using Payroll.Domain.Events;
using Payroll.Domain.Model;
using Payroll.Domain.Repositories;

namespace Payroll.Infrastructure.InMemoryEmployeeRepository
{
    public class InMemoryEmployeeEventSourceRepository 
        : IEmployeeRepository,
        IMessageHandler<EmployeeRegisteredEvent>,
        IMessageHandler<EmployeeSalaryRaisedEvent>,
        IMessageHandler<EmployeeHomeAddressUpdatedEvent>
    {
        private readonly ILogger _logger;

        readonly IList<EmployeeEvent> _events = new List<EmployeeEvent>();

        public InMemoryEmployeeEventSourceRepository(
            ILogger logger
            )
        {
            _logger = logger;
        }

        public bool IsRegistered(EmployeeId id)
        {
            return _events.Any(e => e.EmployeeId.Equals(id));
        }

        public Employee Load(EmployeeId id)
        {
            _logger.Trace("InMemoryRepositoryES", $"starting to load employee {id}");

            var employeeEvents = _events.Where(e => e.EmployeeId.Equals(id));
            Employee result = null;

            foreach (var e in employeeEvents)
            {
                if (e is EmployeeRegisteredEvent)
                {
                    var l = e as EmployeeRegisteredEvent;
                    result = new Employee(l.EmployeeId, l.Name, Address.NotInformed, l.InitialSalary);
                    _logger.Trace("InMemoryRepositoryES", $"replay of {l.MessageType} - {l.Name} (${l.InitialSalary})");
                    Console.WriteLine($"{l.MessageType} - {l.Name} (${l.InitialSalary})");
                }
                else if (e is EmployeeSalaryRaisedEvent)
                {
                    var l = e as EmployeeSalaryRaisedEvent;
                    var newSalary = result.Salary + l.Amount;
                    _logger.Trace("InMemoryRepositoryES", $"replay of {l.MessageType} - ${l.Amount} (from ${result.Salary} to ${newSalary})");
                    
                    result = new Employee(result.Id, result.Name, result.HomeAddress, newSalary);
                }
                else if (e is EmployeeHomeAddressUpdatedEvent)
                {
                    var l = e as EmployeeHomeAddressUpdatedEvent;
                    _logger.Trace("InMemoryRepositoryES", $"replay of {l.MessageType} - from {result.HomeAddress} to {l.NewHomeAddress}");
                    result = new Employee(result.Id, result.Name, l.NewHomeAddress, result.Salary);
                }
            }

            _logger.Trace("InMemoryRepositoryES", $"employee {id} loaded");

            return result;
        }

        #region Unused repository methods

        public void CreateEmployee(EmployeeId id, FullName name, decimal initialSalary)
        {
            _logger.Warn("InMemoryRepositoryES", $"ignoring create employee transaction script request ${id}");
        }

        public void RaiseSalary(EmployeeId id, decimal amount)
        {
            _logger.Warn("InMemoryRepositoryES", $"ignoring raise employee salary transaction script request ${id}");
        }

        public void UpdateHomeAddress(EmployeeId id, Address homeAddress)
        {
            _logger.Warn("InMemoryRepositoryES", $"ignoring update employee address transaction script request ${id}");
        }
        #endregion

        public void Handle(EmployeeRegisteredEvent e)
        {
            _logger.Trace("InMemoryRepositoryES", "handling EmployeeRegisteredEvent");
            _events.Add(e);
        }

        public void Handle(EmployeeSalaryRaisedEvent e)
        {
            _logger.Trace("InMemoryRepositoryES", "handling EmployeeSalaryRaisedEvent");
            _events.Add(e);
        }

        public void Handle(EmployeeHomeAddressUpdatedEvent e)
        {
            _logger.Trace("InMemoryRepositoryES", "handling EmployeeHomeAddressUpdatedEvent");
            _events.Add(e);
        }
    }
}
