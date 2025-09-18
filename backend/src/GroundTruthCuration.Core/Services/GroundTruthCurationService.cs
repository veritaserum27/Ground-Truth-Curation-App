using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;
using GroundTruthCuration.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GroundTruthCuration.Core.Services
{
    /// <summary>
    /// Service for managing ground truth definitions, including creation, retrieval, updating, and deletion.
    /// </summary>
    public class GroundTruthCurationService : IGroundTruthCurationService
    {
        private readonly ILogger<GroundTruthCurationService> _logger;
        private readonly IGroundTruthRepository _groundTruthRepository;
        private readonly IGroundTruthMapper<GroundTruthDefinition, GroundTruthDefinitionDto> _groundTruthDefinitionToDtoMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroundTruthCurationService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging operations.</param>
        /// <param name="groundTruthRepository">The repository for managing ground truth data.</param>
        /// <param name="groundTruthDefinitionToDtoMapper">The mapper for converting ground truth definitions to DTOs.</param>
        public GroundTruthCurationService(ILogger<GroundTruthCurationService> logger, IGroundTruthRepository groundTruthRepository, IGroundTruthMapper<GroundTruthDefinition, GroundTruthDefinitionDto> groundTruthDefinitionToDtoMapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _groundTruthRepository = groundTruthRepository ?? throw new ArgumentNullException(nameof(groundTruthRepository));
            _groundTruthDefinitionToDtoMapper = groundTruthDefinitionToDtoMapper ?? throw new ArgumentNullException(nameof(groundTruthDefinitionToDtoMapper));
        }

        /// <inheritdoc/>
        public async Task<GroundTruthDefinitionDto?> AddGroundTruthContextsAndRelatedEntitiesAsync(Guid groundTruthId, List<GroundTruthContextDto> groundTruthContexts)
        {
            // Get the existing ground truth definition
            var groundTruthDefinition = await _groundTruthRepository.GetGroundTruthDefinitionByIdAsync(groundTruthId);

            if (groundTruthDefinition == null)
            {
                _logger.LogError("Ground truth definition with ID {GroundTruthId} not found.", groundTruthId);
                return null;
            }

            var existingContextIds = groundTruthDefinition.GroundTruthEntries
                .Where(e => e?.GroundTruthContext != null)
                .Select(e => e.GroundTruthContext.ContextId)
                .ToHashSet();

            var incomingContextIds = groundTruthContexts
                .Select(dto => dto.ContextId)
                .ToHashSet();

            var toRemove = existingContextIds.Except(incomingContextIds).ToList();

            if (toRemove.Any())
            {
                // Remove entries with contexts that are not in the incoming list
                await _groundTruthRepository.DeleteGroundTruthContextsAndRelatedEntitiesAsync(groundTruthId, toRemove);
            }

            var groundTruthEntriesToUpdate = new List<GroundTruthEntry>();

            // check if context already exists
            foreach (var contextDto in groundTruthContexts)
            {
                // check if context with same ID already exists
                if (groundTruthDefinition.GroundTruthEntries.Any(e => e != null && e.GroundTruthContext != null &&
                    e.GroundTruthContext.ContextId.ToString().Equals(contextDto.ContextId.ToString(),
                    StringComparison.OrdinalIgnoreCase)))
                {
                    // check if values changed
                    var existingEntry = groundTruthDefinition.GroundTruthEntries.First(e => e != null &&
                    e.GroundTruthContext != null && e.GroundTruthContext.ContextId.ToString().Equals(contextDto.ContextId.ToString(),
                    StringComparison.OrdinalIgnoreCase));

                    var didChange = false;

                    // A GroundTruthEntry with this contextId exists, check root values
                    if (existingEntry != null && existingEntry.GroundTruthContext != null)
                    {
                        existingEntry.GroundTruthContext.ContextType = contextDto.ContextType;
                        didChange = true;
                    }

                    // compare parameters list
                    foreach (var param in contextDto.ContextParameters)
                    {
                        var existingParameter = existingEntry.GroundTruthContext?.ContextParameters.FirstOrDefault(
                            p => p.ParameterId.ToString().Equals(param.ParameterId.ToString(), StringComparison.OrdinalIgnoreCase));
                        if (existingParameter == null || existingParameter.ParameterValue != param.ParameterValue)
                        {
                            // remove existing if exists
                            if (existingParameter != null)
                            {
                                existingEntry.GroundTruthContext?.ContextParameters.ToList().RemoveAll(p => p.ParameterId.Equals(existingParameter.ParameterId));
                            }

                            // parameter is new or value changed
                            existingEntry.GroundTruthContext?.ContextParameters.Add(new ContextParameter
                            {
                                ParameterId = existingParameter?.ParameterId ?? Guid.NewGuid(),
                                ContextId = existingEntry.GroundTruthContext.ContextId,
                                ParameterName = param.ParameterName,
                                ParameterValue = param.ParameterValue,
                                DataType = param.DataType
                            });

                            didChange = true;
                        }
                    }

                    if (didChange && existingEntry != null)
                    {
                        groundTruthEntriesToUpdate.Add(existingEntry);
                    }
                }
                else
                {
                    // Add new context
                    var newContext = new GroundTruthContext
                    {
                        ContextId = contextDto.ContextId,
                        GroundTruthId = groundTruthId,
                        GroundTruthEntryId = Guid.NewGuid(),
                        ContextType = contextDto.ContextType,
                        ContextParameters = contextDto.ContextParameters.Select(p => new ContextParameter
                        {
                            ParameterId = p.ParameterId,
                            ContextId = contextDto.ContextId,
                            ParameterName = p.ParameterName,
                            ParameterValue = p.ParameterValue,
                            DataType = p.DataType
                        }).ToList()
                    };


                    await _groundTruthRepository.AddGroundTruthContextAndRelatedEntitiesAsync(groundTruthId, newContext);
                }
            }
            // Save changes
            // method to remove old contexts
            // method to remove old ground truth entries
            // method to remove context parameters if context removed
            // method to update existing contexts and parameters if changed
            // method to add new contexts and parameters
            // method to add new ground truth entries
            // method to update ground truth definition metadata if needed
            await _groundTruthRepository.UpdateGroundTruthDefinitionAsync(groundTruthDefinition);

            // TODO: execute data queries for new or updated contexts to refresh data

            return _groundTruthDefinitionToDtoMapper.Map(groundTruthDefinition);
        }

        /// <inheritdoc/>
        public Task<GroundTruthDefinitionDto> AddGroundTruthDefinitionAsync(GroundTruthDefinitionDto groundTruthDefinition)
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
            var groundTruthDefinitionEntities = (await _groundTruthRepository.GetAllGroundTruthDefinitionsAsync(filter)).ToList();
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
        public Task<GroundTruthDefinitionDto> UpdateGroundTruthDataQueryDefinitionsAsync(Guid groundTruthId, List<DataQueryDefinitionDto> dataQueryDefinitions)
        {
            throw new NotImplementedException();
        }
    }
}
