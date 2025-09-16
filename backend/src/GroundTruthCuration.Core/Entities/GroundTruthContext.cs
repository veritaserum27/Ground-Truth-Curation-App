namespace GroundTruthCuration.Core.Entities
{
    public class GroundTruthContext
    {
        public Guid ContextId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<ContextParameter> ContextParameters { get; set; } = new List<ContextParameter>();
    }
}