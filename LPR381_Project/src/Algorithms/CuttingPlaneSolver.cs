using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LPR381_Project.Models;

namespace LPR381_Project.Algorithms
{
    public static class CuttingPlaneSolver
    {
        /// <summary>
        /// Solves the integer program using Gomory cutting planes.
        /// Adds cuts and re-solves LP relaxation.
        /// </summary>
        public static SimplexResult SolveCuttingPlane(LPModel model, out List<string> allIterations)
        {
            allIterations = new List<string>();
            allIterations.Add("Cutting Plane Algorithm Started");

            LPModel current = model.Clone();
            SimplexResult sol = null;
            int maxIter = 100;

            for (int iter = 0; iter < maxIter; iter++)
            {
                sol = new SimplexSolver().SolvePrimal(current);
                allIterations.AddRange(sol.Iterations);

                if (sol.Status != SolveStatus.Optimal)
                {
                    allIterations.Add($"Terminated: {sol.Status}");
                    break;
                }

                if (IsIntegerSolution(sol.PrimalSolution, model.Variables))
                {
                    allIterations.Add("Integer optimal solution found");
                    break;
                }

                int varIndex = -1;
                double maxFrac = 0;
                for (int i = 0; i < sol.PrimalSolution.Length; i++)
                {
                    if (model.Variables[i].IsInteger || model.Variables[i].IsBinary)
                    {
                        double frac = sol.PrimalSolution[i] - Math.Floor(sol.PrimalSolution[i]);
                        if (frac > maxFrac && frac > 1e-6 && frac < 0.999)
                        {
                            maxFrac = frac;
                            varIndex = i;
                        }
                    }
                }

                if (varIndex == -1)
                {
                    allIterations.Add("No fractional basic variable found");
                    break;
                }

                double[] cutCoeffs = new double[model.ObjectiveCoefficients.Count];
                cutCoeffs[varIndex] = 1;
                double cutRhs = Math.Floor(sol.PrimalSolution[varIndex]);

                current.AddConstraint(new Constraint(cutCoeffs, ConstraintType.MoreThanOrEqual, cutRhs));

                allIterations.Add($"Added Gomory cut: {model.Variables[varIndex].Name} >= {Math.Round(cutRhs, 3)}");
            }

            return sol ?? new SimplexResult { Status = SolveStatus.Infeasible };
        }

        private static bool IsIntegerSolution(double[] values, List<Variable> vars)
        {
            int len = Math.Min(values.Length, vars.Count);

            for (int i = 0; i < len; i++)
            {
                if (vars[i].IsInteger || vars[i].IsBinary)
                {
                    if (Math.Abs(values[i] - Math.Round(values[i])) > 1e-6)
                        return false;
                }
            }
            return true;
        }
    }
}

/*
namespace LPR381_Project.Algorithms
{
    public static class CuttingPlaneSolver
    {
        public static SimplexResult SolveCuttingPlane(LPModel model, out List<string> allIterations)
        {
            allIterations = new List<string>();
            allIterations.Add("Cutting Plane Algorithm Started");

            LPModel current = model.Clone();
            SimplexResult sol = null!;
            int maxIter = 100;

            for (int iter = 0; iter < maxIter; iter++)
            {
                sol = SimplexSolver.SolvePrimalSimplex(current);
                allIterations.AddRange(sol.Iterations);

                if (sol.Status != SolveStatus.Optimal)
                {
                    allIterations.Add($"Terminated: {sol.Status}");
                    break;
                }

                if (IsIntegerSolution(sol.PrimalSolution, model.Variables))
                {
                    allIterations.Add("Integer optimal solution found");
                    break;
                }

                int varIndex = -1;
                double maxFrac = 0;
                for (int i = 0; i < sol.PrimalSolution.Length; i++)
                {
                    if (model.Variables[i].IsInteger || model.Variables[i].IsBinary)
                    {
                        double frac = sol.PrimalSolution[i] - Math.Floor(sol.PrimalSolution[i]);
                        if (frac > maxFrac && frac > 1e-6 && frac < 0.999)
                        {
                            maxFrac = frac;
                            varIndex = i;
                        }
                    }
                }

                if (varIndex == -1)
                {
                    allIterations.Add("No fractional basic variable found");
                    break;
                }

                double[] cutCoeffs = new double[model.ObjectiveCoefficients.Count];
                cutCoeffs[varIndex] = 1;
                double cutRhs = Math.Floor(sol.PrimalSolution[varIndex]);

                current.Constraints.Add(new Constraint { Coeffs = cutCoeffs, Type = ConstraintType.MoreThanOrEqual, RHS = cutRhs });

                allIterations.Add($"Added Gomory cut: {model.Variables[varIndex].Name} >= {Math.Round(cutRhs, 3)}");
            }

            return sol ?? new SimplexResult { Status = SolveStatus.Infeasible };
        }

        private static bool IsIntegerSolution(double[] values, List<Variable> vars)
        {
            int len = Math.Min(values.Length, vars.Count);

            for (int i = 0; i < len; i++)
            {
                if (vars[i].IsInteger || vars[i].IsBinary)
                {
                    if (Math.Abs(values[i] - Math.Round(values[i])) > 1e-6)
                        return false; // Not integer
                }
            }
            return true;
        }
    }
}
*/