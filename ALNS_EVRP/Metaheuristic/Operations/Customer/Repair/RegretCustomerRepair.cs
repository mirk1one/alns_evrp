using ALNS_EVRP.Data;
using ALNS_EVRP.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ALNS_EVRP.Metaheuristic
{
    /// <summary>
    /// Repair that implement the k-regret customer repair
    /// </summary>
    public class RegretCustomerRepair : RepairOperation
    {
        private readonly int _k;

        public RegretCustomerRepair(int k, int allOps) : base(allOps)
        {
            if (k < 2)
                throw new IOException("RegretRepair k value must be >= 2");
            _k = k;
        }

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
                    float[] deltas = Enumerable.Repeat(float.PositiveInfinity, _k).ToArray();
                    Speed bestSpeed = null;
                    int bestNodeIndex = -1;
                    int bestRouteIndex = -1;

                    // I cycle in all routes
                    for (int r = 0; r < sol.Routes.Count; r++)
                    {
                        // I cycle in all nodes except on start depot
                        for (int n = 1; n < sol.Routes[r].Nodes.Count; n++)
                        {
                            // I save all best data of insert node
                            Cost newCost = new Cost(sol.Alpha, sol.Beta, sol.Gamma, sol.Delta, sol.BetaViol, sol.GammaViol, sol.DeltaViol);
                            Speed bestSpeedCurr = null;
                            float bestSpeedCost = float.PositiveInfinity;

                            // I evaluate the best speed to set at insert node
                            foreach (Speed speedSet in sol.Routes[r].Vehicle.Speeds)
                            {
                                // I evaluate the insertion
                                Cost newCostCurr = new Cost(sol.Alpha, sol.Beta, sol.Gamma, sol.Delta, sol.BetaViol, sol.GammaViol, sol.DeltaViol);
                                insertNode.SpeedArrive = speedSet;
                                sol.EvaluateInsertNode(r, n, insertNode, newCostCurr);

                                // If the new route cost inserting the node in route r at index n
                                // is better than the best, i save the position and the best new cost
                                if (newCostCurr.Total < bestSpeedCost)
                                {
                                    bestSpeedCost = newCostCurr.Total;
                                    newCost = newCostCurr.Clone();
                                    bestSpeedCurr = speedSet;
                                }
                            }

                            // I save in deltas all costs in order of cost
                            for (int e = 0; e < _k; e++)
                                if (newCost.Total < deltas[e])
                                {
                                    if (e == 0)
                                    {
                                        bestNodeIndex = n;
                                        bestRouteIndex = r;
                                        bestSpeed = bestSpeedCurr;
                                    }
                                    for (int a = _k - 1; a > e; a--)
                                        deltas[a] = deltas[a - 1];
                                    deltas[e] = newCost.Total;
                                    break;
                                }
                        }
                    }

                    // I calculate the total delta with sum of difference between delta 2 ... k minus delta 1
                    float totalDelta = 0;
                    for (int e = 1; e < _k; e++) totalDelta += deltas[e] - deltas[0];

                    // I insert the node in the best position
                    insertNode.SpeedArrive = bestSpeed;
                    bestPositions.Add(new Position() { RouteIndex = bestRouteIndex, NodeIndex = bestNodeIndex, InsertNode = insertNode.Clone(), Cost = totalDelta });
                }

                // I get the best position with maximum delta cost and I add it at solution (and remove it from request bank)
                Position bestPos = bestPositions.OrderByDescending(p => p.Cost).FirstOrDefault();
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

    public class Regret2CustomerRepair : RegretCustomerRepair
    {
        public Regret2CustomerRepair(int allOps) : base(2, allOps) { }
    }

    public class Regret3CustomerRepair : RegretCustomerRepair
    {
        public Regret3CustomerRepair(int allOps) : base(3, allOps) { }
    }

    public class Regret4CustomerRepair : RegretCustomerRepair
    {
        public Regret4CustomerRepair(int allOps) : base(4, allOps) { }
    }
}
