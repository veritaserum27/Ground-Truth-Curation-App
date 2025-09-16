using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;
using GroundTruthCuration.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroundTruthCuration.Core.Services
{
    /// <summary>
    /// Service for managing ground truth definitions, including creation, retrieval, updating, and deletion.
    /// </summary>
    public class GroundTruthCurationService : IGroundTruthCurationService
    {
        private readonly IGroundTruthRepository _groundTruthRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroundTruthCurationService"/> class.
        /// </summary>
        /// <param name="groundTruthRepository">The repository for managing ground truth data.</param>
        public GroundTruthCurationService(IGroundTruthRepository groundTruthRepository)
        {
            _groundTruthRepository = groundTruthRepository ?? throw new ArgumentNullException(nameof(groundTruthRepository));
        }

        /// <inheritdoc/>
        public Task<GroundTruthDefinition> AddGroundTruthDefinitionAsync(GroundTruthDefinition groundTruthDefinition)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task DeleteGroundTruthDefinitionAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> ExistsGroundTruthDefinitionAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<GroundTruthDefinition>> GetAllGroundTruthDefinitionsAsync(GroundTruthDefinitionFilter filter)
        {
            return await _groundTruthRepository.GetAllGroundTruthDefinitionsAsync(filter);
        }

        /// <inheritdoc/>
        public Task<GroundTruthDefinition?> GetGroundTruthDefinitionByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<IEnumerable<GroundTruthDefinition>> GetGroundTruthDefinitionsByUserAsync(string userId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<IEnumerable<GroundTruthDefinition>> GetGroundTruthDefinitionsByValidationStatusAsync(string validationStatus)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<GroundTruthDefinition> UpdateGroundTruthDefinitionAsync(GroundTruthDefinition groundTruthDefinition)
        {
            throw new NotImplementedException();
        }
    }
}
