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
            -- GroundTruthDefinition
            gtd.groundTruthId AS GroundTruthId,
            gtd.userQuery AS UserQuery,
            gtd.validationStatus AS ValidationStatus,
            gtd.userCreated AS UserCreated,
            gtd.userUpdated AS UserUpdated,
            gtd.creationDateTime AS CreationDateTime,
            gtd.startDateTime AS StartDateTime,
            gtd.endDateTime AS EndDateTime,

            -- GroundTruthEntry
            gte.groundTruthEntryId AS GroundTruthEntryId,
            gte.groundTruthId AS GroundTruthId_Entry,
            gte.response AS Response,
            gte.requiredValuesJSON AS RequiredValuesJson,
            gte.rawDataJSON AS RawDataJson,
            gte.creationDateTime AS CreationDateTime_Entry,
            gte.startDateTime AS StartDateTime_Entry,
            gte.endDateTime AS EndDateTime_Entry,

            -- DataQueryDefinition
            dqd.dataQueryId AS DataQueryId,
            dqd.groundTruthId AS GroundTruthId_DataQuery,
            dqd.datastoreType AS DatastoreType,
            dqd.datastoreName AS DatastoreName,
            dqd.queryTarget AS QueryTarget,
            dqd.queryDefinition AS QueryDefinition,
            dqd.isFullQuery AS IsFullQuery,
            dqd.requiredPropertiesJSON AS RequiredPropertiesJson,
            dqd.userCreated AS UserCreated_DataQuery,
            dqd.userUpdated AS UserUpdated_DataQuery,
            dqd.creationDateTime AS CreationDateTime_DataQuery,
            dqd.startDateTime AS StartDateTime_DataQuery,
            dqd.endDateTime AS EndDateTime_DataQuery,

            -- Comment
            c.commentId AS CommentId,
            c.groundTruthId AS GroundTruthId_Comment,
            c.comment AS CommentText,
            c.commentDateTime AS CommentDateTime,
            c.userId AS UserId,
            c.commentType AS CommentType,

            -- Tag
            t.tagId AS TagId,
            t.name AS Name

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
            baseSql += " WHERE " + string.Join(" AND ", parameters.ParameterNames.Select(name => $"{name} = @{name}"));
        }

        baseSql += ";";

        // connect to database
        using (var connection = new SqlConnection(_connectionString))
        {
            try
            {
                var results = await connection.QueryAsync<GroundTruthDefinition>(baseSql, parameters);
                return results;
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