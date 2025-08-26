using System;
using System.Collections.Generic;
using System.Linq;
using LP381_Project.Algorithms;
using LP381_Project.Utils;
using LPR381_Project.Models;

namespace LP381_Project.Algorithms
{
    public enum SolveStatus { Optimal, Unbounded, Infeasible, IterationLimit }

    public class SimplexResult
    {
        public SolveStatus Status { get; set; }
        public double ObjectiveValue { get; set; }
        public double[] PrimalSolution { get; set; } = Array.Empty<double>();
        public double[] DualPrices { get; set; } = Array.Empty<double>(); // y = c_B^T B^-1
        public List<string> Iterations { get; set; } = new(); // human-readable tables (3 dp)
    }

    public class SimplexSolver
    {
        private const int ITER_LIMIT = 10; // limits iterations to easily handle looping
        //  PUBLIC API

        public SimplexResult SolvePrimalSimplex(LPModel rawModel)
        {
            var (A, b, c, sense, varNames, basicIdx, nonBasicIdx, hasArtificial) =
                Canonicalizer.ToStandardForm(rawModel);

            if (hasArtificial)
            {
                // Phase I to drive artificials to zero
                var phase1 = RunPrimalTableauPhaseI(A, b, c, varNames, basicIdx, nonBasicIdx);
                if (phase1.Status != SolveStatus.Optimal || Math.Round(phase1.ObjectiveValue, 6) != 0)
                {
                    phase1.Status = SolveStatus.Infeasible;
                    return phase1;
                }
                // Rebuild a feasible basis for Phase II (remove artificials, use original c)
                (A, b, c, _, varNames, basicIdx, nonBasicIdx, _) = Canonicalizer.ToStandardForm(rawModel, assumeFeasible: true);
            }

            return RunPrimalTableauPhaseII(A, b, c, varNames, basicIdx, nonBasicIdx, sense);
        }

        public SimplexResult SolveRevisedSimplex(LPModel rawModel)
        {
            var (A, b, c, sense, varNames, basicIdx, nonBasicIdx, hasArtificial) =
                Canonicalizer.ToStandardForm(rawModel);

            if (hasArtificial)
            {
                // Phase I (revised) to get feasible basis
                var phase1 = RunRevisedPhaseI(A, b, c, varNames, ref basicIdx, ref nonBasicIdx);
                if (phase1.Status != SolveStatus.Optimal || Math.Round(phase1.ObjectiveValue, 6) != 0)
                {
                    phase1.Status = SolveStatus.Infeasible;
                    return phase1;
                }
                (A, b, c, _, varNames, basicIdx, nonBasicIdx, _) = Canonicalizer.ToStandardForm(rawModel, assumeFeasible: true);
            }

            return RunRevisedPhaseII(A, b, c, varNames, ref basicIdx, ref nonBasicIdx, sense);
        }

        //  PRIMAL TABLEAU (PHASE I & II)

        private SimplexResult RunPrimalTableauPhaseI(double[,] A, double[] b, double[] cOrig,
            List<string> varNames, List<int> basicIdx, List<int> nonBasicIdx)
        {
            // Build Phase I objective: minimize sum of artificials -> as max, use -sum(artificials)
            var model = TableauBuilder.BuildPhaseITableau(A, b, varNames, basicIdx, nonBasicIdx);
            return PrimalTableauLoop(model);
        }

        private SimplexResult RunPrimalTableauPhaseII(double[,] A, double[] b, double[] c,
            List<string> varNames, List<int> basicIdx, List<int> nonBasicIdx, int sense)
        {
            var model = TableauBuilder.BuildPhaseIITableau(A, b, c, varNames, basicIdx, nonBasicIdx, sense);
            return PrimalTableauLoop(model);
        }

