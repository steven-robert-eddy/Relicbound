using System.Reflection;

namespace Relicbound.Content;

/// <summary>
/// Anchor type used to locate the <c>Relicbound.Content</c> assembly by
/// reflection, e.g. to enumerate its embedded artifact JSON.
/// </summary>
public static class ContentAssembly
{
    public static Assembly Reference => typeof(ContentAssembly).Assembly;
}
