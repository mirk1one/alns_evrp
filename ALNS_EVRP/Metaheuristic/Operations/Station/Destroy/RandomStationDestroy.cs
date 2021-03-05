using ALNS_EVRP.Data;
using ALNS_EVRP.Model;
using System.Collections.Generic;
using System.Linq;

namespace ALNS_EVRP.Metaheuristic
{
    /// <summary>
    /// Destroy that implements the random station destroy
    /// </summary>
    public class RandomStationDestroy : DestroyOperation
    {
        public RandomStationDestroy(int allOps) : base(allOps) { }

        public override Solution Destroy(Solution sol, int q, float p = 1)
        {
            // I get only the stations in routes
            List<Node> stations = new List<Node>();
            foreach (Route r in sol.Routes)
                stations.AddRange(r.Nodes.Where(s => s.Type == TypeNode.Station).ToList());

            // I update number of stations to remove if are minor than q
            int numStations = stations.Count;
            if (numStations < q)
                q = numStations;

            // I remove q casual stations
            for (int removed = 0; removed < q; removed++)
            {
                // I select a random station between the stations found in routes
                int selectedStationIndex = _rand.Next(0, stations.Count);
                Node selectedStation = stations[selectedStationIndex];
                stations.Remove(selectedStation);

                // I remove the node of route selected
                sol.DeleteNode(selectedStation.RouteIndex, selectedStation.NodeIndex);
            }

            // return the new solution
            return sol;
        }
    }
}
