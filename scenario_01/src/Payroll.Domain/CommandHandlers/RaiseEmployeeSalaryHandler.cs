using Payroll.Domain.Commands;
using Payroll.Domain.Events;
using Payroll.Domain.Repositories;
using Payroll.Infrastructure;

namespace Payroll.Domain.CommandHandlers
{
    public class RaiseEmployeeSalaryHandler 
        : IMessageHandler<RaiseEmployeeSalaryCommand>
    {
        private readonly IBus _bus;
        private readonly IEmployeeRepository _repository;
        private readonly ILogger _logger;

        public RaiseEmployeeSalaryHandler(IBus bus, IEmployeeRepository repository, ILogger logger)
        {
            _bus = bus;
            _repository = repository;
            _logger = logger;
        }

        public void Handle(RaiseEmployeeSalaryCommand message)
        {
            _logger.Trace("CommandHandlers", $"raising salary of {message.Id} in {message.Amount}");
            _repository.RaiseSalary(message.Id, message.Amount);

            _logger.Trace("CommandHandlers", "raising EmployeeSalaryEvent");
            _bus.RaiseEvent(new EmployeeSalaryRaisedEvent(message.Id, message.Amount));
        }
    }
}
