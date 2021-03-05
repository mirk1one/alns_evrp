using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using System.Drawing;

namespace Draw_Graph.Utils
{
    public static class EdgeUtils
    {
        public static void CreateLabel(Edge edge)
        {
            var geomEdge = edge.GeometryEdge;
            double width;
            double height;
            StringMeasure.MeasureWithFont(edge.LabelText, new Font(edge.Label.FontName, (float)edge.Label.FontSize), out width, out height);
            edge.Label.GeometryLabel = geomEdge.Label = new Microsoft.Msagl.Core.Layout.Label(width, height, geomEdge);
        }
    }
}
