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
        public double[] Coefficients { get; set; }
        public Operator operators { get; set; }
        public double RHS { get; set; }

        public Constraint(double[] coefficients, Operator operatorType, double rhs)
        {
            Coefficients = coefficients;
            this.operators = operatorType;
            RHS = rhs;
        }
    }
}