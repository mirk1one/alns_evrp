
namespace ALNS_EVRP.Data
{
    internal class Position
    {
        /// <summary>
        /// The route index where the node is going to insert
        /// </summary>
        public int RouteIndex { get; set; }

        /// <summary>
        /// The node index where the node is going to insert
        /// </summary>
        public int NodeIndex { get; set; }

        /// <summary>
        /// The node that will be inserted
        /// </summary>
        public Node InsertNode { get; set; }

        /// <summary>
        /// The cost to insert the node in that route at that position
        /// </summary>
        public float Cost { get; set; }
    }
}
