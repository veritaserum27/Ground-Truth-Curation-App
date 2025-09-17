namespace GroundTruthCuration.Core.Constants
{

    /// <summary>
    /// Specifies the type of datastore used in the ground truth curation process.
    /// </summary>
    public enum DatastoreType
    {
        /// <summary>
        /// Represents a SQL database datastore.
        /// </summary>
        Sql,

        /// <summary>
        /// Represents a Cosmos DB datastore.
        /// </summary>
        CosmosDb
    }
}