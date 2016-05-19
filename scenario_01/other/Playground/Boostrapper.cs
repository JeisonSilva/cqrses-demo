using Payroll.Domain.CommandHandlers;
using Payroll.Domain.Repositories;
using Payroll.Infrastructure;
using Payroll.Infrastructure.InMemoryBus;
using Payroll.Infrastructure.InMemoryEmployeeRepository;
using Payroll.Infrastructure.RavenDbEmployeeRepository;

namespace Playground
{
    enum PersistenceStrategy
    {
        InMemory,
        RavenDb    
    }

    class Boostrapper
    {
        public static void Run(PersistenceStrategy strategy, IDependencyInjector container)
        {
            SetupBus(container);
            SetupDomainCommandHandlers(container);

            if (strategy == PersistenceStrategy.InMemory)
            {
                SetupInMemoryRepo(container);
                SetupInMemoryESRepo(container);
            }
            else
            {
                SetupRavenDbRepo(container);   
            }
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

        private static void SetupRavenDbRepo(SimpleDependencyInjector container)
        {
            container.BindToConstant<IEmployeeRepository>(
                new RavenDbEmployeeRepository()
                );

            container.BindToConstant(new RavenDbEmployeeEventStore());
            container.Get<IBus>().RegisterHandler<RavenDbEmployeeEventStore>();
        }
    }
}
