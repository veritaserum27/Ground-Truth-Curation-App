using GroundTruthCuration.Core.Delegates;
using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;
using GroundTruthCuration.Core.Constants;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GroundTruthCuration.Core.Services;

public class DataQueryExecutionService : IDataQueryExecutionService
{
    private readonly ILogger<DataQueryExecutionService> _logger;
    private readonly IDatastoreRepository _manufacturingDataDocDbRepository;
    private readonly IDatastoreRepository _manufacturingDataRelDbRepository;
    private readonly IGroundTruthRepository _groundTruthRepository;

    public DataQueryExecutionService(ILogger<DataQueryExecutionService> logger,
        DatastoreRepositoryResolver datastoreRepositoryResolver, IGroundTruthRepository groundTruthRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (datastoreRepositoryResolver == null)
        {
            throw new ArgumentNullException(nameof(datastoreRepositoryResolver));
        }
        _manufacturingDataDocDbRepository = datastoreRepositoryResolver("ManufacturingDataDocDb");
        _manufacturingDataRelDbRepository = datastoreRepositoryResolver("ManufacturingDataRelDb");
        _groundTruthRepository = groundTruthRepository ?? throw new ArgumentNullException(nameof(groundTruthRepository));
    }
    public async Task ExecuteDataQueriesAsync(GroundTruthDefinition groundTruthDefinition,
        List<DataQueryDefinition> dataQueryDefinitionsToExecute, List<GroundTruthContext> contextsToExecute)
    {
        if (groundTruthDefinition == null)
        {
            throw new ArgumentNullException(nameof(groundTruthDefinition));
        }

        if (dataQueryDefinitionsToExecute == null)
        {
            throw new ArgumentNullException(nameof(dataQueryDefinitionsToExecute));
        }

        if (contextsToExecute == null)
        {
            throw new ArgumentNullException(nameof(contextsToExecute));
        }

        if (dataQueryDefinitionsToExecute.Count == 0)
        {
            _logger.LogInformation("No data queries to execute. Aborting execution.");
            // Nothing to execute
            return;
        }

        if (contextsToExecute.Count == 0)
        {
            _logger.LogInformation("No contexts to execute data queries against. Aborting execution.");
            // Nothing to execute against
            return;
        }

        _logger.LogInformation("Starting execution of {DataQueryCount} data queries for {ContextCount} contexts in GroundTruthDefinition {GroundTruthId}.",
            dataQueryDefinitionsToExecute.Count, contextsToExecute.Count, groundTruthDefinition.GroundTruthId);

        // for dataquery in ground truth definition
        foreach (var dataQueryDefinition in dataQueryDefinitionsToExecute)
        {
            foreach (var context in contextsToExecute)
            {
                var groundTruthEntry = groundTruthDefinition.GroundTruthEntries
                    .Where(e => e.GroundTruthContext != null)
                    .FirstOrDefault(e => e.GroundTruthContext?.ContextId == context.ContextId);

                if (groundTruthEntry == null)
                {
                    // Add as new
                    groundTruthEntry = new GroundTruthEntry
                    {
                        GroundTruthEntryId = Guid.NewGuid(),
                        GroundTruthContext = context,
                        GroundTruthId = groundTruthDefinition.GroundTruthId,
                    };
                }

                // build query parameters based on context parameters
                if (dataQueryDefinition.DatastoreType == DatastoreType.Sql)
                {
                    // Build SQL query parameters
                    var sqlParameters = new DynamicParameters();
                    foreach (var param in context.ContextParameters)
                    {
                        sqlParameters.Add(param.ParameterName, param.ParameterValue);
                    }

                    // Execute query against relational DB
                    if (dataQueryDefinition.DatastoreName != "ManufacturingDataRelDb")
                    {
                        throw new NotSupportedException($"Datastore '{dataQueryDefinition.DatastoreName}' is not supported for SQL queries.");
                    }

                    var results = await _manufacturingDataRelDbRepository.ExecuteQueryAsync(sqlParameters, dataQueryDefinition);

                    // Extract response required values
                    var responseRequiredValues = new List<string>();

                    // Extract required values from results
                    foreach (var result in results)
                    {
                        var dict = result as IDictionary<string, object>;

                        if (dict != null)
                        {
                            var filteredKeys = dict.Keys
                                .Where(key => dataQueryDefinition.RequiredPropertiesJson.Contains(key));

                            foreach (var key in filteredKeys)
                            {
                                responseRequiredValues.Add($"{key}: {dict[key]}");
                            }
                        }
                    }

                    // save to ground truth entry
                    groundTruthEntry.RequiredValuesJson = JsonSerializer.Serialize(responseRequiredValues);
                    groundTruthEntry.RawDataJson = JsonSerializer.Serialize(results);

                    // TODO: Implement save
                    //await _groundTruthRepository.AddOrUpdateGroundTruthEntryAsync(groundTruthEntry);
                }
                else if (dataQueryDefinition.DatastoreType == DatastoreType.CosmosDb)
                {
                    // Build CosmosDB query parameters
                    var cosmosDbParameters = new Dictionary<string, object>();
                    foreach (var param in context.ContextParameters)
                    {
                        cosmosDbParameters[param.ParameterName] = param.ParameterValue;
                    }
                }

                // save ground truth entry

            }
            // Here we would have logic to execute the actual query against the target repository
            // using the context parameters from the specified contexts.
            // This is a placeholder for demonstration purposes.

            // Example pseudo-code:
            // foreach contextId in contextIdsToExecute
            // if in dataQueryIdsToExecute
            // for context in ground truth definition, execute with appropriate context parameters calling the relevant repository

            // Placeholder for executing data queries against their respective datastores
            // This would involve connecting to the datastore, running the query, and processing results
        }
    }
}
