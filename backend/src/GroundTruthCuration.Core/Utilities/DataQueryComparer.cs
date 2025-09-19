using System.ComponentModel.DataAnnotations;
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
        // all properties in dto must be in entity
        if (dto.RequiredProperties.Any(prop => !entity.RequiredPropertiesJson.Contains(prop)))
        {
            return false;
        }

        // no properties in entity that are not in dto
        var entityProperties = JsonNode.Parse(entity.RequiredPropertiesJson)?.AsArray().Select(n => n?.ToString()).ToHashSet();

        if (entityProperties != null && dto.RequiredProperties != null && !entityProperties.SetEquals(dto.RequiredProperties))
            return false;

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