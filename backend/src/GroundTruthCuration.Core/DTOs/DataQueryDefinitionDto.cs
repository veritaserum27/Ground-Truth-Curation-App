using GroundTruthCuration.Core.Constants;

namespace GroundTruthCuration.Core.DTOs;

public class DataQueryDefinitionDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the data query definition.
    /// </summary>
    public Guid? DataQueryId { get; set; }

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
    /// Gets or sets the required properties as a list of strings.
    /// </summary>
    public ICollection<string> RequiredProperties { get; set; } = new List<string>();


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
}