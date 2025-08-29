

// Chad Work:
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LPR381_Project.Models;
using LPR381_Project.Algorithms;

namespace LPR381_Project.Analysis
{
    public class SensitivityAnalysis
    {
        public LPModel Model { get; private set; }
        public SimplexResult Baseline { get; private set; }
        private readonly SimplexSolver _solver;
        private readonly bool _useRevised;

        public SensitivityAnalysis(LPModel solvedModel, SimplexResult baseline, SimplexSolver solver, bool useRevised = true)
        {
            Model = CloneModel(solvedModel);
            Baseline = baseline;
            _solver = solver;
            _useRevised = useRevised;
        }

        private static LPModel CloneModel(LPModel src)
        {
            var m = new LPModel(src.IsMaximization);
            for (int j = 0; j < src.NumVariables; j++)
                m.AddVariable(src.Variables[j], src.ObjectiveCoefficients[j]);

            for (int i = 0; i < src.NumConstraints; i++)
            {
                var c = src.Constraints[i];
                m.AddConstraint(new Constraint((double[])c.Coeffs.Clone(), c.Type, c.RHS));
            }
            return m;
        }

        public (double increase, double decrease) GetNonBasicVariableRange(string variableName)
        {
            int j = Model.Variables.FindIndex(v => v.Name == variableName);
            double[] a_j = new double[Model.NumConstraints];
            if (j == -1)
                throw new ArgumentException($"Variable '{variableName}' not found in the model.");
            for (int i = 0; i < Model.NumConstraints; i++) a_j[i] = Model.Constraints[i].Coeffs[j];

            double yTa = 0;
            for (int i = 0; i < Baseline.DualPrices.Length; i++) yTa += Baseline.DualPrices[i] * a_j[i];

            double rc = Model.ObjectiveCoefficients[j] - yTa;

            if (Math.Abs(rc) < 1e-6) return (0, 0);

            return (rc < 0) ? (-rc, double.PositiveInfinity) : (0, 0);
        }

        public void ApplyChangeToVariable(string variableName, double newCoefficient)
        {
            int j = Model.Variables.FindIndex(v => v.Name == variableName);
            Model.ObjectiveCoefficients[j] = newCoefficient;
            ReSolve();
        }

        public (double down, double up) GetBasicVariableRange(string variableName, double step = 0.1, int maxSteps = 200)
        {
            int j = Model.Variables.FindIndex(v => v.Name == variableName);
            double baseC = Model.ObjectiveCoefficients[j];
            HashSet<int> baseSupport = Support(Baseline.PrimalSolution);

            // Scan up
            double up = 0;
            for (int k = 1; k <= maxSteps; k++)
            {
                Model.ObjectiveCoefficients[j] = baseC + k * step;
                var r = ReSolvePreview();
                if (r.Status != SolveStatus.Optimal || !baseSupport.SetEquals(Support(r.PrimalSolution))) break;
                up = k * step;
            }

            // Scan down
            double down = 0;
            for (int k = 1; k <= maxSteps; k++)
            {
                Model.ObjectiveCoefficients[j] = baseC - k * step;
                var r = ReSolvePreview();
                if (r.Status != SolveStatus.Optimal || !baseSupport.SetEquals(Support(r.PrimalSolution))) break;
                down = k * step;
            }

            Model.ObjectiveCoefficients[j] = baseC;
            ReSolve();
            return (down, up);
        }

        public (double down, double up) GetConstraintRHSRange(string constraintName, double step = 0.1, int maxSteps = 200)
        {
            int i = int.Parse(constraintName.Substring(1)) - 1;
            double baseRhs = Model.Constraints[i].RHS;
            HashSet<int> baseSupport = Support(Baseline.PrimalSolution);

            // Scan up
            double up = 0;
            for (int k = 1; k <= maxSteps; k++)
            {
                Model.Constraints[i].RHS = baseRhs + k * step;
                var r = ReSolvePreview();
                if (r.Status != SolveStatus.Optimal || !baseSupport.SetEquals(Support(r.PrimalSolution))) break;
                up = k * step;
            }

            // Scan down
            double down = 0;
            for (int k = 1; k <= maxSteps; k++)
            {
                Model.Constraints[i].RHS = baseRhs - k * step;
                var r = ReSolvePreview();
                if (r.Status != SolveStatus.Optimal || !baseSupport.SetEquals(Support(r.PrimalSolution))) break;
                down = k * step;
            }

            Model.Constraints[i].RHS = baseRhs;
            ReSolve();
            return (down, up);
        }

