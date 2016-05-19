using System.Collections.Generic;
using System.Linq;
using Payroll.Domain.Events;
using Payroll.Domain.Model;
using Raven.Client.Indexes;

namespace Payroll.Infrastructure.RavenDbEmployeeRepository
{
    partial class RavenDbEmployeeEventStore
    {
        public IEnumerable<EventsPerEmployeeResult> TopEventSourceEmployees()
        {
            IEnumerable<EventsPerEmployeeResult> results;

            using (var session = _store.OpenSession())
            {
                results = session
                    .Query<EventsPerEmployeeResult, EventsPerEmployeeIndex>()
                    .OrderByDescending(item => item.NumberOfEvents);
            }

            return results;
        }

        public class EventsPerEmployeeResult
        {
            public EmployeeId EmployeeId { get; set; }
            public int NumberOfEvents { get; set; }
        }


        private class EventsPerEmployeeIndex
            : AbstractIndexCreationTask<EmployeeEvent, EventsPerEmployeeResult>
        {

            public override string IndexName => "EmployeeEvents/Summary";

            public EventsPerEmployeeIndex()
            {
                Map = (events) =>
                    from e in events
                    select new EventsPerEmployeeResult
                    {
                        EmployeeId = e.EmployeeId,
                        NumberOfEvents = 1
                    };

                Reduce = (inputs) =>
                    from input in inputs
                    group input by input.EmployeeId into g
                    select new EventsPerEmployeeResult
                    {
                        EmployeeId = g.Key,
                        NumberOfEvents = g.Sum(x => x.NumberOfEvents)
                    };
            }
        }
    }
}
