using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR381_Project
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
            Constraints.Add(constraint);
        }
            
    }
}