using System;
using Payroll.Domain;
using Payroll.Domain.CommandHandlers;
using Payroll.Domain.Commands;
using Payroll.Domain.Model;
using Payroll.Domain.Repositories;
using Payroll.Infrastructure;
using Payroll.Infrastructure.InMemoryBus;

namespace Playground
{
    class Program
    {
        static void Main()
        {
            var container = new SimpleDependencyInjector();

            SetupBus(container);
            SetupDomainCommandHandlers(container);

            //SetupInMemoryRepo(container);
            SetupInMemoryESRepo(container);

            ExecuteSampleCommands(container);
            
            var employee = container.Get<IEmployeeRepository>().Load("12345");
            Console.WriteLine($"Employee {employee.Id} - {employee.Name} salary is ${employee.Salary}");

            var employee2 = container.Get<IEmployeeRepository>().Load("54321");
            Console.WriteLine($"Employee {employee2.Id} - {employee2.Name} salary is ${employee2.Salary}");
        }

        private static void SetupBus(SimpleDependencyInjector container)
        {
            var bus = new NaiveInMemoryBus(container);
            container.BindToConstant<IBus>(bus);
        }

        private static void SetupDomainCommandHandlers(IDependencyInjector container)
        {
            var bus = container.Get<IBus>();
            bus.RegisterHandler<RegisterEmployeeHandler>();
            bus.RegisterHandler<RaiseEmployeeSalaryHandler>();
            bus.RegisterHandler<UpdateEmployeeHomeAddressHandler>();
        }

        private static void SetupInMemoryRepo(SimpleDependencyInjector container)
        {
            container.BindToConstant<IEmployeeRepository>(
                new InMemoryEmployeeRepository()
                );

        }

        private static void SetupInMemoryESRepo(SimpleDependencyInjector container)
        {
            var esrepo = new InMemoryEmployeeEventSourceRepository();
            container.BindToConstant<IEmployeeRepository>(esrepo);
            container.BindToConstant(esrepo);
            container.Get<IBus>().RegisterHandler<InMemoryEmployeeEventSourceRepository>();
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

            bus.SendCommand(new RaiseEmployeeSalaryCommand("12345", 10m));
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
