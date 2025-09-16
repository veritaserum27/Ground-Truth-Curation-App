using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using GroundTruthCuration.Core.DTOs;

namespace GroundTruthCuration.Infrastructure.Repositories;

public class GroundTruthRepository : IGroundTruthRepository
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public GroundTruthRepository(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        _connectionString = _configuration.GetConnectionString("GroundTruthConnectionString");

        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException("The connection string 'GroundTruthConnectionString' was not found in the configuration.");
        }
    }

    public Task<GroundTruthDefinition> AddGroundTruthDefinitionAsync(GroundTruthDefinition groundTruthDefinition)
    {
        throw new NotImplementedException();
    }

    public Task DeleteGroundTruthDefinitionAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsGroundTruthDefinitionAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<GroundTruthDefinition>> GetAllGroundTruthDefinitionsAsync(GroundTruthDefinitionFilter filter)
    {
        string baseSql = @"SELECT
        gtd.groundTruthId AS GroundTruthId,
        gtd.userQuery AS UserQuery,
        gtd.validationStatus AS ValidationStatus,
        gtd.userCreated AS UserCreated,
        gtd.userUpdated AS UserUpdated,
        gtd.creationDateTime AS CreationDateTime,
        gtd.startDateTime AS StartDateTime,
        gtd.endDateTime AS EndDateTime,

        gte.groundTruthEntryId AS GroundTruthEntryId,
        gte.groundTruthId AS GroundTruthId,
        gte.response AS Response,
        gte.requiredValuesJSON AS RequiredValuesJson,
        gte.rawDataJSON AS RawDataJson,
        gte.creationDateTime AS CreationDateTime,
        gte.startDateTime AS StartDateTime,
        gte.endDateTime AS EndDateTime,

        dqd.dataQueryId AS DataQueryId,
        dqd.groundTruthId AS GroundTruthId,
        dqd.datastoreType AS DatastoreType,
        dqd.datastoreName AS DatastoreName,
        dqd.queryTarget AS QueryTarget,
        dqd.queryDefinition AS QueryDefinition,
        dqd.isFullQuery AS IsFullQuery,
        dqd.requiredPropertiesJSON AS RequiredPropertiesJson,
        dqd.userCreated AS UserCreated,
        dqd.userUpdated AS UserUpdated,
        dqd.creationDateTime AS CreationDateTime,
        dqd.startDateTime AS StartDateTime,
        dqd.endDateTime AS EndDateTime,

        c.commentId AS CommentId,
        c.groundTruthId AS GroundTruthId,
        c.comment AS CommentText,
        c.commentDateTime AS CommentDateTime,
        c.userId AS UserId,
        c.commentType AS CommentType,

        t.tagId AS TagId,
        t.name AS Name,
        t.description AS Description

        FROM [dbo].[GROUND_TRUTH_DEFINITION] gtd
        LEFT JOIN [dbo].[GROUND_TRUTH_ENTRY] gte ON gtd.groundTruthId = gte.groundTruthId
        LEFT JOIN [dbo].[DATA_QUERY_DEFINITION] dqd ON gtd.groundTruthId = dqd.groundTruthId
        LEFT JOIN [dbo].[COMMENT] c ON gtd.groundTruthId = c.groundTruthId
        LEFT JOIN [dbo].[GROUND_TRUTH_TAG] gtdt ON gtd.groundTruthId = gtdt.groundTruthId
        LEFT JOIN [dbo].[TAG] t ON gtdt.tagId = t.tagId";

        DynamicParameters parameters = new DynamicParameters();

        if (filter.UserId is not null)
        {
            parameters.Add("UserId", filter.UserId);
        }
        if (filter.ValidationStatus is not null)
        {
            parameters.Add("ValidationStatus", filter.ValidationStatus);
        }
        if (filter.CreatedDateRange is not null)
        {
            if (filter.CreatedDateRange?.Start is not null)
            {
                parameters.Add("StartDate", filter.CreatedDateRange?.Start);
            }
            if (filter.CreatedDateRange?.End is not null)
            {
                parameters.Add("EndDate", filter.CreatedDateRange?.End);
            }
        }
        if (filter.GroundTruthContext is not null)
        {
            // TODO: get parameters from context
        }

        if (parameters.ParameterNames.Any())
        {
            var whereClauses = new List<string>();
            if (parameters.ParameterNames.Contains("UserId"))
            {
                whereClauses.Add("gtd.userCreated = @UserId");
            }
            if (parameters.ParameterNames.Contains("ValidationStatus"))
            {
                whereClauses.Add("gtd.validationStatus = @ValidationStatus");
            }
            if (parameters.ParameterNames.Contains("StartDate"))
            {
                whereClauses.Add("gtd.createdAt >= @StartDate");
            }
            if (parameters.ParameterNames.Contains("EndDate"))
            {
                whereClauses.Add("gtd.createdAt <= @EndDate");
            }
            // Add more mappings as needed for other filters
            if (whereClauses.Any())
            {
                baseSql += " WHERE " + string.Join(" AND ", whereClauses);
            }
        }

        baseSql += ";";

        // connect to database
        using (var connection = new SqlConnection(_connectionString))
        {
            try
            {
                var groundTruthDict = new Dictionary<Guid, GroundTruthDefinition>();

                var results = await connection.QueryAsync<GroundTruthDefinition, GroundTruthEntry, DataQueryDefinition, Comment, Tag, GroundTruthDefinition>(
                    baseSql,
                    (gtd, entry, dq, comment, tag) =>
                    {
                        if (!groundTruthDict.TryGetValue(gtd.GroundTruthId, out var groundTruth))
                        {
                            groundTruth = gtd;
                            groundTruth.GroundTruthEntries = new List<GroundTruthEntry>();
                            groundTruth.DataQueryDefinitions = new List<DataQueryDefinition>();
                            groundTruth.Comments = new List<Comment>();
                            groundTruth.Tags = new List<Tag>();
                            groundTruthDict.Add(groundTruth.GroundTruthId, groundTruth);
                        }

                        if (entry != null && entry.GroundTruthEntryId != Guid.Empty && !groundTruth.GroundTruthEntries.Any(e => e.GroundTruthEntryId == entry.GroundTruthEntryId))
                        {
                            groundTruth.GroundTruthEntries.Add(entry);
                        }
                        if (dq != null && dq.DataQueryId != Guid.Empty && !groundTruth.DataQueryDefinitions.Any(d => d.DataQueryId == dq.DataQueryId))
                        {
                            groundTruth.DataQueryDefinitions.Add(dq);
                        }
                        if (comment != null && comment.CommentId != Guid.Empty && !groundTruth.Comments.Any(c => c.CommentId == comment.CommentId))
                        {
                            groundTruth.Comments.Add(comment);
                        }
                        if (tag != null && tag.TagId != Guid.Empty && !groundTruth.Tags.Any(tg => tg.TagId == tag.TagId))
                        {
                            groundTruth.Tags.Add(tag);
                        }

                        return groundTruth;
                    },
                    splitOn: "GroundTruthEntryId,DataQueryId,CommentId,TagId"
                );

                return groundTruthDict.Values;
            }
            catch (Exception ex)
            {
                // Log exception (not implemented here)
                throw new Exception("Error executing SQL query", ex);
            }
        }
    }

    public Task<GroundTruthDefinition?> GetGroundTruthDefinitionByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GroundTruthDefinition>> GetGroundTruthDefinitionsByUserAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GroundTruthDefinition>> GetGroundTruthDefinitionsByValidationStatusAsync(string validationStatus)
    {
        throw new NotImplementedException();
    }

    public Task<GroundTruthDefinition> UpdateGroundTruthDefinitionAsync(GroundTruthDefinition groundTruthDefinition)
    {
        throw new NotImplementedException();
    }
}