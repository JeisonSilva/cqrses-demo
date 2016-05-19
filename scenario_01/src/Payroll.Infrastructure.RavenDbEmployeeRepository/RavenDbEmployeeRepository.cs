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
        private readonly ILogger _logger;

        public RavenDbEmployeeRepository(ILogger logger)
        {
            _logger = logger;
        }

        public bool IsRegistered(EmployeeId id)
        {
            var lid = $"employees/{id}";
            return DocumentStoreHolder
                .Instance
                .DatabaseCommands.Head(lid) != null;
        }

        public Employee Load(EmployeeId id)
        {
            _logger.Trace("RavenDBRepository", $"Loading employee {id}");
            using (var session = DocumentStoreHolder.Instance.OpenSession())
            {
                return session.Load<Employee>($"employees/{id}");
            }
            
        }

        public void CreateEmployee(EmployeeId id, FullName name, decimal initialSalary)
        {
            _logger.Trace("RavenDBRepository", $"Creating a new employee ({id})");
            using (var session = DocumentStoreHolder.Instance.OpenSession())
            {
                var employee = new Employee(id, name, Address.NotInformed, initialSalary);
                session.Store(employee);
                session.SaveChanges();
            }
        }

        public void RaiseSalary(EmployeeId id, decimal amount)
        {
            _logger.Trace("RavenDBRepository", $"Raising salary ({id})");
            DocumentStoreHolder.Instance.DatabaseCommands.Patch($"employees/{id}", new ScriptedPatchRequest
            {
                Script = $"this.Salary += {amount.ToInvariantString()};"
            });
        }

        public void UpdateHomeAddress(EmployeeId id, Address homeAddress)
        {
            _logger.Trace("RavenDBRepository", $"Updating address ({id})");
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
