namespace Pvtor.Application.Contracts.Notes.Operations;

public static class MarkNoteAsHidden
{
    public readonly record struct Request(long NoteId);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success : Response;

        public sealed record PersistenceFailure(string Message) : Response;

        public sealed record NotFound(string Message) : Response;
    }
}