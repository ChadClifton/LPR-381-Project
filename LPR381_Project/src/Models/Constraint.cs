using System;
using System.Collections.Generic;
using System.Linq;

namespace LPR381_Project.Models
{
    public enum ConstraintType
    {
        LessThanOrEqual, // <=
        MoreThanOrEqual, // >=
        Equal            // =
    }

    public class Constraint
    {
        public double[] Coeffs { get; set; }
        public string Relation { get; set; }
        public double RHS { get; set; }
        public ConstraintType Type { get; set; }

        public Constraint() { }

        public Constraint(double[] coeffs, ConstraintType type, double rhs)
        {
            Coeffs = coeffs;
            Type = type;
            RHS = rhs;
        }

        public static ConstraintType ParseRelation(string relation)
        {
            switch (relation)
            {
                case "<=":
                    return ConstraintType.LessThanOrEqual;
                case ">=":
                    return ConstraintType.MoreThanOrEqual;
                case "=":
                    return ConstraintType.Equal;
                default:
                    throw new ArgumentException($"Invalid constraint relation: {relation}");
            }
        }
    }
}