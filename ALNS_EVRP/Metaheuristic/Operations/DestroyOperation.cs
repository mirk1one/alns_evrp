using ALNS_EVRP.Data;
using ALNS_EVRP.Model;
using System;

namespace ALNS_EVRP.Metaheuristic
{
    public abstract class DestroyOperation : BaseOperation
    {
        protected readonly Random _rand = new Random();

        public DestroyOperation(int allOps) : base(allOps) { }

        /// <summary>
        /// Get the cost of the node, that is, from the previous node, the distance +
        /// the time to arrive at node + battery consumed to arrive
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        protected float GetCost(Node n) => n.Dist + n.Dist / (n.SpeedArrive.DistInHour) + n.BatteryConsumed;

        /// <summary>
        /// Get the relatedness measure between two nodes, that is the weighted distance +
        /// the weghted time + the weighted battery consumed between the two nodes
        /// </summary>
        /// <param name="n1">First node</param>
        /// <param name="n2">Second node</param>
        /// <param name="v">Vehicle that across the node</param>
        /// <param name="alpha">Weight of distance</param>
        /// <param name="beta">Weight of time</param>
        /// <param name="gamma">Weight of battery consume</param>
        /// <returns></returns>
        protected float GetRelatednessMeasure(Node n1, Node n2, ElectricVehicle v, float alpha, float beta) =>
            alpha * n1.GetDistance(n2) + beta * Math.Abs(n1.ReadyTime - n2.ReadyTime) +
            50 * (n1.RouteIndex == -1 || n2.RouteIndex == -1 || n1.RouteIndex != n2.RouteIndex ? 0 : 1) +
            50 * (n1.Demand - n2.Demand);

        /// <summary>
        /// Method that destroy routes, removing nodes
        /// </summary>
        /// <param name="sol">Enter solution found</param>
        /// <param name="q">Scope of the search</param>
        /// <param name="p">Randomness of selection request</param>
        /// <returns>The destroyed solution</returns>
        public abstract Solution Destroy(Solution sol, int q, float p);
    }
}