        private SimplexResult PrimalTableauLoop(TableauModel tm)
        {
            var res = new SimplexResult();
            int iter = 0;

            while (iter++ < ITER_LIMIT)
            {
                tm.AppendIterationTo(res.Iterations);

                int enter = tm.SelectEnteringVariable(); // most negative reduced cost (for max)
                if (enter == -1)
                {
                    res.Status = SolveStatus.Optimal;
                    res.ObjectiveValue = tm.GetObjectiveValue();
                    res.PrimalSolution = tm.GetPrimalSolution();
                    res.DualPrices = tm.GetDualPrices();
                    return res;
                }

                int leave = tm.SelectLeavingVariable(enter);
                if (leave == -1)
                {
                    res.Status = SolveStatus.Unbounded;
                    return res;
                }

                tm.Pivot(leave, enter);
            }

            res.Status = SolveStatus.IterationLimit;
            return res;
        }

        //  REVISED SIMPLEX (PHASE I & II)

        private SimplexResult RunRevisedPhaseI(double[,] A, double[] b, double[] c,
            List<string> varNames, ref List<int> basicIdx, ref List<int> nonBasicIdx)
        {
            var rs = new RevisedModel(A, b, c, varNames, basicIdx, nonBasicIdx, phase: 1);
            return RevisedLoop(rs, ref basicIdx, ref nonBasicIdx, phase: 1);
        }

        private SimplexResult RunRevisedPhaseII(double[,] A, double[] b, double[] c,
            List<string> varNames, ref List<int> basicIdx, ref List<int> nonBasicIdx, int sense)
        {
            var rs = new RevisedModel(A, b, c, varNames, basicIdx, nonBasicIdx, phase: 2, sense: sense);
            return RevisedLoop(rs, ref basicIdx, ref nonBasicIdx, phase: 2);
        }

        private SimplexResult RevisedLoop(RevisedModel rs, ref List<int> basicIdx, ref List<int> nonBasicIdx, int phase)
        {
            var res = new SimplexResult();
            int iter = 0;

            while (iter++ < ITER_LIMIT)
            {
                rs.AppendIterationTo(res.Iterations); // prints B^-1, y, reduced costs, etc. (compact)

                // Pricing: reduced costs for non-basics
                var (enterIdx, enterCol) = rs.SelectEntering();
                if (enterIdx == -1)
                {
                    // Optimal for this phase
                    double z = rs.CurrentObjective();
                    if (phase == 1 && Math.Round(z, 6) != 0) { res.Status = SolveStatus.Infeasible; return res; }

                    res.Status = SolveStatus.Optimal;
                    res.ObjectiveValue = z;
                    res.PrimalSolution = rs.CurrentPrimal();
                    res.DualPrices = rs.CurrentDual();
                    basicIdx = rs.BasicIdx; nonBasicIdx = rs.NonBasicIdx; // update back
                    return res;
                }

                // Direction: d = B^-1 * a_enter
                double[] d = rs.Direction(enterCol);

                int leavePos = rs.SelectLeaving(d);
                if (leavePos == -1)
                {
                    res.Status = SolveStatus.Unbounded;
                    return res;
                }

                // Update basis: pivot (update B^-1 via product form or refactor)
                rs.Pivot(leavePos, enterIdx, d);
            }

            res.Status = SolveStatus.IterationLimit;
            return res;
        }
    }

    // Canonicalization utilities

