using GroundTruthCuration.Core.Constants;

namespace GroundTruthCuration.Core.DTOs;

/// <summary>
/// DTO for datastore details
/// </summary>
public class DatastoreDetailsDto
{
    /// <summary>
    /// The type of datastore (e.g. SQL, Cosmos DB, etc.)
    /// </summary>
    public DatastoreType DatastoreType { get; set; }

    /// <summary>
    /// The name of the datastore (e.g. database name, account name, etc.)
    /// </summary>
    public string DatastoreName { get; set; } = string.Empty;

    /// <summary>
    /// The list of query targets in the datastore (e.g. tables, collections, etc.)
    /// </summary>
    public List<string> QueryTargets { get; set; } = new List<string>();
}