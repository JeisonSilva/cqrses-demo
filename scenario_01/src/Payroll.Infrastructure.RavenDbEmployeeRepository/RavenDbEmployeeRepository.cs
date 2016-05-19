using Payroll.Domain;
using Payroll.Domain.Model;
using Payroll.Domain.Repositories;
using Raven.Abstractions.Data;
using Raven.Abstractions.Extensions;
using Raven.Json.Linq;

namespace Payroll.Infrastructure.RavenDbEmployeeRepository
{
    public class RavenDbEmployeeRepository : IEmployeeRepository
    {
        public bool IsRegistered(EmployeeId id)
        {
            var lid = $"employees/{id}";
            return DocumentStoreHolder
                .Instance
                .DatabaseCommands.Head(lid) != null;
        }

        public Employee Load(EmployeeId id)
        {
            using (var session = DocumentStoreHolder.Instance.OpenSession())
            {
                return session.Load<Employee>($"employees/{id}");
            }
            
        }

        public void CreateEmployee(EmployeeId id, FullName name, decimal initialSalary)
        {
            using (var session = DocumentStoreHolder.Instance.OpenSession())
            {
                var employee = new Employee(id, name, Address.NotInformed, initialSalary);
                session.Store(employee);
                session.SaveChanges();
            }
        }

        public void RaiseSalary(EmployeeId id, decimal amount)
        {
            DocumentStoreHolder.Instance.DatabaseCommands.Patch($"employees/{id}", new ScriptedPatchRequest
            {
                Script = $"this.Salary += {amount.ToInvariantString()};"
            });
        }

        public void UpdateHomeAddress(EmployeeId id, Address homeAddress)
        {
            var ro = RavenJObject.FromObject(homeAddress, DocumentStoreHolder.Serializer);

            DocumentStoreHolder.Instance.DatabaseCommands.Patch($"employees/{id}", new[]
            {
                new PatchRequest
                {
                    Type = PatchCommandType.Set,
                    Name = "HomeAddress",
                    Value = ro
                }
            });
        }
    }
}
