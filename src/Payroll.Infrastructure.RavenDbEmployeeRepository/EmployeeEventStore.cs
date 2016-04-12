using System;
using System.Collections.Generic;
using System.Linq;
using Payroll.Domain.Events;
using Payroll.Domain.Model;
using Raven.Abstractions.Indexing;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Imports.Newtonsoft.Json;

namespace Payroll.Infrastructure.RavenDbEmployeeRepository
{
    public class EmployeeEventStore :
        IMessageHandler<EmployeeRegisteredEvent>,
        IMessageHandler<EmployeeHomeAddressUpdatedEvent>,
        IMessageHandler<EmployeeSalaryRaisedEvent>,
        IDisposable
    {
        private readonly DocumentStore _store;

        public EmployeeEventStore()
        {
            _store = new DocumentStore
            {
                Url = "http://localhost:8080/", // server URL
                DefaultDatabase = "RegularDb"
            };

            _store.Conventions.CustomizeJsonSerializer = serializer =>
            {
                serializer.Converters.Add(new EmployeeIdJsonConverter());
            };

            _store.Initialize();
            _store.Conventions.FindTypeTagName = t => "EmployeeEvents";

            InitializeIndexes();
        }

        private void InitializeIndexes()
        {
            new EmployeeEventsSummaryIndex().Execute(_store);
            new EmployeeEventsSalaryPerEmployee().Execute(_store);
        }


        public void HandleInternal(Message message)
        {
            using (var session = _store.OpenSession())
            {
                session.Store(message);
                session.SaveChanges();
            }
        }

        public void Handle(EmployeeRegisteredEvent message)
        {
            HandleInternal(message);
        }

        public void Handle(EmployeeHomeAddressUpdatedEvent message)
        {
            HandleInternal(message);
        }

        public void Handle(EmployeeSalaryRaisedEvent message)
        {
            HandleInternal(message);
        }

        public void Dispose()
        {
            _store.Dispose();
        }

        public class EmployeeIdJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, $"employees/{value}");
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                JsonSerializer serializer)
            {
                var original = (string) reader.Value;
                return (EmployeeId) original.Substring(
                    original.IndexOf("/", StringComparison.Ordinal) + 1);
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof (EmployeeId);
            }
        }

        public IEnumerable<EmployeeEventsSummaryResult> Summary()
        {
            IEnumerable<EmployeeEventsSummaryResult> results;

            using (var session = _store.OpenSession())
            {
                results = session
                    .Advanced
                    .DocumentQuery<EmployeeEventsSummaryResult, EmployeeEventsSummaryIndex>()
                    .ToList();
            }

            return results;
        }


        public IEnumerable<EmployeeSalary> TopSalaries()
        {
            IEnumerable<EmployeeSalary> results;

            using (var session = _store.OpenSession())
            {
                results = session
                    .Advanced
                    .DocumentQuery<EmployeeSalary, EmployeeEventsSalaryPerEmployee>()
                    .OrderByDescending(es => es.Salary)
                    .ToList();
            }

            return results;
        }

        public class EmployeeEventsSummaryResult
        {
            public EmployeeId EmployeeId { get; set; }
            public int NumberOfEvents { get; set; }
        }


        class EmployeeEventsSummaryIndex : AbstractIndexCreationTask
        {

            public override string IndexName => "EmployeeEvents/Summary";

            public override IndexDefinition CreateIndexDefinition()
            {
                return new IndexDefinition
                {
                    Map = @"
from doc in docs.EmployeeEvents
select new {
	EmployeeId = doc.EmployeeId,
	NumberOfEvents=1
}",
                    Reduce = @" 
from result in results
group result by result.EmployeeId into g
select new {EmployeeId=g.Key, NumberOfEvents = g.Sum(x => x.NumberOfEvents)}"
                };
            }
        }

        public class EmployeeSalary
        {
            public EmployeeId EmployeeId { get; set; }
            public string FullName { get; set; }
            public decimal Salary { get; set; }
        }

        public class EmployeeEventsSalaryPerEmployee : AbstractIndexCreationTask
        {
            public override string IndexName => "EmployeeEvents/SalaryPerEmployee";

            public override IndexDefinition CreateIndexDefinition()
            {
                return new IndexDefinition
                {
                    Maps =
                    {
                        @"from doc in docs.EmployeeEvents
where doc.MessageType == ""EmployeeSalaryRaisedEvent""
select new {
	doc.EmployeeId,
	FullName = """",
	Salary = doc.Amount,
}",
                        @"from doc in docs.EmployeeEvents
where doc.MessageType == ""EmployeeRegisteredEvent""
select new {
	doc.EmployeeId,
	FullName = doc.Name.GivenName + "" "" + doc.Name.Surname,
	Salary = doc.InitialSalary,
}"
                    },
                    Reduce = @"from result in results
group result by result.EmployeeId into g
select new {
	EmployeeId = g.Key,
	FullName = g.Aggregate("""", (prev, entry) => entry.FullName != """" ? entry.FullName : prev),
	Salary = g.Sum(x => x.Salary)
}"
                };
            }

        }
    }
}
