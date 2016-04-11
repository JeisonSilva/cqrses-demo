using System;
using System.Dynamic;
using Payroll.Domain;
using Payroll.Domain.Model;
using Payroll.Domain.Repositories;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Raven.Json.Linq;

namespace Payroll.Infrastructure.RavenDbEmployeeRepository
{
    public class EmployeeRepository : IEmployeeRepository, IDisposable
    {
        private readonly IDocumentStore _store;

        public EmployeeRepository()
        {
            _store = new DocumentStore
            {
                Url = "http://localhost:8081/", // server URL
                DefaultDatabase = "RegularDb"
            };

            _store.Initialize();

            _store.Conventions.IdentityTypeConvertors.Add(
                new EmployeeIdConverter()
                );
        }

        
        public bool IsRegistered(EmployeeId id)
        {
            return false;
        }

        public Employee Load(EmployeeId id)
        {
            Employee result;
            using (var session = _store.OpenSession())
            {
                result = session.Load<Employee>(id);
            }
            return result;
        }

        public void CreateEmployee(EmployeeId id, FullName name, decimal initialSalary)
        {
            using (var session = _store.OpenSession())
            {
                var employee = new Employee(id, name, Address.NotInformed, initialSalary);
                session.Store(employee);
                session.SaveChanges();
            }
        }

        public void RaiseSalary(EmployeeId id, decimal amount)
        {
        }

        public void UpdateHomeAddress(EmployeeId id, Address homeAddress)
        {
            var ro = RavenJObject.FromObject(homeAddress);
            ro.Add("$type", homeAddress.GetType().FullName);
            
            _store.DatabaseCommands.Patch(id, new[]
            {
                new PatchRequest()
                {
                    Type = PatchCommandType.Set,
                    Name = "HomeAddress",
                    Value = ro
                }
            });
        }

        public void Dispose()
        {
            _store.Dispose();
        }
    }
}
