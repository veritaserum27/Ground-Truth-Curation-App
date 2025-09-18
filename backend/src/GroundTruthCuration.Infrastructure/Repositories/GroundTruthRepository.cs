using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using GroundTruthCuration.Core.DTOs;
using GroundTruthCuration.Core.Constants;
using Microsoft.Extensions.Logging;

namespace GroundTruthCuration.Infrastructure.Repositories;

/// <summary>
/// Repository for managing ground truth definitions and related entities in the database.
/// </summary>
public class GroundTruthRepository : IGroundTruthRepository
{
    private readonly string _connectionString;
    private readonly ILogger<GroundTruthRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroundTruthRepository"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging repository operations.</param>
    /// <param name="configuration">The application configuration containing connection strings.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger"/> or <paramref name="configuration"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the connection string is missing.</exception>
    public GroundTruthRepository(ILogger<GroundTruthRepository> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        _connectionString = _configuration.GetValue<string>("Datastores:GroundTruthCuration:ConnectionString")
                            ?? throw new InvalidOperationException("The connection string 'Datastores:GroundTruthCuration:ConnectionString' is null or missing.");
    }

    /// <inheritdoc/>
    public async Task<DatabaseStatusDto> GetStatusAsync()
    {
        var builder = new SqlConnectionStringBuilder(_connectionString);
        string server = builder.DataSource;
        string database = builder.InitialCatalog;

        // check connectivity to the database
        var status = new DatabaseStatusDto
        {
            ResourceName = server,
            DatabaseName = database,
            DatabaseType = DatastoreType.Sql.ToString(),
            IsConnected = false,
            LastChecked = DateTime.UtcNow
        };

        using (var connection = new SqlConnection(_connectionString))
        {
            try
            {
                await connection.OpenAsync();
                status.IsConnected = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing the SQL query in GetAllGroundTruthDefinitionsAsync.");
                throw new InvalidOperationException("An error occurred while retrieving ground truth definitions. See inner exception for details.", ex);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        return status;
    }

    /// <inheritdoc/>
    public Task<GroundTruthDefinition> AddGroundTruthDefinitionAsync(GroundTruthDefinition groundTruthDefinition)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<IEnumerable<GroundTruthDefinition>> GetAllGroundTruthDefinitionsAsync(GroundTruthDefinitionFilter? filter)
    {
        _logger.LogInformation("Retrieving all ground truth definitions with filter: {@Filter}", filter);
        string sql = baseSql;

        DynamicParameters parameters = BuildSqlParametersFromFilter(filter);

        string whereClause = BuildWhereClauseFromParameters(parameters);
        if (!string.IsNullOrEmpty(whereClause))
        {
            sql += " WHERE " + whereClause;
        }

        sql += ";";

        // connect to database
        using (var connection = new SqlConnection(_connectionString))
        {
            try
            {
                var groundTruthDict = new Dictionary<Guid, GroundTruthDefinition>();

                await connection.QueryAsync<GroundTruthDefinition, GroundTruthEntry, DataQueryDefinition, Comment, Tag, GroundTruthContext, ContextParameter, GroundTruthDefinition>(
                    sql,
                    (gtd, entry, dq, comment, tag, context, contextParam) => MapGroundTruthDefinition(groundTruthDict, gtd, entry, dq, comment, tag, context, contextParam),
                    param: parameters,
                    splitOn: "GroundTruthEntryId,DataQueryId,CommentId,TagId,ContextId,ParameterId"
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

    /// <inheritdoc/>
    public async Task<GroundTruthDefinition?> GetGroundTruthDefinitionByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving ground truth definition with ID: {Id}", id);
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The ground truth ID cannot be an empty GUID.", nameof(id));
        }

        string sql = baseSql + " WHERE gtd.groundTruthId = @id;";

        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var groundTruthDict = new Dictionary<Guid, GroundTruthDefinition>();

                // Leverage Dapper's multi-mapping feature to map related entities to build full object graph
                await connection.QueryAsync<GroundTruthDefinition, GroundTruthEntry, DataQueryDefinition, Comment, Tag, GroundTruthContext, ContextParameter, GroundTruthDefinition>(
                    sql,
                    (gtd, entry, dq, comment, tag, context, contextParam) => MapGroundTruthDefinition(groundTruthDict, gtd, entry, dq, comment, tag, context, contextParam),
                    new { id },
                    splitOn: "GroundTruthEntryId,DataQueryId,CommentId,TagId,ContextId,ParameterId"
                );
                // Should only have one or zero results
                return groundTruthDict.Values.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ground truth definition with ID: {Id}", id);
            throw new InvalidOperationException($"An error occurred while retrieving the ground truth definition with ID {id}.", ex);
        }
    }

    /// <inheritdoc/>
    public Task<IEnumerable<GroundTruthDefinition>> GetGroundTruthDefinitionsByUserAsync(string userId)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<IEnumerable<GroundTruthDefinition>> GetGroundTruthDefinitionsByValidationStatusAsync(string validationStatus)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async Task<GroundTruthDefinition> UpdateGroundTruthDefinitionAsync(GroundTruthDefinition groundTruthDefinition)
    {
        throw new NotImplementedException();
    }

    public async Task AddGroundTruthContextAndRelatedEntitiesAsync(Guid groundTruthId, GroundTruthContext newContext)
    {
        if (groundTruthId == Guid.Empty)
        {
            throw new ArgumentException("The ground truth ID cannot be an empty GUID.", nameof(groundTruthId));
        }
        if (newContext == null)
        {
            throw new ArgumentNullException(nameof(newContext), "The new context cannot be null.");
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var transaction = await connection.BeginTransactionAsync())
            {
                try
                {
                    // Insert empty GroundTruthEntry first
                    string insertEntrySql = @"
                        INSERT INTO [dbo].[GROUND_TRUTH_ENTRY] (groundTruthEntryId, groundTruthId, response)
                        VALUES (@GroundTruthEntryId, @GroundTruthId, @Response);";
                    await connection.ExecuteAsync(insertEntrySql, new
                    {
                        newContext.GroundTruthEntryId,
                        GroundTruthId = groundTruthId,
                        Response = string.Empty // Placeholder response
                    }, transaction);

                    // Insert GroundTruthContext
                    string insertContextSql = @"
                        INSERT INTO [dbo].[GROUND_TRUTH_CONTEXT] (contextId, groundTruthId, groundTruthEntryId, contextType)
                        VALUES (@ContextId, @GroundTruthId, @GroundTruthEntryId, @ContextType);";

                    newContext.ContextId = Guid.NewGuid(); // Ensure a new GUID is assigned
                    await connection.ExecuteAsync(insertContextSql, new
                    {
                        newContext.ContextId,
                        GroundTruthId = groundTruthId,
                        newContext.GroundTruthEntryId,
                        newContext.ContextType
                    }, transaction);

                    // Insert ContextParameters if any
                    if (newContext.ContextParameters != null && newContext.ContextParameters.Any())
                    {
                        string insertParamSql = @"
                            INSERT INTO [dbo].[CONTEXT_PARAMETER] (parameterId, contextId, parameterName, parameterValue, dataType)
                            VALUES (@ParameterId, @ContextId, @ParameterName, @ParameterValue, @DataType);";

                        foreach (var param in newContext.ContextParameters)
                        {
                            param.ParameterId = Guid.NewGuid(); // Ensure a new GUID is assigned
                            await connection.ExecuteAsync(insertParamSql, new
                            {
                                param.ParameterId,
                                newContext.ContextId,
                                param.ParameterName,
                                param.ParameterValue,
                                param.DataType
                            }, transaction);
                        }
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding ground truth context for GroundTruthId: {GroundTruthId}", groundTruthId);
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException($"Failed to add ground truth context for GroundTruthId: {groundTruthId}. See inner exception for details.", ex);
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }
    }

    public async Task DeleteGroundTruthContextsAndRelatedEntitiesAsync(Guid groundTruthId, IEnumerable<Guid> contextIds)
    {
        if (groundTruthId == Guid.Empty)
        {
            throw new ArgumentException("The ground truth ID cannot be an empty GUID.", nameof(groundTruthId));
        }
        if (contextIds == null || !contextIds.Any())
        {
            throw new ArgumentException("The context IDs collection cannot be null or empty.", nameof(contextIds));
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var transaction = await connection.BeginTransactionAsync())
            {
                try
                {
                    // 1. Delete parameters
                    await connection.ExecuteAsync(
                        "DELETE FROM [dbo].[CONTEXT_PARAMETER] WHERE contextId IN @contextIds;",
                        new { contextIds }, transaction);

                    // 2. Delete contexts
                    await connection.ExecuteAsync(
                        "DELETE FROM [dbo].[GROUND_TRUTH_CONTEXT] WHERE contextId IN @contextIds;",
                        new { contextIds }, transaction);

                    // 3. Delete entries
                    await connection.ExecuteAsync(
                        @"DELETE gte
                    FROM [dbo].[GROUND_TRUTH_ENTRY] gte
                    INNER JOIN [dbo].[GROUND_TRUTH_CONTEXT] gtc ON gte.groundTruthEntryId = gtc.groundTruthEntryId
                    WHERE gte.groundTruthId = @groundTruthId AND gtc.contextId IN @contextIds;",
                        new { groundTruthId, contextIds }, transaction);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error removing ground truth entries by context IDs for GroundTruthId: {GroundTruthId}", groundTruthId);
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException($"Failed to remove ground truth entries for GroundTruthId: {groundTruthId}. See inner exception for details.", ex);
                }
                finally
                {
                    await connection.CloseAsync();
                }
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
    /// <param name="context">The GroundTruthContext entity.</param>
    /// <param name="contextParam">The ContextParameter entity.</param>
    /// <returns>The mapped GroundTruthDefinition.</returns>
    private static GroundTruthDefinition MapGroundTruthDefinition(
        Dictionary<Guid, GroundTruthDefinition> groundTruthDict,
        GroundTruthDefinition gtd,
        GroundTruthEntry entry,
        DataQueryDefinition dq,
        Comment comment,
        Tag tag,
        GroundTruthContext context,
        ContextParameter contextParam)
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

        // Map GroundTruthEntry
        GroundTruthEntry entryRef = null;
        if (entry != null && entry.GroundTruthEntryId != Guid.Empty)
        {
            entryRef = groundTruth.GroundTruthEntries.FirstOrDefault(e => e.GroundTruthEntryId == entry.GroundTruthEntryId);
            if (entryRef == null)
            {
                groundTruth.GroundTruthEntries.Add(entry);
                entryRef = entry;
            }
        }

        // Map DataQueryDefinition
        if (dq != null && dq.DataQueryId != Guid.Empty && !groundTruth.DataQueryDefinitions.Any(d => d.DataQueryId == dq.DataQueryId))
        {
            groundTruth.DataQueryDefinitions.Add(dq);
        }
        // Map Comment
        if (comment != null && comment.CommentId != Guid.Empty && !groundTruth.Comments.Any(c => c.CommentId == comment.CommentId))
        {
            groundTruth.Comments.Add(comment);
        }
        // Map Tag
        if (tag != null && tag.TagId != Guid.Empty && !groundTruth.Tags.Any(tg => tg.TagId == tag.TagId))
        {
            groundTruth.Tags.Add(tag);
        }

        // Map GroundTruthContext to GroundTruthEntry (at most one context per entry, only if entry ids match)
        GroundTruthContext contextRef = null;
        if (context != null && context.ContextId != Guid.Empty && entryRef != null)
        {
            // Only set context if it matches the entry's GroundTruthEntryId
            if (context.GroundTruthEntryId == entryRef.GroundTruthEntryId)
            {
                if (entryRef.GroundTruthContext == null || entryRef.GroundTruthContext.ContextId != context.ContextId)
                {
                    entryRef.GroundTruthContext = context;
                }
                contextRef = entryRef.GroundTruthContext;
            }
        }

        // Map ContextParameter to GroundTruthContext
        if (contextParam != null && contextParam.ParameterId != Guid.Empty && contextRef != null)
        {
            if (!contextRef.ContextParameters.Any(p => p.ParameterId == contextParam.ParameterId))
            {
                contextRef.ContextParameters.Add(contextParam);
            }
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
        if (parameters.ParameterNames.Contains("ValidationStatus"))
        {
            whereClauses.Add("gtd.validationStatus = @ValidationStatus");
        }
        if (parameters.ParameterNames.Contains("UserQuery"))
        {
            whereClauses.Add("gtd.userQuery LIKE @UserQuery");
        }
        // Add more mappings as needed for other filters
        return whereClauses.Any() ? string.Join(" AND ", whereClauses) : string.Empty;
    }

    private static DynamicParameters BuildSqlParametersFromFilter(GroundTruthDefinitionFilter? filter)
    {
        var parameters = new DynamicParameters();


        if (filter?.ValidationStatus is not null)
        {
            parameters.Add("ValidationStatus", filter.ValidationStatus);
        }
        if (filter?.UserQuery is not null)
        {
            parameters.Add("UserQuery", $"%{filter.UserQuery}%");
        }

        return parameters;
    }

    private static string baseSql = @"SELECT
        gtd.groundTruthId AS GroundTruthId,
        gtd.userQuery AS UserQuery,
        gtd.validationStatus AS ValidationStatus,
        gtd.category AS Category,
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
        t.description AS Description,

        gtc.contextId AS ContextId,
        gtc.groundTruthId AS GroundTruthId,
        gtc.groundTruthEntryId AS GroundTruthEntryId,
        gtc.contextType AS ContextType,

        cp.parameterId AS ParameterId,
        cp.contextId AS ContextId,
        cp.parameterName AS ParameterName,
        cp.parameterValue AS ParameterValue,
        cp.dataType AS DataType

        FROM [dbo].[GROUND_TRUTH_DEFINITION] gtd
        LEFT JOIN [dbo].[GROUND_TRUTH_ENTRY] gte ON gtd.groundTruthId = gte.groundTruthId
        LEFT JOIN [dbo].[DATA_QUERY_DEFINITION] dqd ON gtd.groundTruthId = dqd.groundTruthId
        LEFT JOIN [dbo].[COMMENT] c ON gtd.groundTruthId = c.groundTruthId
        LEFT JOIN [dbo].[GROUND_TRUTH_TAG] gtdt ON gtd.groundTruthId = gtdt.groundTruthId
        LEFT JOIN [dbo].[TAG] t ON gtdt.tagId = t.tagId
        LEFT JOIN [dbo].[GROUND_TRUTH_CONTEXT] gtc ON gtc.groundTruthEntryId = gte.groundTruthEntryId
        LEFT JOIN [dbo].[CONTEXT_PARAMETER] cp ON cp.contextId = gtc.contextId";
}