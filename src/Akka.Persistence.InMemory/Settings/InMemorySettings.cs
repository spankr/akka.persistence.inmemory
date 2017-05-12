using Akka.Configuration;

namespace Akka.Persistence.InMemory.Settings
{
    public class InMemorySettings
    {
        /// <summary>
        /// Name of the collection for the event journal or snapshots
        /// </summary>
        public string Collection { get; private set; }

        protected InMemorySettings(Config config)
        {
            Collection = config.GetString("collection");
        }

    }
}
