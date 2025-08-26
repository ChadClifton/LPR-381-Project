using System;
using System.Collections.Generic;
using System.Linq;

namespace LPR381_Project.Models
{
    public class Constraint
    {
        public enum Operator
        {
            LessThanOrEqual, // <=
            MoreThanOrEqual, // >=
            Equal            // =
        }
        public double[] Coeffs { get; set; }
        public Operator Relation { get; set; }
        public double RHS { get; set; }

        public Constraint(double[] coefficients, Operator relationType, double rhs)
        {
            Coeffs = coefficients;
            this.Relation = relationType;
            RHS = rhs;
        }
    }
}