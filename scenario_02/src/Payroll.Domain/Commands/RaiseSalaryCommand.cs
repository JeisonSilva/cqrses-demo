using System;

namespace Payroll.Domain.Commands
{
    public class RaiseSalaryCommand
    {
        public Guid EmployeeId { get; }
        public decimal Amount { get; }

        public RaiseSalaryCommand(Guid employeeId, decimal amount)
        {
            EmployeeId = employeeId;
            Amount = amount;
        }
    }
}
