using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LPR381_Project.Models;

namespace LPR381_Project.Models
{
    public class Variable
    {
        public string Name { get; set; }
        public string SignRestriction { get; set; }
        public bool IsBinary { get; set; }
        public bool IsInteger { get; set; }
        public bool IsUnrestricted { get; set; }

        public Variable(string name, string signRestriction)
        {
            Name = name;
            SignRestriction = signRestriction;
            IsBinary = signRestriction.ToLower() == "bin";
            IsInteger = signRestriction.ToLower() == "int";
            IsUnrestricted = signRestriction.ToLower() == "urs";
        }
    }
}

/*
// (Original Work)
using System;

namespace LPR381_Project.Models
{
    public enum VariableType
    {
        Decision,
        Slack,
        Surplus,
        Artificial
    }

    public class Variable
    {
        public string Name { get; set; }
        public string SignRestriction { get; set; }
        public bool IsBinary { get; set; }
        public bool IsInteger { get; set; }
        public bool IsUnrestricted { get; set; }
        public VariableType Type { get; set; } // Added for solver logic
        public double Value { get; set; } // Added to store the final solution

        public Variable() { } // Parameterless constructor

        public Variable(string name, string signRestriction)
        {
            Name = name;
            SignRestriction = signRestriction;
            SetRestrictionFlags(signRestriction);
        }

        private void SetRestrictionFlags(string signRestriction)
        {
            IsBinary = signRestriction.ToLower() == "bin";
            IsInteger = signRestriction.ToLower() == "int";
            IsUnrestricted = signRestriction.ToLower() == "urs";
        }

        public override string ToString()
        {
            return $"{Name} ({SignRestriction}) = {Value:F3}";
        }
    }
}
 */