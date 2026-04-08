namespace GitExtensions.Extensibility.Git;

public interface IGitItem
{
    /// <summary>
    /// Gets the object ID, or <see langword="null"/> if not known.
    /// </summary>
    /// <remarks>
    /// Nullable because some implementations (e.g. <c>GitRef</c>) may not have a resolved object ID,
    /// such as when representing remote tracking configurations or the <c>NoHead</c> sentinel.
    /// </remarks>
    ObjectId? ObjectId { get; }

    string? Guid { get; }
}
