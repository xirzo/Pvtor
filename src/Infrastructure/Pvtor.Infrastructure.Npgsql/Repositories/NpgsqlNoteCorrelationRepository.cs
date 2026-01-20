using Npgsql;
using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Abstractions.Persistence.Repositories;
using Pvtor.Domain.Notes;
using Pvtor.Domain.Notes.Channels;
using Pvtor.Domain.Notes.Correlations;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Infrastructure.Npgsql.Repositories;

public class NpgsqlNoteCorrelationRepository : INoteCorrelationRepository
{
    private readonly string _connectionString;

    public NpgsqlNoteCorrelationRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task AddAsync(
        NoteCorrelation noteCorrelation,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using NpgsqlCommand command = connection.CreateCommand();

        command.CommandText = """
                                INSERT INTO notes_correlations (note_source_id, note_channel_id, note_id, creation_date) 
                                VALUES (@note_source_id, @note_channel_id, @note_id, @creation_date);
                              """;

        command.Parameters.AddWithValue("@note_source_id", noteCorrelation.NoteCorrelationId.NoteSourceId.Value);
        command.Parameters.AddWithValue("@note_channel_id", noteCorrelation.NoteCorrelationId.NoteChannelId.Value);
        command.Parameters.AddWithValue("@note_id", noteCorrelation.NoteId.Value);
        command.Parameters.AddWithValue("@creation_date", noteCorrelation.CreationDate);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IEnumerable<NoteCorrelation>> QueryAsync(
        NoteCorrelationQuery query,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var parameters = new List<NpgsqlParameter>();
        var commandText =
            new StringBuilder(
                "SELECT note_source_id, note_channel_id, note_id, creation_date FROM notes_correlations");
        var whereConditions = new List<string>();

        if (query.NoteIds.Length > 0)
        {
            string noteIdPlaceholders = string.Join(",", query.NoteIds.Select((_, i) => $"@noteId{i}"));
            whereConditions.Add($"note_id IN ({noteIdPlaceholders})");
            parameters.AddRange(query.NoteIds.Select((noteId, i) =>
                new NpgsqlParameter($"@noteId{i}", noteId.Value)));
        }

        if (query.NoteSourceIds.Length > 0)
        {
            string sourceIdPlaceholders = string.Join(",", query.NoteSourceIds.Select((_, i) => $"@sourceId{i}"));
            whereConditions.Add($"note_source_id IN ({sourceIdPlaceholders})");
            parameters.AddRange(query.NoteSourceIds.Select((sourceId, i) =>
                new NpgsqlParameter($"@sourceId{i}", sourceId.Value)));
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

        var correlations = new List<NoteCorrelation>();
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            correlations.Add(MapNoteCorrelationFromReader(reader));
        }

        return correlations;
    }

    public async Task DeleteAsync(NoteCorrelationId correlationId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using NpgsqlCommand command = connection.CreateCommand();

        command.CommandText = """
                                DELETE FROM notes_correlations
                                WHERE note_source_id = @note_source_id
                                AND note_channel_id = @note_channel_id
                              """;

        command.Parameters.AddWithValue("@note_source_id", correlationId.NoteSourceId.Value);
        command.Parameters.AddWithValue("@note_channel_id", correlationId.NoteChannelId.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private NoteCorrelation MapNoteCorrelationFromReader(NpgsqlDataReader reader)
    {
        return new NoteCorrelation(
            new NoteCorrelationId(
                new NoteSourceId(reader.GetString(0)),
                new NoteChannelId(reader.GetInt64(1))),
            new NoteId(reader.GetInt64(2)),
            reader.GetDateTime(3));
    }
}