using GroundTruthCuration.Core.DTOs;
using GroundTruthCuration.Core.Interfaces;

namespace GroundTruthCuration.Infrastructure.Repositories;

/// <summary>
/// Repository for interacting with the Manufacturing Data RelationalDB SQL Server datastore.
/// </summary>
public class ManufacturingDataRelDbRepository : IDatastoreRepository
{
    /// <inheritdoc/>
    public Task<string> ExecuteQueryAsync<T>(T parameters, string query)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<string> GetStatusAsync()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<DatastoreDetailsDto> GetDatastoreDetailsAsync()
    {
        throw new NotImplementedException();
    }
}