    internal static class Canonicalizer
    {
        // Returns (A, b, c, sense(+1 for max), varNames, basicIdx, nonBasicIdx, hasArtificial)
        public static (double[,] A, double[] b, double[] c, int sense, List<string> varNames,
            List<int> basicIdx, List<int> nonBasicIdx, bool hasArtificial)
            ToStandardForm(LPModel raw, bool assumeFeasible = false)
        {
            // 1) Convert min->max (multiply objective by -1 if min)
            int sense = raw.IsMax ? +1 : -1;
            double[] c = raw.ObjCoefficients.Select(x => sense * x).ToArray();

            // 2) Build A,b, add slack/surplus, and if needed artificials ( or =) for Phase I
            //    Keep track of variable names: x1..xn, s1.., a1.. etc.
            //    Fill basicIdx with slack/artificial columns that start as identity.

            int m = raw.Constraints.Count;
            int n = c.Length;

            var varNames = new List<string>();
            for (int j = 0; j < n; j++) varNames.Add($"x{j + 1}");

            var slackCols = new List<int>(); //slack columns
            var artificialCols = new List<int>(); //artifical columns
            var constraintTypes = raw.Constraints.Select(c => c.operators).ToList();
            int totalCols = n; //x1...xn

            // A and b
            double[,] A = new double[m, n + m * 2]; // (m*2) => enough space for slacks and artificials
            double[] b = new double[m];

            for (int i = 0; i < m; i++)
            {
                b[i] = raw.Constraints[i].RHS;
                for (int j = 0; j < n; j++)
                    A[i, j] = raw.Constraints[i].Coefficients[j];

                if (raw.Constraints[i].operators == Constraint.Operator.LessThanOrEqual)
                {
                    // slack
                    A[i, n + i] = 1.0;
                    varNames.Add($"s{i + 1}");
                    slackCols.Add(totalCols++);
                }
                else if (raw.Constraints[i].operators == Constraint.Operator.MoreThanOrEqual)
                {
                    // slack
                    A[i, n + i] = -1.0;
                    varNames.Add($"s{i + 1}");
                    slackCols.Add(totalCols++);

                    // artificial
                    A[i, n + i] = 1.0;
                    varNames.Add($"a{i + 1}");
                    artificialCols.Add(totalCols++);
                }

                else if (raw.Constraints[i].operators == Constraint.Operator.Equal)
                {
                    // artificial
                    A[i, n + i] = 1.0;
                    varNames.Add($"a{i + 1}");
                    artificialCols.Add(totalCols++);
                }
                            
            }

            // Extend c with zeros for slacks
            c = c.Concat(Enumerable.Repeat(0.0, m)).ToArray();

            // Initial basis = slacks
            var basicIdx = Enumerable.Range(n, m).ToList();
            var nonBasicIdx = Enumerable.Range(0, n).ToList();

            //bool hasArtificial = false; // set to true if you add artificials for  or = constraints

            return (A, b, c, sense, varNames, basicIdx, nonBasicIdx, 
                hasArtificial : artificialCols.Count >0);
        }
    }

    // Tableau structures (Primal)

    internal class TableauModel
    {
        // Tableau matrix includes objective row at index 0:
        // [ z | rdcosts ... | RHS ]
        // Rows 1..m are constraints
        private double[,] T;
        private int m, n; // m constraints, n variables (excl. RHS)
        private List<string> varNames;
        private List<int> basicIdx;   // column indices (0..n-1) of basic vars
        private List<int> nonBasicIdx;

        public TableauModel(double[,] tableau, int m, int n, List<string> varNames,
            List<int> basicIdx, List<int> nonBasicIdx)
        {
            T = tableau; this.m = m; this.n = n; this.varNames = varNames;
            this.basicIdx = new List<int>(basicIdx);
            this.nonBasicIdx = new List<int>(nonBasicIdx);
        }

        public static TableauModel BuildPhaseITableau(double[,] A, double[] b, List<string> varNames,
            List<int> basicIdx, List<int> nonBasicIdx)
        {
            int m = b.Length;
            int n = A.GetLength(1);

            int nArt = m; // one artificial per constraint
            int totalVars = n + nArt;

            double[,] T = new double[m + 1, totalVars + 1]; //+1 for RHS

            //copy constraints into tableau
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                    T[i + 1, j] = A[i, j];
                T[i + 1, n + i] = 1.0; // artificial var
                T[i + 1, totalVars] = b[i]; //rhs
            }

            // Phase I objective: -sum(artificials) => set -1 on artificial columns
            for (int i = 0; i < nArt; i++)
                T[0, n + i] = -1.0;

            T[0, totalVars] = 0.0; //rhs of objective row

            // variable names
            for (int i = 0; i < nArt; i++)
                varNames.Add($"a{i + 1}");

            // Update basis to be artificial vars
            basicIdx.Clear();
            for (int i = 0; i < m; i++)
                basicIdx.Add(n + i);

            nonBasicIdx.Clear();
            for (int j = 0; j < n; j++)
                nonBasicIdx.Add(j);

