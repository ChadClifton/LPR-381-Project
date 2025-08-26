using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR381_Project
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