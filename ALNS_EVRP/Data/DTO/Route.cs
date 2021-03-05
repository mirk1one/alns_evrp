using System;
using System.Collections.Generic;
using System.Linq;

namespace ALNS_EVRP.Data
{
    /// <summary>
    /// Class that handle a route in the problem
    /// </summary>
    public class Route
    {
        /// <summary>
        /// Id represent the number of vehicle that run across it
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Type of route
        /// </summary>
        public TypeRoute Type { get; set; }

        /// <summary>
        /// Total cost of route
        /// </summary>
        public Cost Cost { get; set; }

        /// <summary>
        /// Total load in route
        /// </summary>
        public float Load { get; set; }

        /// <summary>
        /// List of all nodes in the route
        /// </summary>
        public List<Node> Nodes { get; set; }

        /// <summary>
        /// Vehicle used in the route
        /// </summary>
        public ElectricVehicle Vehicle { get; set; }

        /// <summary>
        /// Get if the route is feasible (not violated main constraints)
        /// </summary>
        public bool IsFeasible => Cost.TimeViolation.Value == 0 && Cost.BatteryViolation.Value == 0 && Cost.LoadViolation.Value == 0;

        public Route() { }
        
        public Route(float alpha, float beta, float gamma, float delta, float betaViol, float gammaViol, float deltaViol)
        {
            Id = -1;
            Type = TypeRoute.SingleDepot;
            Cost = new Cost(alpha, beta, gamma, delta, betaViol, gammaViol, deltaViol);
            Load = 0;
            Nodes = new List<Node>();
            Vehicle = new ElectricVehicle();
        }

        public Route Clone()
        {
            List<Node> nodes = new List<Node>();
            this.Nodes.ForEach(n => nodes.Add(n.Clone()));
            return new Route()
            {
                Id = this.Id,
                Type = this.Type,
                Cost = this.Cost.Clone(),
                Load = this.Load,
                Nodes = nodes,
                Vehicle = this.Vehicle
            };
        }

