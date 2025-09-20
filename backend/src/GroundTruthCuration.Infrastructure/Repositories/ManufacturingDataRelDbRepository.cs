using Dapper;
using GroundTruthCuration.Core.Constants;
using GroundTruthCuration.Core.DTOs;
using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GroundTruthCuration.Infrastructure.Repositories;

/// <summary>
/// Repository for interacting with the Manufacturing Data RelationalDB SQL Server datastore.
/// </summary>
public class ManufacturingDataRelDbRepository : IDatastoreRepository
{
    private readonly string _connectionString;
    private readonly ILogger<ManufacturingDataRelDbRepository> _logger;

    public ManufacturingDataRelDbRepository(ILogger<ManufacturingDataRelDbRepository> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        _connectionString = _configuration.GetValue<string>("Datastores:ManufacturingDataRelDb:ConnectionString")
                            ?? throw new InvalidOperationException("The connection string 'Datastores:ManufacturingDataRelDb:ConnectionString' is null or missing.");
    }

    public async Task<ICollection<object>> ExecuteQueryAsync<T>(T parameters, DataQueryDefinition dataQueryDefinition)
    {
        if (dataQueryDefinition == null)
        {
            throw new ArgumentNullException(nameof(dataQueryDefinition));
        }

        var query = buildQueryFromDataQueryDefinition(dataQueryDefinition);
        using (var connection = new SqlConnection(_connectionString))
        {
            try
            {
                await connection.OpenAsync();
                if (EqualityComparer<T>.Default.Equals(parameters, default(T)))
                {
                    var queryResults = await connection.QueryAsync<object>(query);
                    return queryResults.ToList();
                }

                // case to DynamicParameters to support optional parameters
                if (parameters is not DynamicParameters)
                {
                    parameters = (T)(object)new DynamicParameters(parameters);
                }

                var results = await connection.QueryAsync<object>(dataQueryDefinition.QueryDefinition, parameters);
                return results.ToList();
            }
            catch (Exception ex)
            {
                var contextualMessage = $"Error executing query: {query} with parameters: {parameters}.";
                _logger.LogError(ex, contextualMessage);
                throw new InvalidOperationException(contextualMessage, ex);
            }
        }
    }

    /// <inheritdoc/>
    public Task<DatastoreDetailsDto> GetDatastoreDetailsAsync()
    {
        throw new NotImplementedException();
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
                _logger.LogError(ex, "Error connecting to the database");
                status.IsConnected = false;
            }
        }

        return status;
    }

    private static string buildQueryFromDataQueryDefinition(DataQueryDefinition dataQueryDefinition)
    {
        // In a real implementation, this method would construct the SQL query string
        // based on the details in the DataQueryDefinition object.
        // For simplicity, we assume the QueryDefinition property contains the full SQL query.
        if (!dataQueryDefinition.IsFullQuery)
        {
            throw new NotSupportedException("Only full SQL queries are supported in this implementation.");
        }

        return dataQueryDefinition.QueryDefinition;
    }
}

