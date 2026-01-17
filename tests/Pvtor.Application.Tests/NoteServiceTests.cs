using Pvtor.Application.Abstractions;
using Pvtor.Application.Abstractions.Persistence;
using Pvtor.Application.Abstractions.Persistence.Repositories;
using Pvtor.Application.Contracts.Notes.Operations;
using Pvtor.Application.Services;
using Pvtor.Domain.Notes;
using System.Threading.Tasks;

namespace Pvtor.Domain.Tests;

public class NoteServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsNote()
    {
        // Arrange
        IPersistanceContext persistenceContext = Substitute.For<IPersistanceContext>();
        INoteRepository noteRepository = Substitute.For<INoteRepository>();
        INoteCorrelationRecorder noteCorrelationRecorder = Substitute.For<INoteCorrelationRecorder>();
        persistenceContext.NoteRepository.Returns(noteRepository);

        noteRepository.AddAsync(Arg.Any<Note>())
            .Returns(info =>
            {
                Note addedNode = info.Arg<Note>();
                return addedNode;
            });

        var noteService = new NoteService(persistenceContext, noteCorrelationRecorder);
        const string content = "Pvtor";

        // Act
        CreateNote.Response response = await noteService.CreateNoteAsync(new CreateNote.Request(content, string.Empty));

        // Assert
        Assert.IsType<CreateNote.Response.Success>(response);
        var success = response as CreateNote.Response.Success;
        Assert.NotNull(success);
        Assert.Equal(content, success.Note.Content);
    }
}