using Payroll.Domain.Model;

namespace FunWithEntities
{
    class Program
    {
        static void Main(string[] args)
        {
            var employee = new Employee("12345", new FullName("John", "Doe"), 150M);

            employee.ChangeHomeAddress(BrazilianAddress.Factory.New(
                street: "Somewhere avenue",
                number: 42,
                addtionalInfo: null,
                neighborhood: "Bellvue",
                city: "Bigtown",
                state: "RS",
                postalCode: "95000-000"));

            employee.RaiseSalary(25M);
        }
    }
}
