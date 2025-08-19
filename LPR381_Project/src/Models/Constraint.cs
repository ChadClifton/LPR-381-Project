using System;
using System.Collections.Generic;
using System.Linq;

namespace LPR381_Project
{
    public class Constraint
    {
        public enum ConstraintType
        {
            LessThanOrEqual, // <=
            MoreThanOrEqual, // >=
            Equal            // =
        }
        public double[] Coefficients { get; set; }
        public ConstraintType Type { get; set; }
        public double RHS { get; set; }

        public Constraint(double[] coefficients, ConstraintType type, double rhs)
        {
            Coefficients = coefficients;
            Type = type;
            RHS = rhs;
        }
    }
}