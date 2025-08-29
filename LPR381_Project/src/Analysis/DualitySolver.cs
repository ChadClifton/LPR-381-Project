using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LPR381_Project.Models;
using LPR381_Project.Algorithms;

namespace LPR381_Project.Analysis
{
    public class DualitySolver
    {
        private readonly SimplexSolver _solver;

        public DualitySolver(SimplexSolver solver)
        {
            _solver = solver;
        }

        /// <summary>
        /// Construct the dual of the given primal LP (in canonical max form).
        /// Assumes: Max c^T x, s.t. A x <= b, x >= 0.
        /// </summary>
        public LPModel ConstructDual(LPModel primal)
        {
            int m = primal.NumConstraints; // # constraints
            int n = primal.NumVariables; // # variables
            var dual = new LPModel(isMaximization: false); // Dual is minimization
            // Dual variables y1..ym with objective coefficients = b_i (RHS of primal)
            for (int i = 0; i < m; i++)
            {
                var y = new Variable($"y{i + 1}", "+");
                dual.AddVariable(y, primal.Constraints[i].RHS);
            }
            // Dual constraints: A^T y >= c
            for (int j = 0; j < n; j++)
            {
                var col = new double[m];
                for (int i = 0; i < m; i++)
                    col[i] = primal.Constraints[i].Coeffs[j];
                var con = new Constraint(col, ConstraintType.MoreThanOrEqual, primal.ObjectiveCoefficients[j])
                {
                    Relation = ">="
                };
                dual.AddConstraint(con);
            }
            return dual;
        }

        /// <summary>
        /// Solve the dual problem and return results.
        /// </summary>
        public SimplexResult SolveDual(LPModel primal, bool useRevised = true)
        {
            var dual = ConstructDual(primal);
            return useRevised ? _solver.SolveRevised(dual) : _solver.SolvePrimal(dual);
        }

        /// <summary>
        /// Compare primal vs dual results (weak/strong duality check).
        /// </summary>
        public void VerifyDuality(SimplexResult primalRes, SimplexResult dualRes)
        {
            Console.Clear();
            Console.WriteLine("=== Duality Analysis ===");
            Console.WriteLine($"Primal Status: {primalRes.Status}");
            Console.WriteLine($"Dual Status: {dualRes.Status}");

            if (primalRes.Status != SolveStatus.Optimal || dualRes.Status != SolveStatus.Optimal)
            {
                Console.WriteLine("Warning: Duality verification requires both primal and dual to be optimal.");
                return;
            }

            double primalObj = primalRes.ObjectiveValue;
            double dualObj = dualRes.ObjectiveValue;
            double gap = Math.Abs(primalObj - dualObj);
            double tolerance = 1e-6; // Tolerance for floating-point comparison

            Console.WriteLine($"Primal Objective: {primalObj:0.###}");
            Console.WriteLine($"Dual Objective: {dualObj:0.###}");
            Console.WriteLine($"Objective Gap: {gap:0.###}");

            if (gap < tolerance)
            {
                Console.WriteLine("Conclusion: Strong Duality Holds.");
                Console.WriteLine("  - Primal and dual objectives are equal within tolerance, confirming optimality.");
            }
            else
            {
                Console.WriteLine("Conclusion: Weak Duality or Error Detected.");
                Console.WriteLine("  - Objectives differ beyond tolerance; check model or solver results.");
            }

            Console.WriteLine("-----------------------------");
        }
    }
}