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
using System.IO;

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

        private static int FitWithin(int x, int min, int max)
        {
            if (x < min)
                x = min;
            if (x > max)
                x = max;
            return x;
        }

        private void UpdateNodeRadius(int minRadius, int maxRadius)
        {
            const int lengthLB = 16;
            const int lengthUB = 0x100000;

            int minLength = lengthUB;
            int maxLength = lengthLB;
            foreach (CallGraphNode node in graph.Nodes)
            {
                int length = node.Procedure.Size;
                if (length < minLength)
                    minLength = length;
                if (length > maxLength)
                    maxLength = length;
            }
            minLength = FitWithin(minLength, lengthLB, lengthUB);
            maxLength = FitWithin(maxLength, lengthLB, lengthUB);
            double log_min = Math.Log(minLength);
            double log_max = Math.Log(maxLength);

            foreach (CallGraphNode node in graph.Nodes)
            {
                int length = node.Procedure.Length;
                length = FitWithin(length, lengthLB, lengthUB);

                double ratio = (Math.Log(length) - log_min) / (log_max - log_min);
                node.SizeScale = ratio;
                node.Radius = minRadius + (int)(ratio * (maxRadius - minRadius));
            }
        }

        /// <summary>
        /// Draws the call graph randomly on the form. This is a benchmark of
        /// how messy it can be.
        /// </summary>
        private void DrawGraphRandomly()
        {
            int i = 0;
            int itemSize = 80;
            int numItemsPerRow = 15;
            int minRadius = itemSize / 16;
            int maxRadius = itemSize / 4;

            UpdateNodeRadius(minRadius, maxRadius);

            this.SuspendLayout();
            foreach (CallGraphNode node in graph.Nodes)
            {
                int x = itemSize / 2 + (i % numItemsPerRow) * itemSize;
                int y = itemSize / 2 + (i / numItemsPerRow) * itemSize;
                node.Center = new Point(x, y);
#if false
                Label label = new Label();
                label.Location = node.Center;
                label.Text = ""; //  node.Procedure.EntryPoint.ToString();
                this.Controls.Add(label);
                //node.Label = label;
#endif

                i++;
            }
            panelCanvas.Location = new Point(0, 0);
            panelCanvas.Width = numItemsPerRow * itemSize;
            panelCanvas.Height = (i + numItemsPerRow - 1) / numItemsPerRow * itemSize;
            this.ResumeLayout();
        }

        private Pen thickPen = new Pen(Color.Black, 2);

        private void panelCanvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            //g.TranslateTransform(this.AutoScrollPosition.X, this.AutoScrollPosition.Y);
            foreach (CallGraphEdge edge in graph.Edges)
            {
                Point p1 = edge.Source.Center;
                Point p2 = edge.Target.Center;
                if (edge.Target.Procedure.CallType == CallType.Near)
                    g.DrawLine(Pens.Black, p1, p2);
                else
                    g.DrawLine(thickPen, p1, p2);
            }
            foreach (CallGraphNode node in graph.Nodes)
            {
                Rectangle rect = new Rectangle(
                    x: node.Center.X - node.Radius,
                    y: node.Center.Y - node.Radius,
                    width: node.Radius * 2,
                    height: node.Radius * 2
                    );

                Brush fill;
                var features = node.Procedure.Features;
                if ((features & ProcedureFeatures.HasInterrupt) != 0)
                    fill = Brushes.Cyan;
                else if ((features & ProcedureFeatures.HasFpu) != 0)
                    fill = Brushes.Yellow;
                else
                    fill = Brushes.White;

                g.FillEllipse(fill, rect);
                g.DrawEllipse(Pens.Black, rect);
            }
        }

        private int GetDistanceSquared(Point pt1, Point pt2)
        {
            return (pt1.X - pt2.X) * (pt1.X - pt2.X) + (pt1.Y - pt2.Y) * (pt1.Y - pt2.Y);
        }

        private void panelCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            // Find out if we are on any node.
            foreach (CallGraphNode node in graph.Nodes)
            {
                if (GetDistanceSquared(e.Location, node.Center) <= node.Radius * node.Radius)
                {
                    txtStatus.Text = string.Format(
                        "{0} sub_{1}",
                        node.Procedure.EntryPoint,
                        node.Procedure.EntryPoint.LinearAddress);
                    return;
                }
            }
            txtStatus.Text = "";
        }

        private void btnOutputDot_Click(object sender, EventArgs e)
        {
            const double minWidth = 1.2, minHeight = 0.4;
            const double maxWidth = 3.6, maxHeight = 1.2;

            using (StreamWriter writer = new StreamWriter(
                @"D:\Run\GraphViz\release\bin\MyCallGraph.txt"))
            {
                writer.WriteLine("digraph G {");
                writer.WriteLine("  node[shape=box, style=\"rounded,filled\", fixedsize=true];");
                // width = 1.2, height = 0.4

                foreach (var node in graph.Nodes)
                {
                    string fillColor;
                    var features = node.Procedure.Features;
                    if ((features & ProcedureFeatures.HasInterrupt) != 0)
                        fillColor = "cyan";
                    else
                        fillColor = "lightgray";
                    writer.WriteLine("  sub_{0} [width={1}, height={2}, fillcolor={3}];",
                        node.Procedure.EntryPoint.LinearAddress,
                        minWidth + (maxWidth - minWidth) * node.SizeScale,
                        minHeight + (maxHeight - minHeight) * node.SizeScale,
                        fillColor);
                }

                foreach (var edge in graph.Edges)
                {
                    writer.Write("  sub_{0} -> sub_{1}",
                        edge.Source.Procedure.EntryPoint.LinearAddress,
                        edge.Target.Procedure.EntryPoint.LinearAddress);
                    if (edge.Target.Procedure.CallType == CallType.Near)
                    {
                        writer.Write(" [style=dotted]");
                    }
                    writer.WriteLine(";");
                }

                writer.WriteLine("}");
            }
        }
    }

    class CallGraphNode
    {
        public Procedure Procedure;
        public Point Center; // (x,y) on screen
        public int Radius;

        /// <summary>
        /// A float number between [0,1], where 0 indicates the smallest
        /// procedure and 1 indicates the largest procedure. 
        /// </summary>
        public double SizeScale;
        //public Label Label;
    }

    class CallGraphEdge : IGraphEdge<CallGraphNode>
    {
        public CallGraphNode Source { get; set; }

        public CallGraphNode Target { get; set; }
    }

    class CallGraph : Graph<CallGraphNode, CallGraphEdge>
    {
        // TODO: add node data to graph (allow system.void)
        Dictionary<Procedure, CallGraphNode> mapProcToNode
            = new Dictionary<Procedure, CallGraphNode>();

        /// <summary>
        /// Gets the source node.
        /// </summary>
        public CallGraphNode SourceNode { get; private set; }

        /// <summary>
        /// Gets the target node (also known as the sink node). If no target
        /// procedure is specified in the constructor, this value is null.
        /// </summary>
        public CallGraphNode TargetNode { get; private set; }

        public CallGraph(Procedure source)
        {
            // Build the call graph using depth-first search.
            //Dictionary<LinearPointer, bool> visited = new Dictionary<LinearPointer, bool>();

            //this.SourceNode = new CallGraphNode { Procedure = source };
            //this.mapProcToNode.Add(source.EntryPoint.LinearAddress, SourceNode);

            // TBD: we may turn this (non-tail) recursion into a list.
            SourceNode = BuildCallGraphNode(source);
        }

        private CallGraphNode BuildCallGraphNode(Procedure proc)
        {
            CallGraphNode node;
            if (mapProcToNode.TryGetValue(proc, out node))
                return node;

            node = new CallGraphNode { Procedure = proc };
            mapProcToNode[proc] = node;

            foreach (Procedure childProc in proc.GetCallees())
            {
                CallGraphNode childNode = BuildCallGraphNode(childProc);
                CallGraphEdge e = new CallGraphEdge
                {
                    Source = node,
                    Target = childNode,
                };
                base.AddEdge(e);
            }
            return node;
        }
    }
}
