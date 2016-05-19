using System;

using Payroll.Domain;
using Payroll.Domain.Commands;
using Payroll.Domain.Model;
using Payroll.Domain.Repositories;
using Payroll.Infrastructure;
using Payroll.Infrastructure.RavenDbEmployeeRepository;


namespace Playground
{
    class Program
    {
        private readonly PersistenceStrategy _strategy;
        private readonly IDependencyInjector _container;

        static void Main()
        {
            
            new Program(PersistenceStrategy.InMemory)
                .Run();            
        }
        
        public Program(PersistenceStrategy strategy)
        {
            _strategy = strategy;
            _container = new SimpleDependencyInjector();
            Boostrapper.Run(strategy, _container);
        }

        public void Run()
        {
            ExecuteSampleCommands(_container);
        }

        public void DisplaySomeResults()
        {
            var repo = _container.Get<IEmployeeRepository>();

            var employee1 = repo.Load("12345");
            Console.WriteLine($"Employee {employee1.Id} - {employee1.Name} salary is ${employee1.Salary}");
            var employee2 = repo.Load("54321");
            Console.WriteLine($"Employee {employee2.Id} - {employee2.Name} salary is ${employee2.Salary}");


            if (_strategy != PersistenceStrategy.RavenDb) return;

            var es = _container.Get<RavenDbEmployeeEventStore>();
            foreach (var entry in es.TopEventSourceEmployees())
                Console.WriteLine($"Number of events to {entry.EmployeeId} is {entry.NumberOfEvents}");
            
            foreach (var entry in es.TopSalaries())
                Console.WriteLine($"{entry.EmployeeId} -  {entry.FullName} (${entry.Salary})");
        }


        private static void ExecuteSampleCommands(IDependencyInjector container)
        {
            var bus = container.Get<IBus>();

            bus.SendCommand(new RegisterEmployeeCommand(
                            "12345",
                            new FullName("John", "Doe"),
                            100m
                            ));

            bus.SendCommand(new RegisterEmployeeCommand(
                "54321",
                new FullName("Mary", "Loo"),
                103m
                ));

            bus.SendCommand(new UpdateEmployeeHomeAddressCommand(
                "12345",
                BrazilianAddress.Factory.New("Nice street", 42, null, "Good Ville", "MyCity", "XX", "91234-123"))
                    );

            bus.SendCommand(new RaiseEmployeeSalaryCommand(
                id: "12345",  
                amount: 10m));
            bus.SendCommand(new RaiseEmployeeSalaryCommand("12345", 20m));
            bus.SendCommand(new RaiseEmployeeSalaryCommand("12345", 13m));

            bus.SendCommand(new UpdateEmployeeHomeAddressCommand(
                "12345",
                BrazilianAddress.Factory.New("Main avenue", 42, null, "Good Ville", "MyCity", "XX", "91234-123"))
                    );

            bus.SendCommand(new RaiseEmployeeSalaryCommand("12345", 21m));
            bus.SendCommand(new RaiseEmployeeSalaryCommand("12345", 14m));
        }
    }
}
