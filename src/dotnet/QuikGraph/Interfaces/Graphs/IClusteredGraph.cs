﻿using System.Collections;
using JetBrains.Annotations;

namespace QuikGraph
{
    /// <summary>
    /// Represents a graph cluster.
    /// </summary>
    public interface IClusteredGraph
    {
        /// <summary>
        /// Graph clusters.
        /// </summary>
        [NotNull, ItemNotNull]
        IEnumerable Clusters { get; }

        /// <summary>
        /// Number of clusters.
        /// </summary>
        int ClustersCount { get; }

        /// <summary>
        /// Gets or sets the collapse state of this cluster.
        /// </summary>
        bool Collapsed { get; set; }

        /// <summary>
        /// Adds a new cluster.
        /// </summary>
        /// <returns>The added cluster.</returns>
        [NotNull]
        IClusteredGraph AddCluster();

        /// <summary>
        /// Removes the given graph from this cluster.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        void RemoveCluster([NotNull] IClusteredGraph graph);
    }
}