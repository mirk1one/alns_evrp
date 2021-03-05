using ALNS_EVRP.Model;

namespace ALNS_EVRP.Metaheuristic
{
    public abstract class RepairOperation : BaseOperation
    {
        public RepairOperation(int allOps) : base(allOps) { }

        /// <summary>
        /// Method that repair routes, removing nodes
        /// </summary>
        /// <param name="sol">Enter solution found</param>
        /// <param name="q">Number of customers to add at solution</param>
        /// <returns>The repaired solution</returns>
        public abstract Solution Repair(Solution sol, int requestBankSelected = 0);
    }
}
