﻿using ALNS_EVRP.Data;
using ALNS_EVRP.Model;

namespace ALNS_EVRP.Metaheuristic
{
    /// <summary>
    /// Repair that implement the greedy station with comparison repair
    /// </summary>
    public class GreedyStationWithComparisonRepair : RepairOperation
    {
        public GreedyStationWithComparisonRepair(int allOps) : base(allOps) { }

        public override Solution Repair(Solution sol, int requestBankSelected = 0)
        {
            for (int r = 0; r < sol.Routes.Count; r++)
            {
                // Initialize all variable for best cost
                Node bestStation = null;
                float bestCost = float.PositiveInfinity;
                int bestRouteIndex = -1;
                int bestNodeIndex = -1;

                int n;

                // I find the best node with battery remaining negative or I arrive at last node
                for (n = 1; n < sol.Routes[r].Nodes.Count && sol.Routes[r].Nodes[n].BatteryRemaining >= 0; n++) ;

                // If I arrive at final depot, I go to the next route
                if (n == sol.Routes[r].Nodes.Count)
                    continue;

                // I would find the best station to insert in route where is not feasible
                // and try to add it after the start depot or after a station
                // When I found the best station between the arch to the customer
                // and the previous arch to previous customer, I stop
                while (bestNodeIndex == -1 && n > 0 && sol.Routes[r].Nodes[n].Type != TypeNode.Station)
                {
                    // I cycle all stations
                    foreach (Node station in sol.Stations)
                    {
                        // I evaluate the best speed to set at insert node
                        foreach (Speed speedSet in sol.Routes[r].Vehicle.Speeds)
                        {
                            // I evaluate the insertion before the customer node selected
                            Cost newCost = new Cost(sol.Alpha, sol.Beta, sol.Gamma, sol.Delta, sol.BetaViol, sol.GammaViol, sol.DeltaViol);
                            station.SpeedArrive = speedSet;
                            sol.EvaluateInsertNode(r, n, station.Clone(), newCost);

                            // If the new route cost inserting the station in route r at index n
                            // is better than the best, I save the position and the best new cost
                            if (newCost.Total < bestCost && newCost.IsFeasible)
                            {
                                bestRouteIndex = r;
                                bestNodeIndex = n;
                                bestCost = newCost.Total;
                                bestStation = station.Clone();
                            }
                        }

                        // If I'm not at start depot or first customer, I evaluate 2 previous nodes
                        if (n >= 1)
                        {
                            // I evaluate the best speed to set at insert node
                            foreach (Speed speedSet in sol.Routes[r].Vehicle.Speeds)
                            {
                                // I evaluate the insertion before the customer node selected
                                Cost newCost = new Cost(sol.Alpha, sol.Beta, sol.Gamma, sol.Delta, sol.BetaViol, sol.GammaViol, sol.DeltaViol);
                                station.SpeedArrive = speedSet;
                                sol.EvaluateInsertNode(r, n - 1, station.Clone(), newCost);

                                // If the new route cost inserting the station in route r at index n - 1
                                // is better than the best, I save the position and the best new cost
                                if (newCost.Total < bestCost && newCost.IsFeasible)
                                {
                                    bestNodeIndex = n - 1;
                                    bestCost = newCost.Total;
                                    bestStation = station.Clone();
                                }
                            }
                        }
                    }

                    // I go to 2 previous nodes
                    n -= 2;
                }

                // If I find a best position to insert the station, I add it in this position
                if (bestNodeIndex != -1)
                    sol.InsertNode(bestRouteIndex, bestNodeIndex, bestStation);
            }

            // return the new solution
            return sol;
        }
    }
}
