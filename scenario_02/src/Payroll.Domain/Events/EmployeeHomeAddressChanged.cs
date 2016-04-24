using Infrastructure.EventSourcing;
using Payroll.Domain.Model;

namespace Payroll.Domain.Events
{
    public class EmployeeHomeAddressChanged : VersionedEvent<string>
    {
        public Address NewAddress { get; }

        public EmployeeHomeAddressChanged(Address newAddress)
        {
            NewAddress = newAddress;
        }
    }
}