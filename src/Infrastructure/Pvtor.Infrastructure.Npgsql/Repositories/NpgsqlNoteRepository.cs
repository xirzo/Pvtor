using Npgsql;
using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Abstractions.Persistence.Repositories;
using Pvtor.Domain.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Infrastructure.Npgsql.Repositories;

internal sealed class NpgsqlNoteRepository : INoteRepository
{
    private readonly string _connectionString;

    public NpgsqlNoteRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Note> AddAsync(Note note, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using NpgsqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              INSERT INTO notes (content, creation_date, update_date)
                              VALUES (@content, @creation_date, @update_date)
                              RETURNING note_id;
                              """;

        command.Parameters.AddWithValue("@content", note.Content);
        command.Parameters.AddWithValue("@creation_date", note.CreationDate);
        command.Parameters.AddWithValue("@update_date", note.UpdateDate);

        long noteId = (long)(await command.ExecuteScalarAsync(cancellationToken)
                             ?? throw new InvalidOperationException("Failed to get note id from insertion"));

        return new Note(new NoteId(noteId), note.Content, note.CreationDate, note.UpdateDate);
    }

    public async Task<IEnumerable<Note>> QueryAsync(NoteQuery query, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var parameters = new List<NpgsqlParameter>();
        var commandText = new StringBuilder("SELECT note_id, content, creation_date, update_date FROM notes");

        if (query.Ids.Length > 0)
        {
            string placeholders = string.Join(",", query.Ids.Select((_, i) => $"@id{i}"));
            commandText.Append($" WHERE note_id IN ({placeholders})");

            parameters.AddRange(query.Ids.Select((noteId, i) => new NpgsqlParameter($"@id{i}", noteId.Value)));
        }

        await using NpgsqlCommand command = connection.CreateCommand();
#pragma warning disable CA2100
        command.CommandText = commandText.ToString();
#pragma warning restore CA2100
        command.Parameters.AddRange(parameters.ToArray());

        var notes = new List<Note>();
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            notes.Add(MapNoteFromReader(reader));
        }

        return notes;
    }

    public async Task<Note> UpdateAsync(Note note, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using NpgsqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              UPDATE notes 
                              SET content = @content, update_date = @update_date
                              WHERE note_id = @note_id;
                              """;

        command.Parameters.AddWithValue("@content", note.Content);
        command.Parameters.AddWithValue("@update_date", note.UpdateDate);
        command.Parameters.AddWithValue("@note_id", note.NoteId.Value);

        int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

        if (rowsAffected == 0)
        {
            throw new InvalidOperationException($"Note with ID {note.NoteId.Value} not found for update");
        }

        return note;
    }

    private Note MapNoteFromReader(NpgsqlDataReader reader)
    {
        return new Note(
            new NoteId(reader.GetInt64(0)),
            reader.GetString(1),
            reader.GetDateTime(2),
            reader.GetDateTime(3));
    }
}