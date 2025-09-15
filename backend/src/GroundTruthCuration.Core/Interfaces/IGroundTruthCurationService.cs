using GroundTruthCuration.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GroundTruthCuration.Core.Services
{
    public interface IGroundTruthCurationService
    {
        Task<GroundTruthDefinition> CreateGroundTruthDefinitionAsync(string userQuery, string userId);
        Task<IEnumerable<GroundTruthDefinition>> GetGroundTruthDefinitionsForValidationAsync();
        Task<GroundTruthEntry> AddGroundTruthEntryAsync(Guid definitionId, string response, string requiredValuesJson, string rawDataJson);
        Task<GroundTruthDefinition> UpdateValidationStatusAsync(Guid definitionId, string validationStatus, string userId);
        Task<GroundTruthDefinition> GetGroundTruthDefinitionByIdAsync(Guid definitionId);
    }
}
