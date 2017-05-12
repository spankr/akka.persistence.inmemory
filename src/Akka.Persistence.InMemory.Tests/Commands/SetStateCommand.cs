namespace Akka.Persistence.InMemory.Tests.Commands
{
    public class SetStateCommand
    {
        public string NewState { get; private set; }

        public SetStateCommand(string newState)
        {
            NewState = newState;
        }
    }
}
