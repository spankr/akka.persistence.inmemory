using Akka.Configuration;
using System;

namespace Akka.Persistence.InMemory.Settings
{
    public class InMemoryJournalSettings : InMemorySettings
    {
        public string MetadataCollection { get; private set; }

        public InMemoryJournalSettings(Config cfg) : base(cfg)
        {
            if (cfg == null)
                throw new ArgumentNullException(nameof(cfg), "InMemory journal settings cannot be initialized, because required HOCON section couldn't been found");

            MetadataCollection = cfg.GetString("metadata-collection");
        }
    }
}