        public void ApplyChangeToConstraintRHS(string constraintName, double newValue)
        {
            int i = int.Parse(constraintName.Substring(1)) - 1;
            Model.Constraints[i].RHS = newValue;
            ReSolve();
        }

        public void AddConstraint(Constraint newConstraint)
        {
            Model.AddConstraint(newConstraint);
            ReSolve();
        }

        public void AddVariable(Variable newVar, double objectiveCoeff, double[] column)
        {
            Model.AddVariable(newVar, objectiveCoeff);
            for (int i = 0; i < Model.NumConstraints; i++)
            {
                double[] oldCoeffs = Model.Constraints[i].Coeffs;
                double[] newCoeffs = new double[oldCoeffs.Length + 1];
                Array.Copy(oldCoeffs, newCoeffs, oldCoeffs.Length);
                newCoeffs[oldCoeffs.Length] = column[i];
                Model.Constraints[i].Coeffs = newCoeffs;
            }
            ReSolve();
        }

        public double[] GetShadowPrices()
        {
            return Baseline.DualPrices;
        }

        public LPModel ConstructDual()
        {
            return new DualitySolver(_solver).ConstructDual(Model);
        }

        public SimplexResult SolveDual()
        {
            return new DualitySolver(_solver).SolveDual(Model, _useRevised);
        }

        public void VerifyDuality()
        {
            var dualRes = SolveDual();
            new DualitySolver(_solver).VerifyDuality(Baseline, dualRes);
        }

        private void ReSolve()
        {
            Baseline = _useRevised ? _solver.SolveRevised(Model) : _solver.SolvePrimal(Model);
        }

        private SimplexResult ReSolvePreview()
        {
            var snapshot = CloneModel(Model);
            return _useRevised ? _solver.SolveRevised(snapshot) : _solver.SolvePrimal(snapshot);
        }

        private static HashSet<int> Support(double[] x)
        {
            return new HashSet<int>(x.Select((v, i) => (v, i)).Where(t => t.v > 1e-6).Select(t => t.i));
        }
    }
}

