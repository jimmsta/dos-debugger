using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Disassembler;
using X86Codec;
using Util.Data;

namespace DosDebugger
{
    public partial class CallGraphWindow : Form
    {
        public CallGraphWindow()
        {
            InitializeComponent();
        }

        public Procedure SourceProcedure { get; set; }
        private CallGraph graph;

        private void CallGraphWindow_Load(object sender, EventArgs e)
        {
            this.graph = new CallGraph(this.SourceProcedure);
            this.Text = string.Format("{0} procedures", graph.Nodes.Count);

            // Draw the graph.
            DrawGraphRandomly();

            //At the same time,
            // assign a layer number to each node. The source node
            // has layer number 0, the called procedures have layer
            // number 1, etc.
            // However, we also need to remove cycles.
        }

        /// <summary>
        /// Draws the call graph randomly on the form. This is a benchmark of
        /// how messy it can be.
        /// </summary>
        private void DrawGraphRandomly()
        {
            int i = 0;
            int numItemsInRow = 10;
            this.SuspendLayout();
            foreach (CallGraphNode node in graph.Nodes)
            {
                int x = 50 + (i % numItemsInRow) * 100;
                int y = 50 + (i / numItemsInRow) * 100;
                node.Location = new Point(x, y);
                Label label = new Label();
                label.Location = node.Location;
                label.Text = node.Procedure.EntryPoint.ToString();
                this.Controls.Add(label);
                node.Label = label;
                i++;
                //;
            }

            // Connect nodes.
            using (Graphics g = Graphics.FromHwnd(this.Handle))
            {
                foreach (CallGraphEdge edge in graph.Edges)
                {
                    Point p1 = edge.Source.Location;
                    Point p2 = edge.Target.Location;
                    g.DrawLine(Pens.Black, p1, p2);
                    //edge.Source.Label.ForeColor = Color.Blue;
                    //edge.Target.Label.ForeColor = Color.Blue;
                }
            }

            this.ResumeLayout();
        }

        private void CallGraphWindow_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TranslateTransform(this.AutoScrollPosition.X, this.AutoScrollPosition.Y);
            foreach (CallGraphEdge edge in graph.Edges)
            {
                Point p1 = edge.Source.Location;
                Point p2 = edge.Target.Location;
                g.DrawLine(Pens.Black, p1, p2);
            }
        }
    }

    class CallGraphNode
    {
        public Procedure Procedure;
        public Point Location; // (x,y) on screen
        public Label Label;
    }

    class CallGraphEdge : IGraphEdge<CallGraphNode>
    {
        public CallGraphNode Source { get; set; }

        public CallGraphNode Target { get; set; }
    }

    class CallGraph : Graph<CallGraphNode, CallGraphEdge>
    {
        // TODO: add node data to graph (allow system.void)
        Dictionary<LinearPointer, CallGraphNode> mapEntryToNode
            = new Dictionary<LinearPointer, CallGraphNode>();

        /// <summary>
        /// Gets the source node.
        /// </summary>
        public CallGraphNode SourceNode { get; private set; }

        /// <summary>
        /// Gets the target node (also known as the sink node). If no target
        /// procedure is specified in the constructor, this value is null.
        /// </summary>
        //public CallGraphNode TargetNode { get; private set; }

        public CallGraph(Procedure sourceProcedure)
        {
            // Build the call graph using depth-first search.
            Dictionary<LinearPointer, bool> called = new Dictionary<LinearPointer, bool>();

            this.SourceNode = new CallGraphNode { Procedure = sourceProcedure };
            this.mapEntryToNode.Add(sourceProcedure.EntryPoint.LinearAddress, SourceNode);
            BuildCallGraph(SourceNode, called);
        }

        private void BuildCallGraph(CallGraphNode p, IDictionary<LinearPointer, bool> called)
        {
            foreach (Procedure child in p.Procedure.GetCallees())
            {
                LinearPointer entryPoint = child.EntryPoint.LinearAddress;

                CallGraphNode v;
                if (!mapEntryToNode.TryGetValue(entryPoint, out v))
                {
                    v = new CallGraphNode { Procedure = child };
                    mapEntryToNode.Add(entryPoint, v);
                }

                bool hasCycle = called.ContainsKey(entryPoint);
                CallGraphEdge e = new CallGraphEdge
                {
                    Source = p,
                    Target = v,
                };

                base.AddEdge(e);
                if (!hasCycle)
                {
                    called[entryPoint] = true;
                    BuildCallGraph(v, called);
                    called.Remove(entryPoint);
                }
            }
        }
    }
}
