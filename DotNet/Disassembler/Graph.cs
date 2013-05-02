using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Disassembler
{
    /// <summary>
    /// Represents a directed graph with node type TNode and edge type TEdge.
    /// The graph is internally stored as an adjacency list. See
    /// http://en.wikipedia.org/wiki/Adjacency_list for related information
    /// on this data structure.
    /// </summary>
    public class Graph<TNode, TEdge> where TEdge : IGraphEdge<TNode>
    {
        /// <summary>
        /// List of edges. This list has the same number of items as
        /// NextIncoming[] and NextOutgoing[].
        /// </summary>
        List<TEdge> edges = new List<TEdge>();

        /// <summary>
        /// NextIncoming[i] = index of the next edge that points into
        /// edgeData[i].Target.
        /// </summary>
        List<int> NextIncoming = new List<int>();

        /// <summary>
        /// NextOutgoing[i] = index of the next edge that points from
        /// edgeData[i].Source.
        /// </summary>
        List<int> NextOutgoing = new List<int>();

        /// <summary>
        /// FirstIncoming[v] = index of the first edge that points into v.
        /// </summary>
        Dictionary<TNode, int> FirstIncoming = new Dictionary<TNode, int>();

        /// <summary>
        /// FirstOutgoing[v] = index of the first edge that points from v.
        /// </summary>
        Dictionary<TNode, int> FirstOutgoing = new Dictionary<TNode, int>();

        /// <summary>
        /// Comparer to sort the adjacent edges into or out of a node.
        /// If this member is null, the edges are not sorted.
        /// </summary>
        Comparison<TEdge> compareEdges;

        /// <summary>
        /// Creates an empty directed graph. The edges adjacent to each node
        /// are not sorted, and will be returned in unspecified order when
        /// enumerated.
        /// </summary>
        public Graph()
        {
        }

        /// <summary>
        /// Creates a directed graph with the supplied comparison routine to
        /// sort the adjacent edges of a node.
        /// </summary>
        public Graph(Comparison<TEdge> edgeComparison)
        {
            this.compareEdges = edgeComparison;
        }

        /// <summary>
        /// Clears all nodes and edges in this graph.
        /// </summary>
        public void Clear()
        {
            edges.Clear();
            NextIncoming.Clear();
            NextOutgoing.Clear();
            FirstIncoming.Clear();
            FirstOutgoing.Clear();
        }

        public ReadOnlyCollection<TEdge> Edges
        {
            get { return new ReadOnlyCollection<TEdge>(edges); }
        }

        public void AddEdge(TEdge edge)
        {
            int nodeIndex = edges.Count;
            edges.Add(edge);
            NextOutgoing.Add(-1);
            NextIncoming.Add(-1);

            if (compareEdges == null)
            {
                NextOutgoing[nodeIndex] = FirstOutgoing.GetValueOrDefault(edge.Source, -1);
                FirstOutgoing[edge.Source] = nodeIndex;
            }
            else
            {
                int i = FirstOutgoing.GetValueOrDefault(edge.Source, -1);
                int prev = -1;
                while (i >= 0 && compareEdges(edge, edges[i]) >= 0)
                {
                    prev = i;
                    i = NextOutgoing[i];
                }

                if (prev >= 0)
                {
                    NextOutgoing[prev] = nodeIndex;
                }
                else
                {
                    FirstOutgoing[edge.Source] = nodeIndex;
                }
                NextOutgoing[nodeIndex] = i;
            }

            if (compareEdges == null)
            {
                NextIncoming[nodeIndex] = FirstIncoming.GetValueOrDefault(edge.Target, -1);
                FirstIncoming[edge.Target] = nodeIndex;
            }
            else
            {
                int i = FirstIncoming.GetValueOrDefault(edge.Target, -1);
                int prev = -1;
                while (i >= 0 && compareEdges(edge, edges[i]) >= 0)
                {
                    prev = i;
                    i = NextIncoming[i];
                }

                if (prev >= 0)
                {
                    NextIncoming[prev] = nodeIndex;
                }
                else
                {
                    FirstIncoming[edge.Target] = nodeIndex;
                }
                NextIncoming[nodeIndex] = i;
            }
        }

        /// <summary>
        /// Enumerates incoming edges into the given node.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public IEnumerable<TEdge> GetIncomingEdges(TNode target)
        {
            for (int i = FirstIncoming.GetValueOrDefault(target, -1); 
                 i >= 0; 
                 i = NextIncoming[i])
            {
                //Debug.Assert(edgeData[i].Target.Equals(target));
                yield return edges[i];
            }
        }

        /// <summary>
        /// Enumerates outgoing edges from the given node.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public IEnumerable<TEdge> GetOutgoingEdges(TNode source)
        {
            for (int i = FirstOutgoing.GetValueOrDefault(source, -1); 
                 i >= 0; 
                 i = NextOutgoing[i])
            {
                //Debug.Assert(edgeData[i].Source.Equals(source));
                yield return edges[i];
            }
        }
    }

    public interface IGraphEdge<TNode>
    {
        TNode Source { get; }
        TNode Target { get; }
    }

    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            TValue value;
            if (dictionary.TryGetValue(key, out value))
                return value;
            else
                return defaultValue;
        }
    }
}
