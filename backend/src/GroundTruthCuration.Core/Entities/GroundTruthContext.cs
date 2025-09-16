namespace GroundTruthCuration.Core.Entities
{
    /// <summary>
    /// Represents a context for ground truth data, including its parameters and metadata.
    /// </summary>
    public class GroundTruthContext
    {
        /// <summary>
        /// Gets or sets the unique identifier for the context.
        /// </summary>
        public Guid ContextId { get; set; }

        /// <summary>
        /// Gets or sets the name of the context.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the context.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the collection of parameters associated with this context.
        /// </summary>
        public ICollection<ContextParameter> ContextParameters { get; set; } = new List<ContextParameter>();
    }
}