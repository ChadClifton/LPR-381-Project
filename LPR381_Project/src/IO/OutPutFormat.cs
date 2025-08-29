using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LPR381_Project.Algorithms;
using LPR381_Project.Models;

namespace LPR381_Project.IO
{
    public static class OutputFormatter
{
    public static void WriteResults(string filepath, LPModel model, SimplexResult result)
    {
        using var writer = new StreamWriter(filepath);

        writer.WriteLine("=== Canonical Form ===");
        string objType = model.IsMaximization ? "Maximize" : "Minimize";
        string objFunc = string.Join(" + ", model.ObjectiveCoefficients.Select((c, i) => $"{Math.Round(c, 3)} {model.Variables[i].Name}"));
        writer.WriteLine($"{objType}: {objFunc}");

        writer.WriteLine("Subject to:");
        for (int i = 0; i < model.NumConstraints; i++)
        {
            var c = model.Constraints[i];
            string lhs = string.Join(" + ", c.Coeffs.Select((coeff, j) => $"{Math.Round(coeff, 3)} {model.Variables[j].Name}"));
            writer.WriteLine($"{lhs} {c.Relation} {Math.Round(c.RHS, 3)}");
        }

        writer.WriteLine("Variable restrictions:");
        foreach (var v in model.Variables)
        {
            writer.WriteLine($"{v.Name} {v.SignRestriction}");
        }

        writer.WriteLine("\n=== Iterations ===");
        foreach (var iter in result.Iterations)
        {
            writer.WriteLine(iter);
            writer.WriteLine("--------------------");
        }

        writer.WriteLine("\n=== Final Solution ===");
        writer.WriteLine($"Status: {result.Status}");
        writer.WriteLine($"Objective Value: {result.ObjectiveValue:0.000}");

            for (int i = 0; i < Math.Min(model.Variables.Count, result.PrimalSolution.Length); i++)
            {
                writer.WriteLine($"{model.Variables[i].Name} = {result.PrimalSolution[i]:0.000}");
            }
               

        if (result.DualPrices != null && result.DualPrices.Length > 0)
        {
            writer.WriteLine("\nDual Prices:");
            for (int i = 0; i < result.DualPrices.Length; i++)
                writer.WriteLine($"y{i + 1} = {result.DualPrices[i]:0.000}");
        }
    }
}
}
 
/*
// (Origianl work)
using System;
using System.IO;
using System.Linq;
using LPR381_Project.Models;

namespace LPR381_Project.IO
{
    public static class OutputFormatter
    {
        public static void WriteResults(string filepath, SimplexResult result)
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
                {
                    writer.WriteLine($"{v.Name} ∈ {{0,1}}");

                }
                else if (v.IsInteger)
                {
                    writer.WriteLine($"{v.Name} integer");
                }
                    
                else if (v.IsUnrestricted)
                {
                    writer.WriteLine($"{v.Name} unrestricted");
                }    
                else
                {
                    writer.WriteLine($"{v.Name} = 0");
                }
                    
            }

            //This part writes out the iterations
            writer.WriteLine("\n=== Iterations ===");

            if (result.Iterations != null)
            {
                foreach (var iter in result.Iterations)
                {
                    writer.WriteLine(iter);
                    writer.WriteLine("--------------------");
                }

            }
           

            //This part writes out the final solution
            writer.WriteLine("\n=== Final Solution ===");
            writer.WriteLine($"Status: {result.Status}");
            writer.WriteLine($"Objective Value: {result.ObjectiveValue:0.000}");

            if (result.Iterations != null)
            {
                foreach (var iter in result.Iterations)
                    writer.WriteLine(iter + "\n--------------------");
            }

            for (int i = 0; i < result.PrimalSolution.Length; i++)
                writer.WriteLine($"{result.Variables[i].Name} = {result.PrimalSolution[i]:0.000}");

            //This writes out the dual prices if they are available
            if (result.DualPrices != null && result.DualPrices.Length > 0)
            {
                writer.WriteLine("\nDual Prices:");
                for (int i = 0; i < result.DualPrices.Length; i++)
                    writer.WriteLine($"y{i + 1} = {result.DualPrices[i]:0.000}");
            }
        }
    }
}
*/