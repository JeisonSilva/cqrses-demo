using Payroll.Domain.Commands;
using Payroll.Domain.Events;
using Payroll.Domain.Repositories;
using Payroll.Infrastructure;

namespace Payroll.Domain.CommandHandlers
{
    public class UpdateEmployeeHomeAddressHandler 
        : IMessageHandler<UpdateEmployeeHomeAddressCommand>
    {
        private readonly IBus _bus;
        private readonly IEmployeeRepository _repository;
        private readonly ILogger _logger;

        public UpdateEmployeeHomeAddressHandler(
            IBus bus, IEmployeeRepository repository, ILogger logger)
        {
            _bus = bus;
            _repository = repository;
            _logger = logger;
        }

        public void Handle(UpdateEmployeeHomeAddressCommand message)
        {
            _logger.Trace("CommandHandlers", $"updating the address of {message.Id}");
            _repository.UpdateHomeAddress(message.Id, message.HomeAddress);
            _logger.Trace("CommandHandlers", "raising EmployeeHomeAddressUpdatedEvent");
            _bus.RaiseEvent(new EmployeeHomeAddressUpdatedEvent(
                message.Id,
                message.HomeAddress
                ));
        }
    }
}
