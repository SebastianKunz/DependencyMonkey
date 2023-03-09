using JetBrains.Annotations;
using System.Collections.Generic;

namespace ReSharperPlugin.DependencyMonkey.QuikGraph.Interfaces.Graphs
{
    /// <summary>
    /// Represents a set of vertices.
    /// </summary>
    /// <typeparam name="TVertex">Vertex type.</typeparam>
    public interface IVertexSet<TVertex> : IImplicitVertexSet<TVertex>
    {
        /// <summary>
        /// Gets a value indicating whether there are no vertices in this set.
        /// It is true if this vertex set is empty, otherwise false.
        /// </summary>
        bool IsVerticesEmpty { get; }

        /// <summary>
        /// Gets the vertex count.
        /// </summary>
        int VertexCount { get; }

        /// <summary>
        /// Gets the vertices.
        /// </summary>
        [NotNull, ItemNotNull]
        IEnumerable<TVertex> Vertices { get; }
    }
}