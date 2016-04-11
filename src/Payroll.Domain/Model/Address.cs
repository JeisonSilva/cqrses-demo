namespace Payroll.Domain
{
    public abstract class Address
    {
        public abstract string Country { get; }

        class NullAddress : Address
        {
            public override string Country => "Untold.";

            public override string ToString()
            {
                return "Untold";
            }
        }

        public static Address NotInformed => new NullAddress();
    }


}