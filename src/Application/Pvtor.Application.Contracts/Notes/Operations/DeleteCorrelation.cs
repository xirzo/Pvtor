namespace Pvtor.Application.Contracts.Notes.Operations;

public static class DeleteCorrelation
{
    public readonly record struct Request(string CorrelationNoteSourceId, long CorrelationNoteChannelId);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success : Response;

        public sealed record PersistenceFailure(string Message) : Response;
    }
}