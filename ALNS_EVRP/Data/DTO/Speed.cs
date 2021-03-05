using System;

namespace ALNS_EVRP.Data
{
    public class Speed
    {
        /// <summary>
        /// Value of speed
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Battery consumption rate for km
        /// </summary>
        public float BatteryConsumptionRateForKm { get; set; }

        /// <summary>
        /// Convert speed value from km/h to km/min
        /// </summary>
        public float DistInHour => ((float)Value) / 60.0f;

        public override bool Equals(object obj) =>
            obj is Speed speed &&
            Value == speed.Value &&
            BatteryConsumptionRateForKm == speed.BatteryConsumptionRateForKm;

        public override int GetHashCode() => HashCode.Combine(Value, BatteryConsumptionRateForKm);

        public override string ToString() => $"Speed = {Value}; BatteryConsumptionRateForKm = {BatteryConsumptionRateForKm}";
    }
}
