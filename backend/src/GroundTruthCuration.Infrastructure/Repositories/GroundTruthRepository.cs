using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using GroundTruthCuration.Core.DTOs;

namespace GroundTruthCuration.Infrastructure.Repositories;

public class GroundTruthRepository : IGroundTruthRepository
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public GroundTruthRepository(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        _connectionString = _configuration.GetConnectionString("GroundTruthConnectionString");
    }

    public Task<GroundTruthDefinition> AddGroundTruthDefinitionAsync(GroundTruthDefinition groundTruthDefinition)
    {
        throw new NotImplementedException();
    }

    public Task DeleteGroundTruthDefinitionAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsGroundTruthDefinitionAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<GroundTruthDefinition>> GetAllGroundTruthDefinitionsAsync(GroundTruthDefinitionFilter filter)
    {
        string baseSql = "SELECT * FROM GroundTruthDefinitions";

        DynamicParameters parameters = new DynamicParameters();

        if (filter.UserId is not null)
        {
            parameters.Add("UserId", filter.UserId);
        }
        if (filter.ValidationStatus is not null)
        {
            parameters.Add("ValidationStatus", filter.ValidationStatus);
        }
        if (filter.CreatedDateRange is not null)
        {
            if (filter.CreatedDateRange?.Start is not null)
            {
                parameters.Add("StartDate", filter.CreatedDateRange?.Start);
            }
            if (filter.CreatedDateRange?.End is not null)
            {
                parameters.Add("EndDate", filter.CreatedDateRange?.End);
            }
        }
        if (filter.GroundTruthContext is not null)
        {
            // TODO: get parameters from context
        }

        if (parameters.ParameterNames.Any())
        {
            baseSql += " WHERE " + string.Join(" AND ", parameters.ParameterNames.Select(name => $"{name} = @{name}"));
        }

        baseSql += ";";

        // connect to database
        using (var connection = new SqlConnection(_connectionString))
        {
            var results = await connection.QueryAsync<GroundTruthDefinition>(baseSql, parameters);
            return results;
        }
    }

    public Task<GroundTruthDefinition?> GetGroundTruthDefinitionByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GroundTruthDefinition>> GetGroundTruthDefinitionsByUserAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GroundTruthDefinition>> GetGroundTruthDefinitionsByValidationStatusAsync(string validationStatus)
    {
        throw new NotImplementedException();
    }

    public Task<GroundTruthDefinition> UpdateGroundTruthDefinitionAsync(GroundTruthDefinition groundTruthDefinition)
    {
        throw new NotImplementedException();
    }
}