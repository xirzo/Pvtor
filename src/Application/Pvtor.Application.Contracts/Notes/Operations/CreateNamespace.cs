using Pvtor.Application.Contracts.Notes.Models;

namespace Pvtor.Application.Contracts.Notes.Operations;

public static class CreateNamespace
{
    public readonly record struct Request(string Name);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success(NoteNamespaceDto Namespace) : Response;

        public sealed record PersistenceFailure(string Message) : Response;
    }
}
