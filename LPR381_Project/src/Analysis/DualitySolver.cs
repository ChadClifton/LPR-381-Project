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
            int n = primal.NumVariables;   // # variables

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
            Console.WriteLine("---- Duality Check ----");
            Console.WriteLine($"Primal Objective: {primalRes.ObjectiveValue:0.###}");
            Console.WriteLine($"Dual Objective:   {dualRes.ObjectiveValue:0.###}");
            Console.WriteLine($"Gap: {Math.Abs(primalRes.ObjectiveValue - dualRes.ObjectiveValue):0.###}");
        }
    }
}
