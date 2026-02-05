using Npgsql;
using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Abstractions.Persistence.Repositories;
using Pvtor.Domain.Notes;
using Pvtor.Domain.Notes.Namespaces;
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
                              INSERT INTO notes (content, creation_date, update_date, note_namespace_id, name)
                              VALUES (@content, @creation_date, @update_date, @note_namespace_id, @name)
                              RETURNING note_id;
                              """;

        command.Parameters.AddWithValue("@content", note.Content);
        command.Parameters.AddWithValue("@creation_date", note.CreationDate);
        command.Parameters.AddWithValue("@update_date", note.UpdateDate);

        object noteName = note.Name is not null
            ? note.Name
            : DBNull.Value;

        command.Parameters.AddWithValue("@name", noteName);

        object noteNamespaceId = note.NoteNamespaceId.HasValue
            ? note.NoteNamespaceId.Value.Value
            : DBNull.Value;

        command.Parameters.AddWithValue("@note_namespace_id", noteNamespaceId);

        long noteId = (long)(await command.ExecuteScalarAsync(cancellationToken)
                             ?? throw new InvalidOperationException("Failed to get note id from insertion"));

        return new Note(
            new NoteId(noteId),
            note.Name,
            note.Content,
            note.CreationDate,
            note.UpdateDate,
            note.NoteNamespaceId,
            note.IsHidden);
    }

    public async Task<IEnumerable<Note>> QueryAsync(NoteQuery query, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var whereConditions = new List<string>();
        var parameters = new List<NpgsqlParameter>();
        var commandText =
            new StringBuilder(
                "SELECT note_id, content, creation_date, update_date, note_namespace_id, is_hidden, name FROM notes");

        if (query.Ids.Length > 0)
        {
            string placeholders = string.Join(",", query.Ids.Select((_, i) => $"@id{i}"));
            whereConditions.Add($"note_id IN ({placeholders})");
            parameters.AddRange(query.Ids.Select((noteId, i) => new NpgsqlParameter($"@id{i}", noteId.Value)));
        }

        var namespaceFilters = new List<string>();

        if (query.NoteNamespaceIds.Length > 0)
        {
            string placeholders = string.Join(",", query.NoteNamespaceIds.Select((_, i) => $"@ns{i}"));
            namespaceFilters.Add($"note_namespace_id IN ({placeholders})");

            parameters.AddRange(query.NoteNamespaceIds.Select((noteNamespaceId, i) =>
                new NpgsqlParameter($"@ns{i}", noteNamespaceId.Value)));
        }

        if (query.UseNullNamespace)
        {
            namespaceFilters.Add("note_namespace_id IS NULL");
        }

        if (namespaceFilters.Count > 0)
        {
            whereConditions.Add($"({string.Join(" OR ", namespaceFilters)})");
        }

        if (query.OnlyNonHidden)
        {
            whereConditions.Add("is_hidden = false");
        }

        if (query.NoteChannelIds.Length > 0)
        {
            string placeholders = string.Join(",", query.NoteChannelIds.Select((_, i) => $"@ch{i}"));

            whereConditions.Add(
                $"note_id IN (SELECT note_id FROM notes_correlations WHERE note_channel_id IN ({placeholders}))");

            parameters.AddRange(query.NoteChannelIds.Select((channelId, i) =>
                new NpgsqlParameter($"@ch{i}", channelId.Value)));
        }

        if (!string.IsNullOrWhiteSpace(query.Content))
        {
            whereConditions.Add(
                "(tsv_content @@ websearch_to_tsquery('russian', @content) OR tsv_content @@ websearch_to_tsquery('english', @content))");
            parameters.Add(new NpgsqlParameter("@content", query.Content));
        }

        if (whereConditions.Count > 0)
        {
            commandText.Append(" WHERE ");
            commandText.Append(string.Join(" AND ", whereConditions));
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
                              SET content = @content, update_date = @update_date, note_namespace_id = @note_namespace_id, is_hidden = @is_hidden
                              WHERE note_id = @note_id;
                              """;

        command.Parameters.AddWithValue("@content", note.Content);
        command.Parameters.AddWithValue("@update_date", note.UpdateDate);

        object noteNamespaceId = note.NoteNamespaceId.HasValue
            ? note.NoteNamespaceId.Value.Value
            : DBNull.Value;

        command.Parameters.AddWithValue("@note_namespace_id", noteNamespaceId);
        command.Parameters.AddWithValue("@is_hidden", note.IsHidden);
        command.Parameters.AddWithValue("@note_id", note.NoteId.Value);

        int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

        if (rowsAffected == 0)
        {
            throw new InvalidOperationException($"Note with ID {note.NoteId.Value} not found for update");
        }

        return note;
    }

    public async Task DeleteAsync(NoteId noteId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using NpgsqlCommand command = connection.CreateCommand();

        command.CommandText = """
                                DELETE FROM notes
                                WHERE note_id = @note_id
                              """;

        command.Parameters.AddWithValue("@note_id", noteId.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private Note MapNoteFromReader(NpgsqlDataReader reader)
    {
        NoteNamespaceId? namespaceId = reader.IsDBNull(4)
            ? null
            : new NoteNamespaceId(reader.GetInt64(4));

        string? name = reader.IsDBNull(6)
            ? null
            : reader.GetString(6);

        return new Note(
            new NoteId(reader.GetInt64(0)),
            name,
            reader.GetString(1),
            reader.GetDateTime(2),
            reader.GetDateTime(3),
            namespaceId,
            reader.GetBoolean(5));
    }
}