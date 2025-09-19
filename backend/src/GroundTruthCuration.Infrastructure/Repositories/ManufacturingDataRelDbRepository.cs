using GroundTruthCuration.Core.Constants;
using GroundTruthCuration.Core.DTOs;
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

    public Task<ICollection<T>> ExecuteQueryAsync<T>(ICollection<T> parameters, string query)
    {
        throw new NotImplementedException();
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
            finally
            {
                await connection.CloseAsync();
            }
        }

        return status;
    }
}

