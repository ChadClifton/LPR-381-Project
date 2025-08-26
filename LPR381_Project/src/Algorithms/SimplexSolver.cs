using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LPR381_Project
{
    public class SimplexSolver
    {
        public bool IsMaximization { get; set; }
        public double[] ObjectiveCoefficients { get; set; }
        public List<Constraint> Constraints { get; set; }
        public List<Variable> Variables { get; set; }
        public string Status { get; set; }
        public double ObjectiveValue { get; set; }
        public double[] PrimalSolution { get; set; }
        public double[] DualPrices { get; set; }
        public List<string> Iterations { get; set; }


    }
}