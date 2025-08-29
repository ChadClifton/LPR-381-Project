using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LPR381_Project.Models;

namespace LPR381_Project.Algorithms
{
    public static class BnBSolver
    {
        /// <summary>
        /// Solves the integer program using Branch and Bound with Simplex relaxations.
        /// Displays all subproblem iterations as per brief.
        /// </summary>
        public static SimplexResult SolveBranchAndBound(LPModel model, out List<string> allIterations)
        {
            allIterations = new List<string> { "Branch and Bound Simplex Algorithm Started" };

            double bestObj = model.IsMaximization ? double.MinValue : double.MaxValue;
            double[] bestSolution = null;
            SimplexResult bestResult = null;

            SolveBBSRecursive(model, ref bestObj, ref bestSolution, allIterations, ref bestResult);

            if (bestResult == null)
                bestResult = new SimplexResult { Status = SolveStatus.Optimal };

            bestResult.PrimalSolution = bestSolution;
            bestResult.ObjectiveValue = Math.Round(bestObj, 3);
            bestResult.Iterations = allIterations;
            return bestResult;
        }

        private static void SolveBBSRecursive(LPModel model, ref double bestObj, ref double[] bestSolution,
            List<string> allIterations, ref SimplexResult bestResult)
        {
            SimplexResult sol = new SimplexSolver().SolvePrimal(model);
            allIterations.AddRange(sol.Iterations);

            if (sol.Status != SolveStatus.Optimal)
            {
                allIterations.Add($"Fathomed: {sol.Status}");
                return;
            }

            bool isMax = model.IsMaximization;
            bool worse = isMax ? sol.ObjectiveValue <= bestObj : sol.ObjectiveValue >= bestObj;

            if (worse)
            {
                allIterations.Add($"Fathomed: Bound {Math.Round(sol.ObjectiveValue, 3)} not better than best {Math.Round(bestObj, 3)}");
                return;
            }

            if (IsIntegerSolution(sol.PrimalSolution, model.Variables))
            {
                allIterations.Add($"Integer solution found with objective {Math.Round(sol.ObjectiveValue, 3)}");
                bool better = isMax ? sol.ObjectiveValue > bestObj : sol.ObjectiveValue < bestObj;
                if (better)
                {
                    bestObj = sol.ObjectiveValue;
                    bestSolution = (double[])sol.PrimalSolution.Clone();
                    bestResult = sol;
                }
                return;
            }

            int varIndex = GetBranchVar(sol.PrimalSolution, model.Variables);
            if (varIndex == -1) return;

            double value = sol.PrimalSolution[varIndex];
            double floorVal = Math.Floor(value);
            double ceilVal = Math.Ceiling(value);

            allIterations.Add($"Branching on {model.Variables[varIndex].Name} = {Math.Round(value, 3)} (floor {floorVal}, ceil {ceilVal})");

            // Left branch: var <= floor
            LPModel left = model.Clone();
            double[] leftCoeffs = new double[left.ObjectiveCoefficients.Count];
            leftCoeffs[varIndex] = 1;
            left.AddConstraint(new Constraint(leftCoeffs, ConstraintType.LessThanOrEqual, floorVal));
            SolveBBSRecursive(left, ref bestObj, ref bestSolution, allIterations, ref bestResult);

            // Right branch: var >= ceil
            LPModel right = model.Clone();
            double[] rightCoeffs = new double[right.ObjectiveCoefficients.Count];
            rightCoeffs[varIndex] = 1;
            right.AddConstraint(new Constraint(rightCoeffs, ConstraintType.MoreThanOrEqual, ceilVal));
            SolveBBSRecursive(right, ref bestObj, ref bestSolution, allIterations, ref bestResult);
        }

        private static bool IsIntegerSolution(double[] values, List<Variable> vars)
        {
            for (int i = 0; i < vars.Count; i++)
            {
                if (vars[i].IsInteger || vars[i].IsBinary)
                {
                    if (Math.Abs(values[i] - Math.Round(values[i])) > 1e-6)
                        return false;
                }
            }
            return true;
        }

        private static int GetBranchVar(double[] values, List<Variable> vars)
        {
            double maxDist = -1;
            int index = -1;
            for (int i = 0; i < vars.Count; i++)
            {
                if (vars[i].IsInteger || vars[i].IsBinary)
                {
                    double dist = Math.Abs(values[i] - Math.Round(values[i]));
                    if (dist > 1e-6 && dist > maxDist)
                    {
                        maxDist = dist;
                        index = i;
                    }
                }
            }
            return index;
        }
    }
}

