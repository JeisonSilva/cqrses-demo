using System;

namespace Payroll.Infrastructure
{
    public abstract class Message
    {
        public string MessageName { get; private set; }

        protected Message()
        {
            MessageName = GetType().Name;
        }
    }
}
