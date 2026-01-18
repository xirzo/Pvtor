using Pvtor.Application.Contracts.Notes.Models;

namespace Pvtor.Application.Contracts.Notes.Operations;

public static class RegisterChannel
{
    public readonly record struct Request(string SourceChannelId, long? NoteNamespaceId = null);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success(NoteChannelDto Channel) : Response;

        public sealed record PersistenceFailure(string Message) : Response;
    }
}