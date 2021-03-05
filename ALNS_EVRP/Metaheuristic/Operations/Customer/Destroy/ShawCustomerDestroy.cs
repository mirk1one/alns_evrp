using ALNS_EVRP.Data;
using ALNS_EVRP.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ALNS_EVRP.Metaheuristic
{
    /// <summary>
    /// Destroy that implements the shaw customer destroy
    /// </summary>
    public class ShawCustomerDestroy : DestroyOperation
    {
        public ShawCustomerDestroy(int allOps) : base(allOps) { }

        public override Solution Destroy(Solution sol, int q, float p)
        {
            // I get only the stations in routes
            List<Node> customers = new List<Node>();
            foreach (Route r in sol.Routes)
                customers.AddRange(r.Nodes.Where(c => c.Type == TypeNode.Customer).ToList());

            // I update number of stations to remove if are minor than q
            int numCustomers = customers.Count;
            if (numCustomers < q)
                q = numCustomers;

            // If I don't remove customers, I return the solution
            if (q == 0)
                return sol;

            // I add to removed list one casual customer from one of the routes
            int selectedCustomer = _rand.Next(0, customers.Count);
            List<Node> removedCustomers = new List<Node>() { customers[selectedCustomer] };

            // I remove q - 1 customers in order of their relatedness with other customers
            for (int removed = 1; removed < q; removed++)
            {
                // I select one casual customer from the removed
                Node r = removedCustomers[_rand.Next(0, removedCustomers.Count)];

                // L customer list contains all customers in solution without the removed nodes
                List<Node> L = new List<Node>();
                L.AddRange(customers);
                L.RemoveAll(n1 => removedCustomers.Any(n2 => n1.ID == n2.ID));

                // I order in ascending the L node list by the relatedness measure between remove node and all other nodes
                L = L.OrderBy(n => GetRelatednessMeasure(r, n, sol.Routes[n.RouteIndex].Vehicle, sol.Alpha, sol.Beta)).ToList();

                // y ∈ [0,1)
                double y = _rand.NextDouble();

                // I add to remove list using linear selection algorithm
                removedCustomers.Add(L[(int)(Math.Pow(y, p) * L.Count)]);
            }

            // I remove all nodes removed before
            foreach(Node n in removedCustomers)
                sol.DeleteNode(n.RouteIndex, n.NodeIndex);

            // return the new solution
            return sol;
        }
    }
}
