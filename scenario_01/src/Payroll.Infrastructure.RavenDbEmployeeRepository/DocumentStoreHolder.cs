using Payroll.Domain.Events;
using Raven.Client.Document;
using Raven.Imports.Newtonsoft.Json;

namespace Payroll.Infrastructure.RavenDbEmployeeRepository
{
    class DocumentStoreHolder
    {
        static DocumentStoreHolder()
        {
            Instance = new DocumentStore
            {
                Url = "http://localhost:8080/",
                DefaultDatabase = "RegularDb",
                Conventions =
                {
                    CustomizeJsonSerializer =
                        serializer => { serializer.Converters.Add(new EmployeeIdJsonConverter()); }
                }
            };

            var defaultImpl = Instance.Conventions.FindTypeTagName;

            Instance.Conventions.FindTypeTagName = t => t.IsSubclassOf(typeof(EmployeeEvent)) 
                ? "EmployeeEvents" 
                : defaultImpl(t);

            Instance.Conventions.IdentityTypeConvertors.Add(
                new EmployeeIdConverter()
                );

            Instance.Initialize();
            Serializer = Instance.Conventions.CreateSerializer();
            Serializer.TypeNameHandling = TypeNameHandling.All;

            Instance.Initialize();
        }

        public static DocumentStore Instance { get; }

        public static JsonSerializer Serializer { get; }
    }
}