using System;
using System.Configuration;
using Akka.Persistence.TestKit.Snapshot;
using Xunit;

namespace Akka.Persistence.InMemory.Tests
{
    [Collection("InMemorySpec")]
    public class InMemorySnapshotStoreSpec : SnapshotStoreSpec
    {
        private static readonly string CONFIG = @"
            akka.persistence {
                publish-plugin-commands = on
                snapshot-store {
                    plugin = ""akka.persistence.snapshot-store.inmemory""
                    inmemory {
                        class = ""Akka.Persistence.InMemory.Snapshot.InMemorySnapshotStore, Akka.Persistence.InMemory""
                        collection = ""SnapshotStore""
                    }
                }
            }";

        public InMemorySnapshotStoreSpec() : base(CONFIG, "InMemorySnapshotStoreSpec")
        {
            Initialize();
        }
    }
}
