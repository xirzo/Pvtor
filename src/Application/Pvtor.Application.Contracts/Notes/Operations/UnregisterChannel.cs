namespace Pvtor.Application.Contracts.Notes.Operations;

public static class UnregisterChannel
{
    public readonly record struct Request(string SourceChannelId);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success : Response;

        public sealed record PersistenceFailure(string Message) : Response;
    }
}