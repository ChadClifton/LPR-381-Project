using System;
using System.IO;
using System.Collections.Generic;
using Xunit;
using LPR381_Project.Models;
using LPR381_Project.IO;

namespace LPR381_Project.tests
{
    public class FileHandlerTests
    {
        [Fact]
        public void TestLoadModel_Valid()
        {
            string filePath = "data/model1.txt";

            var model = FileHandler.LoadModel(filePath);

            Assert.True(model.IsMaximization);
            Assert.Equal(new double[] { 2, 3, 4 }, model.ObjectiveCoefficients);

            Assert.Equal(2, model.Constraints.Count);
            Assert.Equal(new double[] { 1, 2, 1 }, model.Constraints[0].Coeffs);
            Assert.Equal(10, model.Constraints[0].RHS);

            Assert.Equal("bin", model.Variables[0].SignRestriction);
            Assert.Equal("int", model.Variables[1].SignRestriction);
            Assert.Equal("urs", model.Variables[2].SignRestriction);
        }

        [Fact]
        public void TestWriteResults()
        {
            var result = new SimplexResult
            {
                Status = "Optimal",
                ObjectiveValue = 42,
                IsMaximization = true,
                ObjectiveCoefficients = new double[] { 2, 3, 4 },
                Variables = new List<Variable>
        {
            new Variable { Name = "x1" },
            new Variable { Name = "x2" },
            new Variable { Name = "x3" }
        },
                Constraints = new List<Constraint>
        {
            new Constraint { Coeffs = new double[] {1,2,1}, Relation = "<=", RHS = 10 },
            new Constraint { Coeffs = new double[] {2,1,3}, Relation = "<=", RHS = 15 }
        },
                PrimalSolution = new double[] { 1, 2, 3 },
                DualPrices = new double[] { 0.5, 1.2 },
                Iterations = new List<string> { "Iteration 1 tableau", "Iteration 2 tableau" }
            };

            string outPath = "data/result1.txt";
            FileHandler.WriteResults(outPath, result);

            Assert.True(File.Exists(outPath));
        }
    }

}
