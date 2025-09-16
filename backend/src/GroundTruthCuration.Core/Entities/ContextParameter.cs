namespace GroundTruthCuration.Core.Entities
{
    public class ContextParameter
    {
        public Guid ParameterId { get; set; }
        public Guid ContextId { get; set; }
        public string ParameterName { get; set; } = string.Empty;
        public string ParameterValue { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
    }
}