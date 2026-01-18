namespace Pvtor.Application.Contracts.Notes.Operations;

public static class RecordCorrelation
{
    public readonly record struct Request(long NoteId, string NoteSourceId, long NoteChannelId);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success : Response;

        public sealed record PersistenceFailure(string Message) : Response;
    }
}