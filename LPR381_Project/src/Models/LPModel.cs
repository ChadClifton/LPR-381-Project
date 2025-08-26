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
        public enum IsMax
        {
            Yes,
            No
        }
        public IsMax Max{ get; set; }                     //yes = max, no = min
        public double[] ObjCoeffiecients { get; set; }      //obj coeffiecients  
        public List<Constraint> Constraints { get; set; }   //list of constraints

        public LPModel(IsMax isMax, double[] objCoeffiecients)
        {
            this.ObjCoeffiecients = objCoeffiecients;
            Max = isMax;
            Constraints = new List<Constraint>();
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