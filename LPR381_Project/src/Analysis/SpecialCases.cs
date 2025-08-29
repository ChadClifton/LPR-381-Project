using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LPR381_Project.Algorithms;
using LPR381_Project.Models;

namespace LPR381_Project.Analysis
{
    public static class SpecialCases
    {
        public static bool IsInfeasible(LPModel model, SimplexResult result)
        {
            // Check for structural infeasibility (e.g., all coeffs zero with incompatible RHS)  
            for (int i = 0; i < model.NumConstraints; i++)
            {
                var c = model.Constraints[i];
                if (c.Coeffs.All(coeff => Math.Abs(coeff) < 1e-6) &&
                    ((c.Type == ConstraintType.LessThanOrEqual && c.RHS < 0) ||
                     (c.Type == ConstraintType.MoreThanOrEqual && c.RHS > 0) ||
                     (c.Type == ConstraintType.Equal && c.RHS != 0)))
                    return true;
            }
            // Check solver-detected infeasibility  
            return result.Status == SolveStatus.Infeasible;
        }

        public static bool IsUnbounded(SimplexResult result)
        {
            return result.Status == SolveStatus.Unbounded;
        }

        public static bool IsDegenerate(SimplexResult result)
        {
            // Degeneracy occurs if any basic variable in the primal solution is zero (within tolerance)    
            if (result.PrimalSolution == null) return false;
            return result.PrimalSolution.Any(v => Math.Abs(v) < 1e-6);
        }

        public static bool HasMultipleSolutions(SimplexResult result)
        {
            // Multiple solutions if a non-basic variable has zero reduced cost  
            if (result.DualPrices == null) return false;
            return result.DualPrices.Any(rc => Math.Abs(rc) < 1e-6);
        }

        public static void Report(LPModel model, SimplexResult result)
        {
            Console.Clear();
            Console.WriteLine("=== Special Case Analysis ===");
            Console.WriteLine($"Model: {model.NumVariables} variables, {model.NumConstraints} constraints");
            Console.WriteLine($"Solution Status: {result.Status}");

            bool hasSpecialCase = false;

            if (IsInfeasible(model, result))
            {
                Console.WriteLine("Special Case: Model is infeasible.");
                Console.WriteLine("  - Either structurally infeasible (e.g., zero coeffs with incompatible RHS) or solver detected infeasibility.");
                hasSpecialCase = true;
            }

            if (IsUnbounded(result))
            {
                Console.WriteLine("Special Case: Model is unbounded.");
                Console.WriteLine("  - The objective can be improved indefinitely.");
                hasSpecialCase = true;
            }

            if (IsDegenerate(result))
            {
                Console.WriteLine("Special Case: Model is degenerate.");
                Console.WriteLine("  - At least one basic variable is zero, which may cause cycling.");
                hasSpecialCase = true;
            }

            if (HasMultipleSolutions(result))
            {
                Console.WriteLine("Special Case: Multiple optimal solutions may exist.");
                Console.WriteLine("  - A non-basic variable has a zero reduced cost.");
                hasSpecialCase = true;
            }

            if (!hasSpecialCase)
            {
                Console.WriteLine("No special cases detected.");
                Console.WriteLine("  - The solution appears standard with a unique optimal value.");
            }

            Console.WriteLine("-----------------------------");
        }
    }
}

/*
namespace LPR381_Project.Analysis
{
    public static class SpecialCases
    {
        /// <summary>
        /// Check for infeasibility: any constraint contradictory?
        /// e.g. 0x <= -5
        /// </summary>
        public static bool IsInfeasible(LPModel model)
        {
            return model.Constraints.Any(c =>
                c.Coeffs.All(coeff => Math.Abs(coeff) < 1e-9) && c.RHS < 0);
        }

        /// <summary>
        /// Check for unboundedness based on SimplexResult status.
        /// (Solver must set SolveStatus.Unbounded.)
        /// </summary>
        public static bool IsUnbounded(SimplexResult result)
        {
            return result.Status == SolveStatus.Unbounded;
        }

        /// <summary>
        /// Degeneracy: if a basic feasible solution has one or more basic variables = 0.
        /// </summary>
        public static bool IsDegenerate(SimplexResult result)
        {
            return result.PrimalSolution != null &&
                   result.PrimalSolution.Any(x => Math.Abs(x) < 1e-9);
        }

        /// <summary>
        /// Multiple optima: if reduced cost of a nonbasic variable = 0 at optimality.
        /// </summary>
        public static bool HasAlternateOptima(SimplexResult result)
        {
            if (result.ReducedCosts == null) return false;
            return result.Status == SolveStatus.Optimal &&
                   result.ReducedCosts.Any(rc => Math.Abs(rc) < 1e-9);
        }

        /// <summary>
        /// Print detected special cases.
        /// </summary>
        public static void Report(LPModel model, SimplexResult result)
        {
            Console.WriteLine("---- Special Case Analysis ----");
            if (IsInfeasible(model)) Console.WriteLine("? Model is infeasible.");
            if (IsUnbounded(result)) Console.WriteLine("? Model is unbounded.");
            if (IsDegenerate(result)) Console.WriteLine("? Degeneracy detected (some basic vars = 0).");
            if (HasAlternateOptima(result)) Console.WriteLine("? Multiple optimal solutions exist.");
        }
    }
}
*/