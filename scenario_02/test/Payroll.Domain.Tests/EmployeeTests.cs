using System.Linq;
using NUnit.Framework;
using Payroll.Domain.Events;
using Payroll.Domain.Model;
using SharpTestsEx;

namespace Payroll.Domain.Tests
{
    [TestFixture]
    public class EmployeeTest
    {
        [Test]
        public void AfterCtorNameAndSalaryAreProperlyInitialized()
        {
            var name = new FullName("John", "Doe");
            var employee = new Employee("12345", name, 100M);
            employee.Name.Should().Be(name);
            employee.Salary.Should().Be(100M);
        }

        [Test]
        public void AfterCtorEmployeeRegisteredEventShouldBeInPendingEventsList()
        {
            var name = new FullName("John", "Doe");
            var employee = new Employee("12345", name, 100M);
            employee.PendingEvents.Count().Should().Be(1);
            employee.PendingEvents.First().Should().Be.OfType<EmployeeRegistered>();
        }
    }
}
