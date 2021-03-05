using System.Collections.Generic;

namespace ALNS_EVRP.Data
{
    public class ElectricVehicle
    {
        /// <summary>
        /// Time where a vehicle is available to start
        /// </summary>
        public float StartTime { get; set; }
        
        /// <summary>
        /// Maximum load capacity of vehicle
        /// </summary>
        public float LoadCapacity { get; set; }

        /// <summary>
        /// Battery max capacity
        /// </summary>
        public float BatteryCapacity { get; set; }

        /// <summary>
        /// Battery consumption rate for power (measure kW/km*kg)
        /// </summary>
        public float BatteryConsumptionRate { get; set; }

        /// <summary>
        /// List of speeds that can use the vehicle
        /// </summary>
        public List<Speed> Speeds { get; set; }

        public ElectricVehicle()
        {
            StartTime = 0;
            LoadCapacity = 0;
            BatteryCapacity = 0;
            BatteryConsumptionRate = 0;
            Speeds = new List<Speed>();
        }

        /// <summary>
        /// Get the battery consuption depending from load, velocity consumption and distance
        /// </summary>
        /// <param name="dist">Distance to run across</param>
        /// <param name="speed">Speed set to across the path</param>
        /// <param name="load">Amount of load when across the path</param>
        /// <returns></returns>
        public float GetBatteryConsumption(float dist, Speed speed, float load) =>
            dist * (speed.BatteryConsumptionRateForKm + BatteryConsumptionRate * load);
    }
}