/*
namespace LPR381_Project.Analysis
{
    /// <summary>
    /// Lightweight sensitivity analysis that works with the objects in this project.
    /// It uses: current duals (shadow prices), reduced-cost tests, and (when needed)
    /// small re-solves to bracket allowable ranges.
    /// 
    /// Notes:
    ///  - For nonbasic variables, allowable increase = -reducedCost (maximization).
    ///    Decrease is unbounded (solution remains optimal as rc gets more negative).
    ///  - For basic variables and RHS ranges we bracket numerically by re-solving
    ///    and detecting when the support (positive variables) changes.
    /// </summary>
    public class SensitivityAnalysis
    {
        private readonly SimplexSolver _solver;
        private readonly bool _useRevised;
        public LPModel Model { get; private set; }
        public SimplexResult Baseline { get; private set; }

        public SensitivityAnalysis(LPModel solvedModel,
                                   SimplexResult baseline,
                                   SimplexSolver solver,
                                   bool useRevised = true)
        {
            Model = CloneModel(solvedModel);
            Baseline = baseline;
            _solver = solver;
            _useRevised = useRevised;
            if (Baseline?.DualPrices == null || Baseline.DualPrices.Length == 0)
                throw new InvalidOperationException("Baseline result must include dual prices.");
        }

        // ---------------- Variable (objective coefficient) analysis ----------------

        /// <summary>
        /// Returns (allowableIncrease, allowableDecrease) for a *nonbasic* variable, using reduced-cost.
        /// For max problems: rc_j = c_j - y^T a_j <= 0 at optimum.
        /// AllowableIncrease = -rc_j (until rc becomes 0); AllowableDecrease = +infinity.
        /// If rc ≈ 0 (alternate optimum), we conservatively return (0,0).
        /// </summary>
        public (double increase, double decrease) GetNonBasicVariableRange(string variableName)
        {
            int j = IndexOfVar(variableName);
            var a_j = ColumnOfVar(j);
            double cj = Model.ObjectiveCoefficients[j];
            double yTa = Dot(Baseline.DualPrices, a_j);
            double rc = cj - yTa; // for maximization

            if (Math.Abs(rc) < 1e-8) return (0.0, 0.0);           // alternate optimum
            if (rc < 0) return (-rc, double.PositiveInfinity);     // typical nonbasic
            // If rc>0 the provided baseline would not be optimal; be safe:
            return (0.0, 0.0);
        }

        /// <summary>Apply an objective coefficient change and re-solve (updates Baseline).</summary>
        public void ApplyChangeToVariable(string variableName, double newCoefficient)
        {
            int j = IndexOfVar(variableName);
            Model.ObjectiveCoefficients[j] = newCoefficient;
            ReSolve();
        }

        /// <summary>
        /// For a *basic* variable we bracket a range by nudging c_j up and down until
        /// the support (set of positive vars) changes. Returns (maxDown, maxUp).
        /// </summary>
        public (double down, double up) GetBasicVariableRange(string variableName,
                                                              double step = 0.1,
                                                              int maxSteps = 200)
        {
            int j = IndexOfVar(variableName);
            var baseC = Model.ObjectiveCoefficients[j];

            var baseSupport = Support(Baseline.PrimalSolution);

            // Scan up
            double up = 0.0;
            for (int k = 1; k <= maxSteps; k++)
            {
                Model.ObjectiveCoefficients[j] = baseC + k * step;
                var r = ReSolvePreview();
                if (ChangedSupport(baseSupport, r.PrimalSolution)) break;
                up = k * step;
            }

            // Scan down
            double down = 0.0;
            for (int k = 1; k <= maxSteps; k++)
            {
                Model.ObjectiveCoefficients[j] = baseC - k * step;
                var r = ReSolvePreview();
                if (ChangedSupport(baseSupport, r.PrimalSolution)) break;
                down = k * step;
            }

            // restore and re-solve to baseline
            Model.ObjectiveCoefficients[j] = baseC;
            ReSolve();
            return (down, up);
        }

        // ---------------- Constraint RHS analysis ----------------

        /// <summary>
        /// Brackets RHS allowable +/- range for the i-th constraint (by name "c{i+1}" or by index via name).
        /// Uses repeated re-solves until basis/support changes.
        /// </summary>
        public (double down, double up) GetConstraintRHSRange(string constraintName,
                                                              double step = 0.1,
                                                              int maxSteps = 200)
        {
            int i = IndexOfConstraint(constraintName);
            double baseRhs = Model.Constraints[i].RHS;
            var baseSupport = Support(Baseline.PrimalSolution);

            // Scan up
            double up = 0.0;
            for (int k = 1; k <= maxSteps; k++)
            {
                Model.Constraints[i].RHS = baseRhs + k * step;
                var r = ReSolvePreview();
                if (ChangedSupport(baseSupport, r.PrimalSolution) || r.Status != SolveStatus.Optimal) break;
                up = k * step;
            }

            // Scan down
            double down = 0.0;
            for (int k = 1; k <= maxSteps; k++)
            {
                Model.Constraints[i].RHS = baseRhs - k * step;
                var r = ReSolvePreview();
                if (ChangedSupport(baseSupport, r.PrimalSolution) || r.Status != SolveStatus.Optimal) break;
                down = k * step;
            }

            // restore and re-solve to baseline
            Model.Constraints[i].RHS = baseRhs;
            ReSolve();
            return (down, up);
        }

        /// <summary>Apply RHS change and re-solve (updates Baseline).</summary>
        public void ApplyChangeToConstraintRHS(string constraintName, double newValue)
        {
            int i = IndexOfConstraint(constraintName);
            Model.Constraints[i].RHS = newValue;
            ReSolve();
        }

        public void AddConstraint(Constraint newConstraint)
        {
            // Validate width
            if (newConstraint.Coeffs == null || newConstraint.Coeffs.Length != Model.NumVariables)
                throw new ArgumentException("New constraint must have a coefficient for each variable.");
            Model.Constraints.Add(newConstraint);
            ReSolve();
        }

        // ---------------- Activity (variable/column) analysis ----------------

        /// <summary>
        /// Add a new decision variable column with its objective coefficient and column in A.
        /// </summary>
        public void AddVariable(string name, double objectiveCoeff, double[] column,
                                string signRestriction = "+")
        {
            if (column == null || column.Length != Model.NumConstraints)
                throw new ArgumentException("Column length must equal number of constraints.");

            // 1) Add variable metadata
            var v = new Variable { Name = name, SignRestriction = signRestriction };
            Model.Variables.Add(v);
            Model.ObjectiveCoefficients.Add(objectiveCoeff);

            // 2) Append column to each constraint
            for (int i = 0; i < Model.NumConstraints; i++)
            {
                var row = Model.Constraints[i];
                var newRow = new double[row.Coeffs.Length + 1];
                Array.Copy(row.Coeffs, newRow, row.Coeffs.Length);
                newRow[^1] = column[i];
                row.Coeffs = newRow;
            }

            ReSolve();
        }

        // ---------------- Shadow prices & duality ----------------

        /// <summary>Returns the current shadow prices (dual variables) from the baseline.</summary>
        public double[] GetShadowPrices() => (double[])Baseline.DualPrices.Clone();

        /// <summary>Builds the dual model (for a standard ≤-type primal max). Minimal construction.</summary>
        public LPModel ConstructDual()
        {
            // Primal (max):  max c^T x  s.t. A x ≤ b, x ≥ 0
            // Dual (min):    min b^T y  s.t. A^T y ≥ c, y ≥ 0
            int m = Model.NumConstraints;
            int n = Model.NumVariables;

            var dual = new LPModel(isMaximization: false); // min
            // Dual vars y1..ym
            for (int i = 0; i < m; i++)
                dual.AddVariable(new Variable($"y{i + 1}", "+"), objectiveCoefficient: Model.Constraints[i].RHS);

            // Dual constraints (one per primal var)
            for (int j = 0; j < n; j++)
            {
                var col = ColumnOfVar(j);
                var coeffs = new double[m];
                for (int i = 0; i < m; i++) coeffs[i] = col[i];
                var con = new Constraint(coeffs, ConstraintType.MoreThanOrEqual, Model.ObjectiveCoefficients[j])
                { Relation = ">=" };
                dual.AddConstraint(con);
            }

            return dual;
        }

        /// <summary>Solve the dual and return result.</summary>
        public SimplexResult SolveDual()
        {
            var dual = ConstructDual();
            return _useRevised ? _solver.SolveRevised(dual) : _solver.SolvePrimal(dual);
        }

        /// <summary>Quick weak–duality check: |z* - w*| small.</summary>
        public (double primal, double dual, double gap) VerifyDuality()
        {
            var dualRes = SolveDual();
            return (Baseline.ObjectiveValue, dualRes.ObjectiveValue,
                    Math.Abs(Baseline.ObjectiveValue - dualRes.ObjectiveValue));
        }

        // ---------------- internals ----------------

        private void ReSolve()
        {
            Baseline = _useRevised ? _solver.SolveRevised(Model) : _solver.SolvePrimal(Model);
        }

        private SimplexResult ReSolvePreview()
        {
            // Work on a deep copy so we don’t mutate the working model
            var snapshot = CloneModel(Model);
            return _useRevised ? _solver.SolveRevised(snapshot) : _solver.SolvePrimal(snapshot);
        }

        private static LPModel CloneModel(LPModel src)
        {
            var m = new LPModel(src.IsMaximization);
            for (int j = 0; j < src.NumVariables; j++)
                m.AddVariable(new Variable(src.Variables[j].Name, src.Variables[j].SignRestriction),
                              src.ObjectiveCoefficients[j]);

            foreach (var c in src.Constraints)
            {
                var copy = new Constraint((double[])c.Coeffs.Clone(), c.Type, c.RHS) { Relation = c.Relation };
                m.AddConstraint(copy);
            }
            return m;
        }

        private int IndexOfVar(string name)
        {
            int j = Model.Variables.FindIndex(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (j < 0) throw new ArgumentException($"Unknown variable '{name}'.");
            return j;
        }

        private int IndexOfConstraint(string name)
        {
            // accept "c1".."cm" or a raw index string
            if (name.StartsWith("c", StringComparison.OrdinalIgnoreCase) && int.TryParse(name[1..], out int k))
            {
                if (k < 1 || k > Model.NumConstraints) throw new ArgumentOutOfRangeException(nameof(name));
                return k - 1;
            }
            if (int.TryParse(name, out int idx))
            {
                if (idx < 0 || idx >= Model.NumConstraints) throw new ArgumentOutOfRangeException(nameof(name));
                return idx;
            }
            // otherwise try 0-based match
            return IndexOfVar(name); // fallback (rare)
        }

        private double[] ColumnOfVar(int j)
        {
            var col = new double[Model.NumConstraints];
            for (int i = 0; i < Model.NumConstraints; i++) col[i] = Model.Constraints[i].Coeffs[j];
            return col;
        }

        private static double Dot(double[] a, double[] b)
        {
            double s = 0; for (int i = 0; i < a.Length; i++) s += a[i] * b[i]; return s;
        }

        private static HashSet<int> Support(double[] x)
            => x.Select((v, idx) => (v, idx)).Where(t => t.v > 1e-8).Select(t => t.idx).ToHashSet();

        private static bool ChangedSupport(HashSet<int> baseSupport, double[] xNew)
        {
            var newSupp = Support(xNew);
            return !baseSupport.SetEquals(newSupp);
        }
    }
}
*/