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
        InMemoryEventSourcing,
        RavenDb    
    }

    class Boostrapper
    {
        public static void Run(PersistenceStrategy strategy, IDependencyInjector container)
        {
            container.BindToConstant<ILogger>(new DefaultLogger());

            container.Get<ILogger>().Trace("Bootstrapper", "starting the bootstrapper.");

            SetupBus(container);
            SetupDomainCommandHandlers(container);

            switch (strategy)
            {
                case PersistenceStrategy.InMemory:
                    container.Get<ILogger>().Trace("Bootstrapper", "bootstrapping In-Memory strategy");
                    SetupInMemoryRepo(container);
                    break;
                case PersistenceStrategy.InMemoryEventSourcing:
                    container.Get<ILogger>().Trace("Bootstrapper", "bootstrapping In-Memory ES strategy");
                    SetupInMemoryEsRepo(container);
                    break;
                default:
                    container.Get<ILogger>().Trace("Bootstrapper", "bootstrapping RavenDb strategy");
                    SetupRavenDbRepo(container);
                    break;
            }

            container.Get<ILogger>().Trace("Bootstrapper", "done.");
        }

        private static void SetupBus(IDependencyInjector container)
        {
            container.Get<ILogger>().Trace("Bootstrapper", "initializing In-Memory bus.");

            var bus = container.Get<NaiveInMemoryBus>();
            container.BindToConstant<IBus>(bus);
        }

        private static void SetupDomainCommandHandlers(IDependencyInjector container)
        {
            container.Get<ILogger>().Trace("Bootstrapper", "registering command handlers.");

            var bus = container.Get<IBus>();

            bus.RegisterHandler<RegisterEmployeeHandler>();
            bus.RegisterHandler<RaiseEmployeeSalaryHandler>();
            bus.RegisterHandler<UpdateEmployeeHomeAddressHandler>();
        }

        private static void SetupInMemoryRepo(IDependencyInjector container)
        {
            container.Get<ILogger>().Trace("Bootstrapper", "initialzing In-memory repository");

            container.BindToConstant<IEmployeeRepository>(
                container.Get<InMemoryEmployeeRepository>()
                );

        }

        private static void SetupInMemoryEsRepo(IDependencyInjector container)
        {
            container.Get<ILogger>().Trace("Bootstrapper", "initialzing In-memory event store repo");

            var esrepo = container.Get<InMemoryEmployeeEventSourceRepository>();
            container.BindToConstant<IEmployeeRepository>(esrepo);
            container.BindToConstant(esrepo);
            container.Get<IBus>().RegisterHandler<InMemoryEmployeeEventSourceRepository>();
        }

        private static void SetupRavenDbRepo(IDependencyInjector container)
        {
            container.Get<ILogger>().Trace("Bootstrapper", "initialzing RavenDB repositories");

            container.BindToConstant<IEmployeeRepository>(
                container.Get<RavenDbEmployeeRepository>()
                );


            container.BindToConstant(container.Get<RavenDbEmployeeEventStore>());
            container.Get<IBus>().RegisterHandler<RavenDbEmployeeEventStore>();
        }
    }
}
