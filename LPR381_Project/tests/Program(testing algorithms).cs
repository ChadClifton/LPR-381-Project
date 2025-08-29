//this file is just to test if the algorithms works
using LPR381_Project.IO;
using LPR381_Project.Algorithms;
using LPR381_Project.Models;

        /*
        
            string inputPath = "C:\\Users\\Mosa Work\\OneDrive - belgiumcampus.ac.za\\THIRD YEAR\\lpr381\\LPR Projet Updated 26\\LPR381_Project\\tests\\data\\model1.txt"; //change file path accordingly
            LPModel model = FileHandler.LoadModel(inputPath);

            Console.WriteLine("======= Testing Different Algorithms =======");

            // 1. Simplex Test
            Console.WriteLine("\n--- Simplex Test ---");
            var simplexResult = SimplexSolver.SolvePrimalSimplex(model);
            DisplayResult(simplexResult);

            // 2. Branch and Bound Test
            Console.WriteLine("\n--- Branch and Bound Test ---");
            var bnbResult = BnBSolver.SolveBranchAndBound(model, out List<string> bnbIterations);
            DisplayResult(bnbResult);

            // 3. Cutting Plane Test
            Console.WriteLine("\n--- Cutting Plane Test ---");
            var cpResult = CuttingPlaneSolver.SolveCuttingPlane(model, out List<string> cpIterations);
            DisplayResult(cpResult);

            // 4. Knapsack Test (if your model is actually a knapsack)
            Console.WriteLine("\n--- Knapsack Test ---");
            var knapsackResult = Knapsack.SolveBranchAndBoundKnapsack(model, out List<string> kpIterations);
            DisplayResult(knapsackResult);

            Console.WriteLine("\n======= End of Tests =======");
        

        // Helper method to display results in a consistent format
        static void DisplayResult(SimplexResult result)
        {
            Console.WriteLine($"Status: {result.Status}");
            Console.WriteLine($"Objective Value: {result.ObjectiveValue}");
            if (result.PrimalSolution != null)
            {
                Console.WriteLine("Solution:");
                for (int i = 0; i < result.PrimalSolution.Length; i++)
                    Console.WriteLine($"x{i + 1} = {Math.Round(result.PrimalSolution[i], 3)}");
            }
            Console.WriteLine("Iterations:");
            foreach (var step in result.Iterations)
                Console.WriteLine("  " + step);
        }
    
*/