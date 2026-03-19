using GroundTruthCuration.Core.Delegates;
using GroundTruthCuration.Core.DTOs;
using GroundTruthCuration.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GroundTruthCuration.Core.Services;

public class StatusService : IStatusService
{
    private readonly IDatastoreRepository _docDbRepository;
    private readonly IDatastoreRepository _relDbRepository;
    private readonly IGroundTruthRepository _groundTruthRepository;

    public StatusService(DatastoreRepositoryResolver datastoreRepositoryResolver, IGroundTruthRepository groundTruthRepository)
    {
        if (datastoreRepositoryResolver == null)
        {
            throw new ArgumentNullException(nameof(datastoreRepositoryResolver));
        }
        _docDbRepository = datastoreRepositoryResolver("ManufacturingDataDocDb");
        _relDbRepository = datastoreRepositoryResolver("ManufacturingDataRelDb");
        _groundTruthRepository = groundTruthRepository ?? throw new ArgumentNullException(nameof(groundTruthRepository));
    }
    public async Task<ICollection<DatabaseStatusDto>> GetDatabaseStatusesAsync()
    {
        var statuses = new List<DatabaseStatusDto>();

        // Implementation to retrieve database status
        var docDbStatus = await _docDbRepository.GetStatusAsync();
        var relDbStatus = await _relDbRepository.GetStatusAsync();
        var groundTruthStatus = await _groundTruthRepository.GetStatusAsync();

        statuses.Add(docDbStatus);
        statuses.Add(relDbStatus);
        statuses.Add(groundTruthStatus);

        return statuses;
    }
}