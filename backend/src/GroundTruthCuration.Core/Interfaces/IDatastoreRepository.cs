using GroundTruthCuration.Core.DTOs;

namespace GroundTruthCuration.Core.Interfaces;

/// <summary>
/// Interface for interacting with a datastore that houses system data
/// </summary>
public interface IDatastoreRepository
{
    /// <summary>
    /// Executes the query against the datastore
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parameters">The parameters to include in the query</param>
    /// <param name="query">The query to execute</param>
    /// <returns></returns>
    Task<string> ExecuteQueryAsync<T>(T parameters, string query);

    /// <summary>
    /// Gets the connectivity status of the datastore
    /// </summary>
    /// <returns></returns>
    Task<string> GetStatusAsync();

    /// <summary>
    /// Gets datastore type, name, and list of query targets from the datastore (e.g. tables, collections, etc.)
    /// </summary>
    /// <returns></returns>
    Task<DatastoreDetailsDto> GetDatastoreDetailsAsync();
}
