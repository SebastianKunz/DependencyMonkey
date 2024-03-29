﻿using JetBrains.Annotations;
using ReSharperPlugin.DependencyMonkey.QuikGraph.Interfaces.Edges;
using System.Collections.Generic;

namespace ReSharperPlugin.DependencyMonkey.QuikGraph.Interfaces.Graphs
{
    /// <summary>
    /// A mutable vertex and edge set.
    /// </summary>
    /// <typeparam name="TVertex">Vertex type.</typeparam>
    /// <typeparam name="TEdge">Edge type.</typeparam>
    public interface IMutableVertexAndEdgeSet<TVertex,TEdge>
        : IMutableVertexSet<TVertex>
        , IMutableEdgeListGraph<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        /// <summary>
        /// Adds <paramref name="edge"/> and its vertices to this graph.
        /// </summary>
        /// <param name="edge">The edge to add.</param>
        /// <returns>True if the edge was added, false otherwise.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="edge"/> is <see langword="null"/>.</exception>
        bool AddVerticesAndEdge([NotNull] TEdge edge);

        /// <summary>
        /// Adds a set of edges (and it's vertices if necessary).
        /// </summary>
        /// <param name="edges">Edges to add.</param>
        /// <returns>The number of edges added.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="edges"/> is <see langword="null"/> or at least one of them is <see langword="null"/>.
        /// </exception>
        int AddVerticesAndEdgeRange([NotNull, ItemNotNull] IEnumerable<TEdge> edges);
    }
}