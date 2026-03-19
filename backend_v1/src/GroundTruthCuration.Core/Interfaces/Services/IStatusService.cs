using System.Collections.Generic;
using System.Threading.Tasks;
using GroundTruthCuration.Core.DTOs;
namespace GroundTruthCuration.Core.Interfaces;

public interface IStatusService
{
    Task<ICollection<DatabaseStatusDto>> GetDatabaseStatusesAsync();
}