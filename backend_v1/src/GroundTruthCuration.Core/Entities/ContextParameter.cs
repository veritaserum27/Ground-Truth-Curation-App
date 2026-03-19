namespace GroundTruthCuration.Core.Entities
{
    /// <summary>
    /// Represents a parameter associated with a ground truth context, including its name, value, and data type.
    /// </summary>
    public class ContextParameter
    {
        /// <summary>
        /// Gets or sets the unique identifier for the parameter.
        /// </summary>
        public required Guid ParameterId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the associated context.
        /// </summary>
        public required Guid ContextId { get; set; }

        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        public required string ParameterName { get; set; }

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        public required string ParameterValue { get; set; }

        /// <summary>
        /// Gets or sets the data type of the parameter (e.g., string, int, bool).
        /// </summary>
        public required string DataType { get; set; }
    }
}