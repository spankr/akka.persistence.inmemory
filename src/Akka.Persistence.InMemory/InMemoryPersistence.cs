using Akka.Actor;
using Akka.Configuration;
using Akka.Persistence.InMemory.Settings;
using System;

namespace Akka.Persistence.InMemory
{
    public class InMemoryPersistence : IExtension
    {
        /// <summary>
        /// Returns a default configuration for akka persistence InMemory journal and snapshot store.
        /// </summary>
        /// <returns></returns>
        public static Config DefaultConfiguration()
        {
            return ConfigurationFactory.FromResource<InMemoryPersistence>("Akka.Persistence.InMemory.reference.conf");
        }

        public static InMemoryPersistence Get(ActorSystem system)
        {
            return system.WithExtension<InMemoryPersistence, InMemoryPersistenceProvider>();
        }

        /// <summary>
        /// The settings for the InMemory journal.
        /// </summary>
        public InMemoryJournalSettings JournalSettings { get; }

        /// <summary>
        /// The settings for the snapshot store.
        /// </summary>
        public InMemorySnapshotSettings SnapshotStoreSettings { get; }

        public InMemoryPersistence(ExtendedActorSystem system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));

            // Initialize fallback configuration defaults
            system.Settings.InjectTopLevelFallback(DefaultConfiguration());

            // Read config
            var journalConfig = system.Settings.Config.GetConfig("akka.persistence.journal.inmemory");
            JournalSettings = new InMemoryJournalSettings(journalConfig);

            var snapshotConfig = system.Settings.Config.GetConfig("akka.persistence.snapshot-store.inmemory");
            SnapshotStoreSettings = new InMemorySnapshotSettings(snapshotConfig);
        }

    }
}
