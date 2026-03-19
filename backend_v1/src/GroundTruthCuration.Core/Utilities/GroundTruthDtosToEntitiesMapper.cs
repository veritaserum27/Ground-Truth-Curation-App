using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.DTOs;
using System.Text.Json;

namespace GroundTruthCuration.Core.Utilities;

public static class GroundTruthDtosToEntitiesMapper
{
    /// <summary>
    /// Maps a DataQueryDefinitionDto to a DataQueryDefinition entity.
    /// </summary>
    /// <param name="dataQueryDto"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static DataQueryDefinition MapToDataQueryDefinition(DataQueryDefinitionDto dataQueryDto)
    {
        if (dataQueryDto == null)
        {
            throw new ArgumentNullException(nameof(dataQueryDto));
        }

        return new DataQueryDefinition
        {
            DataQueryId = dataQueryDto.DataQueryId.HasValue && dataQueryDto.DataQueryId.Value != Guid.Empty
                ? dataQueryDto.DataQueryId.Value
                : Guid.NewGuid(),
            GroundTruthId = dataQueryDto.GroundTruthId ?? Guid.NewGuid(),
            DatastoreType = dataQueryDto.DatastoreType,
            DatastoreName = dataQueryDto.DatastoreName,
            QueryDefinition = dataQueryDto.QueryDefinition,
            QueryTarget = dataQueryDto.QueryTarget,
            IsFullQuery = dataQueryDto.IsFullQuery,
            RequiredPropertiesJson = JsonSerializer.Serialize(dataQueryDto.RequiredProperties),
            UserCreated = dataQueryDto.UserCreated ?? string.Empty,
            UserUpdated = dataQueryDto.UserUpdated ?? string.Empty,
        };
    }
}
