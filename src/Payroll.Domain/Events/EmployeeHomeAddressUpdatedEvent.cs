using Payroll.Domain.Model;

namespace Payroll.Domain.Events
{
    public sealed class EmployeeHomeAddressUpdatedEvent
        : EmployeeEvent
    {
        public Address NewHomeAddress { get; }

        public EmployeeHomeAddressUpdatedEvent(
            EmployeeId id, 
            Address address
            ) : base(id)
        {
            NewHomeAddress = address;
        }
    }
}
