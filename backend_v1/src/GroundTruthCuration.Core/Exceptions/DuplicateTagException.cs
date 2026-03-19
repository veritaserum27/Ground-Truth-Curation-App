namespace GroundTruthCuration.Core.Exceptions;

/// <summary>
/// Exception thrown when attempting to create a tag that already exists.
/// </summary>
public class DuplicateTagException : Exception
{
    public string TagName { get; }

    public DuplicateTagException(string tagName)
        : base($"A tag with name '{tagName}' already exists.")
    {
        TagName = tagName;
    }

    public DuplicateTagException(string tagName, Exception inner)
        : base($"A tag with name '{tagName}' already exists.", inner)
    {
        TagName = tagName;
    }
}
