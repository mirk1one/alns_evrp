using ALNS_EVRP.Data;
using ALNS_EVRP.Model;
using System.Collections.Generic;
using System.Linq;

namespace ALNS_EVRP.Metaheuristic
{
    /// <summary>
    /// Destroy that implements the random customer destroy
    /// </summary>
    public class RandomCustomerDestroy : DestroyOperation
    {
        public RandomCustomerDestroy(int allOps) : base(allOps) { }

        public override Solution Destroy(Solution sol, int q, float p = 1)
        {
            // I get only the customers inserted in all routes
            List<Node> customers = new List<Node>();
            foreach (Route r in sol.Routes)
                customers.AddRange(r.Nodes.Where(c => c.Type == TypeNode.Customer).ToList());

            // I update number of customers to remove if are minor than q
            int numCustomers = customers.Count;
            if (numCustomers < q)
                q = numCustomers;

            // I remove q casual customers
            for (int removed = 0; removed < q; removed++)
            {
                // I select a random customer between the route customers
                int selectedCustomerIndex = _rand.Next(0, customers.Count);
                Node selectedCustomer = customers[selectedCustomerIndex];
                customers.Remove(selectedCustomer);

                // I remove the node of route selected
                sol.DeleteNode(selectedCustomer.RouteIndex, selectedCustomer.NodeIndex);
            }

            // return the new solution
            return sol;
        }
    }
}
