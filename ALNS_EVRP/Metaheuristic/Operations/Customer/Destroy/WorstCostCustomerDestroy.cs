using ALNS_EVRP.Data;
using ALNS_EVRP.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ALNS_EVRP.Metaheuristic
{
    /// <summary>
    /// Destroy that implements the worst cost customer destroy
    /// </summary>
    public class WorstCostCustomerDestroy : DestroyOperation
    {
        public WorstCostCustomerDestroy(int allOps) : base(allOps) { }

        public override Solution Destroy(Solution sol, int q, float p)
        {
            // Save here the removed customers
            List<Node> removedCustomers = new List<Node>();

            // I get only the customers in routes, and I order descending them by cost
            List<Node> customers = new List<Node>();
            foreach (Route r in sol.Routes)
                customers.AddRange(r.Nodes.Where(c => c.Type == TypeNode.Customer)
                        .OrderByDescending(s => GetCost(s))
                        .ToList());

            // I update the number of customers to remove if are minor than q
            int numCustomers = customers.Count;
            if (numCustomers < q)
                q = numCustomers;

            // I remove worst q customers in order of their cost
            for (int removed = 0; removed < q; removed++)
            {
                // y ∈ [0,1)
                double y = _rand.NextDouble();

                // I add to remove list using linear selection algorithm
                Node customerSelected = customers[(int)(Math.Pow(y, p) * customers.Count)];
                removedCustomers.Add(customerSelected);
                customers.Remove(customerSelected);
            }

            // I remove all nodes removed before
            foreach (Node n in removedCustomers)
                sol.DeleteNode(n.RouteIndex, n.NodeIndex);

            // return the new solution
            return sol;
        }
    }
}
