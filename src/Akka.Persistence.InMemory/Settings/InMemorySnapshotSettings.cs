using Akka.Configuration;
using System;

namespace Akka.Persistence.InMemory.Settings
{
    public class InMemorySnapshotSettings : InMemorySettings
    {

        public InMemorySnapshotSettings(Config cfg) : base(cfg)
        {
            if (cfg == null)
                throw new ArgumentNullException(nameof(cfg), "InMemory snapshot settings cannot be initialized, because required HOCON section couldn't been found");

        }
    }
}
