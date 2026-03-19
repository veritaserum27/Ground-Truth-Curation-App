using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.DTOs;

namespace GroundTruthCuration.Core.Interfaces;

/// <summary>
/// Provides an interface for managing ground truth definitions in the repository.
/// </summary>
public interface IGroundTruthRepository
{
    /// <summary>
    /// Gets the status of the ground truth repository.
    /// </summary>
    /// <returns></returns>
    Task<DatabaseStatusDto> GetStatusAsync();

    /// <summary>
    /// Retrieves a ground truth definition by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the ground truth definition.</param>
    /// <returns>The ground truth definition if found; otherwise, null.</returns>
    Task<GroundTruthDefinition?> GetGroundTruthDefinitionByIdAsync(Guid id);

    /// <summary>
    /// Retrieves all ground truth definitions from the repository.
    /// </summary>
    /// <returns>A collection of all ground truth definitions.</returns>
    Task<IEnumerable<GroundTruthDefinition>> GetAllGroundTruthDefinitionsAsync(GroundTruthDefinitionFilter? filter);

    /// <summary>
    /// Retrieves all ground truth definitions associated with a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A collection of ground truth definitions associated with the user.</returns>
    Task<IEnumerable<GroundTruthDefinition>> GetGroundTruthDefinitionsByUserAsync(string userId);

    /// <summary>
    /// Retrieves all ground truth definitions with a specific validation status.
    /// </summary>
    /// <param name="validationStatus">The validation status to filter by.</param>
    /// <returns>A collection of ground truth definitions with the specified validation status.</returns>
    Task<IEnumerable<GroundTruthDefinition>> GetGroundTruthDefinitionsByValidationStatusAsync(string validationStatus);

    /// <summary>
    /// Adds a new ground truth definition to the repository.
    /// </summary>
    /// <param name="groundTruthDefinition">The ground truth definition to add.</param>
    /// <returns>The added ground truth definition.</returns>
    Task<GroundTruthDefinition> AddGroundTruthDefinitionAsync(GroundTruthDefinition groundTruthDefinition);

    /// <summary>
    /// Updates an existing ground truth definition in the repository.
    /// </summary>
    /// <param name="groundTruthDefinition">The ground truth definition to update.</param>
    /// <returns>The updated ground truth definition.</returns>
    Task<GroundTruthDefinition> UpdateGroundTruthDefinitionAsync(GroundTruthDefinition groundTruthDefinition);
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

    /// <summary>
    /// Removes ground truth entries associated with specific context IDs for a given ground truth definition. 
    /// Also removes associated contexts and context parameters.
    /// </summary>
    /// <param name="groundTruthId"></param>
    /// <param name="contextIds"></param>
    /// <param name="groundTruthEntryIds">The unique identifiers of the ground truth entries to delete.</param>
    /// <returns></returns>
    Task DeleteGroundTruthContextsAndRelatedEntitiesAsync(Guid groundTruthId, IEnumerable<Guid> contextIds, IEnumerable<Guid> groundTruthEntryIds);

    /// <summary>
    /// Adds a new ground truth context along with its associated context parameters and a new ground truth entry. 
    /// This method ensures that all related data is inserted in a single transaction to maintain data integrity
    /// </summary>
    /// <param name="groundTruthId"></param>
    /// <param name="newContext"></param>
    /// <returns></returns>
    Task AddGroundTruthContextAndRelatedEntitiesAsync(Guid groundTruthId, GroundTruthContext newContext);

    /// <summary>
    /// Updates an existing ground truth context along with its associated context parameters. 
    /// This method ensures that all related data is updated in a single transaction to maintain data integrity.
    /// </summary>
    /// <param name="groundTruthId"></param>
    /// <param name="updatedContext"></param>
    /// <returns></returns>
    Task UpdateGroundTruthContextAndRelatedEntitiesAsync(Guid groundTruthId, GroundTruthContext updatedContext);

    /// <summary>
    /// Removes data query definitions associated with a specific ground truth definition. 
    /// This method ensures that all specified data query definitions are deleted in a single transaction to maintain data integrity.
    /// </summary>
    /// <param name="dataQueryIds"></param>
    /// <returns></returns>
    Task DeleteDataQueryDefinitionsAsync(IEnumerable<Guid> dataQueryIds);

    /// <summary>
    /// Updates an existing data query definition associated with a specific ground truth definition.
    /// </summary>
    /// <param name="dataQueryDefinition"></param>
    /// <returns></returns>
    Task UpdateDataQueryDefinitionAsync(DataQueryDefinition dataQueryDefinition);

    /// <summary>
    /// Adds a new data query definition associated with a specific ground truth definition.
    /// </summary>
    /// <param name="dataQueryDefinition"></param>
    /// <returns></returns>
    Task AddDataQueryDefinitionAsync(DataQueryDefinition dataQueryDefinition);

    /// <summary>
    /// Adds or updates a ground truth entry in the repository. 
    /// If the entry already exists (based on its unique identifier), it will be updated;
    /// otherwise, a new entry will be created.
    /// </summary>
    /// <param name="groundTruthEntry"></param>
    /// <returns></returns>
    Task AddOrUpdateGroundTruthEntryAsync(GroundTruthEntry groundTruthEntry);
}
