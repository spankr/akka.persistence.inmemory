using Akka.Actor;

namespace Akka.Persistence.InMemory
{
    public class InMemoryPersistenceProvider : ExtensionIdProvider<InMemoryPersistence>
    {
        /// <summary>
        /// Creates an actor system extension for akka persistence InMemory support.
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public override InMemoryPersistence CreateExtension(ExtendedActorSystem system)
        {
            return new InMemoryPersistence(system);
        }
    }
}