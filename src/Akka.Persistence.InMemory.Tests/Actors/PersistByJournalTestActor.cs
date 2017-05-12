using Akka.Actor;
using Akka.Event;
using Akka.Persistence.InMemory.Tests.Commands;

namespace Akka.Persistence.InMemory.Tests.Actors
{
    /// <summary>
    /// This test actor only persists by the journal mechanism.
    /// </summary>
    public class PersistByJournalTestActor : ReceivePersistentActor
    {
        public override string PersistenceId => Context.Self.Path.Name;
        protected readonly ILoggingAdapter _log = Logging.GetLogger(Context);

        private TestActorState _state;

        public PersistByJournalTestActor()
        {
            _state = new TestActorState();

            Recover<SetStateCommand>(cmd => {
                _log.Info("Recovering SetStateCommand.");
                _state.State = cmd.NewState;
            });

            Recover<SnapshotOffer>(offer => {
                _log.Info("SnapshotOffer received.");
                _state = offer.Snapshot as TestActorState;
            });

            Command<SetStateCommand>(cmd => {
                Persist(cmd, action => {
                    _state.State = cmd.NewState;
                });
            });

            Command<GetStateCommand>(cmd => {
                Sender.Tell(_state.State);
            });
        }
    }
}
