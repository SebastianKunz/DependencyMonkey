using JetBrains.Annotations;
using ReSharperPlugin.DependencyMonkey.QuikGraph.Structures.Edges;
using System.Diagnostics;

namespace ReSharperPlugin.DependencyMonkey.Model;

/// <summary>
/// Equatable edge to override the equals method.
/// </summary>
[DebuggerDisplay("{" + nameof(Source) + "}->{" + nameof(Target) + "}")]
public class ProjectToUpgradeEdge : EquatableEdge<ProjectToUpgrade>
{
    public ProjectToUpgradeEdge([NotNull] ProjectToUpgrade source, [NotNull] ProjectToUpgrade target) : base(source, target)
    {
    }

    public override bool Equals(EquatableEdge<ProjectToUpgrade> other)
    {
        return Source.Equals(other?.Source) && Target.Equals(other?.Target);
    }
}