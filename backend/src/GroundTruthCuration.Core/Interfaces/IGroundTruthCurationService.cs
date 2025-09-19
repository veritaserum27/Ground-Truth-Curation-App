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
        Task<GroundTruthDefinitionDto> AddGroundTruthDefinitionAsync(GroundTruthDefinitionDto groundTruthDefinition);

        /// <summary>
        /// Updates an existing ground truth definition in the repository.
        /// </summary>
        /// <param name="groundTruthId">The unique identifier of the ground truth definition to update.</param>
        /// <param name="dataQueryDefinitions">The updated list of data query definitions.</param>
        /// <returns>The updated ground truth definition.</returns>
        Task<GroundTruthDefinitionDto> UpdateGroundTruthDataQueryDefinitionsAsync(Guid groundTruthId, List<DataQueryDefinitionDto> dataQueryDefinitions);

        /// <summary>
        /// Adds a new ground truth context to an existing ground truth definition.
        /// </summary>
        /// <param name="groundTruthId">The unique identifier of the ground truth definition to which the context will be added.</param>
        /// <param name="groundTruthContexts">The ground truth contexts to add.</param>
        /// <returns>The updated ground truth definition with the new contexts.</returns>
        Task<GroundTruthDefinitionDto?> AddGroundTruthContextsAndRelatedEntitiesAsync(Guid groundTruthId, List<GroundTruthContextDto> groundTruthContexts);

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
