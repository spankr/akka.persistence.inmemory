using Akka.Actor;
using Akka.Persistence.InMemory.Snapshot;
using Akka.Persistence.InMemory.Tests.Actors;
using Akka.Persistence.InMemory.Tests.Commands;
using System.Collections.Generic;
using Xunit;

namespace Akka.Persistence.InMemory.Tests
{
    public class InMemorySnapshotStoreTests : TestKit.Xunit2.TestKit
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

        public InMemorySnapshotStoreTests() : base(CONFIG)
        {
            InMemorySnapshotStore.ExternalSnapshotSource = new List<SnapshotEntry>();
        }

        [Fact(DisplayName = "We can save state to the snapshot store")]
        public void CanSaveStateToSnapshotStore()
        {
            const string EXPECTED_STATE = "Bob was here";

            var snapshotActor = Sys.ActorOf(Props.Create(() => new PersistBySnapshotTestActor()), "test-actor");
            snapshotActor.Tell(new SetStateCommand(EXPECTED_STATE));
            ExpectNoMsg();

            var snapshots = InMemorySnapshotStore.ExternalSnapshotSource;

            Assert.Equal(1, snapshots.Count);
            Assert.IsType(typeof(TestActorState), snapshots[0].Snapshot);
            Assert.Equal(EXPECTED_STATE, (snapshots[0].Snapshot as TestActorState).State);
        }

        [Fact(DisplayName ="We can load state from the snapshot store")]
        public void CanLoadStateFromSnapshotStore()
        {
            const string ACTOR_NAME = "test-actorX";
            const string EXPECTED_STATE = "Ken Hartley Reed";
            var snapshots = InMemorySnapshotStore.ExternalSnapshotSource;
            snapshots.Add(new SnapshotEntry()
            {
                Id = ACTOR_NAME+"_0",
                PersistenceId = ACTOR_NAME,
                SequenceNr = 0,
                Snapshot = new TestActorState() { State = EXPECTED_STATE },
                Timestamp = 1
            });

            // verify the SnapshotOffer was triggered when we create our actor
            EventFilter.Info(message: "SnapshotOffer received.").ExpectOne(() =>
            {
                var snapshotActor = Sys.ActorOf(Props.Create(() => new PersistBySnapshotTestActor()), ACTOR_NAME);
                snapshotActor.Tell(new GetStateCommand());
            });

            ExpectMsg<string>(msg => msg == EXPECTED_STATE);

        }
    }
}