            var tm = new TableauModel(T, m, totalVars, varNames, basicIdx, nonBasicIdx);
            tm.MakeObjectiveBasicConsistent();
            return tm;
        }

        public static TableauModel BuildPhaseIITableau(double[,] A, double[] b, double[] c,
            List<string> varNames, List<int> basicIdx, List<int> nonBasicIdx, int sense)
        {
            // Build typical simplex tableau for max with given basis
            int m = b.Length;
            int n = A.GetLength(1);

            double[,] T = new double[m + 1, n + 1]; // last column is RHS
            // Objective row: z - c^T x = 0  => store as [ -c | 0 ] for max
            for (int j = 0; j < n; j++) T[0, j] = -c[j];
            T[0, n] = 0.0;

            // Constraints
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++) T[i + 1, j] = A[i, j];
                T[i + 1, n] = b[i];
            }

            var tm = new TableauModel(T, m, n, varNames, basicIdx, nonBasicIdx);
            tm.MakeObjectiveBasicConsistent();
            return tm;
        }

        internal void MakeObjectiveBasicConsistent()
        {
            // Row ops to ensure reduced costs reflect current basis (z-row + sum c_B * row_B = current)
            foreach (var bcol in basicIdx)
            {
                int row = RowOfBasic(bcol);
                double coeff = T[0, bcol];
                if (Math.Abs(coeff) > 1e-12)
                {
                    double factor = coeff;
                    for (int j = 0; j <= n; j++)
                        T[0, j] -= factor * T[row, j];
                }
            }
        }

        private int RowOfBasic(int col)
        {
            for (int i = 1; i <= m; i++)
                if (Math.Abs(T[i, col] - 1) < 1e-9 && Enumerable.Range(0, n).All(j => j == col || Math.Abs(T[i, j]) < 1e-9))
                    return i;
            // Fallback: scan for pivot 1 in column
            for (int i = 1; i <= m; i++)
                if (Math.Abs(T[i, col]) > 1e-9) return i;
            return 1; // safe default
        }

        public int SelectEnteringVariable()
        {
            // Most negative reduced cost in z-row (ignoring RHS at col n)
            int enter = -1; double mostNeg = 0;
            for (int j = 0; j < n; j++)
            {
                if (T[0, j] < mostNeg - 1e-12)
                {
                    mostNeg = T[0, j];
                    enter = j;
                }
            }
            return enter;
        }

        public int SelectLeavingVariable(int enterCol)
        {
            double best = double.PositiveInfinity;
            int leaveRow = -1;
            for (int i = 1; i <= m; i++)
            {
                double aij = T[i, enterCol];
                if (aij > 1e-12)
                {
                    double ratio = T[i, n] / aij;
                    if (ratio < best - 1e-12)
                    {
                        best = ratio;
                        leaveRow = i;
                    }
                }
            }
            return leaveRow; // -1 => unbounded
        }

        public void Pivot(int leaveRow, int enterCol)
        {
            double piv = T[leaveRow, enterCol];
            // Normalize pivot row
            for (int j = 0; j <= n; j++) T[leaveRow, j] /= piv;

            // Eliminate column
            for (int i = 0; i <= m; i++)
            {
                if (i == leaveRow) continue;
                double factor = T[i, enterCol];
                if (Math.Abs(factor) > 1e-12)
                    for (int j = 0; j <= n; j++)
                        T[i, j] -= factor * T[leaveRow, j];
            }
            // Update basis mapping
            int oldBasicCol = basicIdx[leaveRow - 1];
            basicIdx[leaveRow - 1] = enterCol;
            nonBasicIdx.Remove(enterCol);
            nonBasicIdx.Add(oldBasicCol);

            //optional: writing enterCol and leaveRow in the Console
            //Console.WriteLine($"Pivoting: entering = {enterCol}, leaving = {leaveRow}");
        }

        public double GetObjectiveValue() => Math.Round(T[0, n], 3);

        public double[] GetPrimalSolution()
        {
            double[] x = new double[n];
            for (int k = 0; k < basicIdx.Count; k++)
            {
                int col = basicIdx[k];
                int row = RowOfBasic(col);
                x[col] = Math.Max(0, T[row, n]);
            }
            return x.Select(v => Math.Round(v, 3)).ToArray();
        }

        public double[] GetDualPrices()
        {
            // Dual prices = coefficients on RHS in z-row after making basis consistent (with sign)
            double[] y = new double[m];
            // For tableau method, y can be recovered by solving B^T y = c_B, or read from consistent z-row compared against rows.
            for (int i = 0; i < m; i++)
            {
                int bCol = basicIdx[i];
                y[i] = -T[0, bCol]; // negate since c_B already subtracted from z-row
            }
            return y.Select(v => Math.Round(v, 3)).ToArray();
        }

        public void AppendIterationTo(List<string> log)
        {
            var lines = new List<string>();
            lines.Add("Tableau (3 dp):");
            for (int i = 0; i <= m; i++)
            {
                var row = new List<string>();
                for (int j = 0; j <= n; j++)
                    row.Add(Math.Round(T[i, j], 3).ToString("0.000"));
                lines.Add(string.Join(" | ", row));
            }
            log.Add(string.Join(Environment.NewLine, lines));
        }
    }

    internal static class TableauBuilder
    {
        public static TableauModel BuildPhaseITableau(double[,] A, double[] b, List<string> varNames,
            List<int> basicIdx, List<int> nonBasicIdx)
        {
            //try
            //{
                int m = b.Length;
                int n = A.GetLength(1);
                int nArt = m; //artificial per constraint

                int totalVars = n + nArt;

                // Tableau size: (m + 1) rows (objective + constraints), (totalVars + 1) columns (+1 for RHS)
                double[,] T = new double[m + 1, totalVars + 1];

                // Fill constraints rows (1..m)
                for (int i = 0; i < m; i++)
                {
                    // Original variables
                    for (int j = 0; j < n; j++)
                        T[i + 1, j] = A[i, j];

                    // Artificial var per row
                    T[i + 1, n + i] = 1.0;

                    // RHS
                    T[i + 1, totalVars] = b[i];
                }

                //Objective row for Phase I: maximize -sum(artificials)
                for (int i = 0; i < nArt; i++)
                    T[0, n + i] = -1.0;

                //Update variable names
                for (int i = 0; i < nArt; i++)
                    varNames.Add($"a{i + 1}");

                //Set new basis
                basicIdx.Clear();
                for (int i = 0; i < m; i++)
                    basicIdx.Add(n + i); // artificial vars

                //Set non-basic vars
                nonBasicIdx.Clear();
                for (int j = 0; j < n; j++)
                    nonBasicIdx.Add(j); // original vars

                // Create tableau model and adjust objective row
                var tm = new TableauModel(T, m, totalVars, varNames, basicIdx, nonBasicIdx);
            tm.MakeObjectiveBasicConsistent(); // adjust objective row based on initial basis
            return tm;
        }

        public static TableauModel BuildPhaseIITableau(double[,] A, double[] b, double[] c,
            List<string> varNames, List<int> basicIdx, List<int> nonBasicIdx, int sense)
        {
            return TableauModel.BuildPhaseIITableau(A, b, c, varNames, basicIdx, nonBasicIdx, sense);
        }
    }

    // Revised simplex structures

    internal class RevisedModel
    {
        private double[,] A;   // m x n
        private double[] b;    // m
        private double[] c;    // n
        private int m, n;
        private int phase;
        private int sense;
        public List<string> VarNames { get; }
        public List<int> BasicIdx { get; private set; }
        public List<int> NonBasicIdx { get; private set; }

        private double[,] BInv; // current inverse of basis matrix

        public RevisedModel(double[,] A, double[] b, double[] c, List<string> names,
            List<int> basicIdx, List<int> nonBasicIdx, int phase, int sense = +1)
        {
            this.A = A; this.b = b; this.c = c; this.phase = phase; this.sense = sense;
            m = b.Length; n = A.GetLength(1);
            VarNames = names;
            BasicIdx = new List<int>(basicIdx);
            NonBasicIdx = new List<int>(nonBasicIdx);
            RecomputeBInv();
        }

        private void RecomputeBInv()
        {
            var B = MathUtils.Submatrix(A, rows: Enumerable.Range(0, m).ToArray(), cols: BasicIdx.ToArray());
            BInv = MathUtils.Invert(B);
        }

        public (int enterIdx, double[] a_enter) SelectEntering()
        {
            // y^T = c_B^T B^-1
            double[] cB = BasicIdx.Select(j => c[j]).ToArray();
            double[,] yT = MathUtils.Multiply(MathUtils.Transpose(MathUtils.RowToVector1D(cB)), BInv); // (1xm)
            // reduced costs for non-basics: c_j - y^T a_j
            int enter = -1; double mostNeg = 0; double[] aEnter = null;
            foreach (var j in NonBasicIdx)
            {
                double[] a_j = MathUtils.Column(A, j);
                double yTa = MathUtils.Dot(yT, a_j);
                double rc = c[j] - yTa; // for max
                if (rc < mostNeg - 1e-12)
                {
                    mostNeg = rc;
                    enter = j;
                    aEnter = a_j;
                }
            }
            return (enter, aEnter);
        }

        public double[] Direction(double[] aEnter)
        {
            // d = B^-1 * a_enter
            return MathUtils.Multiply(BInv, aEnter);
        }

        public int SelectLeaving(double[] d)
        {
            double best = double.PositiveInfinity; int leavePos = -1;
            // x_B = B^-1 * b
            double[] xB = MathUtils.Multiply(BInv, b);
            for (int i = 0; i < m; i++)
            {
                if (d[i] > 1e-12)
                {
                    double ratio = xB[i] / d[i];
                    if (ratio < best - 1e-12) { best = ratio; leavePos = i; }
                }
            }
            return leavePos;
        }

        public void Pivot(int leavePos, int enterIdx, double[] d)
        {
            // Update basis indices
            int leaveIdx = BasicIdx[leavePos];
            BasicIdx[leavePos] = enterIdx;
            NonBasicIdx.Remove(enterIdx);
            NonBasicIdx.Add(leaveIdx);

            // Update BInv (simple approach: recompute; faster: product form update)
            RecomputeBInv();
        }

        public double CurrentObjective()
        {
            // z = c_B^T x_B  where x_B = B^-1 b  (max form)
            var cB = BasicIdx.Select(j => c[j]).ToArray();
            var xB = MathUtils.Multiply(BInv, b);
            return Math.Round(MathUtils.Dot(MathUtils.RowToVector1D(cB), xB), 3);
        }

        public double[] CurrentPrimal()
        {
            double[] x = new double[n];
            double[] xB = MathUtils.Multiply(BInv, b);
            for (int i = 0; i < m; i++) x[BasicIdx[i]] = xB[i];
            return x.Select(v => Math.Round(Math.Max(0, v), 3)).ToArray();
        }

        public double[] CurrentDual()
        {
            // y^T = c_B^T B^-1
            double[] cB = BasicIdx.Select(j => c[j]).ToArray();
            var yT = MathUtils.Multiply(MathUtils.Transpose(MathUtils.RowToVector1D(cB)), BInv); // 1 x m
            double[] y = MathUtils.RowToVector2D(yT, 0);
            return y.Select(v => Math.Round(v, 3)).ToArray();
        }

        public void AppendIterationTo(List<string> log)
        {
            var lines = new List<string>();
            lines.Add("Revised iteration (3 dp):");
            lines.Add($"Basis: {string.Join(", ", BasicIdx.Select(j => VarNames[j]))}");
            lines.Add($"NonBasis: {string.Join(", ", NonBasicIdx.Select(j => VarNames[j]))}");
            lines.Add("B^-1:");
            lines.Add(MathUtils.PrettyMatrix(BInv, 3));
            lines.Add($"x_B: {string.Join(", ", CurrentPrimal().Where((v, i) => BasicIdx.Contains(i)).Select(v => v.ToString("0.000")))}");
            lines.Add($"y: {string.Join(", ", CurrentDual().Select(v => v.ToString("0.000")))}");
            log.Add(string.Join(Environment.NewLine, lines));
        }
    }
}