using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Domain.Notes.Correlations;

namespace Pvtor.Application.Mapping;

public static class NoteCorrelationMappingExtensions
{
    public static NoteCorrelationDto MapToDto(this NoteCorrelation correlation)
    {
        return new NoteCorrelationDto(
            correlation.NoteCorrelationId.NoteSourceId.Value,
            correlation.NoteCorrelationId.NoteChannelId.Value,
            correlation.NoteId.Value,
            correlation.CreationDate);
    }
}