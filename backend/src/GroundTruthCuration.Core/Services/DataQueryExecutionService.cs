using GroundTruthCuration.Core.Delegates;
using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;
using GroundTruthCuration.Core.Constants;
using GroundTruthCuration.Core.Utilities;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using GroundTruthCuration.Core.DTOs;
using System.Text;

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
        List<DataQueryDefinition> dataQueryDefinitionsUnchanged,
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

            await processDataQueriesForContext(dataQueryDefinitionsUnchanged, dataQueryDefinitionsToExecute, context,
                   groundTruthEntry);
        }
    }

    public async Task ExecuteDataQueriesByGroundTruthIdAsync(Guid groundTruthDefinitionId)
    {
        var groundTruthDefinition = await _groundTruthRepository.GetGroundTruthDefinitionByIdAsync(groundTruthDefinitionId);
        if (groundTruthDefinition == null)
        {
            throw new ArgumentException($"Ground truth definition with ID {groundTruthDefinitionId} not found.");
        }

        var contexts = groundTruthDefinition.GroundTruthEntries
            .Where(e => e.GroundTruthContext != null)
            .Select(e => e.GroundTruthContext!)
            .ToList();

        foreach (var context in contexts)
        {
            await processDataQueriesForContext(new List<DataQueryDefinition>(), groundTruthDefinition.DataQueryDefinitions.ToList(), context,
                groundTruthDefinition.GroundTruthEntries
                .First(e => e.GroundTruthContext?.ContextId == context.ContextId));
        }
    }

    private async Task processDataQueriesForContext(List<DataQueryDefinition> dataQueryDefinitionsUnchanged, List<DataQueryDefinition> dataQueryDefinitions,
        GroundTruthContext context, GroundTruthEntry groundTruthEntry)
    {
        var totalRecordCount = 0;
        var aggregatedResults = new List<object>();
        var interimResponse = new StringBuilder();
        var responseRequiredValues = new HashSet<string>();

        // Don't overwrite existing data if data query definition is unchanged, something with empty guid right now
        if (!string.IsNullOrEmpty(groundTruthEntry.RawDataJson))
        {
            interimResponse.AppendLine();
        }

        var groundTruthEntryDto = GroundTruthEntitiesToDtosMapper.MapToGroundTruthEntryDto(groundTruthEntry);

        if (groundTruthEntryDto == null)
        {
            throw new InvalidOperationException($"Failed to map GroundTruthEntry with ID {groundTruthEntry.GroundTruthEntryId} to DTO.");
        }

        // populate with existing values that won't be executed
        var dataQueryIdsUnchanged = dataQueryDefinitionsUnchanged.Select(dq => dq.DataQueryId).ToHashSet();

        var existingRawData = groundTruthEntryDto.RawData
            .Where(rd => dataQueryIdsUnchanged.Contains(rd.DataQueryId))
            .ToList();

        if (existingRawData.Count > 0 && dataQueryDefinitionsUnchanged.Count > 0)
        {
            totalRecordCount += existingRawData.Sum(rd => rd.RawData?.Count ?? 0);
            aggregatedResults.AddRange(existingRawData);
            interimResponse.AppendLine($"Retained {totalRecordCount} existing raw data records for {dataQueryDefinitionsUnchanged.Count} unchanged data query definition(s).");

            // for each data query definition that is unchanged, extract required values from existing raw data
            foreach (var dataQueryDefinition in dataQueryDefinitionsUnchanged)
            {
                var extractedValues = extractRequiredValuesFromResultsList(
                    existingRawData
                    .Where(rd => rd.DataQueryId == dataQueryDefinition.DataQueryId)
                    .SelectMany(rd => rd.RawData)
                    .ToList(),
                    dataQueryDefinition);
                responseRequiredValues.UnionWith(extractedValues);
                interimResponse.AppendLine($"Existing required value(s) retained: {string.Join(", ", extractedValues)}.");
            }
        }

        foreach (var dataQueryDefinition in dataQueryDefinitions)
        {
            // Build query parameters based on context parameters
            if (dataQueryDefinition.DatastoreType == DatastoreType.Sql)
            {
                // Build SQL query parameters
                var sqlParameters = buildSqlParameters(context);

                // Execute query against relational DB
                if (!string.Equals(dataQueryDefinition.DatastoreName, "ManufacturingDataRelDb",
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new NotSupportedException($"Datastore '{dataQueryDefinition.DatastoreName}' is not supported for SQL queries.");
                }

                var results = await _manufacturingDataRelDbRepository.ExecuteQueryAsync(sqlParameters, dataQueryDefinition);

                totalRecordCount += results.Count;
                var newRawData = new RawDataDto
                {
                    DataQueryId = dataQueryDefinition.DataQueryId,
                    RawData = results
                        .Select(r => r is Dictionary<string, object> dict
                            ? dict
                            : r is IDictionary<string, object> idict
                                ? new Dictionary<string, object>(idict)
                                : r.GetType().GetProperties().ToDictionary(
                                    prop => prop.Name,
                                    prop => prop.GetValue(r) ?? new object()))
                        .ToList()
                };
                aggregatedResults.Add(newRawData);
                // Extract required values from results
                var extractedValues = extractRequiredValuesFromResultsList(newRawData.RawData.ToList(), dataQueryDefinition);
                responseRequiredValues.UnionWith(extractedValues);

                interimResponse.AppendLine($"Retrieved {results.Count} {dataQueryDefinition.DatastoreType} records from {dataQueryDefinition.DatastoreName}. It includes the following required values: {string.Join(", ", extractedValues)}");
            }
            else if (dataQueryDefinition.DatastoreType == DatastoreType.CosmosDb)
            {
                // TODO: build query string with parameters, incorporate data types
                // Build document query parameters
                var queryParameters = new Dictionary<string, string>();
                foreach (var param in context.ContextParameters)
                {
                    queryParameters[param.ParameterName] = param.ParameterValue;
                }

                // Execute query against document DB
                if (!string.Equals(dataQueryDefinition.DatastoreName, "ManufacturingDataDocDb",
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new NotSupportedException($"Datastore '{dataQueryDefinition.DatastoreName}' is not supported for Document queries.");
                }

                var results = await _manufacturingDataDocDbRepository.ExecuteQueryAsync(queryParameters, dataQueryDefinition);

                totalRecordCount += results.Count;
                var newRawData = new RawDataDto
                {
                    DataQueryId = dataQueryDefinition.DataQueryId,
                    RawData = results.Cast<Dictionary<string, object>>().ToList()
                };
                aggregatedResults.Add(newRawData);

                var extractedValues = extractRequiredValuesFromResultsList(newRawData.RawData.ToList(), dataQueryDefinition);
                responseRequiredValues.UnionWith(extractedValues);
                interimResponse.AppendLine($"Retrieved {results.Count} {dataQueryDefinition.DatastoreType} records from {dataQueryDefinition.DatastoreName}. It includes the following required values: {string.Join(", ", extractedValues)}");
            }
            // save to ground truth entry
            groundTruthEntry.RequiredValuesJson = JsonSerializer.Serialize(responseRequiredValues);
            groundTruthEntry.RawDataJson = JsonSerializer.Serialize(aggregatedResults);
            groundTruthEntry.Response = interimResponse.ToString();
            await _groundTruthRepository.AddOrUpdateGroundTruthEntryAsync(groundTruthEntry);
            _logger.LogInformation("Completed processing data queries for context {ContextId}. Aggregated {TotalResults} results.",
                context.ContextId, totalRecordCount);
        }
    }

    private HashSet<string> extractRequiredValuesFromResultsList(List<Dictionary<string, object>> results, DataQueryDefinition dataQueryDefinition)
    {
        var responseRequiredValues = new HashSet<string>();

        if (results == null || results.Count == 0 || String.IsNullOrEmpty(dataQueryDefinition.RequiredPropertiesJson))
        {
            return responseRequiredValues;
        }

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
                    responseRequiredValues.Add(dict[key]?.ToString() ?? string.Empty);
                }
            }
        }
        return responseRequiredValues;
    }

    private static DynamicParameters buildSqlParameters(GroundTruthContext context)
    {
        var sqlParameters = new DynamicParameters();
        foreach (var param in context.ContextParameters)
        {
            if (String.Equals(param.DataType, "integer", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(param.ParameterValue, out int intValue))
            {
                sqlParameters.Add(param.ParameterName, intValue);
            }
            else if (String.Equals(param.DataType, "float", StringComparison.OrdinalIgnoreCase)
                && double.TryParse(param.ParameterValue, out double doubleValue))
            {
                sqlParameters.Add(param.ParameterName, doubleValue);
            }
            else if (String.Equals(param.DataType, "boolean", StringComparison.OrdinalIgnoreCase)
                && bool.TryParse(param.ParameterValue, out bool boolValue))
            {
                sqlParameters.Add(param.ParameterName, boolValue);
            }
            else
            {
                // Default to string
                sqlParameters.Add(param.ParameterName, param.ParameterValue);
            }
            sqlParameters.Add(param.ParameterName, param.ParameterValue);
        }
        return sqlParameters;
    }
}
