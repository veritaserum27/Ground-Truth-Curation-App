using GroundTruthCuration.Core.Entities;

namespace GroundTruthCuration.Core.Interfaces;

/// <summary>
/// Provides an interface for managing ground truth definitions in the repository.
/// </summary>
public interface IGroundTruthDefinitionRepository
{
    /// <summary>
    /// Retrieves a ground truth definition by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the ground truth definition.</param>
    /// <returns>The ground truth definition if found; otherwise, null.</returns>
    Task<GroundTruthDefinition?> GetByIdAsync(Guid id);

    /// <summary>
    /// Retrieves all ground truth definitions from the repository.
    /// </summary>
    /// <returns>A collection of all ground truth definitions.</returns>
    Task<IEnumerable<GroundTruthDefinition>> GetAllAsync();

    /// <summary>
    /// Retrieves all ground truth definitions associated with a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A collection of ground truth definitions associated with the user.</returns>
    Task<IEnumerable<GroundTruthDefinition>> GetByUserAsync(string userId);

    /// <summary>
    /// Retrieves all ground truth definitions with a specific validation status.
    /// </summary>
    /// <param name="validationStatus">The validation status to filter by.</param>
    /// <returns>A collection of ground truth definitions with the specified validation status.</returns>
    Task<IEnumerable<GroundTruthDefinition>> GetByValidationStatusAsync(string validationStatus);

    /// <summary>
    /// Updates an existing ground truth definition in the repository.
    /// </summary>
    /// <param name="groundTruthDefinition">The ground truth definition to update.</param>
    /// <returns>The updated ground truth definition.</returns>
    Task<GroundTruthDefinition> UpdateAsync(GroundTruthDefinition groundTruthDefinition);

    /// <summary>
    /// Deletes a ground truth definition from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the ground truth definition to delete.</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Checks if a ground truth definition exists in the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the ground truth definition.</param>
    /// <returns>True if the ground truth definition exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(Guid id);
}
