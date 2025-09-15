using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;

namespace GroundTruthCuration.Infrastructure.Repositories;

public class GroundTruthRepository : IGroundTruthRepository
{
    private readonly string _connectionString;

    public GroundTruthRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("GroundTruthConnectionString");
    }

    public Task<GroundTruthDefinition> AddGroundTruthDefinitionAsync(GroundTruthDefinition groundTruthDefinition)
    {
        throw new NotImplementedException();
    }

    public Task DeleteGroundTruthDefinitionAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsGroundTruthDefinitionAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GroundTruthDefinition>> GetAllGroundTruthDefinitionsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<GroundTruthDefinition?> GetGroundTruthDefinitionByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GroundTruthDefinition>> GetGroundTruthDefinitionsByUserAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GroundTruthDefinition>> GetGroundTruthDefinitionsByValidationStatusAsync(string validationStatus)
    {
        throw new NotImplementedException();
    }

    public Task<GroundTruthDefinition> UpdateGroundTruthDefinitionAsync(GroundTruthDefinition groundTruthDefinition)
    {
        throw new NotImplementedException();
    }

    // Repository methods using _connectionString to interact with the database
}