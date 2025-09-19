namespace GroundTruthCuration.Core.Interfaces;

public interface IGroundTruthComparer<in T1, in T2>
{
    bool HaveSameValues(T1 obj1, T2 obj2);
}