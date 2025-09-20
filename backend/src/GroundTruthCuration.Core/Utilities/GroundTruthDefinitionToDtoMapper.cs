using System.Text.Json;
using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;
using Microsoft.Extensions.Logging;
using GroundTruthCuration.Core.DTOs;

namespace GroundTruthCuration.Core.Utilities;

public class GroundTruthDefinitionToDtoMapper : IGroundTruthMapper<GroundTruthDefinition, GroundTruthDefinitionDto>
{
    private readonly ILogger<GroundTruthDefinitionToDtoMapper> _logger;
    public GroundTruthDefinitionToDtoMapper(ILogger<GroundTruthDefinitionToDtoMapper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public GroundTruthDefinitionDto Map(GroundTruthDefinition source)
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
            GroundTruthEntries = source.GroundTruthEntries?.Select(entry => new GroundTruthEntryDto
            {
                GroundTruthEntryId = entry.GroundTruthEntryId,
                GroundTruthId = entry.GroundTruthId,
                GroundTruthContext = Map(entry.GroundTruthContext),
                Response = entry.Response,
                RequiredValues = string.IsNullOrWhiteSpace(entry.RequiredValuesJson) ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(entry.RequiredValuesJson) ?? new List<string>(),
                RawData = Map(entry.RawDataJson),
                CreationDateTime = entry.CreationDateTime,
            }).ToList() ?? new List<GroundTruthEntryDto>(),
            DataQueryDefinitions = source.DataQueryDefinitions?.Select(query => Map(query)).ToList() ?? new List<DataQueryDefinitionDto>(),
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

    public ContextParameterDto? Map(ContextParameter? parameter)
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

    public GroundTruthContextDto? Map(GroundTruthContext? context)
    {
        if (context == null) return null;

        return new GroundTruthContextDto
        {
            ContextId = context.ContextId,
            ContextType = context.ContextType,
            GroundTruthId = context.GroundTruthId,
            GroundTruthEntryId = context.GroundTruthEntryId,
            ContextParameters = context.ContextParameters?
                .Select(param => Map(param))
                .Where(mappedParam => mappedParam != null)
                .Cast<ContextParameterDto>()
                .ToList() ?? new List<ContextParameterDto>()
        };
    }

    public DataQueryDefinitionDto Map(DataQueryDefinition source)
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

    public List<RawDataDto> Map(string rawDataJson)
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
                _logger.LogInformation("Deserialized RawData entry. Keys: {Keys}", string.Join(", ", rawDataDict.Keys));


                var dataQueryId = rawDataDict.ContainsKey("dataQueryId") && rawDataDict["dataQueryId"] is JsonElement idElement && idElement.ValueKind == JsonValueKind.String
                    ? Guid.Parse(idElement.GetString() ?? string.Empty)
                    : Guid.Empty;

                var rawDataList = rawDataDict.ContainsKey("rawData") && rawDataDict["rawData"] is JsonElement dataElement && dataElement.ValueKind == JsonValueKind.Array
                    ? JsonSerializer.Deserialize<List<Dictionary<string, object>>>(dataElement.GetRawText()) ?? new List<Dictionary<string, object>>()
                    : new List<Dictionary<string, object>>();

                rawDataDtos.Add(new RawDataDto
                {
                    DataQueryId = dataQueryId,
                    RawData = rawDataList
                });
            }
        }
        catch (JsonException)
        {
            _logger.LogError("Failed to deserialize RawDataJson: {RawDataJson}", rawDataJson);
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

