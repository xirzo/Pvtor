using Microsoft.Data.Sqlite;
using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Abstractions.Persistence.Repositories;
using Pvtor.Domain.Notes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Infrastructure.Sqlite.Repositories;

public class SqliteNoteCorrelationRepository : INoteCorrelationRepository
{
    private readonly string _connectionString;

    public SqliteNoteCorrelationRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task AddAsync(
        NoteCorrelation noteCorrelation,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using SqliteCommand command = connection.CreateCommand();

        command.CommandText = """
                                INSERT INTO note_correlations (note_id, note_source_id, creation_date) 
                                VALUES ($note_id,  $note_source_id, $creation_date);
                              """;

        command.Parameters.AddWithValue("$note_id", noteCorrelation.NoteCorrelationId.NoteId.Value);
        command.Parameters.AddWithValue("$note_source_id", noteCorrelation.NoteCorrelationId.NoteSourceId.Value);
        command.Parameters.AddWithValue("$creation_date", noteCorrelation.CreationDate);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IEnumerable<NoteCorrelation>> QueryAsync(
        NoteCorrelationQuery query,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var parameters = new List<SqliteParameter>();
        var commandText = new StringBuilder("SELECT note_id, note_source_id, creation_date FROM note_correlations");
        var whereConditions = new List<string>();

        if (query.NoteIds.Length > 0)
        {
            string noteIdPlaceholders = string.Join(",", query.NoteIds.Select((_, i) => $"@noteId{i}"));
            whereConditions.Add($"note_id IN ({noteIdPlaceholders})");
            parameters.AddRange(query.NoteIds.Select((noteId, i) =>
                new SqliteParameter($"@noteId{i}", noteId.Value)));
        }

        if (query.NoteSourceIds.Length > 0)
        {
            string sourceIdPlaceholders = string.Join(",", query.NoteSourceIds.Select((_, i) => $"@sourceId{i}"));
            whereConditions.Add($"note_source_id IN ({sourceIdPlaceholders})");
            parameters.AddRange(query.NoteSourceIds.Select((sourceId, i) =>
                new SqliteParameter($"@sourceId{i}", sourceId.Value)));
        }

        if (whereConditions.Count > 0)
        {
            commandText.Append(" WHERE ");
            commandText.Append(string.Join(" AND ", whereConditions));
        }

        await using SqliteCommand command = connection.CreateCommand();
#pragma warning disable CA2100
        command.CommandText = commandText.ToString();
#pragma warning restore CA2100
        command.Parameters.AddRange(parameters);

        var correlations = new List<NoteCorrelation>();
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            correlations.Add(MapNoteCorrelationFromReader(reader));
        }

        return correlations;
    }

    private NoteCorrelation MapNoteCorrelationFromReader(SqliteDataReader reader)
    {
        return new NoteCorrelation(
            new NoteCorrelationId(
                new NoteSourceId(reader.GetString(1)),
                new NoteId(reader.GetInt64(0))),
            reader.GetDateTime(2));
    }
}