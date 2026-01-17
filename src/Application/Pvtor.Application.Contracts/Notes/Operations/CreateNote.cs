using Pvtor.Application.Contracts.Notes.Models;

namespace Pvtor.Application.Contracts.Notes.Operations;

public static class CreateNote
{
    public readonly record struct Request(string Content, string Source);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success(NoteDto Note) : Response;

        public sealed record PersistenceFailure(string Message) : Response;
    }
}