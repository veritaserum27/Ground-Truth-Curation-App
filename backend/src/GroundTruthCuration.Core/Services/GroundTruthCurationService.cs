using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;
using GroundTruthCuration.Core.DTOs;
using GroundTruthCuration.Core.Utilities;
using GroundTruthCuration.Core.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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
        private readonly IGroundTruthMapper<DataQueryDefinitionDto, DataQueryDefinition> _dataQueryFromDtoMapper;
        private readonly IGroundTruthComparer<GroundTruthContextDto, GroundTruthContext> _contextComparer;
        private readonly IGroundTruthComparer<DataQueryDefinitionDto, DataQueryDefinition> _dataQueryComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroundTruthCurationService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging operations.</param>
        /// <param name="groundTruthRepository">The repository for managing ground truth data.</param>
        /// <param name="groundTruthDefinitionToDtoMapper">The mapper for converting ground truth definitions to DTOs.</param>
        /// <param name="contextComparer">The comparer for validating context data consistency.</param>
        /// <param name="dataQueryComparer">The comparer for validating data query definitions.</param>
        /// <param name="dataQueryFromDtoMapper"></param>
        public GroundTruthCurationService(ILogger<GroundTruthCurationService> logger, IGroundTruthRepository groundTruthRepository,
            IGroundTruthMapper<GroundTruthDefinition, GroundTruthDefinitionDto> groundTruthDefinitionToDtoMapper,
            IGroundTruthComparer<GroundTruthContextDto, GroundTruthContext> contextComparer,
            IGroundTruthComparer<DataQueryDefinitionDto, DataQueryDefinition> dataQueryComparer,
            IGroundTruthMapper<DataQueryDefinitionDto, DataQueryDefinition> dataQueryFromDtoMapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _groundTruthRepository = groundTruthRepository ?? throw new ArgumentNullException(nameof(groundTruthRepository));
            _groundTruthDefinitionToDtoMapper = groundTruthDefinitionToDtoMapper ?? throw new ArgumentNullException(nameof(groundTruthDefinitionToDtoMapper));
            _contextComparer = contextComparer ?? throw new ArgumentNullException(nameof(contextComparer));
            _dataQueryComparer = dataQueryComparer ?? throw new ArgumentNullException(nameof(dataQueryComparer));
            _dataQueryFromDtoMapper = dataQueryFromDtoMapper ?? throw new ArgumentNullException(nameof(dataQueryFromDtoMapper));
        }

        /// <inheritdoc/>
        public async Task<GroundTruthDefinitionDto?> UpdateGroundTruthContextsAndRelatedEntitiesAsync(Guid groundTruthId, List<GroundTruthContextDto> groundTruthContexts)
        {
            // Get the existing ground truth definition
            var groundTruthDefinition = await _groundTruthRepository.GetGroundTruthDefinitionByIdAsync(groundTruthId);

            if (groundTruthDefinition == null)
            {
                _logger.LogError("Ground truth definition with ID {GroundTruthId} not found.", groundTruthId);
                return null; // Explicitly returning null for nullable return type
            }

            foreach (var contextDto in groundTruthContexts)
            {
                if (contextDto.ContextId == Guid.Empty)
                {
                    contextDto.ContextId = Guid.NewGuid();
                }
                if (contextDto.GroundTruthId == Guid.Empty)
                {
                    contextDto.GroundTruthId = groundTruthId;
                }
            }

            var existingContextIds = groundTruthDefinition.GroundTruthEntries
                .Where(e => e?.GroundTruthContext != null)
                .Select(e => e.GroundTruthContext?.ContextId)
                .ToHashSet();

            var incomingContextIds = groundTruthContexts
                .Select(dto => dto.ContextId)
                .ToHashSet();

            await deleteObsoleteContextsAsync(groundTruthDefinition, existingContextIds, incomingContextIds);

            // process contexts
            foreach (var contextDto in groundTruthContexts)
            {
                // check if context with same ID already exists
                if (existingContextIds.Contains(contextDto.ContextId))
                {
                    await processExistingContextAsync(groundTruthDefinition, contextDto, groundTruthId);
                }
                else
                {
                    await addNewContextAsync(contextDto, groundTruthId);
                }
            }

            // TODO: execute data queries for new or updated contexts to refresh data

            // Retrieve the updated ground truth definition
            var updatedGroundTruthDefinition = await _groundTruthRepository.GetGroundTruthDefinitionByIdAsync(groundTruthId);

            if (updatedGroundTruthDefinition == null)
            {
                _logger.LogError("Ground truth definition with ID {GroundTruthId} not found after update.", groundTruthId);
                return null;
            }

            return _groundTruthDefinitionToDtoMapper.Map(updatedGroundTruthDefinition);
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
        public async Task<GroundTruthDefinitionDto?> UpdateGroundTruthDataQueryDefinitionsAsync(Guid groundTruthId, List<DataQueryDefinitionDto> dataQueryDefinitions)
        {
            // get existing ground truth definition
            var groundTruthDefinition = await _groundTruthRepository.GetGroundTruthDefinitionByIdAsync(groundTruthId);
            if (groundTruthDefinition == null)
            {
                _logger.LogError("Ground truth definition with ID {GroundTruthId} not found. Aborting data query definition updates.", groundTruthId);
                return null;
            }

            // determine definitions to remove
            var existingDataQueryIds = groundTruthDefinition.DataQueryDefinitions
                .Select(dq => dq.DataQueryId)
                .ToHashSet();

            var incomingDataQueryIds = dataQueryDefinitions
                .Select(dq => dq.DataQueryId ?? Guid.Empty) // Treat null as empty
                .Where(id => id != Guid.Empty) // Exclude empty GUIDs
                .ToHashSet();

            await deleteObsoleteDataQueriesAsync(existingDataQueryIds, incomingDataQueryIds);

            // check for changes in existing definitions
            foreach (var dataQueryDefinition in dataQueryDefinitions)
            {
                if (dataQueryDefinition.DataQueryId != null && existingDataQueryIds.Contains(dataQueryDefinition.DataQueryId.Value))
                {
                    await processExistingDataQueryAsync(groundTruthDefinition, dataQueryDefinition);
                }
                else
                {
                    await addNewDataQueryAsync(dataQueryDefinition, groundTruthId);
                }
            }

            // TODO: execute data queries for new or updated definitions

            // return updated ground truth definition
            var updatedGroundTruthDefinition = await _groundTruthRepository.GetGroundTruthDefinitionByIdAsync(groundTruthId);
            if (updatedGroundTruthDefinition == null)
            {
                _logger.LogError("Ground truth definition with ID {GroundTruthId} not found after data query updates.", groundTruthId);
                return null;
            }

            return _groundTruthDefinitionToDtoMapper.Map(updatedGroundTruthDefinition);
        }

        private async Task deleteObsoleteDataQueriesAsync(HashSet<Guid> existingDataQueryIds, HashSet<Guid> incomingDataQueryIds)
        {
            var toRemove = existingDataQueryIds.Except(incomingDataQueryIds).ToList();
            if (toRemove.Any())
            {
                await _groundTruthRepository.DeleteDataQueryDefinitionsAsync(toRemove);
            }
        }

        private async Task processExistingDataQueryAsync(GroundTruthDefinition groundTruthDefinition, DataQueryDefinitionDto dataQueryDto)
        {
            // get the existing version
            var existingDataQuery = groundTruthDefinition.DataQueryDefinitions
                .FirstOrDefault(dq => dq.DataQueryId == dataQueryDto.DataQueryId);
            if (existingDataQuery == null)
            {
                _logger.LogError("Inconsistent state: Data Query ID {DataQueryId} found in existing IDs but no matching entry.", dataQueryDto.DataQueryId);
                return;
            }

            if (!_dataQueryComparer.HasSameValues(dataQueryDto, existingDataQuery))
            {
                _logger.LogInformation("Updating existing data query with ID {DataQueryId}", dataQueryDto.DataQueryId);

                // convert to entity
                var dataQueryEntity = _dataQueryFromDtoMapper.Map(dataQueryDto);

                await _groundTruthRepository.UpdateDataQueryDefinitionAsync(dataQueryEntity);
            }
        }

        private async Task addNewDataQueryAsync(DataQueryDefinitionDto dataQueryDto, Guid groundTruthId)
        {
            // Add new data query
            var newDataQuery = new DataQueryDefinition
            {
                DataQueryId = dataQueryDto.DataQueryId ?? Guid.NewGuid(),
                DatastoreName = dataQueryDto.DatastoreName,
                DatastoreType = dataQueryDto.DatastoreType,
                GroundTruthId = groundTruthId,
                QueryDefinition = dataQueryDto.QueryDefinition,
                QueryTarget = dataQueryDto.QueryTarget,
                IsFullQuery = dataQueryDto.IsFullQuery,
                RequiredPropertiesJson = JsonSerializer.Serialize(dataQueryDto.RequiredProperties),
                UserCreated = dataQueryDto.UserCreated,
                UserUpdated = dataQueryDto.UserUpdated,
                CreationDateTime = DateTime.UtcNow
            };

            await _groundTruthRepository.AddDataQueryDefinitionAsync(newDataQuery);
        }

        private async Task deleteObsoleteContextsAsync(GroundTruthDefinition groundTruthDefinition, HashSet<Guid?> existingContextIds, HashSet<Guid> incomingContextIds)
        {
            var toRemove = existingContextIds.Where(id => id != null).Select(id => id!.Value).Except(incomingContextIds).ToList();
            var groundTruthEntryIdsToRemove = groundTruthDefinition.GroundTruthEntries
                .Where(e => e?.GroundTruthContext != null && toRemove.Contains(e.GroundTruthContext.ContextId))
                .Select(e => e.GroundTruthEntryId)
                .ToList();

            if (toRemove.Any())
            {
                // Remove entries with contexts that are not in the incoming list
                await _groundTruthRepository.DeleteGroundTruthContextsAndRelatedEntitiesAsync(groundTruthDefinition.GroundTruthId, toRemove, groundTruthEntryIdsToRemove);
            }
        }

        private async Task processExistingContextAsync(GroundTruthDefinition groundTruthDefinition, GroundTruthContextDto contextDto, Guid groundTruthId)
        {
            // get the existing version
            var existingGroundTruthEntry = groundTruthDefinition.GroundTruthEntries
                .FirstOrDefault(e => e?.GroundTruthContext != null && e.GroundTruthContext.ContextId == contextDto.ContextId);
            if (existingGroundTruthEntry == null)
            {
                _logger.LogError("Inconsistent state: Context ID {ContextId} found in existing IDs but no matching entry.", contextDto.ContextId);
                return;
            }
            if (existingGroundTruthEntry.GroundTruthContext == null)
            {
                _logger.LogError("Inconsistent state: Existing entry for Context ID {ContextId} has null GroundTruthContext.", contextDto.ContextId);
                return;
            }

            if (!_contextComparer.HasSameValues(contextDto, existingGroundTruthEntry.GroundTruthContext))
            {
                _logger.LogInformation("Updating existing context with ID {ContextId}", contextDto.ContextId);

                // convert to entity
                var contextEntity = new GroundTruthContext
                {
                    ContextId = contextDto.ContextId,
                    GroundTruthId = groundTruthId,
                    GroundTruthEntryId = contextDto.GroundTruthEntryId == Guid.Empty ? Guid.NewGuid() : contextDto.GroundTruthEntryId,
                    ContextType = contextDto.ContextType,
                    ContextParameters = contextDto.ContextParameters.Select(p => new ContextParameter
                    {
                        ParameterId = p.ParameterId != Guid.Empty ? p.ParameterId : Guid.NewGuid(),
                        ContextId = contextDto.ContextId,
                        ParameterName = p.ParameterName,
                        ParameterValue = p.ParameterValue,
                        DataType = p.DataType
                    }).ToList()
                };

                // update record in table
                await _groundTruthRepository.UpdateGroundTruthContextAndRelatedEntitiesAsync(groundTruthId, contextEntity);
            }
            else
            {
                _logger.LogInformation("No changes detected for context with ID {ContextId}, skipping update.", contextDto.ContextId);
            }
        }

        private async Task addNewContextAsync(GroundTruthContextDto contextDto, Guid groundTruthId)
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
                    ParameterId = p.ParameterId != Guid.Empty ? p.ParameterId : Guid.NewGuid(),
                    ContextId = contextDto.ContextId,
                    ParameterName = p.ParameterName,
                    ParameterValue = p.ParameterValue,
                    DataType = p.DataType
                }).ToList()
            };

            await _groundTruthRepository.AddGroundTruthContextAndRelatedEntitiesAsync(groundTruthId, newContext);
        }
    }
}
