using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LPR381_Project.Utils;
using LPR381_Project.Models;
using LPR381_Project.Algorithms;
using LPR381_Project.Analysis;
using LPR381_Project.IO;

/*
(Grace work)

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
 */


namespace LPR381_Project
{
    class Program
    {
        private static LPModel model;
        private static SimplexResult lastResult;
        private static SensitivityAnalysis sa;
        private static DualitySolver ds = new DualitySolver(new SimplexSolver());
        private static string algo = "primal";

        static void DisplayMainMenu()
        {
            Console.Clear();
            Console.WriteLine("=== LPR381 Solver ===");
            Console.WriteLine("1. Load Input File");
            Console.WriteLine($"2. Select Algorithm (current: {algo})");
            Console.WriteLine("3. Solve Model");
            Console.WriteLine("4. Export Results");
            Console.WriteLine("5. Sensitivity Analysis");
            Console.WriteLine("6. Special Cases");
            Console.WriteLine("7. Duality");
            Console.WriteLine("8. Exit");
        }

        static void Main(string[] args)
        {
            bool running = true;

            while (running)
            {
                DisplayMainMenu();
                Console.Write("Choose option: ");
                string choice = Console.ReadLine();

                Console.Clear(); // Clear console after selection
                switch (choice)
                {
                    case "1":
                        Console.Write("Enter file path: ");
                        string filePath = Console.ReadLine();
                        model = FileHandler.LoadModel(filePath);
                        lastResult = null;
                        sa = null;
                        DisplayMainMenu(); // Show main menu again
                        break;

                    case "2":
                        DisplayAlgorithmMenu();
                        string algoChoice = Console.ReadLine().ToLower();
                        algo = algoChoice switch
                        {
                            "a" => "primal",
                            "b" => "revised",
                            "c" => "bnb",
                            "d" => "cutting",
                            "e" => "knapsack",
                            _ => algo
                        };
                        DisplayMainMenu(); // Return to main menu
                        break;

                    case "3":
                        if (model == null)
                        {
                            Console.WriteLine("No model loaded. Please load a model first.");
                            Console.WriteLine("\nPress any key to return to main menu...");
                            Console.ReadKey();
                            DisplayMainMenu();
                            break;
                        }
                        List<string> iterations;
                        lastResult = algo switch
                        {
                            "primal" => new SimplexSolver().SolvePrimal(model),
                            "revised" => new SimplexSolver().SolveRevised(model),
                            "bnb" => BnBSolver.SolveBranchAndBound(model, out iterations),
                            "cutting" => CuttingPlaneSolver.SolveCuttingPlane(model, out iterations),
                            "knapsack" => Knapsack.SolveBranchAndBoundKnapsack(model, out iterations),
                            _ => null
                        };
                        if (lastResult != null)
                        {
                            DisplaySolution(lastResult);
                            sa = new SensitivityAnalysis(model, lastResult, new SimplexSolver(), algo == "revised");
                            Console.WriteLine("\nPress any key to return to main menu...");
                            Console.ReadKey();
                        }
                        else
                        {
                            Console.WriteLine("Failed to solve the model.");
                            Console.WriteLine("\nPress any key to return to main menu...");
                            Console.ReadKey();
                        }
                        DisplayMainMenu(); // Return to main menu
                        break;

                    case "4":
                        if (lastResult == null)
                        {
                            Console.WriteLine("No solution available. Solve a model first.");
                            Console.WriteLine("\nPress any key to return to main menu...");
                            Console.ReadKey();
                            DisplayMainMenu();
                            break;
                        }
                        Console.Write("Enter output path: ");
                        string outPath = Console.ReadLine();
                        OutputFormatter.WriteResults(outPath, model, lastResult);
                        Console.WriteLine("Results exported successfully.");
                        Console.WriteLine("\nPress any key to return to main menu...");
                        Console.ReadKey();
                        DisplayMainMenu(); // Return to main menu
                        break;

                    case "5":
                        if (sa == null)
                        {
                            Console.WriteLine("No sensitivity analysis available. Solve a model first.");
                            Console.WriteLine("\n\nPress any key to return to main menu...");
                            Console.ReadKey();
                            DisplayMainMenu();
                            break;
                        }
                        DisplaySensitivityMenu();
                        break;

                    case "6":
                        if (model == null || lastResult == null)
                        {
                            Console.WriteLine("No model or solution available. Load and solve a model first.");
                            Console.WriteLine("\nPress any key to return to main menu...");
                            Console.ReadKey();
                            DisplayMainMenu();
                            break;
                        }
                        SpecialCases.Report(model, lastResult);
                        Console.WriteLine("\nPress any key to return to main menu...");
                        Console.ReadKey();
                        DisplayMainMenu(); // Return to main menu
                        break;

                    case "7":
                        if (model == null || lastResult == null)
                        {
                            Console.WriteLine("No model or solution available. Load and solve a model first.");
                            Console.WriteLine("\nPress any key to return to main menu...");
                            Console.ReadKey();
                            DisplayMainMenu();
                            break;
                        }
                        try
                        {
                            var dual = ds.ConstructDual(model);
                            var dualRes = ds.SolveDual(model, algo == "revised");
                            ds.VerifyDuality(lastResult, dualRes);
                            Console.WriteLine("\nDuality analysis completed.");
                            Console.WriteLine("\nPress any key to return to main menu...");
                            Console.ReadKey();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error during duality analysis: {ex.Message}");
                        }
                        DisplayMainMenu(); // Return to main menu
                        break;

                    case "8":
                        running = false;
                        Console.WriteLine("Exiting program...");
                        Thread.Sleep(2000); // Pause for 2 seconds (2000 milliseconds)
                        break;

                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        Console.WriteLine("\nPress any key to return to main menu...");
                        Console.ReadKey();
                        DisplayMainMenu(); // Return to main menu on invalid input
                        break;
                }
            }
        }

        

