using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using GroundTruthCuration.Core.Constants;

namespace GroundTruthCuration.Core.Entities;

/// <summary>
/// Represents the definition of a data query associated with a ground truth entry, including metadata and query details.
/// </summary>
public class DataQueryDefinition
{
    /// <summary>
    /// Gets or sets the unique identifier for the data query definition.
    /// </summary>
    public Guid DataQueryId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the identifier of the associated ground truth definition.
    /// </summary>
    public required Guid GroundTruthId { get; set; }

    /// <summary>
    /// Gets or sets the type of datastore (e.g., SQL, CosmosDb).
    /// </summary>
    public DatastoreType DatastoreType { get; set; } = DatastoreType.Sql;

    /// <summary>
    /// Gets or sets the name of the datastore.
    /// </summary>
    public required string DatastoreName { get; set; }

    /// <summary>
    /// Gets or sets the target (e.g., table, collection, endpoint) for the query.
    /// </summary>
    public required string QueryTarget { get; set; }

    /// <summary>
    /// Gets or sets the query definition (e.g., SQL, GraphQL, or API query string).
    /// </summary>
    public required string QueryDefinition { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a full query or a partial query.
    /// </summary>
    public bool IsFullQuery { get; set; } = false;

    /// <summary>
    /// Gets or sets the required properties for the query, in JSON format.
    /// </summary>
    public string RequiredPropertiesJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username of the user who created this query definition.
    /// </summary>
    public string UserCreated { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username of the user who last updated this query definition.
    /// </summary>
    public string UserUpdated { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when this query definition was created.
    /// </summary>
    public DateTime CreationDateTime { get; set; }

    /// <summary>
    /// Gets or sets the start date and time for the query.
    /// </summary>
    public DateTime StartDateTime { get; set; }

    /// <summary>
    /// Gets or sets the end date and time for the query.
    /// </summary>
    public DateTime? EndDateTime { get; set; } = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataQueryDefinition"/> class with default values.
    /// </summary>
    public DataQueryDefinition()
    {
        var now = DateTime.UtcNow;
        CreationDateTime = now;
        StartDateTime = now;
    }
}
