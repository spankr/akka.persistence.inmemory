using Akka.Actor;
using Akka.Event;
using Akka.Persistence.InMemory.Tests.Commands;

namespace Akka.Persistence.InMemory.Tests.Actors
{
    /// <summary>
    /// This test actor only persists by the snapshot store mechanism.
    /// </summary>
    public class PersistBySnapshotTestActor : ReceivePersistentActor
    {
        public override string PersistenceId => Context.Self.Path.Name;
        protected readonly ILoggingAdapter _log = Logging.GetLogger(Context);

        private TestActorState _state;

        public PersistBySnapshotTestActor()
        {
            _state = new TestActorState();

            Recover<SnapshotOffer>(offer => {
                _log.Info("SnapshotOffer received.");
                _state = offer.Snapshot as TestActorState;
            });

            Command<SetStateCommand>(cmd => {
                _state.State = cmd.NewState;
                SaveSnapshot(_state);
            });
            Command<GetStateCommand>(cmd => {
                Sender.Tell(_state.State);
            });
        }
    }
}
