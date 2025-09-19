using System.Text.Json.Nodes;
using GroundTruthCuration.Core.DTOs;
using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;

namespace GroundTruthCuration.Core.Utilities;

public class DataQueryComparer : IGroundTruthComparer<DataQueryDefinitionDto, DataQueryDefinition>
{
    public bool HasSameValues(DataQueryDefinitionDto dto, DataQueryDefinition entity)
    {
        if (dto == null || entity == null)
        {
            return false;
        }

        // compare response required properties
        if (dto.RequiredProperties.Any(prop => !entity.RequiredPropertiesJson.Contains(prop)))
        {
            return false;
        }

        return dto.DataQueryId == entity.DataQueryId &&
               dto.DatastoreName == entity.DatastoreName &&
               dto.DatastoreType == entity.DatastoreType &&
               dto.QueryTarget == entity.QueryTarget &&
               dto.IsFullQuery == entity.IsFullQuery &&
               dto.GroundTruthId == entity.GroundTruthId &&
               dto.IsFullQuery == entity.IsFullQuery &&
               dto.QueryDefinition == entity.QueryDefinition;
    }
}