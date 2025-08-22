using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using LPR381_Project.Models;

namespace LPR381_Project.IO
{
    public class FileHandler
    {
        // Reading the input file and building the LPModel
        public static LPModel ReadInputFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Input file not found: {path}");
            }

            // Read all lines and removes the empty/whitespace lines
            var lines = File.ReadAllLines(path)
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .ToList();

            var model = new LPModel();

            try
            {
                //Reads the objective function line
                string objLine = lines[0].Trim().ToLower();

                // Trying to determine if it is a max or min problem
                if (objLine.StartsWith("max")) model.objectiveType = LPModel.ObjectiveType.max;
                else if (objLine.StartsWith("min")) model.objectiveType = LPModel.ObjectiveType.min;
                else throw new Exception("Objective function has to be either max or min");

                // Gettting the expression after 'max' or 'min'
                string expr = objLine.Substring(3).Trim();

                //Parsing each term of the objective function
                foreach (var term in expr.Split('+', StringSplitOptions.RemoveEmptyEntries))
                {
                    var clean = term.Trim();
                    var parts = clean.Split('x');

                    //Validation of term format
                    if (parts.Length != 2)
                        throw new Exception($"Invalid objective term: {clean}");

                    double coeff = double.Parse(parts[0]);
                    string varName = "x" + parts[1];

                    //Adding it to the model
                    model.ObjCoeffiecients[varName] = coeff;

                    //Adds the variable to the list if it does not exist
                    if (!model.Variables.Any(v => v.Name == varName))
                        model.Variables.Add(new Variable { Name = varName, Type = "+" });
                }

                //Parsing the constraints from the rest of the lines
                for (int i = 1; i < lines.Count; i++)
                {
                    string line = lines[i].Trim();

                    //Ignoring or skipping header lines, used subject to as its the most common one
                    if (line.ToLower().StartsWith("subject to")) continue;

                    //Determining the constraint operator and splitting the right/left side
                    if (line.Contains(">=") || line.Contains("<=") || line.Contains("="))
                    {
                        var constraint = new Constraint();
                        string[] sides;

                        // Constraint splitting
                        if (line.Contains(">="))
                        {
                            sides = line.Split(">=");
                            constraint.operators = Constraint.Operator.MoreThanOrEqual;
                        }
                        else if (line.Contains("<="))
                        {
                            sides = line.Split("<=");
                            constraint.operators = Constraint.Operator.LessThanOrEqual;
                        }
                        else
                        {
                            sides = line.Split("=");
                            constraint.operators = Constraint.Operator.Equal;
                        }

                        //Parsing each term on the left side of the constraint
                        foreach (var term in sides[0].Split('+', StringSplitOptions.RemoveEmptyEntries))
                        {
                            var clean = term.Trim();
                            var parts = clean.Split('x');

                            //Validating constraint term format
                            if (parts.Length != 2)
                                throw new Exception($"Invalid constraint term: {clean}");

                            double coeff = double.Parse(parts[0]);
                            string varName = "x" + parts[1];

                            constraint.Coefficients[varName] = coeff;
                        }

                        // Parsing the right hand side value
                        constraint.RHS = double.Parse(sides[1]);
                        model.Constraints.Add(constraint);
                    }

                    // Parsing the variable types
                    string lowerLine = line.ToLower();

                    if (lowerLine.Contains(">="))
                    {
                        string varName = line.Split(">=")[0].Trim();
                        var variable = model.Variables.FirstOrDefault(v => v.Name == varName);
                        if (variable != null) variable.Type = "+";
                    }

                    if (lowerLine.Contains("<="))
                    {
                        string varName = line.Split("<=")[0].Trim();
                        var variable = model.Variables.FirstOrDefault(v => v.Name == varName);
                        if (variable != null) variable.Type = "-";
                    }

                    if (lowerLine.Contains("urs"))
                    {
                        string varName = line.Split(' ')[0].Trim();
                        var variable = model.Variables.FirstOrDefault(v => v.Name == varName);
                        if (variable != null) variable.Type = "urs";
                    }

                    if (lowerLine.Contains("int"))
                    {
                        string varName = line.Split(' ')[0].Trim();
                        var variable = model.Variables.FirstOrDefault(v => v.Name == varName);
                        if (variable != null) variable.Type = "int";
                    }

                    if (lowerLine.Contains("bin"))
                    {
                        string varName = line.Split(' ')[0].Trim();
                        var variable = model.Variables.FirstOrDefault(v => v.Name == varName);
                        if (variable != null) variable.Type = "bin";
                    }
                }
            }
            catch (Exception ex)
            {
                //Message that will be displayed if there is a problem parsing the file
                throw new Exception($"Error parsing file: {ex.Message}");
            }

            return model;
        }

        // Writing output to a file
        public static void WriteOutputFile(string path, string content)
        {
            File.WriteAllText(path, content);
        }
    }
}