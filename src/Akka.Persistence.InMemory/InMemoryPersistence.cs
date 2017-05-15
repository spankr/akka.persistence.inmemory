using Akka.Actor;
using Akka.Configuration;
using Akka.Persistence.InMemory.Journal;
using Akka.Persistence.InMemory.Settings;
using Akka.Persistence.InMemory.Snapshot;
using System;
using System.Collections.Generic;

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
        public List<JournalEntry> JournalSource { get; private set; }
        public List<MetadataEntry> MetadataSource { get; private set; }

        /// <summary>
        /// The settings for the snapshot store.
        /// </summary>
        public InMemorySnapshotSettings SnapshotStoreSettings { get; }
        public List<SnapshotEntry> SnapshotSource { get; private set; }


        public InMemoryPersistence(ExtendedActorSystem system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));

            // Initialize fallback configuration defaults
            system.Settings.InjectTopLevelFallback(DefaultConfiguration());

            // Read config
            var journalConfig = system.Settings.Config.GetConfig("akka.persistence.journal.inmemory");
            JournalSettings = new InMemoryJournalSettings(journalConfig);
            JournalSource = new List<JournalEntry>();
            MetadataSource = new List<MetadataEntry>();

            var snapshotConfig = system.Settings.Config.GetConfig("akka.persistence.snapshot-store.inmemory");
            SnapshotStoreSettings = new InMemorySnapshotSettings(snapshotConfig);
            SnapshotSource = new List<SnapshotEntry>();
        }

    }
}
