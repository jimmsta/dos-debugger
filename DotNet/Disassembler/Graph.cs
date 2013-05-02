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
        /// List of edges. This list has the same number of items as ListNext.
        /// </summary>
        List<TEdge> edgeData = new List<TEdge>();

        /// <summary>
        /// Contains the 'next' link of both vertices of each edge in the
        /// graph. That is, 
        /// <code>EdgeData[EdgeLink[i].NextIncoming].Target = EdgeData[i].Target</code>, and
        /// <code>EdgeData[EdgeLink[i].NextOutgoing].Source = EdgeData[i].Source</code>.
        /// </summary>
        List<EdgeLink> edgeLink = new List<EdgeLink>();

        /// <summary>
        /// Contains the index of the first edge pointing to or from each
        /// node. That is, 
        /// <code>edgeData[ListHead[node].NextIncoming].Target = node</code>, and
        /// <code>edgeData[ListHead[node].NextOutgoing].Source = node</code>.
        /// </summary>
        Dictionary<TNode, EdgeLink> ListHead = new Dictionary<TNode, EdgeLink>();

        /// <summary>
        /// Represents a node in the cross reference map. For performance
        /// reason, the actual node data (XRef object) is placed separately
        /// in the NodeData[] list. Therefore this structure only contains
        /// the node link fields.
        /// </summary>
        struct EdgeLink
        {
            /// <summary>
            /// Index of the next xref node that points to Target; -1 if none.
            /// </summary>
            public int NextIncoming;

            /// <summary>
            /// Index of the next xref node that points from Source; -1 if none.
            /// </summary>
            public int NextOutgoing;
        }

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
            edgeData.Clear();
            edgeLink.Clear();
            ListHead.Clear();
        }

        public ReadOnlyCollection<TEdge> Edges
        {
            get { return new ReadOnlyCollection<TEdge>(edgeData); }
        }

        private EdgeLink GetListHead(TNode node)
        {
            EdgeLink headLink;
            if (!ListHead.TryGetValue(node, out headLink))
            {
                headLink.NextOutgoing = -1;
                headLink.NextIncoming = -1;
            }
            return headLink;
        }

        public void AddEdge(TEdge edge)
        {
            int nodeIndex = edgeData.Count;

            EdgeLink nodeLink = new EdgeLink();

            EdgeLink headLink = GetListHead(edge.Source);
            if (compareEdges == null)
            {
                nodeLink.NextOutgoing = headLink.NextOutgoing;
                headLink.NextOutgoing = nodeIndex;
                ListHead[edge.Source] = headLink;
            }
            else
            {
                int i = headLink.NextOutgoing, prev = -1;
                while (i >= 0 && compareEdges(edge, edgeData[i]) >= 0)
                {
                    prev = i;
                    i = edgeLink[i].NextOutgoing;
                }
                nodeLink.NextOutgoing = i;
                if (prev >= 0)
                {
                    EdgeLink prevLink = edgeLink[prev];
                    prevLink.NextOutgoing = nodeIndex;
                    edgeLink[prev] = prevLink;
                }
                else
                {
                    headLink.NextOutgoing = nodeIndex;
                    ListHead[edge.Source] = headLink;
                }
            }

            headLink = GetListHead(edge.Target);
            if (compareEdges == null)
            {
                nodeLink.NextIncoming = headLink.NextIncoming;
                headLink.NextIncoming = nodeIndex;
                ListHead[edge.Target] = headLink;
            }
            else
            {
                int i = headLink.NextIncoming, prev = -1;
                while (i >= 0 && compareEdges(edge, edgeData[i]) >= 0)
                {
                    prev = i;
                    i = edgeLink[i].NextIncoming;
                }
                nodeLink.NextIncoming = i;
                if (prev >= 0)
                {
                    EdgeLink prevLink = edgeLink[prev];
                    prevLink.NextIncoming = nodeIndex;
                    edgeLink[prev] = prevLink;
                }
                else
                {
                    headLink.NextIncoming = nodeIndex;
                    ListHead[edge.Target] = headLink;
                }
            }

            // Since XRefLink is a struct, we must add it after updating all
            // its fields.
            edgeLink.Add(nodeLink);
            edgeData.Add(edge);
        }

        /// <summary>
        /// Enumerates incoming edges into the given node.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public IEnumerable<TEdge> GetIncomingEdges(TNode target)
        {
            for (int i = GetListHead(target).NextIncoming; i >= 0; i = edgeLink[i].NextIncoming)
            {
                //Debug.Assert(edgeData[i].Target.Equals(target));
                yield return edgeData[i];
            }
        }

        /// <summary>
        /// Enumerates outgoing edges from the given node.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public IEnumerable<TEdge> GetOutgoingEdges(TNode source)
        {
            for (int i = GetListHead(source).NextOutgoing; i >= 0; i = edgeLink[i].NextOutgoing)
            {
                //Debug.Assert(edgeData[i].Source.Equals(source));
                yield return edgeData[i];
            }
        }
    }

    public interface IGraphEdge<TNode>
    {
        TNode Source { get; }
        TNode Target { get; }
    }
}