        /// <summary>
        /// Function that update all data nodes in route from the start index
        /// </summary>
        public void UpdateRouteData()
        {
            // Initialize variables used
            Speed speedSet = Vehicle.Speeds.OrderBy(s => s.Value).First();          // Speed set is the lowest of vehicle
            float currLoad = Nodes.Sum(n => n.Demand);                              // Load is total demand all nodes
            float remainingBattery = Vehicle.BatteryCapacity;                       // Battery starts from capacity vattery of vehicle
            float time = 0;                                                         // Time start from 0
            float timeViolation = 0;                                                // Time violation is equal to 0
            int clonedStations = 0;                                                 // Number of cloned stations

            // Set data stat depot
            Node startDepot = Nodes[0];                                             // Get start depot
            startDepot.Reset();                                                     // Reset values of start depot
            startDepot.RouteIndex = Id-1;                                           // Set route like id - 1
            startDepot.NodeIndex = 0;                                               // Depot is the first node
            startDepot.BatteryRemaining = remainingBattery;                         // Battery remaining is full battery
            startDepot.Load = currLoad;                                             // Load is all demand nodes

            // Set data all after nodes
            for (int n=1; n < Nodes.Count; n++)
            {
                Node currNode = Nodes[n];                                           // Get current node
                Node prevNode = Nodes[n-1];                                         // Get previous node
                currNode.Reset();                                                   // Reset values of current node
                if (currNode.Type == TypeNode.Station)                              // If curr node is a station, I change it id with clone id
                {
                    currNode.ID = $"R{Id:D2}{clonedStations:D2}{currNode.ID[^3..]}";
                    clonedStations++;
                }
                currNode.RouteIndex = Id-1;                                         // Set route like id - 1
                currNode.NodeIndex = n;                                             // Set index node like index cycle
                float dist = prevNode.GetDistance(currNode);                        // Distance between previous and current node
                currNode.Dist = dist;                                               // Set the distance to current node
                time = prevNode.EndServiceTime + dist / speedSet.DistInHour;        // Time is the previuos end service time + time to reach the node at speed set
                currNode.Time = time;                                               // Set time to arrive at current node
                if (time > currNode.DueDate)                                        // If I violated arriving after the due date
                    timeViolation += time - currNode.DueDate;                       // I add the violation to all violantion nodes
                currNode.SpeedArrive = speedSet;                                    // I set the speed set at current node
                float batteryConsumed = Vehicle.GetBatteryConsumption(dist, speedSet, currLoad); // Battery consumed is set getting the consumption of vehicle
                remainingBattery -= batteryConsumed;                                // Remaining battery is the same minus the battery consumed to arrive at node
                currNode.Load = currLoad;                                           // I set the current load
                currLoad -= currNode.Demand;                                        // I update the current load removing the demand of the node
                currNode.BatteryRemaining = remainingBattery;                       // I set the remaining battery at current node
                currNode.BatteryConsumed = batteryConsumed;                         // I set the battery consumed at current node
                if (currNode.Type == TypeNode.Station && currNode.BatteryRemaining > -0.01) // If I found a station, I recharge all battery
                {
                    currNode.BatteryRecharged = Vehicle.BatteryCapacity - remainingBattery; // I calculate the total battery recharged
                    currNode.TimeRecharging = currNode.GetTimeToRecharge(Vehicle.BatteryCapacity, remainingBattery); // I calculate the time to recharge from remaining to full battery
                    remainingBattery = Vehicle.BatteryCapacity;                     // The battery is full
                }
            }

            if(clonedStations > 1)
            {
                int i = 0;
            }

            int nodeIndex = 1;                                                          // The node index to explore the route
            if (Nodes.All(n => n.BatteryRemaining > -0.01))                             // Only if all station have battery remaining positive, I update all battery costs
            {
                for (; Nodes[nodeIndex].Type == TypeNode.Customer; nodeIndex++);        // I find the first station or final depot
                while (Nodes[nodeIndex].Type != TypeNode.Depot)                         // While I find the end depot
                {
                    Node station = Nodes[nodeIndex];                                    // Get the station node
                    float battConsNext = 0;                                             // Total battery consumed from previous to next station
                    int n;
                    for (n = nodeIndex + 1; Nodes[n].Type == TypeNode.Customer; n++)    // I calculate the total battery consumed from previous station to after (or end depot)
                        battConsNext += Nodes[n].BatteryConsumed;
                    battConsNext += Nodes[n].BatteryConsumed;
                    // If the remaining battery is minus than the battery consumed to arrive at next station/end depot, I recalculate
                    // time to recharge from station and the battery to use to arrive to next station/end depot
                    if (station.BatteryRemaining < battConsNext)
                    {
                        station.BatteryRecharged = battConsNext - station.BatteryRemaining; // How I recharge is the consumed to arrive minus the remaining in station
                        station.TimeRecharging = station.GetTimeToRecharge(battConsNext, station.BatteryRemaining); // I calculate the to recharge
                        nodeIndex++;                                                    // I go to the next node
                        for (; Nodes[nodeIndex].Type == TypeNode.Customer; nodeIndex++) // While I find customers, I update customer data
                        {
                            battConsNext -= Nodes[nodeIndex].BatteryConsumed;           // I update the battery consume minus what I consume at node
                            Nodes[nodeIndex].BatteryRemaining = battConsNext;           // I update battery remaining
                            Nodes[nodeIndex].Time = Nodes[nodeIndex - 1].EndServiceTime + // I update the time to arrive at customer
                                Nodes[nodeIndex - 1].GetDistance(Nodes[nodeIndex]) / Nodes[nodeIndex].SpeedArrive.DistInHour;
                        }
                        Nodes[nodeIndex].BatteryRemaining = 0;                          // The battery remained at station/end depot is 0
                        Nodes[nodeIndex].Time = Nodes[nodeIndex - 1].EndServiceTime +   // I update the time to arrive at station/end depot
                            Nodes[nodeIndex - 1].GetDistance(Nodes[nodeIndex]) / Nodes[nodeIndex].SpeedArrive.DistInHour;
                    }
                    else
                    {
                        nodeIndex++;                                                    // I go to the next node
                        for (; Nodes[nodeIndex].Type == TypeNode.Customer; nodeIndex++); // I find the first station or final depot
                    }
                }
            }

            // I update all route cost data and total load

            Load = Nodes.Sum(n => n.Demand);
            Cost.Dist.Value = Nodes.Sum(n => n.Dist);
            Cost.Time.Value = Nodes.Last().Time - Vehicle.StartTime;
            Cost.Battery.Value = Nodes.Sum(n => n.BatteryConsumed);
            Cost.TimeViolation.Value = timeViolation;
            Cost.BatteryViolation.Value = remainingBattery < -0.01 ? Math.Abs(remainingBattery) : 0;
            Cost.LoadViolation.Value = Vehicle.LoadCapacity < Load ? Load - Vehicle.LoadCapacity : 0;
        }
    }
}
