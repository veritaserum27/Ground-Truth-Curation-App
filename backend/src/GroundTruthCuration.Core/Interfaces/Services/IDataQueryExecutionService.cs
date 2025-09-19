using GroundTruthCuration.Core.Entities;

namespace GroundTruthCuration.Core.Interfaces;

public interface IDataQueryExecutionService
{
    /// <summary>
    /// Executes the specified data queries within the given ground truth definition and contexts.
    /// This will eventually handled in a background job.
    /// </summary>
    /// <param name="groundTruthDefinition"></param>
    /// <param name="dataQueryDefinitionsToExecute"></param>
    /// <param name="contextsToExecute"></param>
    /// <returns></returns>
    Task ExecuteDataQueriesAsync(GroundTruthDefinition groundTruthDefinition, List<DataQueryDefinition> dataQueryDefinitionsToExecute, List<GroundTruthContext> contextsToExecute);
}