﻿namespace Akka.Persistence.InMemory.Journal
{
    /// <summary>
    /// Class used for storing intermediate result of the <see cref="IPersistentRepresentation"/>
    /// </summary>
    public class JournalEntry
    {
        public string Id { get; set; }

        public string PersistenceId { get; set; }

        public long SequenceNr { get; set; }

        public bool IsDeleted { get; set; }

        public object Payload { get; set; }

        public string Manifest { get; set; }
    }
}
