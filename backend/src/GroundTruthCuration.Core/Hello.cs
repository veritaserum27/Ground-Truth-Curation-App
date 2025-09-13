namespace GroundTruthCuration.Core;

public class Hello : IHello
{
    public string GetHelloMessage(string name)
    {
        return $"Hello, {name}!";
    }
}
