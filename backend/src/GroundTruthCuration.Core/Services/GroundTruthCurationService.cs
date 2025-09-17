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
        private readonly IGroundTruthMapper<GroundTruthDefinition, GroundTruthDefinitionDto> _groundTruthDefinitionToDtoMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroundTruthCurationService"/> class.
        /// </summary>
        /// <param name="groundTruthRepository">The repository for managing ground truth data.</param>
        /// <param name="groundTruthDefinitionToDtoMapper">The mapper for converting ground truth definitions to DTOs.</param>
        public GroundTruthCurationService(IGroundTruthRepository groundTruthRepository, IGroundTruthMapper<GroundTruthDefinition, GroundTruthDefinitionDto> groundTruthDefinitionToDtoMapper)
        {
            _groundTruthRepository = groundTruthRepository ?? throw new ArgumentNullException(nameof(groundTruthRepository));
            _groundTruthDefinitionToDtoMapper = groundTruthDefinitionToDtoMapper ?? throw new ArgumentNullException(nameof(groundTruthDefinitionToDtoMapper));
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
        public async Task<IEnumerable<GroundTruthDefinitionDto>> GetAllGroundTruthDefinitionsAsync(GroundTruthDefinitionFilter? filter)
        {
            List<GroundTruthDefinition> groundTruthDefinitionEntities = (List<GroundTruthDefinition>)await _groundTruthRepository.GetAllGroundTruthDefinitionsAsync(filter);

            // map entities to DTOs
            return groundTruthDefinitionEntities.Select(entity => _groundTruthDefinitionToDtoMapper.Map(entity)).ToList();
        }

        /// <inheritdoc/>
        public async Task<GroundTruthDefinitionDto?> GetGroundTruthDefinitionByIdAsync(Guid id)
        {
            GroundTruthDefinition? groundTruthDefinition = await _groundTruthRepository.GetGroundTruthDefinitionByIdAsync(id);

            if (groundTruthDefinition == null)
            {
                return null;
            }

            // map entity to DTO
            return _groundTruthDefinitionToDtoMapper.Map(groundTruthDefinition);
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
