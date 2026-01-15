using Microsoft.Data.Sqlite;
using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Abstractions.Persistence.Repositories;
using Pvtor.Domain.Notes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Infrastructure.Sqlite.Repositories;

internal sealed class SqliteNoteRepository : INoteRepository
{
    private readonly string _connectionString;

    public SqliteNoteRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Note> AddAsync(Note note, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqliteCommand command = connection.CreateCommand();

        command.CommandText = """
                              INSERT INTO notes (content, creation_date)
                              VALUES ($content, $creation_date);
                              SELECT last_insert_rowid();
                              """;

        command.Parameters.AddWithValue("$content", note.Content);
        command.Parameters.AddWithValue("$creation_date", note.CreationDate);

        long noteId = (long)(await command.ExecuteScalarAsync(cancellationToken)
                             ?? throw new InvalidOperationException("Failed to get note id from insertion"));

        return new Note(new NoteId(noteId), note.Content, note.CreationDate);
    }

    public IEnumerable<Note> Query(NoteQuery query)
    {
        throw new NotImplementedException();
    }
}