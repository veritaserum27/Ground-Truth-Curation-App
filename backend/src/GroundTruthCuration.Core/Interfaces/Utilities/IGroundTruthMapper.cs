namespace GroundTruthCuration.Core.Interfaces;

public interface IGroundTruthMapper<in TSource, out TDestination>
{
    TDestination Map(TSource source);
}