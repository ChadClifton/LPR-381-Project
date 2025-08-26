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
        //public enum ObjectiveType
        //{
        //    max,
        //    min
        //}
        //public ObjectiveType objectiveType { get; set; }    //max or min
        public bool IsMax { get; set; } //true = y, false = n
        public double[] ObjCoefficients { get; set; }      //obj coefficients 
        public List<Constraint> Constraints { get; set; }   //list of constraints

        public LPModel(bool isMax, double[] objCoefficients)
        {
            this.ObjCoefficients = objCoefficients; 
            this.IsMax = isMax;
            //this.objectiveType = objectiveType; (ObjectiveType objectiveType)
            Constraints = new List<Constraint>();
        }

        public void AddConstraint(Constraint constraint)
        {
            Constraints.Add(constraint);
        }
            
    }
}