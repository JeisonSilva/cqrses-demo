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

        public IEnumerable<EmployeeEventsSummaryResult> TopEventSourceEmployees()
        {
            IEnumerable<EmployeeEventsSummaryResult> results;

            using (var session = _store.OpenSession())
            {
                results = session
                    .Query<EmployeeEventsSummaryResult, EmployeeEventsSummaryIndex>()
                    .OrderByDescending(item => item.NumberOfEvents)
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
                    .Query<EmployeeSalary, EmployeeEventsSalaryPerEmployee>()
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


        class EmployeeEventsSummaryIndex 
            : AbstractIndexCreationTask<EmployeeEvent, EmployeeEventsSummaryResult>
        {

            public override string IndexName => "EmployeeEvents/Summary";

            public EmployeeEventsSummaryIndex()
            {
                Map = (events) =>
                    from e in events
                    select new EmployeeEventsSummaryResult
                    {
                        EmployeeId = e.EmployeeId,
                        NumberOfEvents = 1
                    };

                Reduce = (inputs) =>
                    from input in inputs
                    group input by input.EmployeeId into g
                    select new EmployeeEventsSummaryResult
                    {
                        EmployeeId = g.Key,
                        NumberOfEvents = g.Sum(x => x.NumberOfEvents)
                    };
            }
        }

        public class EmployeeSalary
        {
            public EmployeeId EmployeeId { get; set; }
            public string FullName { get; set; }
            public decimal Salary { get; set; }
        }

        public class EmployeeEventsSalaryPerEmployee : AbstractMultiMapIndexCreationTask<EmployeeSalary>
        {
            public override string IndexName => "EmployeeEvents/SalaryPerEmployee";

            public EmployeeEventsSalaryPerEmployee()
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
                    select new EmployeeSalary()
                    {
                        EmployeeId = g.Key,
                        FullName = g.Aggregate("", (a, b) => b.FullName != "" ? b.FullName : a),
                        Salary = g.Sum(x => x.Salary)
                    };
            }
        }
    }
}
