using GroundTruthCuration.Core.DTOs;
using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;

namespace GroundTruthCuration.Core.Utilities;

public class ContextParameterComparer : IGroundTruthComparer<ContextParameterDto, ContextParameter>
{
    public bool HasSameValues(ContextParameterDto contextParameterDto, ContextParameter contextParameter)
    {
        if (contextParameterDto == null || contextParameter == null)
        {
            return false;
        }

        // check properties
        if (contextParameterDto.ParameterId != contextParameter.ParameterId ||
            contextParameterDto.DataType != contextParameter.DataType ||
            contextParameterDto.ParameterName != contextParameter.ParameterName ||
            contextParameterDto.ParameterValue != contextParameter.ParameterValue)
        {
            return false;
        }

        return true;
    }
}