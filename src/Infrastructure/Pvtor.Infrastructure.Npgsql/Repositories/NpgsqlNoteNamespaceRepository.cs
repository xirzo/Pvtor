using Npgsql;
using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Abstractions.Persistence.Repositories;
using Pvtor.Domain.Notes.Namespaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Infrastructure.Npgsql.Repositories;

public class NpgsqlNoteNamespaceRepository : INoteNamespaceRepository
{
    private readonly string _connectionString;

    public NpgsqlNoteNamespaceRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<NoteNamespace> AddAsync(NoteNamespace noteNamespace)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = """
                              INSERT INTO notes_namespaces (name, creation_date)
                              VALUES (@name, @creation_date)
                              RETURNING note_namespace_id, name, creation_date;
                              """;

        command.Parameters.AddWithValue("@name", noteNamespace.Name);
        command.Parameters.AddWithValue("@creation_date", noteNamespace.CreationDate);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }

        throw new InvalidOperationException("Database failed to return the inserted note namespace.");
    }

    public async Task<IEnumerable<NoteNamespace>> QueryAsync(
        NoteNamespaceQuery query,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var parameters = new List<NpgsqlParameter>();
        var commandText = new StringBuilder("SELECT note_namespace_id, name, creation_date FROM notes_namespaces");
        var whereConditions = new List<string>();

        if (query.NoteNamespaceIds.Length > 0)
        {
            string idsPlaceholders = string.Join(",", query.NoteNamespaceIds.Select((_, i) => $"@id{i}"));
            whereConditions.Add($"note_namespace_id IN ({idsPlaceholders})");
            parameters.AddRange(query.NoteNamespaceIds.Select((id, i) => new NpgsqlParameter($"@id{i}", id.Value)));
        }

        if (query.Names.Length > 0)
        {
            string namePlaceholders = string.Join(",", query.Names.Select((_, i) => $"@name{i}"));
            whereConditions.Add($"name IN ({namePlaceholders})");
            parameters.AddRange(query.Names.Select((name, i) => new NpgsqlParameter($"@name{i}", name)));
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

        var result = new List<NoteNamespace>();
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapFromReader(reader));
        }

        return result;
    }

    private static NoteNamespace MapFromReader(NpgsqlDataReader reader)
    {
        return new NoteNamespace(
            new NoteNamespaceId(reader.GetInt64(0)),
            reader.GetString(1),
            reader.GetDateTime(2));
    }
}