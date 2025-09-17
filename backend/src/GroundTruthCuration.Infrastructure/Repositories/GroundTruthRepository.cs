using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using GroundTruthCuration.Core.DTOs;
using System.Reflection.Metadata.Ecma335;

namespace GroundTruthCuration.Infrastructure.Repositories;

/// <summary>
/// Repository for managing ground truth definitions and related entities in the database.
/// </summary>
public class GroundTruthRepository : IGroundTruthRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroundTruthRepository"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration containing connection strings.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="configuration"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the connection string is missing.</exception>
    public GroundTruthRepository(IConfiguration configuration)
    {
        IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        _connectionString = _configuration.GetConnectionString("GroundTruthConnectionString")
                            ?? throw new InvalidOperationException("The connection string 'GroundTruthConnectionString' is null or missing.");

        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException("The connection string 'GroundTruthConnectionString' was not found in the configuration.");
        }
    }

    /// <summary>
    /// Adds a new ground truth definition to the database.
    /// </summary>
    /// <param name="groundTruthDefinition">The ground truth definition to add.</param>
    /// <returns>The added ground truth definition.</returns>
    public Task<GroundTruthDefinition> AddGroundTruthDefinitionAsync(GroundTruthDefinition groundTruthDefinition)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Deletes a ground truth definition by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the ground truth definition.</param>
    public Task DeleteGroundTruthDefinitionAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks if a ground truth definition exists by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the ground truth definition.</param>
    /// <returns>True if the definition exists; otherwise, false.</returns>
    public Task<bool> ExistsGroundTruthDefinitionAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves all ground truth definitions, optionally filtered by the provided filter.
    /// </summary>
    /// <param name="filter">Optional filter for querying ground truth definitions.</param>
    /// <returns>A collection of ground truth definitions.</returns>
    public async Task<IEnumerable<GroundTruthDefinition>> GetAllGroundTruthDefinitionsAsync(GroundTruthDefinitionFilter? filter)
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

        DynamicParameters parameters = BuildSqlParametersFromFilter(filter);

        string whereClause = BuildWhereClauseFromParameters(parameters);
        if (!string.IsNullOrEmpty(whereClause))
        {
            baseSql += " WHERE " + whereClause;
        }

        baseSql += ";";

        // connect to database
        using (var connection = new SqlConnection(_connectionString))
        {
            try
            {
                var groundTruthDict = new Dictionary<Guid, GroundTruthDefinition>();

                await connection.QueryAsync<GroundTruthDefinition, GroundTruthEntry, DataQueryDefinition, Comment, Tag, GroundTruthDefinition>(
                    baseSql,
                    (gtd, entry, dq, comment, tag) => MapGroundTruthDefinition(groundTruthDict, gtd, entry, dq, comment, tag),
                    splitOn: "GroundTruthEntryId,DataQueryId,CommentId,TagId"
                );

                return groundTruthDict.Values;
            }
            catch (Exception ex)
            {
                // Log exception (not implemented here)
                throw new InvalidOperationException("An error occurred while executing the SQL query.", ex);
            }
        }
    }

    /// <summary>
    /// Maps the results of a Dapper multi-mapping query to a GroundTruthDefinition and its related entities.
    /// </summary>
    /// <param name="groundTruthDict">Dictionary to track unique GroundTruthDefinition objects.</param>
    /// <param name="gtd">The GroundTruthDefinition entity.</param>
    /// <param name="entry">The GroundTruthEntry entity.</param>
    /// <param name="dq">The DataQueryDefinition entity.</param>
    /// <param name="comment">The Comment entity.</param>
    /// <param name="tag">The Tag entity.</param>
    /// <returns>The mapped GroundTruthDefinition.</returns>
    private static GroundTruthDefinition MapGroundTruthDefinition(
        Dictionary<Guid, GroundTruthDefinition> groundTruthDict,
        GroundTruthDefinition gtd,
        GroundTruthEntry entry,
        DataQueryDefinition dq,
        Comment comment,
        Tag tag)
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
    }

    /// <summary>
    /// Builds a WHERE clause string from the provided Dapper parameters.
    /// </summary>
    /// <param name="parameters">The Dapper DynamicParameters object.</param>
    /// <returns>A WHERE clause string or an empty string if no parameters are present.</returns>
    private static string BuildWhereClauseFromParameters(DynamicParameters parameters)
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
            whereClauses.Add("gtd.creationDateTime >= @StartDate");
        }
        if (parameters.ParameterNames.Contains("EndDate"))
        {
            whereClauses.Add("gtd.creationDateTime <= @EndDate");
        }
        // Add more mappings as needed for other filters
        return whereClauses.Any() ? string.Join(" AND ", whereClauses) : string.Empty;
    }

    /// <summary>
    /// Retrieves a ground truth definition by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the ground truth definition.</param>
    /// <returns>The ground truth definition if found; otherwise, null.</returns>
    public Task<GroundTruthDefinition?> GetGroundTruthDefinitionByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves ground truth definitions created by a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A collection of ground truth definitions created by the user.</returns>
    public Task<IEnumerable<GroundTruthDefinition>> GetGroundTruthDefinitionsByUserAsync(string userId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves ground truth definitions by their validation status.
    /// </summary>
    /// <param name="validationStatus">The validation status to filter by.</param>
    /// <returns>A collection of ground truth definitions with the specified validation status.</returns>
    public Task<IEnumerable<GroundTruthDefinition>> GetGroundTruthDefinitionsByValidationStatusAsync(string validationStatus)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Updates an existing ground truth definition in the database.
    /// </summary>
    /// <param name="groundTruthDefinition">The ground truth definition to update.</param>
    /// <returns>The updated ground truth definition.</returns>
    public Task<GroundTruthDefinition> UpdateGroundTruthDefinitionAsync(GroundTruthDefinition groundTruthDefinition)
    {
        throw new NotImplementedException();
    }

    private static DynamicParameters BuildSqlParametersFromFilter(GroundTruthDefinitionFilter? filter)
    {
        var parameters = new DynamicParameters();

        if (filter?.UserId is not null)
        {
            parameters.Add("UserId", filter.UserId);
        }
        if (filter?.ValidationStatus is not null)
        {
            parameters.Add("ValidationStatus", filter.ValidationStatus);
        }
        if (filter?.CreatedDateRange is not null)
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
        if (filter?.ContextId is not null)
        {
            parameters.Add("ContextId", filter.ContextId);
        }

        return parameters;
    }
}