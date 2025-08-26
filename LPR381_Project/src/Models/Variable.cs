using System;

namespace LPR381_Project.Models
{
    public enum VaribleType
    {
        Decision,
        Slack,
        Surplus,
        Artificial
    }

    public class Variable
    {
        public string Name { get; set; }
        public VaribleType Type { get; set; }
        public int Index { get; set; } //col in tableau
        public double Value { get; set; } //final after solving

        public Variable(string name, VaribleType type, int index)
        {
            Name = name;
            Type = type;
            Index = index;
            Value = 0.0;
        }
        public override string ToString() {
            return $"{Name} ({Type}): {Value}";
        }
    }
}