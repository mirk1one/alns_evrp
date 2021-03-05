using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using System.Drawing;

namespace Draw_Graph.Utils
{
    public static class NodeUtils
    {
        public static void CreateLabelAndBoundary(Node node, Shape shape, double x, double y)
        {
            node.Attr.Shape = Shape.DrawFromGeometry;
            node.Attr.LabelMargin *= 2;
            double width;
            double height;
            StringMeasure.MeasureWithFont(node.Label.Text, new Font(node.Label.FontName, (float)node.Label.FontSize), out width, out height);
            node.Label.Width = width;
            node.Label.Height = height;
            int r = node.Attr.LabelMargin;
            ICurve curve;
            if(shape == Shape.Box)
                curve = CurveFactory.CreateRectangle(width + r * 2, height + r * 2, new Microsoft.Msagl.Core.Geometry.Point(x, y));
            else if(shape == Shape.Circle)
                curve = CurveFactory.CreateCircle(width > height ? width : height, new Microsoft.Msagl.Core.Geometry.Point(x, y));
            else
                curve = CurveFactory.CreateHouse(width + r * 2, height + r, new Microsoft.Msagl.Core.Geometry.Point(x, y));
            node.GeometryNode.BoundaryCurve = curve;
        }
    }
}
