using Microsoft.Data.Sqlite;
using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Abstractions.Persistence.Repositories;
using Pvtor.Domain.Notes;
using System.Collections.Generic;
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

    public Task<IEnumerable<NoteCorrelation>> QueryAsync(
        NoteCorrelationQuery query,
        CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
}