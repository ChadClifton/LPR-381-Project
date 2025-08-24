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
                

            var lines = File.ReadAllLines(filePath)
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .ToArray();

            //Making sure file has at east 3 sections (objective function, constraints and the restrictions)
            if (lines.Length < 3)
                throw new InvalidDataException("File missing required sections.");

            
            // Parsing the objective Function
            
            var objLine = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            bool isMax = objLine[0].ToLower() == "max";
            if (!isMax && objLine[0].ToLower() != "min")
                throw new InvalidDataException("Objective must start with 'max' or 'min'");

           
            var objectiveCoeffs = new List<double>();
            var variables = new List<Variable>();

            //Parsing the terms in the objective function 
            for (int i = 1; i < objLine.Length; i += 2)
            {
                double coeff = double.Parse(tokens[i]);
                string varName = objLine[i + 1].ToLower();

                objectiveCoeffs.Add(coeff);
                variables.Add(new Variable { Name = varName, Type = "+", SignRestriction = "+" });
            }

            
            //This part parses the onstraints
            
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

                constraints.Add(new Constraint { Coeffs = coeffs, Relation = relation, RHS = rhs });
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

        public static void WriteResults(string filePath, SimplexResult result)
        {
            using var writer = new StreamWriter(filepath);

            //Writing it into canonical form
            writer.WriteLine("=== Canonical Form ===");
           
            string objType = result.IsMaximization ? "Maximize" : "Minimize";
            string objFunc = string.Join(" + ",
                result.ObjectiveCoefficients
                      .Select((c, i) => $"{c} {result.Variables[i].Name}"));
            writer.WriteLine($"{objType}: {objFunc}");

            writer.WriteLine("Subject to:");
            for (int i = 0; i < result.Constraints.Count; i++)
            {
                var c = result.Constraints[i];
                string lhs = string.Join(" + ",
                    c.Coeffs.Select((coeff, j) => $"{coeff} {result.Variables[j].Name}"));
                writer.WriteLine($"{lhs} {c.Relation} {c.RHS}");
            }

            //This part writes out the variable restrictions
            writer.WriteLine("Variable restrictions:");
            foreach (var v in result.Variables)
            {
                if (v.IsBinary)
                    writer.WriteLine($"{v.Name} ∈ {{0,1}}");
                else if (v.IsInteger)
                    writer.WriteLine($"{v.Name} integer");
                else if (v.IsUnrestricted)
                    writer.WriteLine($"{v.Name} unrestricted");
                else
                    writer.WriteLine($"{v.Name} ≥ 0");
            }

            //This part writes out the iterations
            writer.WriteLine("\n=== Iterations ===");
            foreach (var iter in result.Iterations)
            {
                writer.WriteLine(iter);
                writer.WriteLine("--------------------");
            }

            //This part writes out the final solution
            writer.WriteLine("\n=== Final Solution ===");
            writer.WriteLine($"Status: {result.Status}");
            writer.WriteLine($"Objective Value: {result.ObjectiveValue:0.000}");

            for (int i = 0; i < result.PrimalSolution.Length; i++)
                writer.WriteLine($"{result.Variables[i].Name} = {result.PrimalSolution[i]:0.000}");

            //This writes out the dual prices if they are available
            if (result.DualPrices.Length > 0)
            {
                writer.WriteLine("\nDual Prices:");
                for (int i = 0; i < result.DualPrices.Length; i++)
                    writer.WriteLine($"y{i + 1} = {result.DualPrices[i]:0.000}");
            }
        }
    }
}
