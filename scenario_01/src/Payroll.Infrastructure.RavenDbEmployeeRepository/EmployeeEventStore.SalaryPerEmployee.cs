using System.Collections.Generic;
using System.Linq;
using Payroll.Domain.Events;
using Payroll.Domain.Model;
using Raven.Client.Indexes;

namespace Payroll.Infrastructure.RavenDbEmployeeRepository
{
    partial class RavenDbEmployeeEventStore
    {
        public IEnumerable<SalaryPerEmployeeResult> TopSalaries()
        {
            IEnumerable<SalaryPerEmployeeResult> results;

            using (var session = DocumentStoreHolder.Instance.OpenSession())
            {
                results = session
                    .Query<SalaryPerEmployeeResult, SalaryPerEmployeeIndex>()
                    .OrderByDescending(es => es.Salary);
            }

            return results;
        }

        public class SalaryPerEmployeeResult
        {
            public EmployeeId EmployeeId { get; set; }
            public string FullName { get; set; }
            public decimal Salary { get; set; }
        }

        private class SalaryPerEmployeeIndex 
            : AbstractMultiMapIndexCreationTask<SalaryPerEmployeeResult>
        {
            public override string IndexName => "EmployeeEvents/SalaryPerEmployee";

            public SalaryPerEmployeeIndex()
            {
                AddMap<EmployeeSalaryRaisedEvent>(events =>
                    from e in events
                    where e.MessageType == "EmployeeSalaryRaisedEvent"
                    select new
                    {
                        e.EmployeeId,
                        FullName = "",
                        Salary = e.Amount
                    });

                AddMap<EmployeeRegisteredEvent>(events =>
                    from e in events
                    where e.MessageType == "EmployeeRegisteredEvent"
                    select new
                    {
                        e.EmployeeId,
                        FullName = e.Name.GivenName + "  " + e.Name.Surname,
                        Salary = e.InitialSalary
                    });

                Reduce = inputs =>
                    from input in inputs
                    group input by input.EmployeeId
                    into g
                    select new SalaryPerEmployeeResult()
                    {
                        EmployeeId = g.Key,
                        FullName = g.Aggregate("", (a, b) => b.FullName != "" ? b.FullName : a),
                        Salary = g.Sum(x => x.Salary)
                    };
            }
        }
    }
}
