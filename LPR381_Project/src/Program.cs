using LPR381_Project;




// Chad Work
/*
using System;
using LP381_Project.IO;
using LP381_Project.Models;
using LP381_Project.Algorithms;
using LP381_Project.Analysis;

namespace LP381_Project
{
    class Program
    {
        static void Main(string[] args)
        {
            LPModel model = null;
            SensitivityAnalysis analysis = null;
            bool running = true;

            while (running)
            {
                Console.WriteLine("\n=== LP381 Solver ===");
                Console.WriteLine("1. Load Input File");
                Console.WriteLine("2. Select Algorithm");
                Console.WriteLine("3. Solve Model");
                Console.WriteLine("4. Sensitivity Analysis");
                Console.WriteLine("5. Export Results");
                Console.WriteLine("6. Exit");
                Console.Write("Choose option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Enter file path: ");
                        string filePath = Console.ReadLine();
                        model = FileHandler.LoadModel(filePath);
                        Console.WriteLine("Model loaded successfully.");
                        break;

                    case "2":
                        Console.WriteLine("Select Algorithm:");
                        Console.WriteLine(" a) Primal Simplex");
                        Console.WriteLine(" b) Revised Simplex");
                        Console.WriteLine(" c) Branch & Bound (Simplex)");
                        Console.WriteLine(" d) Branch & Bound Knapsack");
                        Console.WriteLine(" e) Cutting Plane");
                        Console.Write("Choice: ");
                        string algoChoice = Console.ReadLine();
                        // TODO: Store algorithm selection
                        break;

                    case "3":
                        if (model == null)
                        {
                            Console.WriteLine("Load a model first!");
                        }
                        else
                        {
                            Console.WriteLine("Solving model...");
                            // TODO: Call chosen solver
                            // TODO: Display iterations
                            analysis = new SensitivityAnalysis(model);
                        }
                        break;

                    case "4":
                        if (analysis == null)
                        {
                            Console.WriteLine("Solve a model first!");
                        }
                        else
                        {
                            ShowSensitivityMenu(analysis);
                        }
                        break;

                    case "5":
                        Console.Write("Enter output file path: ");
                        string outPath = Console.ReadLine();
                        // TODO: Export results using OutputFormatter
                        break;

                    case "6":
                        running = false;
                        break;

                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }

        static void ShowSensitivityMenu(SensitivityAnalysis analysis)
        {
            Console.WriteLine("\n=== Sensitivity Analysis ===");
            Console.WriteLine(" a) Non-Basic Variable Range");
            Console.WriteLine(" b) Change Non-Basic Variable");
            Console.WriteLine(" c) Basic Variable Range");
            Console.WriteLine(" d) Change Basic Variable");
            Console.WriteLine(" e) Constraint RHS Range");
            Console.WriteLine(" f) Change Constraint RHS");
            Console.WriteLine(" g) Add Variable");
            Console.WriteLine(" h) Add Constraint");
            Console.WriteLine(" i) Shadow Prices");
            Console.WriteLine(" j) Duality");
            Console.Write("Choice: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "a":
                    // TODO: Call analysis.GetNonBasicVariableRange()
                    break;
                case "b":
                    // TODO: Call analysis.ApplyChangeToNonBasicVariable()
                    break;
                case "c":
                    // TODO: Call analysis.GetBasicVariableRange()
                    break;
                case "d":
                    // TODO: Call analysis.ApplyChangeToBasicVariable()
                    break;
                case "e":
                    // TODO: Call analysis.GetConstraintRHRange()
                    break;
                case "f":
                    // TODO: Call analysis.ApplyChangeToConstraintRHS()
                    break;
                case "g":
                    // TODO: Call analysis.AddVariable()
                    break;
                case "h":
                    // TODO: Call analysis.AddConstraint()
                    break;
                case "i":
                    // TODO: Call analysis.CalculateShadowPrices()
                    break;
                case "j":
                    // TODO: Call analysis.ConstructDual() + SolveDual() + VerifyDuality()
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }
    }
}
*/

using System;
using System.Collections.Generic;
using LPR381_Project.IO;
using LPR381_Project.Models;
using LP381_Project.IO;

namespace LP381_Project
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Load a model (simulate a file path)
            string inputFile = "tests/data/model1.txt";
            LPModel model;
            try
            {
                model = FileHandler.LoadModel(inputFile);
                Console.WriteLine("Model loaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading model: {ex.Message}");
                return;
            }

            // 2. Simulate solving the model and producing a SimplexResult
            var result = new SimplexResult
            {
                IsMaximization = model.IsMaximization,
                Variables = model.Variables,
                Constraints = model.Constraints,
                ObjectiveCoefficients = model.ObjectiveCoefficients,
                Iterations = new List<string> { "Iteration 1: x1=0, x2=0", "Iteration 2: x1=1, x2=0.5" },
                PrimalSolution = new double[] { 1, 0.5 },
                DualPrices = new double[] { 2, 0 },
                Status = "Optimal",
                ObjectiveValue = 3.5
            };

            // 3. Write results to an output file
            string outputFile = "tests/data/result1.txt";
            try
            {
                OutputFormatter.WriteResults(outputFile, result);
                Console.WriteLine($"Results written to {outputFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing results: {ex.Message}");
            }
        }
    }
}