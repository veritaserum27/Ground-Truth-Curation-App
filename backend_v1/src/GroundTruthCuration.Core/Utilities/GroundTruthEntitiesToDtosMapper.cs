using System.Text.Json;
using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.DTOs;

namespace GroundTruthCuration.Core.Utilities;

public static class GroundTruthEntitiesToDtosMapper
{
    public static GroundTruthDefinitionDto MapToGroundTruthDefinitionDto(GroundTruthDefinition source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var dto = new GroundTruthDefinitionDto
        {
            GroundTruthId = source.GroundTruthId,
            UserQuery = source.UserQuery,
            ValidationStatus = source.ValidationStatus,
            Category = source.Category,
            UserCreated = source.UserCreated,
            UserUpdated = source.UserUpdated,
            CreationDateTime = source.CreationDateTime,
            GroundTruthEntries = source.GroundTruthEntries?.Select(entry => MapToGroundTruthEntryDto(entry)).ToList() ?? new List<GroundTruthEntryDto>(),
            DataQueryDefinitions = source.DataQueryDefinitions?.Select(query => MapToDataQueryDefinitionDto(query)).ToList() ?? new List<DataQueryDefinitionDto>(),
            Comments = source.Comments?.Select(comment => new CommentDto
            {
                CommentId = comment.CommentId,
                CommentText = comment.CommentText,
                CommentType = comment.CommentType,
                UserCreated = comment.UserId,
                CreationDateTime = comment.CommentDateTime
            }).ToList() ?? new List<CommentDto>(),
            Tags = source.Tags?.Select(tag => new TagDto
            {
                TagId = tag.TagId,
                Name = tag.Name,
                Description = tag.Description
            }).ToList() ?? new List<TagDto>()
        };

        return dto;
    }

    public static GroundTruthEntryDto MapToGroundTruthEntryDto(GroundTruthEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        return new GroundTruthEntryDto
        {
            GroundTruthEntryId = entry.GroundTruthEntryId,
            GroundTruthId = entry.GroundTruthId,
            GroundTruthContext = MapToGroundTruthContextDto(entry.GroundTruthContext),
            Response = entry.Response,
            RequiredValues = string.IsNullOrWhiteSpace(entry.RequiredValuesJson) ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(entry.RequiredValuesJson) ?? new List<string>(),
            RawData = MapToRawDataDtoList(entry.RawDataJson),
            CreationDateTime = entry.CreationDateTime,
        };
    }

    public static ContextParameterDto? MapToContextParameterDto(ContextParameter? parameter)
    {
        if (parameter == null) return null;

        return new ContextParameterDto
        {
            ParameterId = parameter.ParameterId,
            ParameterName = parameter.ParameterName,
            ParameterValue = parameter.ParameterValue,
            DataType = parameter.DataType
        };
    }

    public static GroundTruthContextDto? MapToGroundTruthContextDto(GroundTruthContext? context)
    {
        if (context == null) return null;

        return new GroundTruthContextDto
        {
            ContextId = context.ContextId,
            ContextType = context.ContextType,
            GroundTruthId = context.GroundTruthId,
            GroundTruthEntryId = context.GroundTruthEntryId,
            ContextParameters = context.ContextParameters?
                .Select(param => MapToContextParameterDto(param))
                .Where(mappedParam => mappedParam != null)
                .Cast<ContextParameterDto>()
                .ToList() ?? new List<ContextParameterDto>()
        };
    }

    public static DataQueryDefinitionDto MapToDataQueryDefinitionDto(DataQueryDefinition source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        return new DataQueryDefinitionDto
        {
            DataQueryId = source.DataQueryId,
            QueryDefinition = source.QueryDefinition,
            QueryTarget = source.QueryTarget,
            IsFullQuery = source.IsFullQuery,
            DatastoreName = source.DatastoreName,
            DatastoreType = source.DatastoreType,
            RequiredProperties = string.IsNullOrWhiteSpace(source.RequiredPropertiesJson) ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(source.RequiredPropertiesJson) ?? new List<string>(),
            GroundTruthId = source.GroundTruthId,
            UserCreated = source.UserCreated,
            CreationDateTime = source.CreationDateTime
        };
    }

    public static List<RawDataDto> MapToRawDataDtoList(string rawDataJson)
    {
        var rawDataDtos = new List<RawDataDto>();

        if (string.IsNullOrWhiteSpace(rawDataJson))
        {
            return rawDataDtos;
        }

        try
        {
            var rawDataDictionaries = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(rawDataJson);
            if (rawDataDictionaries == null)
            {
                return rawDataDtos;
            }

            // the raw string should now be a list of dictionaries with DataQueryId and RawData keys
            foreach (var rawDataDict in rawDataDictionaries)
            {

                // Prefer camelCase, fallback to PascalCase for compatibility
                var dataQueryIdKey = rawDataDict.ContainsKey("dataQueryId") ? "dataQueryId" :
                                    rawDataDict.ContainsKey("DataQueryId") ? "DataQueryId" : null;

                var rawDataKey = rawDataDict.ContainsKey("rawData") ? "rawData" :
                                rawDataDict.ContainsKey("RawData") ? "RawData" : null;

                Guid dataQueryId = Guid.Empty;
                if (dataQueryIdKey != null && rawDataDict[dataQueryIdKey] is JsonElement idElement && idElement.ValueKind == JsonValueKind.String)
                {
                    Guid.TryParse(idElement.GetString(), out dataQueryId);
                }

                if (dataQueryId == Guid.Empty)
                {
                    // TODO: handle missing or invalid DataQueryId if necessary
                    continue;
                }

                var rawDataList = new List<Dictionary<string, object>>();
                if (rawDataKey != null && rawDataDict[rawDataKey] is JsonElement dataElement && dataElement.ValueKind == JsonValueKind.Array)
                {
                    rawDataList = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(dataElement.GetRawText()) ?? new List<Dictionary<string, object>>();
                }

                rawDataDtos.Add(new RawDataDto
                {
                    DataQueryId = dataQueryId,
                    RawData = rawDataList
                });
            }
        }
        catch (JsonException)
        {
            // Handle JSON deserialization errors if necessary
            return new List<RawDataDto>
            {
                new RawDataDto
                {
                    DataQueryId = Guid.Empty,
                    RawData = new List<Dictionary<string, object>>()
                }
            };
        }
        return rawDataDtos;
    }
}

