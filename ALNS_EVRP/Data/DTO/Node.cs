using System;

namespace ALNS_EVRP.Data
{
    public class Node
    {
        /// <summary>
        /// String ID of node
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Type of node (d = depot, f = station, c = customer)
        /// </summary>
        public TypeNode Type { get; set; }

        /// <summary>
        /// Coordinate position x
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Coordinate position y
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Value of demand (zero from depot and station)
        /// </summary>
        public float Demand { get; set; }

        /// <summary>
        /// Time window given be ready time (given in minutes from 0, zero from depot and station)
        /// </summary>
        public float ReadyTime { get; set; }

        /// <summary>
        /// Due date (given in minutes from 0)
        /// </summary>
        public float DueDate { get; set; }

        /// <summary>
        /// Service time (zero for depot and stations)
        /// </summary>
        public float ServiceTime { get; set; }

        /// <summary>
        /// Battery recharging rate(measure  min/kW) -> only for station
        /// </summary>
        public float BatteryRechargingRate { get; set; }

        /// <summary>
        /// Contain the number of route where the node is
        /// </summary>
        public int RouteIndex { get; set; }

        /// <summary>
        /// Contain the position of node in the route RouteIndex
        /// </summary>
        public int NodeIndex { get; set; }

        /// <summary>
        /// Distance travelled from previous node
        /// </summary>
        public float Dist { get; set; }

        /// <summary>
        /// Time when vehicle arrive at node
        /// </summary>
        public float Time { get; set; }

        /// <summary>
        /// Battery that have vehicle at node
        /// </summary>
        public float BatteryRemaining { get; set; }

        /// <summary>
        /// Battery consumed to arrive from previous node
        /// </summary>
        public float BatteryConsumed { get; set; }

        /// <summary>
        /// How much battery is recharged in the node -> only for station
        /// </summary>
        public float BatteryRecharged { get; set; }

        /// <summary>
        /// How much time to recharge battery in the node -> only for station
        /// </summary>
        public float TimeRecharging { get; set; }

        /// <summary>
        /// Speed set to arrive from previous node
        /// </summary>
        public Speed SpeedArrive { get; set; }

        /// <summary>
        /// How much load at node
        /// </summary>
        public float Load { get; set; }

        /// <summary>
        /// If the node is routed by a vehicle -> only for customer
        /// </summary>
        public bool Routed { get; set; }

        public Node()
        {
            RouteIndex = -1;
            NodeIndex = -1;
            Dist = 0;
            Time = 0;
            BatteryRemaining = 0;
            BatteryConsumed = 0;
            BatteryRecharged = 0;
            TimeRecharging = 0;
            SpeedArrive = null;
            Load = 0;
            Routed = false;
        }

        public override bool Equals(object obj) =>
            obj is Node node &&
            ID == node.ID &&
            Type == node.Type &&
            X == node.X &&
            Y == node.Y &&
            Demand == node.Demand &&
            ReadyTime == node.ReadyTime &&
            DueDate == node.DueDate &&
            ServiceTime == node.ServiceTime;

        public override int GetHashCode() =>
            HashCode.Combine(ID, Type, X, Y, Demand, ReadyTime, DueDate, ServiceTime);

        public override string ToString() =>
            $"StringID = {ID}; Type = {Type}; X = {X}; Y = {Y}; Demand = {Demand}; ReadyTime = {ReadyTime}; DueDate = {DueDate}; ServiceTime = {ServiceTime}";

        /// <summary>
        /// Get the distance between this node and destination node
        /// I calculate it with euclidean distance
        /// </summary>
        /// <param name="destination">Node of destination</param>
        /// <returns></returns>
        public float GetDistance(Node destination) =>
            (float)Math.Sqrt((X - destination.X) * (X - destination.X) + (Y - destination.Y) * (Y - destination.Y));

        /// <summary>
        /// Get the time to start service the node. If it arrives before
        /// the ready time, the start is the ready time, else the time arrived
        /// </summary>
        /// <returns>The time that start to serve the node</returns>
        public float StartServiceTime => Time < ReadyTime ? ReadyTime : Time;

        /// <summary>
        /// Get the time to end service the node, that is when it starts to
        /// serve the node + the service time + the time to recharge the vehicle
        /// </summary>
        /// <returns>The time that end to serve the node</returns>
        public float EndServiceTime => StartServiceTime + ServiceTime + TimeRecharging;

        /// <summary>
        /// Get the time to recharge from remaining battery to max battery
        /// </summary>
        /// <param name="maxBattery">The maximum level battery to recharge</param>
        /// <param name="remainingBattery">The remaining battery</param>
        /// <returns></returns>
        public float GetTimeToRecharge(float maxBattery, float remainingBattery) => BatteryRechargingRate * (maxBattery - remainingBattery);

        /// <summary>
        /// Get the maximum level to rechairge with maximum time
        /// </summary>
        /// <param name="maxTime">The maximum time to recharge battery</param>
        /// <param name="remainingBattery">The remaining battery</param>
        /// <returns></returns>
        public float GetLevelToRecharge(float maxTime, float remainingBattery) => maxTime / BatteryRechargingRate + remainingBattery;

        /// <summary>
        /// Function that reset all transaction node data
        /// </summary>
        public void Reset()
        {
            RouteIndex = -1;
            NodeIndex = -1;
            Dist = 0;
            Time = 0;
            BatteryRemaining = 0;
            BatteryConsumed = 0;
            BatteryRecharged = 0;
            TimeRecharging = 0;
            SpeedArrive = null;
            Load = 0;
            Routed = false;
        }

        public Node Clone() =>
            new Node()
            {
                ID = this.ID,
                Type = this.Type,
                X = this.X,
                Y = this.Y,
                Demand = this.Demand,
                ReadyTime = this.ReadyTime,
                DueDate = this.DueDate,
                ServiceTime = this.ServiceTime,
                BatteryRechargingRate = this.BatteryRechargingRate,
                RouteIndex = this.RouteIndex,
                NodeIndex = this.NodeIndex,
                Dist = this.Dist,
                Time = this.Time,
                BatteryRemaining = this.BatteryRemaining,
                BatteryConsumed = this.BatteryConsumed,
                BatteryRecharged = this.BatteryRecharged,
                TimeRecharging = this.TimeRecharging,
                SpeedArrive = this.SpeedArrive,
                Load = this.Load,
                Routed = this.Routed,
            };
    }
}
