using GroundTruthCuration.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroundTruthCuration.Core.Services
{
    public class GroundTruthCurationService : IGroundTruthCurationService
    {
        public async Task<GroundTruthDefinition> CreateGroundTruthDefinitionAsync(string userQuery, string userId)
        {
            // TODO: Implement actual logic
            await Task.CompletedTask;
            return new GroundTruthDefinition();
        }

        public async Task<IEnumerable<GroundTruthDefinition>> GetGroundTruthDefinitionsForValidationAsync()
        {
            // TODO: Implement actual logic
            await Task.CompletedTask;
            return Enumerable.Empty<GroundTruthDefinition>();
        }

        public async Task<GroundTruthEntry> AddGroundTruthEntryAsync(Guid definitionId, string response, string requiredValuesJson, string rawDataJson)
        {
            // TODO: Implement actual logic
            await Task.CompletedTask;
            return new GroundTruthEntry();
        }

        public async Task<GroundTruthDefinition> UpdateValidationStatusAsync(Guid definitionId, string validationStatus, string userId)
        {
            // TODO: Implement actual logic
            await Task.CompletedTask;
            return new GroundTruthDefinition();
        }

        public Task<GroundTruthDefinition> GetGroundTruthDefinitionByIdAsync(Guid definitionId)
        {
            throw new NotImplementedException();
        }
    }
}
