using Akka.Persistence.InMemory.Settings;
using Akka.Persistence.Snapshot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Akka.Persistence.InMemory.Snapshot
{
    /// <summary>
    /// A SnapshotStore implementation for writing snapshots in memory.
    /// </summary>
    public class InMemorySnapshotStore : SnapshotStore
    {
        private readonly InMemorySnapshotSettings _settings;

        private List<SnapshotEntry> _snapshotCollection;

        public InMemorySnapshotStore()
        {
            _settings = InMemoryPersistence.Get(Context.System).SnapshotStoreSettings;
        }

        protected override void PreStart()
        {
            base.PreStart();

            _snapshotCollection = InMemoryPersistence.Get(Context.System).SnapshotSource;
        }

        protected override Task<SelectedSnapshot> LoadAsync(string persistenceId, SnapshotSelectionCriteria criteria)
        {
            var filter = CreateRangeFilter(persistenceId, criteria);

            return Task.Run(() =>
            {
                var snapshot = _snapshotCollection.Where(filter).OrderByDescending(x => x.SequenceNr).Take(1).Select(x => ToSelectedSnapshot(x)).FirstOrDefault();
                return snapshot;
            });
        }

        protected override async Task SaveAsync(SnapshotMetadata metadata, object snapshot)
        {
            await Task.Run(() =>
            {
                var snapshotEntry = ToSnapshotEntry(metadata, snapshot);
                var existingSnapshot = _snapshotCollection.FirstOrDefault(CreateSnapshotIdFilter(snapshotEntry.Id));

                if (existingSnapshot != null)
                {
                    existingSnapshot.Snapshot = snapshotEntry.Snapshot;
                    existingSnapshot.Timestamp = snapshotEntry.Timestamp;
                }
                else
                {
                    _snapshotCollection.Add(snapshotEntry);
                }
            });
        }

        protected override Task DeleteAsync(SnapshotMetadata metadata)
        {
            Func<SnapshotEntry, bool> pred = x => x.PersistenceId == metadata.PersistenceId &&
            (metadata.SequenceNr <= 0 || metadata.SequenceNr == long.MaxValue || x.SequenceNr == metadata.SequenceNr) &&
            (metadata.Timestamp == DateTime.MinValue || metadata.Timestamp == DateTime.MaxValue || x.Timestamp == metadata.Timestamp.Ticks);

            return Task.Run(() =>
            {
                var snapshot = _snapshotCollection.FirstOrDefault(pred);
                _snapshotCollection.Remove(snapshot);
            });
        }

        protected override Task DeleteAsync(string persistenceId, SnapshotSelectionCriteria criteria)
        {
            var filter = CreateRangeFilter(persistenceId, criteria);


            return Task.Run(() => { _snapshotCollection.RemoveAll(x => filter(x)); });
        }

        private Func<SnapshotEntry, bool> CreateSnapshotIdFilter(string snapshotId)
        {
            return x => x.Id == snapshotId;
        }

        private Func<SnapshotEntry, bool> CreateRangeFilter(string persistenceId, SnapshotSelectionCriteria criteria)
        {
            return (x => x.PersistenceId == persistenceId &&
            (criteria.MaxSequenceNr <= 0 || criteria.MaxSequenceNr == long.MaxValue || x.SequenceNr <= criteria.MaxSequenceNr) &&
            (criteria.MaxTimeStamp == DateTime.MinValue || criteria.MaxTimeStamp == DateTime.MaxValue || x.Timestamp <= criteria.MaxTimeStamp.Ticks));
        }

        private static SnapshotEntry ToSnapshotEntry(SnapshotMetadata metadata, object snapshot)
        {
            return new SnapshotEntry
            {
                Id = metadata.PersistenceId + "_" + metadata.SequenceNr,
                PersistenceId = metadata.PersistenceId,
                SequenceNr = metadata.SequenceNr,
                Snapshot = snapshot,
                Timestamp = metadata.Timestamp.Ticks
            };
        }

        private static SelectedSnapshot ToSelectedSnapshot(SnapshotEntry entry)
        {
            return new SelectedSnapshot(new SnapshotMetadata(entry.PersistenceId, entry.SequenceNr, new DateTime(entry.Timestamp)), entry.Snapshot);
        }

    }
}
