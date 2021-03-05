using ALNS_EVRP.Data;
using ALNS_EVRP.Model;
using System.Collections.Generic;
using System.Linq;

namespace ALNS_EVRP.Metaheuristic
{
    /// <summary>
    /// Destroy that implements the worst charge usage station destroy
    /// </summary>
    public class WorstChargeUsageStationDestroy : DestroyOperation
    {
        private readonly float _minLevel = 0.7f;    // min level of high charge level (70%)

        public WorstChargeUsageStationDestroy(int allOps) : base(allOps) { }

        public override Solution Destroy(Solution sol, int q, float p = 1)
        {
            // I get only the stations in routes with high charge level (>= _minLevel)
            List<Node> highLevelStations = new List<Node>();
            foreach (Route r in sol.Routes)
                highLevelStations.AddRange(r.Nodes.Where(s => s.Type == TypeNode.Station && s.BatteryRemaining >= sol.Routes[s.RouteIndex].Vehicle.BatteryCapacity * _minLevel).ToList());

            // I update number of stations to remove if are minor than q
            int numStations = highLevelStations.Count;
            if (numStations < q)
                q = numStations;

            // I remove q high charge stations
            for (int removed = 0; removed < q; removed++)
            {
                // Select the first station found
                Node station = highLevelStations.First();
                highLevelStations.Remove(station);

                // I remove the node of route selected
                sol.DeleteNode(station.RouteIndex, station.NodeIndex);
            }

            // return the new solution
            return sol;
        }
    }
}
