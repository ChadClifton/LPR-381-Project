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
        public static bool IsInfeasible(LPModel model)
        {
            for (int i = 0; i < model.NumConstraints; i++)
            {
                var c = model.Constraints[i];
                if (c.Coeffs.All(coeff => Math.Abs(coeff) < 1e-6) && ((c.Type == ConstraintType.LessThanOrEqual && c.RHS < 0) || (c.Type == ConstraintType.MoreThanOrEqual && c.RHS > 0) || (c.Type == ConstraintType.Equal && c.RHS != 0)))
                    return true;
            }
            return false;
        }

        public static bool IsUnbounded(SimplexResult result)
        {
            return result.Status == SolveStatus.Unbounded;
        }

        public static void Report(LPModel model, SimplexResult result)
        {
            Console.WriteLine("---- Special Case Analysis ----");
            if (IsInfeasible(model)) Console.WriteLine("Model is infeasible.");
            if (IsUnbounded(result)) Console.WriteLine("Model is unbounded.");
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