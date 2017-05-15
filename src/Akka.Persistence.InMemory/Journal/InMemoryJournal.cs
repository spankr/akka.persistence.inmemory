using Akka.Actor;
using Akka.Persistence.InMemory.Settings;
using Akka.Persistence.Journal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Akka.Persistence.InMemory.Journal
{
    public class InMemoryJournal : AsyncWriteJournal
    {
        private readonly InMemoryJournalSettings _settings;
        private List<JournalEntry> _journalCollection;
        private List<MetadataEntry> _metadataCollection;

        public InMemoryJournal()
        {
            _settings = InMemoryPersistence.Get(Context.System).JournalSettings;
        }

        protected override void PreStart()
        {
            base.PreStart();

            _journalCollection = InMemoryPersistence.Get(Context.System).JournalSource;
            _metadataCollection = InMemoryPersistence.Get(Context.System).MetadataSource;
        }

        public override async Task ReplayMessagesAsync(IActorContext context, string persistenceId, long fromSequenceNr, long toSequenceNr, long max, Action<IPersistentRepresentation> recoveryCallback)
        {
            // Limit allows only integer
            var limitValue = max >= int.MaxValue ? int.MaxValue : (int)max;

            // Do not replay messages if limit equal zero
            if (limitValue == 0)
                return;

            await Task.Run(() =>
            {
                var collections = _journalCollection
                    .Where(x => x.PersistenceId == persistenceId && (fromSequenceNr <= 0 || fromSequenceNr <= x.SequenceNr) && (toSequenceNr == long.MaxValue || x.SequenceNr <= toSequenceNr))
                    .OrderBy(x => x.SequenceNr)
                    .Take(limitValue)
                    .ToList();

                collections.ForEach(entry =>
                {
                    recoveryCallback(ToPersistenceRepresentation(entry, context.Sender));
                });
            });
        }

        public override async Task<long> ReadHighestSequenceNrAsync(string persistenceId, long fromSequenceNr)
        {
            return await Task.Run(() =>
            {
                var highestSequenceNr = _metadataCollection.Where(x => x.PersistenceId == persistenceId).OrderByDescending(x => x.SequenceNr).DefaultIfEmpty(new MetadataEntry() { SequenceNr = 0 }).First();

                return highestSequenceNr.SequenceNr;
            });
        }

        protected override async Task<IImmutableList<Exception>> WriteMessagesAsync(IEnumerable<AtomicWrite> messages)
        {
            var messageList = messages.ToList();
            var writeTasks = messageList.Select(async message =>
            {
                await Task.Run(() =>
                {
                    var persistentMessages = ((IImmutableList<IPersistentRepresentation>)message.Payload).ToArray();

                    var journalEntries = persistentMessages.Select(ToJournalEntry).ToList();
                    _journalCollection.AddRange(journalEntries);
                });
            });

            await SetHighSequenceId(messageList);

            return await Task<IImmutableList<Exception>>
                .Factory
                .ContinueWhenAll(writeTasks.ToArray(),
                    tasks => tasks.Select(t => t.IsFaulted ? TryUnwrapException(t.Exception) : null).ToImmutableList());
        }

        protected override Task DeleteMessagesToAsync(string persistenceId, long toSequenceNr)
        {
            return Task.Run(() =>
            {
                _journalCollection.RemoveAll(x => x.PersistenceId == persistenceId && (toSequenceNr == long.MaxValue || x.SequenceNr <= toSequenceNr));
            });
        }

        private JournalEntry ToJournalEntry(IPersistentRepresentation message)
        {
            return new JournalEntry
            {
                Id = message.PersistenceId + "_" + message.SequenceNr,
                IsDeleted = message.IsDeleted,
                Payload = message.Payload,
                PersistenceId = message.PersistenceId,
                SequenceNr = message.SequenceNr,
                Manifest = message.Manifest
            };
        }

        private Persistent ToPersistenceRepresentation(JournalEntry entry, IActorRef sender)
        {
            return new Persistent(entry.Payload, entry.SequenceNr, entry.PersistenceId, entry.Manifest, entry.IsDeleted, sender);
        }

        private async Task SetHighSequenceId(IList<AtomicWrite> messages)
        {
            var persistenceId = messages.Select(c => c.PersistenceId).First();
            var highSequenceId = messages.Max(c => c.HighestSequenceNr);

            await Task.Run(() =>
            {
                var entry = _metadataCollection.FirstOrDefault(x => x.PersistenceId == persistenceId);

                if (entry == null)
                {
                    entry = new MetadataEntry
                    {
                        Id = persistenceId,
                        PersistenceId = persistenceId,
                        SequenceNr = highSequenceId
                    };
                    _metadataCollection.Add(entry);
                }
                else
                {
                    entry.SequenceNr = highSequenceId;
                }
            });
        }
    }
}
