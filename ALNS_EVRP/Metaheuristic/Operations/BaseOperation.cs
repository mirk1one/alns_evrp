
namespace ALNS_EVRP.Metaheuristic
{
    public class BaseOperation
    {
        /// <summary>
        /// Number of times that operation is used (θi)
        /// </summary>
        public float Times { get; set; }

        /// <summary>
        /// Score of heuristic obtained (πi)
        /// </summary>
        public float Score { get; set; }

        /// <summary>
        /// Weight of using the operation (wij)
        /// </summary>
        public float Weight { get; set; }

        /// <summary>
        /// Probability of using operation respect to all operations (P)
        /// </summary>
        public float Probability { get; set; }

        public BaseOperation(int allOps)
        {
            Times = 0;
            Weight = 1;
            Probability = 1.0f / allOps;
        }
    }
}
