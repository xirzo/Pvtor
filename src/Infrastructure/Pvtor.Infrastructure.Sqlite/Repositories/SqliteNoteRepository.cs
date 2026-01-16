using Microsoft.Data.Sqlite;
using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Abstractions.Persistence.Repositories;
using Pvtor.Domain.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public async Task<IEnumerable<Note>> Query(NoteQuery query, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var parameters = new List<SqliteParameter>();
        var commandText = new StringBuilder("SELECT note_id, content, creation_date FROM notes");

        if (query.Ids.Length > 0)
        {
            string placeholders = string.Join(",", query.Ids.Select((_, i) => $"@id{i}"));
            commandText.Append($" WHERE note_id IN ({placeholders})");

            parameters.AddRange(query.Ids.Select((noteId, i) => new SqliteParameter($"@id{i}", noteId)));
        }

        await using SqliteCommand command = connection.CreateCommand();
#pragma warning disable CA2100
        command.CommandText = commandText.ToString();
#pragma warning restore CA2100
        command.Parameters.AddRange(parameters);

        var notes = new List<Note>();
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            notes.Add(MapNoteFromReader(reader));
        }

        return notes;
    }

    private Note MapNoteFromReader(SqliteDataReader reader)
    {
        return new Note(new NoteId(reader.GetInt64(0)), reader.GetString(1), reader.GetDateTime(2));
    }
}