using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using LP381_Project.Models;
using LPR381_Project;

namespace LP381_Project.IO
{
    public static class FileHandler
    {
        public static LPModel LoadModel(string filepath)
        {
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException("Input file not found.");
            }
                

            var lines = File.ReadAllLines(filepath)
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .ToArray();

            //Making sure file has at east 3 sections (objective function, constraints and the restrictions)
            if (lines.Length < 3)
            {
                throw new InvalidDataException("File missing required sections.");
            }
    
            
            // Parsing the objective Function
            
            var objLine = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            bool isMax = objLine[0].ToLower() == "max";
            if (!isMax && objLine[0].ToLower() != "min")
            {
                throw new InvalidDataException("Objective must start with 'max' or 'min'");
            }
               

           
            var objectiveCoeffs = new List<double>();
            var variables = new List<Variable>();

            //Parsing the terms in the objective function 
            for (int i = 1; i < objLine.Length; i += 2)
            {
                double coeff = double.Parse(objLine[i]);
                string varName = objLine[i + 1].ToLower();

                objectiveCoeffs.Add(coeff);
                variables.Add(new Variable { Name = varName});
            }

            
            //This part parses the constraints
            
            var constraints = new List<Constraint>();
            for (int i = 1; i < lines.Length - 1; i++)
            {
                var parts = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

                // Extracting the values of relation and the rhs
                string relation = parts[parts.Count - 2];
                double rhs = double.Parse(parts.Last());

                parts.RemoveRange(parts.Count - 2, 2);

                var coeffs = new double[variables.Count];

                // Parsing the constraint terms
                for (int j = 0; j < parts.Count; j += 2)
                {
                    double coeff = double.Parse(parts[j]);
                    string varName = parts[j + 1].ToLower();

                    int index = variables.FindIndex(v => v.Name == varName);
                    if (index == -1)
                        throw new InvalidDataException($"Unknown variable {varName} in constraints");

                    coeffs[index] = coeff;
                }

                constraints.Add(new Constraint { Coeffs = coeffs, Relation = relation, RHS = rhs, Type = Constraint.ParseRelation(relation) });
            }

         
            //Parsing the variable restrictions

            var restrictions = lines.Last().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (restrictions.Length != variables.Count)
                throw new InvalidDataException("Mismatch between number of variables and restrictions");

            for (int j = 0; j < variables.Count; j++)
            {
                string res = restrictions[j].ToLower();

                variables[j].SignRestriction = res;
                variables[j].IsBinary = res == "bin";
                variables[j].IsInteger = res == "int";
                variables[j].IsUnrestricted = res == "urs";
            }

            
            // Build and return the Model
            
            return new LPModel
            {
                IsMaximization = isMax,
                ObjectiveCoefficients = objectiveCoeffs.ToArray(),
                Constraints = constraints,
                Variables = variables
            };
        }
    }
}
