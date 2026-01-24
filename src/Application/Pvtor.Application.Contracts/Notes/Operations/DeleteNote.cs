namespace Pvtor.Application.Contracts.Notes.Operations;

public static class DeleteNote
{
    public readonly record struct Request(long NoteId);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success : Response;

        public sealed record PersistenceFailure(string Message) : Response;
    }
}