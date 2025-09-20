using GroundTruthCuration.Core.DTOs;
using GroundTruthCuration.Core.Interfaces;
using GroundTruthCuration.Core.Constants;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GroundTruthCuration.Core.Entities;

namespace GroundTruthCuration.Infrastructure.Repositories;

/// <summary>
/// Repository for interacting with the Manufacturing Data DocumentDB CosmosDB datastore
/// </summary>
public class ManufacturingDataDocDbRepository : IDatastoreRepository
{
    private readonly ILogger<ManufacturingDataDocDbRepository> _logger;
    private readonly string _connectionString;
    private readonly string _databaseName;
    public ManufacturingDataDocDbRepository(ILogger<ManufacturingDataDocDbRepository> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _connectionString = _configuration.GetValue<string>("Datastores:ManufacturingDataDocDb:ConnectionString") ?? throw new InvalidOperationException("Connection string 'Datastores:ManufacturingDataDocDb:ConnectionString' is null or missing.");
        _databaseName = _configuration.GetValue<string>("Datastores:ManufacturingDataDocDb:DatabaseName") ?? throw new InvalidOperationException("Database name 'Datastores:ManufacturingDataDocDb:DatabaseName' is null or missing.");
    }

    public Task<ICollection<object>> ExecuteQueryAsync<T>(T parameters, DataQueryDefinition dataQueryDefinition)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<DatastoreDetailsDto> GetDatastoreDetailsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseStatusDto> GetStatusAsync()
    {
        var accountName = string.Empty;
        var endpoint = new System.Data.Common.DbConnectionStringBuilder { ConnectionString = _connectionString }["AccountEndpoint"]?.ToString();
        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            var uri = new Uri(endpoint);
            accountName = uri.Host.Split('.')[0];
        }

        var status = new DatabaseStatusDto
        {
            ResourceName = accountName,
            DatabaseName = _databaseName,
            DatabaseType = DatastoreType.CosmosDb.ToString(),
            IsConnected = false,
            LastChecked = DateTime.UtcNow
        };

        try
        {
            // Attempt to create a Cosmos DB client to check connectivity
            var cosmosClient = new CosmosClient(_connectionString);
            var databaseResponse = await cosmosClient.GetDatabase(_databaseName).ReadAsync();

            if (databaseResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                status.IsConnected = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to Cosmos DB");
            status.IsConnected = false;
        }

        return status;
    }
}
