using Pvtor.Application.Abstractions.Persistence;
using Pvtor.Application.Abstractions.Persistence.Repositories;
using Pvtor.Application.Contracts.Notes.Operations;
using Pvtor.Application.Services;
using Pvtor.Domain.Notes;

namespace Pvtor.Domain.Tests;

public class NoteServiceTests
{
    [Fact]
    public void Create_ValidRequest_ReturnsNote()
    {
        // Arrange
        IPersistanceContext persistenceContext = Substitute.For<IPersistanceContext>();
        INoteRepository noteRepository = Substitute.For<INoteRepository>();
        persistenceContext.NoteRepository.Returns(noteRepository);

        noteRepository.Add(Arg.Any<Note>())
            .Returns(info =>
            {
                Note addedNode = info.Arg<Note>();
                return addedNode;
            });

        var noteService = new NoteService(persistenceContext);
        const string content = "Pvtor";

        // Act
        CreateNote.Response response = noteService.CreateNote(new CreateNote.Request(content));

        // Assert
        Assert.IsType<CreateNote.Response.Success>(response);
        var success = response as CreateNote.Response.Success;
        Assert.NotNull(success);
        Assert.Equal(content, success.Note.Content);
    }
}