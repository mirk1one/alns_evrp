using ALNS_EVRP.Data;
using ALNS_EVRP.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ALNS_EVRP.Metaheuristic
{
    /// <summary>
    /// Destroy that implements the worst cost station destroy
    /// </summary>
    public class WorstCostStationDestroy : DestroyOperation
    {
        public WorstCostStationDestroy(int allOps) : base(allOps) { }

        public override Solution Destroy(Solution sol, int q, float p)
        {
            // Save here the removed stations
            List<Node> removedStations = new List<Node>();

            // I get only the stations in routes, and I order them by cost
            List<Node> stations = new List<Node>();
            foreach (Route r in sol.Routes)
                stations.AddRange(r.Nodes.Where(s => s.Type == TypeNode.Station)
                        .OrderByDescending(s => GetCost(s))
                        .ToList());

            // I update number of stations to remove if are minor than q
            int numStations = stations.Count;
            if (numStations < q)
                q = numStations;

            // I remove worst q stations in order of their relatedness
            for (int removed = 0; removed < q; removed++)
            {
                // y ∈ [0,1)
                double y = _rand.NextDouble();

                // I add to remove list using linear selection algorithm
                Node stationSelected = stations[(int)(Math.Pow(y, p) * stations.Count)];
                removedStations.Add(stationSelected);
                stations.Remove(stationSelected);
            }

            // I remove all stations selected before
            foreach(Node s in removedStations)
                sol.DeleteNode(s.RouteIndex, s.NodeIndex);

            // return the new solution
            return sol;
        }
    }
}
