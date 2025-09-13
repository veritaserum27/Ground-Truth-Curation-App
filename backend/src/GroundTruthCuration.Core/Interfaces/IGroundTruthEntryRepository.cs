using GroundTruthCuration.Core.Entities;

namespace GroundTruthCuration.Core.Interfaces;

public interface IGroundTruthEntryRepository
{
    Task<GroundTruthEntry?> GetByIdAsync(Guid id);
    Task<IEnumerable<GroundTruthEntry>> GetByGroundTruthDefinitionIdAsync(Guid groundTruthId);
    Task<GroundTruthEntry> AddAsync(GroundTruthEntry groundTruthEntry);
    Task<GroundTruthEntry> UpdateAsync(GroundTruthEntry groundTruthEntry);
    Task DeleteAsync(Guid id);
}
