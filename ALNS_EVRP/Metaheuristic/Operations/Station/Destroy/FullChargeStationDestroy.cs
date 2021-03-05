using ALNS_EVRP.Data;
using ALNS_EVRP.Model;
using System.Collections.Generic;
using System.Linq;

namespace ALNS_EVRP.Metaheuristic
{
    /// <summary>
    /// Destroy that implements the full charge station destroy
    /// </summary>
    public class FullChargeStationDestroy : DestroyOperation
    {
        public FullChargeStationDestroy(int allOps) : base(allOps) { }

        public override Solution Destroy(Solution sol, int q, float p = 1)
        {
            // I get only the stations in routes with full charge level
            List<Node> fullLevelStations = new List<Node>();
            foreach (Route r in sol.Routes)
                fullLevelStations.AddRange(r.Nodes.Where(n => n.Type == TypeNode.Station &&
                                            n.BatteryRemaining == sol.Routes[n.RouteIndex].Vehicle.BatteryCapacity).ToList());

            // I update number of stations to remove if are minor than q
            int numStations = fullLevelStations.Count;
            if (numStations < q)
                q = numStations;

            // I remove q full charge stations
            for (int removed = 0; removed < q; removed++)
            {
                // Select the first station found
                Node station = fullLevelStations.First();
                fullLevelStations.Remove(station);

                // I remove the node of route selected
                sol.DeleteNode(station.RouteIndex, station.NodeIndex);
            }

            return sol;
        }
    }
}
