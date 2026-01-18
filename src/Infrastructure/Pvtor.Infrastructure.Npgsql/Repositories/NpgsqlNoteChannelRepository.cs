using Npgsql;
using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Abstractions.Persistence.Repositories;
using Pvtor.Domain.Notes.Channels;
using Pvtor.Domain.Notes.Namespaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Infrastructure.Npgsql.Repositories;

public class NpgsqlNoteChannelRepository : INoteChannelRepository
{
    private readonly string _connectionString;

    public NpgsqlNoteChannelRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<NoteChannel> AddAsync(NoteChannel channel, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using NpgsqlCommand command = connection.CreateCommand();

        command.CommandText = """
                                INSERT INTO notes_channels (note_source_channel_id, creation_date, note_namespace_id) 
                                VALUES (@note_source_channel_id, @creation_date, @note_namespace_id)
                                RETURNING note_channel_id, note_source_channel_id, creation_date, note_namespace_id;
                              """;

        command.Parameters.AddWithValue("@note_source_channel_id", channel.NoteSourceChannelId);
        command.Parameters.AddWithValue("@creation_date", channel.CreationDate);

        object noteNamespaceId = channel.NoteNamespaceId.HasValue
            ? channel.NoteNamespaceId.Value.Value
            : DBNull.Value;
        command.Parameters.AddWithValue("@note_namespace_id", noteNamespaceId);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return MapFromReader(reader);
        }

        throw new InvalidOperationException("Database failed to return the inserted note channel.");
    }

    public async Task<IEnumerable<NoteChannel>> QueryAsync(
        NoteChannelQuery query,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var parameters = new List<NpgsqlParameter>();
        var commandText =
            new StringBuilder(
                "SELECT note_channel_id, note_source_channel_id, creation_date, note_namespace_id FROM notes_channels");
        var whereConditions = new List<string>();

        if (query.Ids.Length > 0)
        {
            string idsPlaceholders = string.Join(",", query.Ids.Select((_, i) => $"@id{i}"));
            whereConditions.Add($"note_channel_id IN ({idsPlaceholders})");
            parameters.AddRange(query.Ids.Select((id, i) => new NpgsqlParameter($"@id{i}", id.Value)));
        }

        if (query.NoteSourceChannelIds.Length > 0)
        {
            string sourceIdPlaceholders = string.Join(",", query.NoteSourceChannelIds.Select((_, i) => $"@sid{i}"));
            whereConditions.Add($"note_source_channel_id IN ({sourceIdPlaceholders})");
            parameters.AddRange(query.NoteSourceChannelIds.Select((sid, i) => new NpgsqlParameter($"@sid{i}", sid)));
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

        var result = new List<NoteChannel>();
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapFromReader(reader));
        }

        return result;
    }

    public async Task RemoveBySourceChannelIdAsync(string noteSourceChannelId, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using NpgsqlCommand command = connection.CreateCommand();

        command.CommandText = """
                                DELETE FROM notes_channels 
                                WHERE note_source_channel_id = @note_source_channel_id
                              """;

        command.Parameters.AddWithValue("@note_source_channel_id", noteSourceChannelId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private NoteChannel MapFromReader(NpgsqlDataReader reader)
    {
        NoteNamespaceId? namespaceId = reader.IsDBNull(3)
            ? null
            : new NoteNamespaceId(reader.GetInt64(3));

        return new NoteChannel(
            new NoteChannelId(reader.GetInt64(0)),
            reader.GetString(1),
            reader.GetDateTime(2),
            namespaceId);
    }
}