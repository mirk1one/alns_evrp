using ALNS_EVRP.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ALNS_EVRP.Model
{
    /// <summary>
    /// Class that define the problem to be resolved
    /// </summary>
    public class Problem
    {
        private readonly float _alpha;
        private readonly float _beta;
        private readonly float _gamma;
        private readonly float _delta;
        private readonly float _betaViol;
        private readonly float _gammaViol;
        private readonly float _deltaViol;

        /// <summary>
        /// List of depot nodes, in total are 2 * NumV
        /// For every vehicle, first depot is the start depot, second is the end depot
        /// </summary>
        public List<Node> Depots { get; set; }

        /// <summary>
        /// List of station nodes
        /// </summary>
        public List<Node> Stations { get; set; }

        /// <summary>
        /// List of customer nodes
        /// </summary>
        public List<Node> Customers { get; set; }

        /// <summary>
        /// Number of vehicles
        /// </summary>
        public int NumV { get; set; }

        /// <summary>
        /// List of electric vehicles
        /// </summary>
        public List<ElectricVehicle> Vehicles { get; set; }

        /// <summary>
        /// Solution of problem
        /// </summary>
        public Solution Solution { get; set; }

        /// <summary>
        /// Dictionary of distance between nodes
        /// For example, if the distance between C1 and C2 is 10,
        /// in the dictionary you find key "C1C2" with value 10.
        /// You don't find distance between the same node (is equal 0) and
        /// the opposite of the same nodes ("C1C2" but not "C2C1", because are equals)
        /// </summary>
        public Dictionary<string,float> Dist { get; set; }

        /// <summary>
        /// Get the distance value between two nodes from distance matrix
        /// </summary>
        /// <param name="Id1">Id of first node</param>
        /// <param name="Id2">Id of second node</param>
        /// <returns></returns>
        public float GetDist(string Id1, string Id2)
        {
            float result;
            if (!Dist.TryGetValue($"{Id1}{Id2}", out result))
                Dist.TryGetValue($"{Id2}{Id1}", out result);
            return result;
        }

        public Problem(Input input, float alpha, float beta, float gamma, float delta, float betaViol, float gammaViol, float deltaViol)
        {
            _alpha = alpha;
            _beta = beta;
            _gamma = gamma;
            _delta = delta;
            _betaViol = betaViol;
            _gammaViol = gammaViol;
            _deltaViol = deltaViol;

            // Assign constants
            NumV = input.NumV;
            Vehicles = new List<ElectricVehicle>();
            for (int v = 0; v < NumV; v++)
            {
                ElectricVehicle vehicle = new ElectricVehicle
                {
                    LoadCapacity = input.C,
                    BatteryCapacity = input.Q,
                    BatteryConsumptionRate = input.Lcr,
                    Speeds = input.Speeds
                };
                Vehicles.Add(vehicle);
            }

            // Create all nodes
            Depots = input.Nodes.Where(n => n.Type == TypeNode.Depot).ToList();
            Customers = input.Nodes.Where(n => n.Type == TypeNode.Customer).ToList();
            Stations = input.Nodes.Where(n => n.Type == TypeNode.Station).ToList();
            Stations.ForEach(s =>
                {
                    s.ID = $"S{Int32.Parse(s.ID[1..]):D2}";
                    s.BatteryRechargingRate = input.G;                              // I set recharging rate to all stations
                });


            // Create distance dictionary
            Dist = new Dictionary<string, float>();
            List<Node> allNodes = Depots.Union(Stations).Union(Customers).ToList();
            for (int i = 0; i < allNodes.Count; i++)
                for (int j = i + 1; j < allNodes.Count; j++)
                    if(!(allNodes[i].Type == TypeNode.Depot && allNodes[j].Type == TypeNode.Depot))
                        Dist.Add($"{allNodes[i].ID}{allNodes[j].ID}", allNodes[i].GetDistance(allNodes[j]));
        }

        /// <summary>
        /// Method that incorporate the model to find the start solution
        /// </summary>
        public void FindStartSolution()
        {
            try
            {
                Solution sol = new Solution(_alpha, _beta, _gamma, _delta, _betaViol, _gammaViol, _deltaViol); // Create the solution

                TypeRoute typeRoute = Depots.Count == 1 ? TypeRoute.SingleDepot :
                                      Depots.Count == NumV ? TypeRoute.OneDepotPerRoute : TypeRoute.DifferentDepots;

                for (int v = 0; v < NumV; v++)                                      // Create routes with id is the number of vehicle that run across it
                    sol.Routes.Add(new Route(_alpha, _beta, _gamma, _delta, _betaViol, _gammaViol, _deltaViol) { Id = v + 1, Vehicle = Vehicles[v] });

                int nodesNotRouted = Customers.ToList().Count;                      // Find solution for all vehicles

                for (int v = 0; v < NumV; v++)
                {
                    // Get nodes
                    Node initialDepot = typeRoute == TypeRoute.SingleDepot ? Depots[0].Clone() :
                        typeRoute == TypeRoute.OneDepotPerRoute ? Depots[v].Clone() : Depots[2 * v].Clone();
                    Node finalDepot = typeRoute == TypeRoute.SingleDepot ? Depots[0].Clone() :
                        typeRoute == TypeRoute.OneDepotPerRoute ? Depots[v].Clone() : Depots[2 * v + 1].Clone();
                    List<Node> routeNodes = sol.Routes[v].Nodes;
                    int addPos = 1;

                    // Get vehicle route values
                    ElectricVehicle vehicle = sol.Routes[v].Vehicle;
                    float loadCapacity = vehicle.LoadCapacity;
                    float remainingBattery = vehicle.BatteryCapacity;
                    List<Speed> speeds = vehicle.Speeds.OrderBy(s => s.BatteryConsumptionRateForKm).ToList();
                    Speed currSpeed = null;
                    float time = vehicle.StartTime;
                    float timeViolation = 0;
                    float totalLoad = 0;

                    bool built = false;

                    initialDepot.NodeIndex = 0;
                    initialDepot.RouteIndex = v;
                    initialDepot.BatteryRemaining = remainingBattery;

                    currSpeed = speeds[0];

                    routeNodes.Add(initialDepot);

                    if (nodesNotRouted == 0)                                        // If there aren't any other requests, I stop and add the final depot
                    {                                                               // I don't update any value because I don't run across any route
                        finalDepot.NodeIndex = 1;
                        finalDepot.RouteIndex = v;
                        finalDepot.SpeedArrive = currSpeed;
                        routeNodes.Add(finalDepot);
                        built = true;
                    }

                    // I start to build the initial solution without load and battery

                    while (!built)                                                  // I stay in this cycle until I don't find minimum path
                    {
                        int indexNode = -1;                                         // I want to memorize the best node index
                        Cost bestCost = new Cost(_alpha, _beta, _gamma, _delta, _betaViol, _gammaViol, _deltaViol); // I want to save the best cost
                        Node last = routeNodes.Count == 0 ? null : routeNodes[^1];  // Get the last node in the vehicle route
                        float battery = vehicle.BatteryCapacity;

                        for (int n = 0; n < Customers.Count; n++)                      // I analyze all customers
                        {
                            Node candidate = Customers[n];                          // I get the candidate node from his index
                            if (!candidate.Routed)                                  // If the node was visited, I don't do anything
                            {
                                // Get the distance cost between the last node in the vehicle route and the candidate node
                                float currDist = GetDist(last.ID, candidate.ID);
                                // Get the distance cost between the candidate node in the vehicle route and the final depot node
                                float depotDist = GetDist(candidate.ID, finalDepot.ID);

                                float currBattery = battery - vehicle.GetBatteryConsumption(currDist, currSpeed, 0);

                                // I want to save the new cost
                                Cost newCost = new Cost(_alpha, _beta, _gamma, _delta, _betaViol, _gammaViol, _deltaViol);
                                newCost.Dist.Value = currDist;
                                newCost.Time.Value = candidate.ReadyTime;
                                newCost.Battery.Value = vehicle.GetBatteryConsumption(currDist, currSpeed, 0);

                                // If the distance cost is better than the best
                                // and if the demand of candidate node can be loaded on vehicle
                                // and if the instant time + current cost converted in km to candidate is minor or equal than the candidate due date
                                // and if the instant time + current cost converted in km to final depot + service time + cost from candidate to depot converted in km
                                //      is minor or equal than the final depot due date
                                // I memorize the next node of vehicle route, new best cost and data of departure, arrival and wait time
                                if (newCost.Total < bestCost.Total && candidate.Demand <= (loadCapacity - totalLoad) &&
                                        (last.EndServiceTime + currDist / currSpeed.DistInHour) <= candidate.DueDate &&
                                        (last.EndServiceTime + currDist / currSpeed.DistInHour + candidate.ServiceTime + depotDist / currSpeed.DistInHour) <= finalDepot.DueDate &&
                                        currBattery > -0.01 && currBattery + vehicle.GetBatteryConsumption(currDist + depotDist, currSpeed, 0) > -0.01)
                                {
                                    indexNode = n;
                                    bestCost = newCost;
                                }
                            }
                        }

                        if (indexNode != -1)                                         // If I found a new node to add to vehicle route
                        {
                            Customers[indexNode].Routed = true;                     // I update that the node is routed (i can't use it after with this and other vehicles)

                            Node bestNode = Customers[indexNode].Clone();           // Get best node from index

                            bestNode.RouteIndex = v;                                // Set the index of route where node is in
                            bestNode.NodeIndex = addPos;                            // Set the index order of node in route
                            addPos++;

                            bestNode.Dist = bestCost.Dist.Value;                    // I set the new best cost distance
                            bestNode.SpeedArrive = currSpeed;                       // I set the speed to arrive

                            totalLoad += bestNode.Demand;                           // I add the demand to load

                            // Time to arrive at node is time of previous node + service time of previous node + distance to arrive to this node
                            time = last.EndServiceTime + bestCost.Dist.Value / currSpeed.DistInHour;
                            bestNode.Time = time;                                   // I set the time to arrive and serve
                            if (time > bestNode.DueDate)                            // If I violated arriving after the due date
                                timeViolation += time - bestNode.DueDate;           // I add the violation to all violantion nodes

                            battery -= vehicle.GetBatteryConsumption(bestNode.Dist, currSpeed, 0);

                            routeNodes.Add(bestNode);                               // I add the best node

                            sol.Routes[v].Cost.Dist.Value += bestCost.Dist.Value;   // I update the route distance cost

                            nodesNotRouted--;
                        }
                        else
                        {
                            float dist = GetDist(last.ID, finalDepot.ID);           // Get the distance to return to the final depot from last node

                            finalDepot.RouteIndex = v;                              // Set the index of route where node is in
                            finalDepot.NodeIndex = addPos;                          // Set the index order of node in route

                            finalDepot.Dist = dist;                                 // I set the new best cost distance
                            time = last.EndServiceTime + dist / currSpeed.DistInHour; // I add the distance from last node to depot
                            finalDepot.Time = time;                                 // I set the time to arrive at depot

                            finalDepot.SpeedArrive = currSpeed;                     // I set the speed to arrive

                            routeNodes.Add(finalDepot);

                            sol.Routes[v].Cost.Dist.Value += dist;                  // I update the route distance cost
                            sol.Routes[v].Cost.Time.Value = time - vehicle.StartTime; // I update the route time

                            built = true;                                           // I have built the route of vehicle, stop
                        }
                    }

                    float currTotalLoad;

                    do
                    {
                        currTotalLoad = routeNodes.Sum(n => n.Demand);

                        routeNodes[0].Load = currTotalLoad;
                        remainingBattery = vehicle.BatteryCapacity;

                        // Now I verify if the vehicle can do all route with correct load and battery
                        for (int n = 1; n < routeNodes.Count; n++)
                        {
                            Node arrNode = routeNodes[n];
                            float dist = GetDist(routeNodes[n - 1].ID, arrNode.ID);

                            // I remove from battery the consumption to reach the node
                            float batteryConsumed = vehicle.GetBatteryConsumption(dist, currSpeed, currTotalLoad);
                            remainingBattery -= batteryConsumed;                   // I remove battery consumed from remaining battery
                            arrNode.BatteryRemaining = remainingBattery;           // I set the battery power
                            arrNode.BatteryConsumed = batteryConsumed;             // I set the last battery consume to arrive

                            arrNode.Load = currTotalLoad;                          // I set the load when I arrived to customer node
                            currTotalLoad -= arrNode.Demand;                       // I remove from total load the demand of node
                        }

                        // If total battery is negative, i remove last customer and it is available for next routes
                        if (remainingBattery < -0.01)
                        {
                            Node removeNode = routeNodes[^2];
                            Node customer = Customers.FirstOrDefault(c => c.ID == removeNode.ID);
                            if (customer != null) customer.Routed = false;
                            routeNodes.Remove(removeNode);
                            nodesNotRouted++;
                        }

                    } while (remainingBattery < -0.01);

                    sol.Routes[v].Nodes = routeNodes;
                    sol.Routes[v].Load = routeNodes.Sum(n => n.Demand);
                    sol.Routes[v].Cost.Dist.Value = routeNodes.Sum(n => n.Dist);
                    sol.Routes[v].Cost.Time.Value = routeNodes.Last().Time - vehicle.StartTime;
                    sol.Routes[v].Cost.Battery.Value = routeNodes.Sum(n => n.BatteryConsumed);
                    sol.Routes[v].Cost.TimeViolation.Value = timeViolation;
                    sol.Routes[v].Cost.BatteryViolation.Value = remainingBattery < -0.01 ? Math.Abs(remainingBattery) : 0;
                    sol.Routes[v].Cost.LoadViolation.Value = currTotalLoad > vehicle.LoadCapacity ? currTotalLoad - vehicle.LoadCapacity : 0;
                }

                sol.RequestBank = new List<Node>();
                Customers.ForEach(c => { if (!c.Routed) { c.Reset(); sol.RequestBank.Add(c); } });

                sol.Stations = Stations;

                // I update all solution parameters
                sol.Cost.Dist.Value = sol.Routes.Sum(r => r.Cost.Dist.Value);
                sol.Cost.Time.Value = sol.Routes.Sum(r => r.Cost.Time.Value);
                sol.Cost.Battery.Value = sol.Routes.Sum(r => r.Cost.Battery.Value);
                sol.Cost.RequestBank.Value = sol.RequestBank.Count;
                sol.Cost.TimeViolation.Value = sol.Routes.Sum(r => r.Cost.TimeViolation.Value);
                sol.Cost.BatteryViolation.Value = sol.Routes.Sum(r => r.Cost.BatteryViolation.Value);
                sol.Cost.LoadViolation.Value = sol.Routes.Sum(r => r.Cost.LoadViolation.Value);

                Solution = sol;        // Update the solution with the find
            } catch(Exception ex)
            {
                throw new Exception($"Exception: {ex.Message}");
            }
        }
    }
}