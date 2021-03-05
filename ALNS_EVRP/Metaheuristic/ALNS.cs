using ALNS_EVRP.Data;
using ALNS_EVRP.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ALNS_EVRP.Metaheuristic
{
    public class ALNS
    {
        private readonly Random _random = new Random();
        private readonly List<DestroyOperation> _destroyCustomerOperations;
        private readonly List<RepairOperation> _repairCustomerOperations;
        private readonly List<DestroyOperation> _destroyStationOperations;
        private readonly List<RepairOperation> _repairStationOperations;

        // ALNS
        private readonly float _sigma1 = 40;       // σ1: score of best solution found
        private readonly float _sigma2 = 10;       // σ2: score of local solution found
        private readonly float _sigma3 = 2;        // σ3: score of worst accepted solution found
        private readonly float _r = 0.1f;          // Reaction factor: how quickly weight-adjustment alg. reacts to change (10%)

        // Simulated annealing
        private readonly float _P0 = 0.99f;        // P0: start probability of acceptance (99%)
        private readonly float _lambda = 0.1f;     // λ: start probability you want to accept (10%)
        private readonly float _c = 0.99f;         // Cooling rate
        private readonly float _maxIt = 10000;     // Max iteration to stop searching
        private readonly float _updIt = 100;       // Every iteration where i update weights of operations (100, 200, 300, ..., _maxIt)
        private double _T;                         // Temperature simulated annealing
        private double _T0;                        // Start temperature
        private double _TF;                        // End temperature, where reached stop searching

        /// <summary>
        /// The best global solution find with ALNS
        /// </summary>
        public Solution BestSol { get; private set; }

        /// <summary>
        /// The best local solution find with ALNS
        /// </summary>
        public Solution LocalSol { get; private set; }

        /// <summary>
        /// History of all solutions found with ALNS
        /// </summary>
        public List<Solution> AllSol { get; private set; }

        public ALNS(Solution startSol)
        {
            _destroyCustomerOperations = new List<DestroyOperation>()
            {
                new RandomCustomerDestroy(3),
                new ShawCustomerDestroy(3),
                new WorstCostCustomerDestroy(3)
            };

            _repairCustomerOperations = new List<RepairOperation>()
            {
                new GreedyCustomerRepair(4),
                new Regret2CustomerRepair(4),
                new Regret3CustomerRepair(4),
                new Regret4CustomerRepair(4)
            };

            _destroyStationOperations = new List<DestroyOperation>()
            {
                new RandomStationDestroy(4),
                new WorstCostStationDestroy(4),
                new FullChargeStationDestroy(4),
                new WorstChargeUsageStationDestroy(4)
            };

            _repairStationOperations = new List<RepairOperation>()
            {
                new GreedyStationRepair(3),
                new GreedyStationWithComparisonRepair(3),
                new BestStationRepair(3)
            };

            startSol.Type = TypeSolution.LocalSol;
            LocalSol = startSol.Clone();

            startSol.Type = TypeSolution.BestSol;
            BestSol = startSol.Clone();
        }

        /// <summary>
        /// Select a random destroy operation, based on their weights
        /// </summary>
        /// <param name="destroyOperations">List of destroy operations that I can select</param>
        /// <returns>The destroy operation selected</returns>
        private DestroyOperation GetDestroyOperation(List<DestroyOperation> destroyOperations)
        {
            double random = _random.NextDouble();
            double threshold = 0.0f;
            foreach(DestroyOperation destroyOp in destroyOperations)
            {
                threshold += destroyOp.Probability;
                if (random <= threshold)
                {
                    destroyOp.Times++;
                    return destroyOp;
                }
            }
            return null;
        }

        /// <summary>
        /// Select a random repair operation, based on their weights
        /// </summary>
        /// <param name="repairOperations">List of repair operations that I can select</param>
        /// <returns>The repair operation selected</returns>
        private RepairOperation GetRepairOperation(List<RepairOperation> repairOperations)
        {
            double random = _random.NextDouble();
            double threshold = 0.0f;
            foreach (RepairOperation repairOp in repairOperations)
            {
                threshold += repairOp.Probability;
                if (random <= threshold)
                {
                    repairOp.Times++;
                    return repairOp;
                }
            }
            return null;
        }

        /// <summary>
        /// Function that apply the ALNS to an initial solution
        /// </summary>
        /// <param name="q">Scope of search</param>
        /// <returns></returns>
        public Solution AdaptiveLargeNeighborhoodSearch(int q)
        {
            Solution startSol = BestSol;

            _T0 = -(_lambda / Math.Log(_P0)) * startSol.Cost.Total; // Logarithmic cooling schedule start
            _TF = 0.00001f;                                         // Final temp is closest to 0
            _T = _T0;                                               // Start temperature from _T0

            startSol.Iteration = 0;                                 // Set to initial solution the iteration to 0
            startSol.Temperature = _T0;                             // Set to initial solution the temperature to _T0

            AllSol = new List<Solution>();                          // I initialize the list of all solutions found
            AllSol.Add(startSol);                                   // I add the initial solution

            var watch = System.Diagnostics.Stopwatch.StartNew();    // I want to calculate the time to find a solution
            int iter = 1;                                           // Number of iterations
            bool betterSol = false;                                 // Find a better solution almost one time from raise temperature
            int worseSequence = 0;                                  // Number of worse sequence found
            Solution newSol;                                        // The new solution found

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Iter 0 | Temperature {_T0} | Cost sol: {BestSol.Cost.Total} -> global");

            while (true)
            {
                newSol = LocalSol.Clone();                          // The new solution is equal to local solution found

                DestroyOperation destroyCOp = null;
                RepairOperation repairCOp = null;
                DestroyOperation destroySOp = null;
                RepairOperation repairSOp = null;

                // 0: select only customer operations
                // 1: select only station operations
                // 2: select customer operations, then station operations
                // 3: select station operations, then customer operations
                int selected = _random.Next(0, 3);                  // Random select how type of operations I use

                int requestBankSelected = 0;                        // I add customers only if the new solution is feasible and
                if (newSol.Cost.IsFeasible/* && selected != 1*/)        // the operations are not only for stations
                    requestBankSelected = q / 2;                    // I add half of q customers in request bank

                //if (selected != 0)                                  // I use customer operation only if I don't select only station operations
                //{
                    // Selecting casual destroy and repair customer operations, based on their weights
                    destroyCOp = GetDestroyOperation(_destroyCustomerOperations);
                    repairCOp = GetRepairOperation(_repairCustomerOperations);
                    //Console.Write($"Des: {destroyCOp.GetType()} | Rep: {repairCOp.GetType()} | ");
                //}

                if (selected != 0)                                  // I use station operation only if I don't select only customer operations
                {
                    // Selecting casual destroy and repair station operations, based on their weights
                    destroySOp = GetDestroyOperation(_destroyStationOperations);
                    repairSOp = GetRepairOperation(_repairStationOperations);
                    //Console.Write($"Des: {destroySOp.GetType()} | Rep: {repairSOp.GetType()} | ");
                }

                switch(selected)
                {
                    case 0:
                        // Destroy then repair customers
                        newSol = destroyCOp.Destroy(newSol, q, 5);
                        newSol = repairCOp.Repair(newSol, requestBankSelected);
                        break;
                    //case 1:
                        // Destroy then repair stations
                        //newSol = destroySOp.Destroy(newSol, q, 5);
                        //newSol = repairSOp.Repair(newSol);
                        break;
                    case 1:
                        // Destroy then repair customers
                        newSol = destroyCOp.Destroy(newSol, q, 5);
                        newSol = repairCOp.Repair(newSol, requestBankSelected);
                        // Destroy then repair stations
                        newSol = destroySOp.Destroy(newSol, q, 5);
                        newSol = repairSOp.Repair(newSol);
                        break;
                    case 2:
                        // Destroy then repair stations
                        newSol = destroySOp.Destroy(newSol, q, 5);
                        newSol = repairSOp.Repair(newSol);
                        // Destroy then repair customers
                        newSol = destroyCOp.Destroy(newSol, q, 5);
                        newSol = repairCOp.Repair(newSol, requestBankSelected);
                        break;
                }

                /*Console.WriteLine("");
                foreach (var r in newSol.Routes)
                {
                    Console.Write($"Route {r.Id - 1}: - ");
                    foreach (var n in r.Nodes)
                        Console.Write($"{n.ID} - ");
                    Console.WriteLine("");
                }*/

                newSol.Iteration = iter;                                // Set iteration to new solution
                newSol.Temperature = _T;                                // Set temperature to new solution

                // If the new solution cost is minor then the local solution cost, I have a new local solution
                if(newSol.Cost.Total < LocalSol.Cost.Total)
                {
                    LocalSol = newSol.Clone();                          // Set the new solution to local solution
                    worseSequence = 0;                                  // Reset the worse sequence
                    // If the new solution cost is minor then the best solution cost, I have the new best solution
                    if (newSol.Cost.Total < BestSol.Cost.Total)
                    {
                        betterSol = true;                               // Found a better solution
                        newSol.Type = TypeSolution.BestSol;             // Set the type of solution to best
                        BestSol = newSol.Clone();                       // Set the new solution to best solution

                        //if (selected != 1)                              // If I used the customer operations, I update their score
                        //{
                            destroyCOp.Score += _sigma1;                // I add σ1 to destroy customer operation
                            repairCOp.Score += _sigma1;                 // I add σ1 to repair customer operation
                        //}
                        if (selected != 0)                              // If I used the station operations, I update their score
                        {
                            destroySOp.Score += _sigma1;                // I add σ1 to destroy station operation
                            repairSOp.Score += _sigma1;                 // I add σ1 to repair station operation
                        }
                    }
                    // Else is a new local solution
                    else
                    {
                        newSol.Type = TypeSolution.LocalSol;            // Set the type of solution to local

                        //if (selected != 1)                              // If I used the customer operations, I update their score
                        //{
                            destroyCOp.Score += _sigma2;                // I add σ2 to destroy customer operation
                            repairCOp.Score += _sigma2;                 // I add σ2 to repair customer operation
                        //}
                        if (selected != 0)                              // If I used the station operations, I update their score
                        {
                            destroySOp.Score += _sigma2;                // I add σ2 to destroy station operation
                            repairSOp.Score += _sigma2;                 // I add σ2 to repair station operation
                        }
                    }
                }
                // Else the new solution isn't better then local and best solutions
                else
                {
                    newSol.Type = TypeSolution.WorseSol;                // Set the type of solution to worse
                    worseSequence++;                                    // I increment the worse sequence

                    double prob = _random.NextDouble();                 // I select a random probability
                    // For simulated annealing, I can accept the solution as local with prob < e^-[(snew - slocal)/T]
                    if (prob < Math.Exp(-(newSol.Cost.Total - LocalSol.Cost.Total) / _T))
                    {
                        newSol.Type = TypeSolution.AcceptedSol;         // Set the type of solution to accepted
                        LocalSol = newSol.Clone();                      // Set the new solution to local solution

                        //if (selected != 1)                              // If I used the customer operations, I update their score
                        //{
                            destroyCOp.Score += _sigma3;                // I add σ3 to destroy customer operation
                            repairCOp.Score += _sigma3;                 // I add σ3 to repair customer operation
                        //}
                        if (selected != 0)                              // If I used the station operations, I update their score
                        {
                            destroySOp.Score += _sigma3;                // I add σ3 to destroy station operation
                            repairSOp.Score += _sigma3;                 // I add σ3 to repair station operation
                        }
                    }
                }

                // On console i print all data, and i use color on different cases:
                // Green -> best solution
                // Orange -> local solution
                // White -> accepted solution
                // Gray -> worse solution
                // Red -> raise the temperature
                switch (newSol.Type)
                {
                    case TypeSolution.BestSol:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Iter {iter} | Temperature {_T} | Cost sol: {newSol.Cost.Total} -> global");
                        break;
                    case TypeSolution.LocalSol:
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine($"Iter {iter} | Temperature {_T} | Cost sol: {newSol.Cost.Total} -> local");
                        break;
                    case TypeSolution.AcceptedSol:
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"Iter {iter} | Temperature {_T} | Cost sol: {newSol.Cost.Total} -> accepted");
                        break;
                    case TypeSolution.WorseSol:
                        Console.ForegroundColor = worseSequence == (_maxIt / 10) && betterSol ? ConsoleColor.Red : ConsoleColor.DarkGray;
                        Console.WriteLine($"Iter {iter} | Temperature {_T} | Cost sol: {newSol.Cost.Total} -> worse");
                        break;
                }

                // If I have worse sequence for maxIt/10 and it was found a new solution,
                // I raise the temperature a 3/4 of total
                if (worseSequence == (_maxIt / 10) && betterSol)
                {
                    worseSequence = 0;
                    _T += _T0 * 3 / 4;
                    betterSol = false;
                }

                // Every _updIt I update operation weigths
                if (iter % _updIt == 0 && iter > 0)
                {
                    // Update weights of destroy operations used
                    _destroyCustomerOperations.ForEach(desOp => desOp.Weight = desOp.Weight * (1 - _r) + (desOp.Times == 0 ? 0 : _r * desOp.Score / desOp.Times));
                    _destroyCustomerOperations.ForEach(desOp => desOp.Probability = desOp.Weight / _destroyCustomerOperations.Sum(dop => dop.Weight));

                    // Update weights of repair operation used
                    _repairCustomerOperations.ForEach(repOp => repOp.Weight = repOp.Weight * (1 - _r) + (repOp.Times == 0 ? 0 : _r * repOp.Score / repOp.Times));
                    _repairCustomerOperations.ForEach(repOp => repOp.Probability = repOp.Weight / _repairCustomerOperations.Sum(rop => rop.Weight));
                }

                AllSol.Add(newSol);             // Add the new solution to the list of all solutions

                _T = _c * _T;                   // Exponential cooling
                iter++;                         // Update iterations

                if (_T <= _TF) break;           // I terminate when I reached the final temperature
                if (iter >= _maxIt) break;      // I terminate when I reached maximum number of iterations
            }

            watch.Stop();                       // Stop timer because ALNS is finished

            // I print all operations times used
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine($"Random Customer Destroy used {_destroyCustomerOperations[0].Times} times");
            Console.WriteLine($"Shaw Customer Destroy used {_destroyCustomerOperations[1].Times} times");
            Console.WriteLine($"Worst Cost Customer Destroy used {_destroyCustomerOperations[2].Times} times");

            Console.WriteLine($"Random Station Destroy used {_destroyStationOperations[0].Times} times");
            Console.WriteLine($"Worst Cost Station Destroy used {_destroyStationOperations[1].Times} times");
            Console.WriteLine($"Full Charge Station Destroy used {_destroyStationOperations[2].Times} times");
            Console.WriteLine($"Worst Charge Usage Station Destroy used {_destroyStationOperations[3].Times} times");

            Console.WriteLine($"Greedy Customer Repair used {_repairCustomerOperations[0].Times} times");
            Console.WriteLine($"2-Regret Customer Repair used {_repairCustomerOperations[1].Times} times");
            Console.WriteLine($"3-Regret Customer Repair used {_repairCustomerOperations[2].Times} times");
            Console.WriteLine($"4-Regret Customer Repair used {_repairCustomerOperations[3].Times} times");

            Console.WriteLine($"Greedy Station Repair used {_repairStationOperations[0].Times} times");
            Console.WriteLine($"Greedy Station With Comparison Repair used {_repairStationOperations[1].Times} times");
            Console.WriteLine($"Best Station Repair used {_repairStationOperations[2].Times} times");

            // I print the time to find the solution
            Console.WriteLine($"Time: {watch.ElapsedMilliseconds/1000} sec");

            // If the last best solution found isn't feasible, I return the last feasible solution
            if(!BestSol.IsFeasible)
                return AllSol.Where(s => s.Type == TypeSolution.BestSol && s.IsFeasible).LastOrDefault();

            // I return the last best solution found
            return BestSol;
        }
    }
}
