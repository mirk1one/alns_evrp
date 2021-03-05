using ALNS_EVRP.Data;
using Draw_Graph.Element;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ALNS_EVRP.Model
{
    public class Solution
    {
        /// <summary>
        /// Weight of distance
        /// </summary>
        public float Alpha { get; }

        /// <summary>
        /// Weight of time
        /// </summary>
        public float Beta { get; }

        /// <summary>
        /// Weight of battery
        /// </summary>
        public float Gamma { get; }

        /// <summary>
        /// Weight of customers not served
        /// </summary>
        public float Delta { get; }

        /// <summary>
        /// Weight of time violation
        /// </summary>
        public float BetaViol { get; }

        /// <summary>
        /// Weight of battery violation
        /// </summary>
        public float GammaViol { get; }

        /// <summary>
        /// Weight of load violation
        /// </summary>
        public float DeltaViol { get; }

        /// <summary>
        /// All total costs of solution
        /// </summary>
        public Cost Cost { get; set; }

        /// <summary>
        /// Iteration where solution was found
        /// </summary>
        public int Iteration { get; set; }

        /// <summary>
        /// All total costs of solution
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// Type of solution found at iteration
        /// </summary>
        public TypeSolution Type { get; set; }

        /// <summary>
        /// Routes built
        /// </summary>
        public List<Route> Routes { get; set; }

        /// <summary>
        /// List of customer not served in routes
        /// </summary>
        public List<Node> RequestBank { get; set; }

        /// <summary>
        /// List of stations of problem
        /// </summary>
        public List<Node> Stations { get; set; }

        /// <summary>
        /// List of customers removed from customer destory operations
        /// </summary>
        public List<Node> RemovedCustomers { get; set; }

        /// <summary>
        /// Get if the solution is feasible (not violated main constraints)
        /// </summary>
        public bool IsFeasible => Routes.All(r => r.IsFeasible);

        public Solution() { }

        public Solution(float alpha, float beta, float gamma, float delta, float betaViol, float gammaViol, float deltaViol)
        {
            Alpha = alpha;
            Beta = beta;
            Gamma = gamma;
            Delta = delta;
            BetaViol = betaViol;
            GammaViol = gammaViol;
            Cost = new Cost(alpha, beta, gamma, delta, betaViol, gammaViol, deltaViol);
            Routes = new List<Route>();
            RequestBank = new List<Node>();
            Stations = new List<Node>();
            RemovedCustomers = new List<Node>();
        }

        public Solution Clone()
        {
            List<Route> routes = new List<Route>();
            this.Routes.ForEach(r => routes.Add(r.Clone()));
            List<Node> customersNotServed = new List<Node>();
            this.RequestBank.ForEach(c => customersNotServed.Add(c.Clone()));
            List<Node> stations = new List<Node>();
            this.Stations.ForEach(s => stations.Add(s.Clone()));
            List<Node> removedNodes = new List<Node>();
            this.RemovedCustomers.ForEach(n => removedNodes.Add(n.Clone()));
            return new Solution()
            {
                Cost = this.Cost.Clone(),
                Iteration = this.Iteration,
                Temperature = this.Temperature,
                Type = this.Type,
                Routes = routes,
                RequestBank = customersNotServed,
                Stations = stations,
                RemovedCustomers = removedNodes
            };
        }


        /// <summary>
        /// Delete a node from one route of solution, and prev/succ station if there is
        /// </summary>
        /// <param name="routePos">Number of route where node is</param>
        /// <param name="nodePos">Position of node in route</param>
        /// <returns>True if the node is deleted, else false</returns>
        public bool DeleteNode(int routePos, int nodePos)
        {
            if (Routes.Count <= routePos)
                return false;
            Route route = Routes[routePos];
            if (route.Nodes.Count <= 2 || route.Nodes.Count - 1 <= nodePos)
                return false;
            Node node = Routes[routePos].Nodes[nodePos];
            if (node.Type == TypeNode.Customer)
            {
                // Implement the RCwPS: Remove Customer with Preceding Station
                if (nodePos != 0 && Routes[routePos].Nodes[nodePos - 1].Type == TypeNode.Station)
                    route.Nodes.RemoveAt(nodePos - 1);
                // Implement the RCwSS: Remove Customer with Succeding Station
                if (nodePos != Routes[routePos].Nodes.Count - 1 && Routes[routePos].Nodes[nodePos + 1].Type == TypeNode.Station)
                    route.Nodes.RemoveAt(nodePos + 1);
                RemovedCustomers.Add(node);
            }
            route.Nodes.Remove(node);
            route.UpdateRouteData();
            node.Reset();
            return true;
        }

        /// <summary>
        /// Evaluate the insertion of a node in one route of solution
        /// </summary>
        /// <param name="routePos">Number of route where node is</param>
        /// <param name="nodePos">Position of node in route</param>
        /// <param name="insertNode">Node to insert in</param>
        /// <param name="cost">Cost of the inserting route where I save</param>
        /// <returns>True if the node is inserted, else false</returns>
        public bool EvaluateInsertNode(int routePos, int nodePos, Node insertNode, Cost cost)
        {
            if (routePos == -1 || nodePos == -1 || Routes.Count <= routePos)
                return false;
            Route route = Routes[routePos].Clone();
            if (route.Nodes.Count - 1 < nodePos)
                return false;
            route.Nodes.Insert(nodePos, insertNode);
            route.UpdateRouteData();
            cost.Dist = route.Cost.Dist;
            cost.Time = route.Cost.Time;
            cost.Battery = route.Cost.Battery;
            cost.TimeViolation = route.Cost.TimeViolation;
            cost.BatteryViolation = route.Cost.BatteryViolation;
            cost.LoadViolation = route.Cost.LoadViolation;
            return true;
        }

        /// <summary>
        /// Insertion of node in one route of solution
        /// </summary>
        /// <param name="routePos">Number of route where node is</param>
        /// <param name="nodePos">Position of node in route</param>
        /// <param name="insertNode">Node to insert in</param>
        /// <returns>True if the node is inserted, else false</returns>
        public bool InsertNode(int routePos, int nodePos, Node insertNode)
        {
            if (routePos == -1 || nodePos == -1 || Routes.Count <= routePos)
                return false;
            Route route = Routes[routePos];
            if (route.Nodes.Count - 1 < nodePos)
                return false;
            route.Nodes.Insert(nodePos, insertNode);
            route.UpdateRouteData();
            Cost.Dist.Value = Routes.Sum(r => r.Cost.Dist.Value);
            Cost.Time.Value = Routes.Sum(r => r.Cost.Time.Value);
            Cost.Battery.Value = Routes.Sum(r => r.Cost.Battery.Value);
            Cost.TimeViolation.Value = Routes.Sum(r => r.Cost.TimeViolation.Value);
            Cost.BatteryViolation.Value = Routes.Sum(r => r.Cost.BatteryViolation.Value);
            Cost.LoadViolation.Value = Routes.Sum(r => r.Cost.LoadViolation.Value);
            Cost.RequestBank.Value = RequestBank.Count;
            return true;
        }

        /// <summary>
        /// Get the graph to pass to Draw_Graph library to get draw elements (nodes and edges)
        /// </summary>
        /// <returns>The draw graph to draw and show it</returns>
        public DrawGraph GetDrawRouteGraph()
        {
            DrawGraph graph = new DrawGraph();
            DrawNode drawStartDepot = null;
            for (int r = 0; r < Routes.Count; r++)
            {
                if (Routes[r].Type == TypeRoute.SingleDepot && r == 0 || Routes[r].Type == TypeRoute.OneDepotPerRoute && r < Routes.Count || Routes[r].Type == TypeRoute.DifferentDepots)
                {
                    Node startDepot = Routes[r].Nodes[0];
                    drawStartDepot = new DrawNode() { Label = startDepot.ID, Color = DrawColor.Gray, Type = DrawNodeType.House, X = startDepot.X, Y = startDepot.Y };
                    graph.Nodes.Add(drawStartDepot);
                }
                bool start = true;
                for(int n = 1; n < Routes[r].Nodes.Count - 1; n++)
                {
                    Node insertNode = Routes[r].Nodes[n];
                    DrawNode insertDrawNode = new DrawNode() { Label = insertNode.ID, Color = (DrawColor)r, Type = insertNode.Type == TypeNode.Customer ? DrawNodeType.Circle : DrawNodeType.Rectangle, X = insertNode.X, Y = insertNode.Y };
                    DrawNode prevDrawNode = start ? drawStartDepot : graph.Nodes[graph.Nodes.Count - 1];
                    start = false;
                    DrawEdge insertDrawEdge = new DrawEdge() { Label = null, Color = (DrawColor)r, StartNodeId = prevDrawNode.Label, EndNodeId = insertDrawNode.Label };
                    graph.Nodes.Add(insertDrawNode);
                    graph.Edges.Add(insertDrawEdge);
                }
                DrawNode lastDrawNode = graph.Nodes[graph.Nodes.Count - 1];
                DrawNode drawEndDepot;
                if (Routes[r].Type == TypeRoute.DifferentDepots)
                {
                    Node endDepot = Routes[r].Nodes[Routes[r].Nodes.Count - 1];
                    drawEndDepot = new DrawNode() { Label = endDepot.ID, Color = DrawColor.Gray, Type = DrawNodeType.House, X = endDepot.X, Y = endDepot.Y };
                    graph.Nodes.Add(drawEndDepot);
                }
                else
                    drawEndDepot = drawStartDepot;
                DrawEdge lastDrawEdge = new DrawEdge() { Label = null, Color = (DrawColor)r, StartNodeId = lastDrawNode.Label, EndNodeId = drawEndDepot.Label };
                graph.Edges.Add(lastDrawEdge);
            }
            return graph;
        }
    }
}
