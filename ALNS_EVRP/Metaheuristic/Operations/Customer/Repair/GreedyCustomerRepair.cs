using ALNS_EVRP.Data;
using ALNS_EVRP.Model;
using System.Collections.Generic;
using System.Linq;

namespace ALNS_EVRP.Metaheuristic
{
    /// <summary>
    /// Repair that implement the greedy customer repair
    /// </summary>
    public class GreedyCustomerRepair : RepairOperation
    {
        public GreedyCustomerRepair(int allOps) : base(allOps) { }

        public override Solution Repair(Solution sol, int requestBankSelected)
        {
            // The total customer to insert are the removed customers + a subset in request bank if it is not empty
            int totInsertCustomers = sol.RemovedCustomers.Count + requestBankSelected;
            // I insert all removed customers in request bank
            while (sol.RemovedCustomers.Count != 0)
            {
                Node insertNode = sol.RemovedCustomers[0];
                sol.RemovedCustomers.RemoveAt(0);
                sol.RequestBank.Add(insertNode);
            }

            // I cycle on the number of customer to insert
            for (int c = 0; c < totInsertCustomers; c++)
            {
                // I save here the best positions
                List<Position> bestPositions = new List<Position>();

                // I cycle on the all customers in request bank
                for (int i = 0; i < sol.RequestBank.Count; i++)
                {
                    // I get the customer selected from request bank
                    Node insertNode = sol.RequestBank[i];

                    // Initialize all variable for best cost
                    float bestCost = float.PositiveInfinity;
                    Speed bestSpeed = null;
                    int bestNodeIndex = -1;
                    int bestRouteIndex = -1;

                    // I cycle in all routes
                    for (int r = 0; r < sol.Routes.Count; r++)
                    {
                        // I cycle in all nodes except on start depot
                        for (int n = 1; n < sol.Routes[r].Nodes.Count; n++)
                        {
                            // I evaluate the best speed to set at insert node
                            foreach (Speed speedSet in sol.Routes[r].Vehicle.Speeds)
                            {
                                // I evaluate the insertion
                                Cost newCost = new Cost(sol.Alpha, sol.Beta, sol.Gamma, sol.Delta, sol.BetaViol, sol.GammaViol, sol.DeltaViol);
                                insertNode.SpeedArrive = speedSet;
                                sol.EvaluateInsertNode(r, n, insertNode, newCost);

                                // If the new route cost inserting the node in route r at index n
                                // is better than the best, i save the position and the best new cost
                                if (newCost.Total < bestCost)
                                {
                                    bestNodeIndex = n;
                                    bestRouteIndex = r;
                                    bestCost = newCost.Total;
                                    bestSpeed = speedSet;
                                }
                            }
                        }
                    }

                    // I insert the node in the best position
                    insertNode.SpeedArrive = bestSpeed;
                    bestPositions.Add(new Position() { RouteIndex = bestRouteIndex, NodeIndex = bestNodeIndex, InsertNode = insertNode.Clone(), Cost = bestCost });
                }

                // I get the best position with minumum cost and I add it at solution (and remove it from request bank)
                Position bestPos = bestPositions.OrderBy(p => p.Cost).FirstOrDefault();
                if (bestPos != null)
                {
                    sol.InsertNode(bestPos.RouteIndex, bestPos.NodeIndex, bestPos.InsertNode);
                    sol.RequestBank.Remove(sol.RequestBank.Single(n => n.ID == bestPos.InsertNode.ID));
                }
            }

            // return the new solution
            return sol;
        }
    }
}
