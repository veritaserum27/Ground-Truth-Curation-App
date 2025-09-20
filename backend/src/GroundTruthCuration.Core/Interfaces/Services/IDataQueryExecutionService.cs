using GroundTruthCuration.Core.Entities;

namespace GroundTruthCuration.Core.Interfaces;

public interface IDataQueryExecutionService
{
    /// <summary>
    /// Executes the specified data queries within the given ground truth definition and contexts.
    /// This will eventually handled in a background job.
    /// </summary>
    /// <param name="groundTruthDefinition"></param>
    /// <param name="dataQueryDefinitionsUnchanged"></param>
    /// <param name="dataQueryDefinitionsToExecute"></param>
    /// <param name="contextsToExecute"></param>
    /// <returns></returns>
    Task ExecuteDataQueriesAsync(GroundTruthDefinition groundTruthDefinition, List<DataQueryDefinition> dataQueryDefinitionsUnchanged, List<DataQueryDefinition> dataQueryDefinitionsToExecute, List<GroundTruthContext> contextsToExecute);

    /// <summary>
    /// Executes all data queries associated with the specified ground truth definition ID.
    /// This will eventually handled in a background job.
    /// </summary>
    /// <param name="groundTruthDefinitionId"></param>
    /// <returns></returns>
    Task ExecuteDataQueriesByGroundTruthIdAsync(Guid groundTruthDefinitionId);
}