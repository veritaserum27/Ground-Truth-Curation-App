using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;

namespace GroundTruthCuration.Infrastructure.Repositories;

public class InMemoryGroundTruthEntryRepository : IGroundTruthEntryRepository
{
    private readonly List<GroundTruthEntry> _groundTruthEntries;

    public InMemoryGroundTruthEntryRepository()
    {
        _groundTruthEntries = new List<GroundTruthEntry>();
    }

    public async Task<GroundTruthEntry?> GetByIdAsync(Guid id)
    {
        await Task.Delay(10); // Simulate async operation
        return _groundTruthEntries.FirstOrDefault(entry => entry.GroundTruthEntryId == id);
    }

    public async Task<IEnumerable<GroundTruthEntry>> GetByGroundTruthDefinitionIdAsync(Guid groundTruthId)
    {
        await Task.Delay(10); // Simulate async operation
        return _groundTruthEntries
            .Where(entry => entry.GroundTruthId == groundTruthId)
            .ToList();
    }

    public async Task<GroundTruthEntry> AddAsync(GroundTruthEntry groundTruthEntry)
    {
        await Task.Delay(10); // Simulate async operation
        _groundTruthEntries.Add(groundTruthEntry);
        return groundTruthEntry;
    }

    public async Task<GroundTruthEntry> UpdateAsync(GroundTruthEntry groundTruthEntry)
    {
        await Task.Delay(10); // Simulate async operation
        var existingIndex = _groundTruthEntries
            .FindIndex(entry => entry.GroundTruthEntryId == groundTruthEntry.GroundTruthEntryId);
        
        if (existingIndex >= 0)
        {
            _groundTruthEntries[existingIndex] = groundTruthEntry;
        }
        
        return groundTruthEntry;
    }

    public async Task DeleteAsync(Guid id)
    {
        await Task.Delay(10); // Simulate async operation
        var groundTruthEntry = _groundTruthEntries
            .FirstOrDefault(entry => entry.GroundTruthEntryId == id);
        
        if (groundTruthEntry != null)
        {
            _groundTruthEntries.Remove(groundTruthEntry);
        }
    }
}
