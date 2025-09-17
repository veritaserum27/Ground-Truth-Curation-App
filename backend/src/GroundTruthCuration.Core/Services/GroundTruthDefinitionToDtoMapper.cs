using System.Text.Json;
using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;

namespace GroundTruthCuration.Core.DTOs;

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
            UserCreated = source.UserCreated,
            UserUpdated = source.UserUpdated,
            CreationDateTime = source.CreationDateTime,
            GroundTruthEntries = source.GroundTruthEntries?.Select(entry => new GroundTruthEntryDto
            {
                GroundTruthEntryId = entry.GroundTruthEntryId,
                GroundTruthId = entry.GroundTruthId,
                Context = entry.Context,
                Response = entry.Response,
                RequiredValues = string.IsNullOrWhiteSpace(entry.RequiredValuesJson) ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(entry.RequiredValuesJson) ?? new List<string>(),
                RawData = ConvertToRawDataDto(entry.RawDataJson),
                CreationDateTime = entry.CreationDateTime,
                StartDateTime = entry.StartDateTime,
                EndDateTime = entry.EndDateTime
            }).ToList() ?? new List<GroundTruthEntryDto>(),
            DataQueryDefinitions = source.DataQueryDefinitions?.Select(query => new DataQueryDefinitionDto
            {
                DataQueryId = query.DataQueryId,
                QueryName = query.QueryName,
                QueryString = query.QueryString,
                UserCreated = query.UserCreated,
                CreationDateTime = query.CreationDateTime
            }).ToList() ?? new List<DataQueryDefinitionDto>(),
            Comments = source.Comments?.Select(comment => new CommentDto
            {
                CommentId = comment.CommentId,
                GroundTruthId = comment.GroundTruthId,
                UserComment = comment.UserComment,
                UserCreated = comment.UserCreated,
                CreationDateTime = comment.CreationDateTime
            }).ToList() ?? new List<CommentDto>(),
            Tags = source.Tags?.Select(tag => new TagDto
            {
                TagId = tag.TagId,
                GroundTruthId = tag.GroundTruthId,
                TagName = tag.TagName
            }).ToList() ?? new List<TagDto>()
        };

        return dto;
    }

    private RawDataDto ConvertToRawDataDto(Dictionary<string, object> rawData)
    {
        return new RawDataDto
        {
            DataQueryId = rawData.ContainsKey("DataQueryId") && rawData["DataQueryId"] is Guid id ? id : Guid.Empty,
            RawData = rawData.ContainsKey("RawData") && rawData["RawData"] is List<Dictionary<string, object>> data ? data : new List<Dictionary<string, object>>()
        };
    }

    private RawDataDto Map(string rawDataJson)
    {
        if (string.IsNullOrWhiteSpace(rawDataJson))
        {
            return new RawDataDto
            {
                DataQueryId = Guid.Empty,
                RawData = new List<Dictionary<string, object>>()
            };
        }

        try
        {
            var rawDataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(rawDataJson);
            if (rawDataDict == null)
            {
                return new RawDataDto
                {
                    DataQueryId = Guid.Empty,
                    RawData = new List<Dictionary<string, object>>()
                };
            }

            var dataQueryId = rawDataDict.ContainsKey("DataQueryId") && rawDataDict["DataQueryId"] is JsonElement idElement && idElement.ValueKind == JsonValueKind.String
                ? Guid.Parse(idElement.GetString() ?? string.Empty)
                : Guid.Empty;

            var rawDataList = rawDataDict.ContainsKey("RawData") && rawDataDict["RawData"] is JsonElement dataElement && dataElement.ValueKind == JsonValueKind.Array
                ? JsonSerializer.Deserialize<List<Dictionary<string, object>>>(dataElement.GetRawText()) ?? new List<Dictionary<string, object>>()
                : new List<Dictionary<string, object>>();

            return new RawDataDto
            {
                DataQueryId = dataQueryId,
                RawData = rawDataList
            };
        }
        catch (JsonException)
        {
            _logger.LogError("Failed to deserialize RawDataJson: {RawDataJson}", rawDataJson);
            // Handle JSON deserialization errors if necessary
            return new RawDataDto
            {
                DataQueryId = Guid.Empty,
                RawData = new List<Dictionary<string, object>>()
            };
        }
    }
}

