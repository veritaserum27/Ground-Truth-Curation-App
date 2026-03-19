using GroundTruthCuration.Core.DTOs;
using GroundTruthCuration.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace GroundTruthCuration.Core.Interfaces
{
    /// <summary>
    /// Provides methods for managing and retrieving ground truth definitions.
    /// </summary>
    public interface IGroundTruthCurationService
    {
        /// <summary>
        /// Retrieves a ground truth definition by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the ground truth definition.</param>
        /// <returns>The ground truth definition if found; otherwise, null.</returns>
        Task<GroundTruthDefinitionDto?> GetGroundTruthDefinitionByIdAsync(Guid id);

        /// <summary>
        /// Retrieves all ground truth definitions from the repository.
        /// </summary>
        /// <returns>A collection of all ground truth definitions.</returns>
        Task<IEnumerable<GroundTruthDefinitionDto>> GetAllGroundTruthDefinitionsAsync(GroundTruthDefinitionFilter? filter);

        /// <summary>
        /// Adds a new ground truth definition to the repository.
        /// </summary>
        /// <param name="groundTruthDefinition">The ground truth definition to add.</param>
        /// <returns>The added ground truth definition.</returns>
        Task<GroundTruthDefinitionDto?> AddGroundTruthDefinitionAsync(GroundTruthDefinitionDto groundTruthDefinition);

        /// <summary>
        /// Adds, updates, or removes data query definitions associated with a ground truth definition in the repository.
        /// </summary>
        /// <param name="groundTruthId">The unique identifier of the ground truth definition to update.</param>
        /// <param name="dataQueryDefinitions">The updated list of data query definitions.</param>
        /// <returns>The updated ground truth definition.</returns>
        Task<GroundTruthDefinitionDto?> UpdateGroundTruthDataQueryDefinitionsAsync(Guid groundTruthId, List<DataQueryDefinitionDto> dataQueryDefinitions);

        /// <summary>
        /// Adds, updates, or removes ground truth contexts and related entities for an existing ground truth definition.
        /// The result will be an updated ground truth definition that reflects the changes.
        /// </summary>
        /// <param name="groundTruthId">The unique identifier of the ground truth definition to update.</param>
        /// <param name="groundTruthContexts">The updated list of ground truth contexts.</param>
        /// <returns>The updated ground truth definition.</returns>
        Task<GroundTruthDefinitionDto?> UpdateGroundTruthContextsAndRelatedEntitiesAsync(Guid groundTruthId, List<GroundTruthContextDto> groundTruthContexts);

        /// <summary>
        /// Deletes a ground truth definition from the repository by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the ground truth definition to delete.</param>
        Task DeleteGroundTruthDefinitionAsync(Guid id);

        /// <summary>
        /// Checks if a ground truth definition exists in the repository by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the ground truth definition.</param>
        /// <returns>True if the ground truth definition exists; otherwise, false.</returns>
        Task<bool> ExistsGroundTruthDefinitionAsync(Guid id);
    }
}
