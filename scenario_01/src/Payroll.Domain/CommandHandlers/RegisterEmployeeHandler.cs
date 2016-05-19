using Payroll.Domain.Commands;
using Payroll.Domain.Events;
using Payroll.Domain.Repositories;
using Payroll.Infrastructure;

namespace Payroll.Domain.CommandHandlers
{
    public class RegisterEmployeeHandler 
        : IMessageHandler<RegisterEmployeeCommand>
    {
        private readonly IBus _bus;
        private readonly IEmployeeRepository _repository;
        private readonly ILogger _logger;

        public RegisterEmployeeHandler(IBus bus, IEmployeeRepository repository, ILogger logger)
        {
            _bus = bus;
            _repository = repository;
            _logger = logger;
        }

        public void Handle(RegisterEmployeeCommand message)
        {
            if (!_repository.IsRegistered(message.Id))
            {
                _logger.Trace($"registering employee {message.Id}");
                _repository.CreateEmployee(message.Id, message.Name, message.InitialSalary);

                _logger.Trace("raising EmployeeRegisteredEvent");
                _bus.RaiseEvent(
                    new EmployeeRegisteredEvent(
                        message.Id,
                        message.Name,
                        message.InitialSalary
                        )
                    );
            }
            else
            {
                _logger.Trace($"rejecting to register an existent employee {message.Id}");
                _logger.Trace("raising FailedToRegisterEmployeeEvent");
                _bus.RaiseEvent(new FailedToRegisterEmployeeEvent(message.Id));
            }
        }
    }
}
