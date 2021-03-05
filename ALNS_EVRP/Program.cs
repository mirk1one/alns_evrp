using ALNS_EVRP.Data;
using ALNS_EVRP.Metaheuristic;
using ALNS_EVRP.Model;
using ALNS_EVRP.Utils;
using Draw_Graph.Element;
using Draw_Graph.Utils;
using System;
using System.IO;

namespace ALNS_EVRP
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Input inputData = null;

            try
            {
                // Load data from file
                string path = @"C:\Users\mirko\Downloads\EVRP_Instances_Large_Project_2020\c101_21.txt";
                string[] fileLines = File.ReadAllLines(path);
                inputData = FileUtils.GetInputFromFileStrings(fileLines);
                Console.WriteLine($"INPUT LOADED:\n {inputData}");
            }
            catch (FileLoadException flEx)
            {
                Console.WriteLine($"Problem with loading file: {flEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Problem with loading file: {ex.Message}");
            }

            float alpha = 1;           // α: weight of distance
            float beta = 0.01f;        // β: weight of time
            float gamma = 1;           // γ: weight of battery
            float delta = 1000;        // δ: weight of customers not served
            float betaViol = 1000;     // βV: weight of time violation
            float gammaViol = 1000;    // γV: weight of battery violation
            float deltaViol = 1000;    // δV: weight of load violation

            try
            {
                // Find initial solution
                Problem problem = new Problem(inputData, alpha, beta, gamma, delta, betaViol, gammaViol, deltaViol);
                problem.FindStartSolution();
                Solution sol = problem.Solution;

                // Find best solution with Adaptive Large Neighborhood Search
                ALNS aLNS = new ALNS(sol);
                int q = 5;
                aLNS.AdaptiveLargeNeighborhoodSearch(q);

                // Create file excel
                ExcelUtils.CreateSolutionExcel(sol, aLNS.BestSol, aLNS.AllSol);

                // Create image graph
                DrawGraph graph = aLNS.BestSol.GetDrawRouteGraph();
                GraphUtils.ShowGraph(graph);
            } catch(Exception ex)
            {
                Console.WriteLine($"Problem with finding solution: {ex.Message}");
            }
        }
    }
}
