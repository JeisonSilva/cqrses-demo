using Payroll.Domain.Model;

namespace Payroll.Domain.Repositories
{
    public interface IEmployeeRepository
    {
        Employee Load(EmployeeId id);
        void CreateEmployee(
            EmployeeId id,
            FullName name,
            decimal initialSalary);

        void RaiseSalary(EmployeeId id, decimal amount);
        void UpdateHomeAddress(EmployeeId id, Address homeAddress);
    }
}