        static void DisplayAlgorithmMenu()
        {
            Console.Clear();
            Console.WriteLine("=== Select Algorithm ===");
            Console.WriteLine(" a) Primal Simplex");
            Console.WriteLine(" b) Revised Simplex");
            Console.WriteLine(" c) Branch & Bound Simplex");
            Console.WriteLine(" d) Cutting Plane");
            Console.WriteLine(" e) Branch & Bound Knapsack");
            Console.Write("Choose algorithm: ");
        }

        static void DisplaySensitivityMenu()
        {
            Console.Clear();
            Console.WriteLine("=== Sensitivity Analysis ===");
            Console.WriteLine("1. Non-Basic Variable Range");
            Console.WriteLine("2. Basic Variable Range");
            Console.WriteLine("3. Constraint RHS Range");
            Console.WriteLine("4. Apply Change to Variable Coefficient");
            Console.WriteLine("5. Apply Change to Constraint RHS");
            Console.WriteLine("6. Add New Constraint");
            Console.WriteLine("7. Add New Variable");
            Console.WriteLine("8. Display Shadow Prices");
            Console.WriteLine("9. Construct and Solve Dual");
            Console.WriteLine("10. Verify Duality");
            Console.WriteLine("11. Return to Main Menu");
            Console.Write("Choose option: ");
            string choice = Console.ReadLine();

            Console.Clear(); // Clear after selection
            switch (choice)
            {
                case "1": // Non-Basic Variable Range
                    Console.Write("Enter non-basic variable name (e.g., x1): ");
                    string nbVar = Console.ReadLine();
                    try
                    {
                        var (inc, dec) = sa.GetNonBasicVariableRange(nbVar);
                        Console.WriteLine($"Non-Basic Variable Range for {nbVar}:");
                        Console.WriteLine($"Increase: {inc:0.000}, Decrease: {dec:0.000}");
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                    break;

                case "2": // Basic Variable Range
                    Console.Write("Enter basic variable name (e.g., x1): ");
                    string bVar = Console.ReadLine();
                    try
                    {
                        var (down, up) = sa.GetBasicVariableRange(bVar);
                        Console.WriteLine($"Basic Variable Range for {bVar}:");
                        Console.WriteLine($"Down: {down:0.000}, Up: {up:0.000}");
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                    break;

                case "3": // Constraint RHS Range
                    Console.Write("Enter constraint name (e.g., c1): ");
                    string cName = Console.ReadLine();
                    try
                    {
                        var (down, up) = sa.GetConstraintRHSRange(cName);
                        Console.WriteLine($"Constraint RHS Range for {cName}:");
                        Console.WriteLine($"Down: {down:0.000}, Up: {up:0.000}");
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Error: Invalid constraint name format. Use c1, c2, etc.");
                    }
                    break;

                case "4": // Apply Change to Variable Coefficient
                    Console.Write("Enter variable name (e.g., x1): ");
                    string varName = Console.ReadLine();
                    Console.Write("Enter new coefficient: ");
                    if (double.TryParse(Console.ReadLine(), out double newCoeff))
                    {
                        sa.ApplyChangeToVariable(varName, newCoeff);
                        Console.WriteLine($"Coefficient for {varName} updated to {newCoeff:0.000}.");
                    }
                    else
                    {
                        Console.WriteLine("Error: Invalid coefficient value.");
                    }
                    break;

                case "5": // Apply Change to Constraint RHS
                    Console.Write("Enter constraint name (e.g., c1): ");
                    string conName = Console.ReadLine();
                    Console.Write("Enter new RHS value: ");
                    if (double.TryParse(Console.ReadLine(), out double newRhs))
                    {
                        sa.ApplyChangeToConstraintRHS(conName, newRhs);
                        Console.WriteLine($"RHS for {conName} updated to {newRhs:0.000}.");
                    }
                    else
                    {
                        Console.WriteLine("Error: Invalid RHS value.");
                    }
                    break;

                case "6": // Add New Constraint
                    Console.Write("Enter constraint coefficients (space-separated, e.g., 1 2 3): ");
                    var coeffsInput = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    double[] coeffs = coeffsInput.Select(double.Parse).ToArray();
                    Console.Write("Enter relation (<=, >=, =): ");
                    string relation = Console.ReadLine();
                    Console.Write("Enter RHS value: ");
                    if (double.TryParse(Console.ReadLine(), out double rhs))
                    {
                        var constraint = new Constraint(coeffs, Constraint.ParseRelation(relation), rhs) { Relation = relation };
                        sa.AddConstraint(constraint);
                        Console.WriteLine("Constraint added successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Error: Invalid RHS value.");
                    }
                    break;

                case "7": // Add New Variable
                    Console.Write("Enter variable name (e.g., x4): ");
                    string newVarName = Console.ReadLine();
                    Console.Write("Enter objective coefficient: ");
                    if (double.TryParse(Console.ReadLine(), out double objCoeff))
                    {
                        Console.Write("Enter constraint coefficients (space-separated): ");
                        var colInput = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        double[] column = colInput.Select(double.Parse).ToArray();
                        var newVar = new Variable(newVarName, "+");
                        sa.AddVariable(newVar, objCoeff, column);
                        Console.WriteLine($"Variable {newVarName} added successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Error: Invalid coefficient value.");
                    }
                    break;

                case "8": // Display Shadow Prices
                    var shadowPrices = sa.GetShadowPrices();
                    Console.WriteLine("Shadow Prices:");
                    for (int i = 0; i < shadowPrices.Length; i++)
                    {
                        Console.WriteLine($"c{i + 1}: {shadowPrices[i]:0.000}");
                    }
                    break;

                case "9": // Construct and Solve Dual
                    var dual = sa.ConstructDual();
                    var dualRes = sa.SolveDual();
                    Console.WriteLine("Dual constructed and solved.");
                    DisplaySolution(dualRes); // Reuse DisplaySolution method
                    break;

                case "10": // Verify Duality
                    sa.VerifyDuality();
                    Console.WriteLine("Duality verified.");
                    break;

                case "11": // Return to Main Menu
                    DisplayMainMenu();
                    return;

                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }

            Console.WriteLine("\nPress any key to return to sensitivity menu...");
            Console.ReadKey();
            DisplaySensitivityMenu(); // Loop back to sensitivity menu
        }

        static void DisplaySolution(SimplexResult result)
        {
            Console.Clear();
            Console.WriteLine("=== Solution ===");
            Console.WriteLine($"Status: {result.Status}");
            Console.WriteLine($"Objective Value: {result.ObjectiveValue:0.000}");
            Console.WriteLine("Variables:");

            // Iterate over the PrimalSolution array and use the model's variable names
                        int loopLimit = Math.Min(result.PrimalSolution.Length, model.Variables.Count);
            for (int i = 0; i < loopLimit; i++)
            {
                Console.WriteLine($"  {model.Variables[i].Name}: {result.PrimalSolution[i]:0.000}");
            }


            Console.WriteLine("\nIterations:");
            foreach (var iteration in result.Iterations)
            {
                Console.WriteLine($"  {iteration}");
            }
        }
    }
}