using Draw_Graph.Element;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Layout.MDS;
using System.Windows.Forms;

namespace Draw_Graph.Utils
{
    public static class GraphUtils
    {
        public static void ShowGraph(DrawGraph drawGraph)
        {
            Form form = new Form();
            GViewer viewer = new GViewer();

            var graph = new Graph();

            foreach(var e in drawGraph.Edges)
            {
                Edge edge = graph.AddEdge(e.StartNodeId, e.EndNodeId);
                edge.Attr.Color = ColorUtils.GetColor(e.Color);
                edge.LabelText = e.Label;
            }

            foreach(var n in drawGraph.Nodes)
            {
                Node node = graph.FindNode(n.Label);
                node.Attr.FillColor = ColorUtils.GetColor(n.Color);
                node.Attr.Shape = n.Type == DrawNodeType.Circle ? Shape.Circle :
                                    n.Type == DrawNodeType.Rectangle ? Shape.Box : Shape.House;
                node.Label.FontSize = 3;
            }

            graph.CreateGeometryGraph();

            foreach (var n in drawGraph.Nodes)
            {
                Node node = graph.FindNode(n.Label);
                NodeUtils.CreateLabelAndBoundary(node, node.Attr.Shape, n.X, n.Y);
            }

            foreach (var edge in graph.Edges)
            {
                edge.Attr.ArrowheadAtTarget = ArrowStyle.Normal;
                if (edge.Label != null)
                    EdgeUtils.CreateLabel(edge);
            }

            var geomGraph = graph.GeometryGraph;

            var geomGraphComponents = Microsoft.Msagl.Core.Layout.GraphConnectedComponents.CreateComponents(geomGraph.Nodes, geomGraph.Edges);
            var settings = new SugiyamaLayoutSettings();
            foreach (var subgraph in geomGraphComponents)
            {
                var layout = new LayeredLayout(subgraph, settings);
                subgraph.Margins = settings.NodeSeparation / 2;
                layout.Run();
            }

            MdsGraphLayout.PackGraphs(geomGraphComponents, settings);

            geomGraph.UpdateBoundingBox();

            viewer.NeedToCalculateLayout = false;
            viewer.Graph = graph;

            form.SuspendLayout();
            viewer.Dock = DockStyle.Fill;
            form.Controls.Add(viewer);
            form.ResumeLayout();
            form.Height = 600;
            form.Width = 600;
            //show the form 
            form.ShowDialog();
        }
    }
}
