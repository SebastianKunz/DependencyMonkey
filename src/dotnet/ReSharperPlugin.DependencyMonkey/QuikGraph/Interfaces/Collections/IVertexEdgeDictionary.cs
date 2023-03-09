#if SUPPORTS_SERIALIZATION || SUPPORTS_CLONEABLE
using System;
#endif
using JetBrains.Annotations;
using ReSharperPlugin.DependencyMonkey.QuikGraph.Interfaces.Edges;
using System.Collections.Generic;
#if SUPPORTS_SERIALIZATION
using System.Runtime.Serialization;
#endif

namespace ReSharperPlugin.DependencyMonkey.QuikGraph.Interfaces.Collections
{
    /// <summary>
    /// A cloneable dictionary of vertices associated to their edges.
    /// </summary>
    /// <typeparam name="TVertex">Vertex type.</typeparam>
    /// <typeparam name="TEdge">Edge type.</typeparam>
    public interface IVertexEdgeDictionary<TVertex, TEdge> : IDictionary<TVertex, IEdgeList<TVertex, TEdge>>
#if SUPPORTS_CLONEABLE
        , ICloneable
#endif
#if SUPPORTS_SERIALIZATION
        , ISerializable
#endif
     where TEdge : IEdge<TVertex>
    {
        /// <summary>
        /// Gets a clone of the dictionary. The vertices and edges are not cloned.
        /// </summary>
        /// <returns>Cloned dictionary.</returns>
        [Pure]
        [NotNull]
#if SUPPORTS_CLONEABLE
        new
#endif
        IVertexEdgeDictionary<TVertex, TEdge> Clone();
    }
}