using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR381_Project
{
    public class LPModel
    {
        public bool IsMax { get; set; }                     //ture = max, false = min
        public double[] ObjCoeffiecients { get; set; }      //obj coeffiecients  
        public List<Constraint> Constraints { get; set; }   //list of constraints

        public LPModel(double[] objCoeffiecients, bool isMax)
        {
            this.ObjCoeffiecients = objCoeffiecients;
            this.IsMax = isMax;
            Constraints = new List<Constraint>();
        }
    }
}