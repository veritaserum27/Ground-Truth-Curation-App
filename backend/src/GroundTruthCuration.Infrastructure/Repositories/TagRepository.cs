using Dapper;
using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GroundTruthCuration.Infrastructure.Repositories;

/// <summary>
/// Dapper-based repository for Tag persistence.
/// </summary>
public class TagRepository : ITagRepository
{
    private readonly string _connectionString;
    private readonly ILogger<TagRepository> _logger;

    public TagRepository(ILogger<TagRepository> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (configuration is null) throw new ArgumentNullException(nameof(configuration));
        _connectionString = configuration.GetValue<string>("Datastores:GroundTruthCuration:ConnectionString")
            ?? throw new InvalidOperationException("The connection string 'Datastores:GroundTruthCuration:ConnectionString' is null or missing.");
    }

    public async Task<Tag> AddTagAsync(Tag tag)
    {
        const string sql = @"INSERT INTO [dbo].[TAG] (tagId, name, description) VALUES (@TagId, @Name, @Description);";
        using var connection = new SqlConnection(_connectionString);
        try
        {
            await connection.ExecuteAsync(sql, new { tag.TagId, tag.Name, tag.Description });
            return tag;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Failed to insert tag {TagId} ({Name})", tag.TagId, tag.Name);
            throw new InvalidOperationException("Failed to insert tag.", ex);
        }
    }

    public async Task<Tag?> GetTagByNameAsync(string name)
    {
        const string sql = @"SELECT tagId AS TagId, name AS Name, description AS Description FROM [dbo].[TAG] WHERE LOWER(name) = LOWER(@Name);";
        using var connection = new SqlConnection(_connectionString);
        try
        {
            return await connection.QueryFirstOrDefaultAsync<Tag>(sql, new { Name = name });
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Failed to fetch tag by name {Name}", name);
            throw new InvalidOperationException("Failed to retrieve tag by name.", ex);
        }
    }
}
