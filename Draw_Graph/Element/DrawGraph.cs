using System.Collections.Generic;

namespace Draw_Graph.Element
{
    public class DrawGraph
    {
        public List<DrawNode> Nodes { get; set; }

        public List<DrawEdge> Edges { get; set; }

        public DrawGraph()
        {
            Nodes = new List<DrawNode>();
            Edges = new List<DrawEdge>();
        }
    }
}
