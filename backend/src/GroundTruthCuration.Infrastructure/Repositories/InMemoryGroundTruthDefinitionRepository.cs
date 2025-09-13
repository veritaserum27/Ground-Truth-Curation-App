using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;

namespace GroundTruthCuration.Infrastructure.Repositories;

public class InMemoryGroundTruthDefinitionRepository : IGroundTruthDefinitionRepository
{
    private readonly List<GroundTruthDefinition> _groundTruthDefinitions;

    public InMemoryGroundTruthDefinitionRepository()
    {
        _groundTruthDefinitions = new List<GroundTruthDefinition>();
    }

    public async Task<GroundTruthDefinition?> GetByIdAsync(Guid id)
    {
        await Task.Delay(10); // Simulate async operation
        return _groundTruthDefinitions.FirstOrDefault(gt => gt.GroundTruthId == id);
    }

    public async Task<IEnumerable<GroundTruthDefinition>> GetAllAsync()
    {
        await Task.Delay(10); // Simulate async operation
        return _groundTruthDefinitions.ToList();
    }

    public async Task<IEnumerable<GroundTruthDefinition>> GetByUserAsync(string userId)
    {
        await Task.Delay(10); // Simulate async operation
        return _groundTruthDefinitions
            .Where(gt => gt.UserCreated == userId)
            .ToList();
    }

    public async Task<IEnumerable<GroundTruthDefinition>> GetByValidationStatusAsync(string validationStatus)
    {
        await Task.Delay(10); // Simulate async operation
        return _groundTruthDefinitions
            .Where(gt => gt.ValidationStatus == validationStatus)
            .ToList();
    }

    public async Task<GroundTruthDefinition> AddAsync(GroundTruthDefinition groundTruthDefinition)
    {
        await Task.Delay(10); // Simulate async operation
        _groundTruthDefinitions.Add(groundTruthDefinition);
        return groundTruthDefinition;
    }

    public async Task<GroundTruthDefinition> UpdateAsync(GroundTruthDefinition groundTruthDefinition)
    {
        await Task.Delay(10); // Simulate async operation
        var existingIndex = _groundTruthDefinitions
            .FindIndex(gt => gt.GroundTruthId == groundTruthDefinition.GroundTruthId);

        if (existingIndex >= 0)
        {
            _groundTruthDefinitions[existingIndex] = groundTruthDefinition;
        }

        return groundTruthDefinition;
    }

    public async Task DeleteAsync(Guid id)
    {
        await Task.Delay(10); // Simulate async operation
        var groundTruthDefinition = _groundTruthDefinitions
            .FirstOrDefault(gt => gt.GroundTruthId == id);

        if (groundTruthDefinition != null)
        {
            _groundTruthDefinitions.Remove(groundTruthDefinition);
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        await Task.Delay(10); // Simulate async operation
        return _groundTruthDefinitions.Any(gt => gt.GroundTruthId == id);
    }
}
