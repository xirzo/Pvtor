using Npgsql;
using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Abstractions.Persistence.Repositories;
using Pvtor.Domain.Notes.Channels;
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

    public async Task<IEnumerable<NoteChannel>> AddAsync(
        NoteChannelQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.NoteSourceChannelIds.Length == 0)
        {
            return Enumerable.Empty<NoteChannel>();
        }

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var parameters = new List<NpgsqlParameter>();
        var valuesClauses = new List<string>();
        DateTime now = DateTime.UtcNow;

        for (int i = 0; i < query.NoteSourceChannelIds.Length; i++)
        {
            string pName = $"@p{i}";
            string dName = $"@d{i}";
            valuesClauses.Add($"({pName}, {dName})");
            parameters.Add(new NpgsqlParameter(pName, query.NoteSourceChannelIds[i]));
            parameters.Add(new NpgsqlParameter(dName, now));
        }

        await using NpgsqlCommand command = connection.CreateCommand();

#pragma warning disable CA2100
        command.CommandText = $"""
                               INSERT INTO notes_channels (note_source_channel_id, creation_date) 
                               VALUES {string.Join(", ", valuesClauses)}
                               RETURNING note_channel_id, note_source_channel_id, creation_date;
                               """;
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

    public async Task<IEnumerable<NoteChannel>> QueryAsync(
        NoteChannelQuery query,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var parameters = new List<NpgsqlParameter>();
        var commandText =
            new StringBuilder("SELECT note_channel_id, note_source_channel_id, creation_date FROM notes_channels");
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

    private NoteChannel MapFromReader(NpgsqlDataReader reader)
    {
        return new NoteChannel(
            new NoteChannelId(reader.GetInt64(0)),
            reader.GetString(1),
            reader.GetDateTime(2));
    }
}