/*
namespace LPR381_Project.Algorithms
{
    public static class BnBSolver
    {
        public static SimplexResult SolveBranchAndBound(LPModel model, out List<string> allIterations)
        {
            allIterations = new List<string> { };
            allIterations.Add("Branch and Bound Simplex Algorithm Started");

            double bestObj = model.IsMaximization ? double.MinValue : double.MaxValue;
            double[] bestSolution = null!;
            SimplexResult bestResult = null!;

            SolveBBSRecursive(model, ref bestObj, ref bestSolution, allIterations, ref bestResult);

            if (bestResult == null)
                bestResult = new SimplexResult { Status = SolveStatus.Optimal };

            bestResult.PrimalSolution = bestSolution;
            bestResult.ObjectiveValue = Math.Round(bestObj, 3);
            bestResult.Iterations = allIterations;
            return bestResult;
        }

        private static void SolveBBSRecursive(LPModel model, ref double bestObj, ref double[] bestSolution,
            List<string> allIterations, ref SimplexResult bestResult)
        {
            SimplexResult sol = SimplexSolver.SolvePrimalSimplex(model);
            allIterations.AddRange(sol.Iterations);

            if (sol.Status != SolveStatus.Optimal)
            {
                allIterations.Add($"Fathomed: {sol.Status}");
                return;
            }

            bool isMax = model.IsMaximization;
            bool worse = isMax ? sol.ObjectiveValue <= bestObj : sol.ObjectiveValue >= bestObj;

            if (worse)
            {
                allIterations.Add($"Fathomed: Bound {Math.Round(sol.ObjectiveValue, 3)} not better than best {Math.Round(bestObj, 3)}");
                return;
            }

            if (IsIntegerSolution(sol.PrimalSolution, model.Variables))
            {
                allIterations.Add($"Integer solution found with objective {Math.Round(sol.ObjectiveValue, 3)}");
                bool better = isMax ? sol.ObjectiveValue > bestObj : sol.ObjectiveValue < bestObj;
                if (better)
                {
                    bestObj = sol.ObjectiveValue;
                    bestSolution = (double[])sol.PrimalSolution.Clone();
                    bestResult = sol;
                }
                return;
            }

            int varIndex = GetBranchVar(sol.PrimalSolution, model.Variables);
            if (varIndex == -1) return;

            double value = sol.PrimalSolution[varIndex];
            double floorVal = Math.Floor(value);
            double ceilVal = Math.Ceiling(value);

            allIterations.Add($"Branching on {model.Variables[varIndex].Name} = {Math.Round(value, 3)} (floor {floorVal}, ceil {ceilVal})");

            // Left branch
            LPModel left = model.Clone();
            double[] leftCoeffs = new double[left.ObjectiveCoefficients.Count];
            leftCoeffs[varIndex] = 1;
            left.Constraints.Add(new Constraint { Coeffs = leftCoeffs, Type = ConstraintType.LessThanOrEqual, RHS = floorVal });
            SolveBBSRecursive(left, ref bestObj, ref bestSolution, allIterations, ref bestResult);

            // Right branch
            LPModel right = model.Clone();
            double[] rightCoeffs = new double[right.ObjectiveCoefficients.Count];
            rightCoeffs[varIndex] = 1;
            right.Constraints.Add(new Constraint { Coeffs = rightCoeffs, Type = ConstraintType.MoreThanOrEqual, RHS = ceilVal });
            SolveBBSRecursive(right, ref bestObj, ref bestSolution, allIterations, ref bestResult);
        }

        private static bool IsIntegerSolution(double[] values, List<Variable> vars)
        {
            for (int i = 0; i < vars.Count; i++)
            {
                if (vars[i].IsInteger || vars[i].IsBinary)
                {
                    if (Math.Abs(values[i] - Math.Round(values[i])) > 1e-6)
                        return false; // Not integer
                }
            }

            return true;
        }

        private static int GetBranchVar(double[] values, List<Variable> vars)
        {
            double maxDist = -1;
            int index = -1;
            for (int i = 0; i < vars.Count; i++)
            {
                if (vars[i].IsInteger || vars[i].IsBinary)
                {
                    double dist = Math.Abs(values[i] - Math.Round(values[i]));
                    if (dist > 1e-6 && dist > maxDist)
                    {
                        maxDist = dist;
                        index = i;
                    }
                }
            }
            return index;
        }
    }
}
*/