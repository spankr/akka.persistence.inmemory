using Akka.Actor;
using Akka.Persistence.InMemory.Journal;
using Akka.Persistence.InMemory.Tests.Actors;
using Akka.Persistence.InMemory.Tests.Commands;
using System.Collections.Generic;
using Xunit;

namespace Akka.Persistence.InMemory.Tests
{
    public class InMemoryJournalTests : Akka.TestKit.Xunit2.TestKit
    {
        private static readonly string CONFIG = @"
            akka.persistence {
                publish-plugin-commands = on
                journal {
                    plugin = ""akka.persistence.journal.inmemory""
                    inmemory {
                        class = ""Akka.Persistence.InMemory.Journal.InMemoryJournal, Akka.Persistence.InMemory""
                        collection = ""EventJournal""
                        metadata-collection = ""Metadata""
                    }
                }
            }";

        public InMemoryJournalTests() : base(CONFIG) { }

        [Fact(DisplayName = "We can save events to the journal")]
        public void CanStoreToJournal()
        {
            const string ACTOR_NAME = "test-actor";
            const string EXPECTED_STATE01 = "Bob was here";
            const string EXPECTED_STATE02 = "So was Red";

            var actor = Sys.ActorOf(Props.Create(() => new PersistByJournalTestActor()), ACTOR_NAME);
            actor.Tell(new SetStateCommand(EXPECTED_STATE01));
            ExpectNoMsg();
            actor.Tell(new SetStateCommand(EXPECTED_STATE02));
            ExpectNoMsg();

            var journal = InMemoryPersistence.Get(Sys).JournalSource;
            Assert.Equal(2, journal.Count);
            Assert.Equal(ACTOR_NAME, journal[0].PersistenceId);
            Assert.Equal(EXPECTED_STATE01, (journal[0].Payload as SetStateCommand).NewState);
            Assert.Equal(ACTOR_NAME, journal[1].PersistenceId);
            Assert.Equal(EXPECTED_STATE02, (journal[1].Payload as SetStateCommand).NewState);

            var metadata = InMemoryPersistence.Get(Sys).MetadataSource;
            Assert.Equal(1, metadata.Count);
            Assert.Equal(2, metadata[0].SequenceNr);
            Assert.Equal(ACTOR_NAME, metadata[0].PersistenceId);
        }

        [Fact(DisplayName = "We can load events from the journal")]
        public void CanLoadFromJournal()
        {
            const string ACTOR_NAME = "test-actor";
            const string EXPECTED_STATE01 = "State01";
            const string EXPECTED_STATE02 = "State02";

            var journal = InMemoryPersistence.Get(Sys).JournalSource;
            var metadata = InMemoryPersistence.Get(Sys).MetadataSource;

            journal.Add(new JournalEntry()
            {
                Id = ACTOR_NAME + "_2",
                PersistenceId = ACTOR_NAME,
                SequenceNr = 2,
                Manifest = "",
                Payload = new SetStateCommand(EXPECTED_STATE02)
            });
            journal.Add(new JournalEntry()
            {
                Id = ACTOR_NAME + "_1",
                PersistenceId = ACTOR_NAME,
                SequenceNr = 1,
                Manifest = "",
                Payload = new SetStateCommand(EXPECTED_STATE01)
            });
            metadata.Add(new MetadataEntry()
            {
                Id = ACTOR_NAME,
                PersistenceId = ACTOR_NAME,
                SequenceNr = 2
            });

            EventFilter.Info(message: "Recovering SetStateCommand.").Expect(2, () =>
            {
                var actor = Sys.ActorOf(Props.Create(() => new PersistByJournalTestActor()), ACTOR_NAME);
                actor.Tell(new GetStateCommand());
                ExpectMsg<string>(msg => msg == EXPECTED_STATE02);
            });
        }

    }
}
