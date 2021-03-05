
namespace ALNS_EVRP.Data
{
    public class Cost
    {
        /// <summary>
        /// The best distance cost to across over all routes
        /// </summary>
        public CostDetail Dist { get; set; }

        /// <summary>
        /// The best time cost to across over all routes
        /// </summary>
        public CostDetail Time { get; set; }

        /// <summary>
        /// The best battery cost used in total of all vehicles
        /// </summary>
        public CostDetail Battery { get; set; }

        /// <summary>
        /// Cost of customer not served in the solution
        /// </summary>
        public CostDetail RequestBank { get; set; }

        /// <summary>
        /// Cost of time violation arriving after the due date of nodes
        /// </summary>
        public CostDetail TimeViolation { get; set; }

        /// <summary>
        /// Cost of battery violation consuming over the available battery
        /// </summary>
        public CostDetail BatteryViolation { get; set; }

        /// <summary>
        /// Cost of load violation consuming over the available battery
        /// </summary>
        public CostDetail LoadViolation { get; set; }

        /// <summary>
        /// Get the total cost based on value and weight
        /// </summary>
        /// <returns></returns>
        public float Total => Dist.Total + Time.Total + Battery.Total + RequestBank.Total +
                                TimeViolation.Total + BatteryViolation.Total + LoadViolation.Total;

        /// <summary>
        /// Get if the cost is feasible or not (violation values are equals to 0)
        /// </summary>
        public bool IsFeasible => TimeViolation.Value == 0 && BatteryViolation.Value == 0 && LoadViolation.Value == 0;

        public Cost() {}

        public Cost(float alpha, float beta, float gamma, float delta, float betaViol, float gammaViol, float deltaViol)
        {
            Dist = new CostDetail() { Value = float.PositiveInfinity, Weight = alpha };
            Time = new CostDetail() { Value = float.PositiveInfinity, Weight = beta };
            Battery = new CostDetail() { Value = float.PositiveInfinity, Weight = gamma };
            RequestBank = new CostDetail() { Value = 0, Weight = delta };
            TimeViolation = new CostDetail() { Value = 0, Weight = betaViol };
            BatteryViolation = new CostDetail() { Value = 0, Weight = gammaViol };
            LoadViolation = new CostDetail() { Value = 0, Weight = deltaViol };
        }

        public Cost Clone() =>
            new Cost()
            {
                Dist = this.Dist.Clone(),
                Time = this.Time.Clone(),
                Battery = this.Battery.Clone(),
                RequestBank = this.RequestBank.Clone(),
                TimeViolation = this.TimeViolation.Clone(),
                BatteryViolation = this.BatteryViolation.Clone(),
                LoadViolation = this.LoadViolation.Clone()
            };
    }

    public class CostDetail
    {
        /// <summary>
        /// The value of the cost detail
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// The weight of the cost value
        /// </summary>
        public float Weight { get; set; }

        /// <summary>
        /// The total value is value based on weight
        /// </summary>
        public float Total => Value * Weight;

        public CostDetail Clone() => new CostDetail() { Value = this.Value, Weight = this.Weight };
    }
}
