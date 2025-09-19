using GroundTruthCuration.Core.Interfaces;
using GroundTruthCuration.Core.DTOs;
using GroundTruthCuration.Core.Entities;

namespace GroundTruthCuration.Core.Utilities;

public class GroundTruthContextComparer : IGroundTruthComparer<GroundTruthContextDto, GroundTruthContext>
{
    private readonly IGroundTruthComparer<ContextParameterDto, ContextParameter> _contextParameterComparer;

    public GroundTruthContextComparer(IGroundTruthComparer<ContextParameterDto, ContextParameter> contextParameterComparer)
    {
        _contextParameterComparer = contextParameterComparer ?? throw new ArgumentNullException(nameof(contextParameterComparer));
    }

    public bool HasSameValues(GroundTruthContextDto contextDto, GroundTruthContext context)
    {
        if (contextDto == null || context == null)
        {
            return false;
        }

        // check root properties
        if (contextDto.ContextId != context.ContextId ||
            contextDto.GroundTruthId != context.GroundTruthId ||
            contextDto.ContextType != context.ContextType)
        {
            return false;
        }

        // check parameters
        if (contextDto.ContextParameters.Count != context.ContextParameters.Count)
        {
            return false;
        }

        foreach (var paramDto in contextDto.ContextParameters)
        {
            // find matching id
            var matchingParam = context.ContextParameters.FirstOrDefault(p => p.ParameterId == paramDto.ParameterId);
            if (matchingParam == null || !_contextParameterComparer.HasSameValues(paramDto, matchingParam))
            {
                return false;
            }
        }

        return true;
    }
}