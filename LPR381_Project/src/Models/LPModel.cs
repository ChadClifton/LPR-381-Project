using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LPR381_Project.Models;

namespace LPR381_Project.Models
{
    public class LPModel
    {
        public bool IsMaximization { get; set; }
        public List<double> ObjectiveCoefficients { get; private set; } = new List<double>();
        public List<Constraint> Constraints { get; private set; } = new List<Constraint>();
        public List<Variable> Variables { get; private set; } = new List<Variable>();

        public int NumVariables => Variables.Count;
        public int NumConstraints => Constraints.Count;

        public LPModel(bool isMaximization)
        {
            IsMaximization = isMaximization;
            ObjectiveCoefficients = new List<double>();
            Constraints = new List<Constraint>();
            Variables = new List<Variable>();
        }

        public void AddVariable(Variable variable, double objectiveCoefficient)
        {
            Variables.Add(variable);
            ObjectiveCoefficients.Add(objectiveCoefficient);
        }

        public void AddConstraint(Constraint constraint)
        {
            Constraints.Add(constraint);
        }

        public LPModel Clone()
        {
            var clone = new LPModel(IsMaximization);
            for (int j = 0; j < NumVariables; j++)
                clone.AddVariable(Variables[j], ObjectiveCoefficients[j]);
            for (int i = 0; i < NumConstraints; i++)
            {
                var c = Constraints[i];
                clone.AddConstraint(new Constraint((double[])c.Coeffs.Clone(), c.Type, c.RHS) { Relation = c.Relation });
            }
            return clone;
        }
    }
}

/*
(Origianl work)
using LPR381_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LPR381_Project.Models.LPModel;

namespace LPR381_Project.Models
{
    public class LPModel
    {
        public bool IsMaximization { get; set; }
        public List<double> ObjectiveCoefficients { get; private set; }
        public List<Constraint> Constraints { get; private set; }
        public List<Variable> Variables { get; private set; }
        public int NumVariables => Variables.Count;
        public int NumConstraints => Constraints.Count;

        public LPModel(bool isMaximization)
        {
            IsMaximization = isMaximization;
            ObjectiveCoefficients = new List<double>();
            Constraints = new List<Constraint>();
            Variables = new List<Variable>();
        }

        public void AddVariable(Variable variable, double objectiveCoefficient)
        {
            if (Variables.Any(v => v.Name == variable.Name))
            {
                throw new ArgumentException($"A variable with the name '{variable.Name}' already exists.");
            }
            Variables.Add(variable);
            ObjectiveCoefficients.Add(objectiveCoefficient);
        }

        public void AddConstraint(Constraint constraint)
        {
            if (constraint.Coeffs.Length != NumVariables)
            {
                throw new InvalidOperationException("Constraint coefficients count must match the number of variables.");
            }
            Constraints.Add(constraint);
        }
    }
}
 */