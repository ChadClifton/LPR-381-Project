using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using LPR381_Project.IO;
using LPR381_Project.Models;

namespace LPR381_Project.Tests
{
    public class FileHandlerTests
    {
        [Fact]
        public void TestReadInputFile_Valid()
        {
            //Calls the method that we want to test with a valid input file
            var model = FileHandler.ReadInputFile("tests/data/valid_input.txt");

            //Checking the linear programming model 
            Assert.Equal("max", model.ObjectiveType);
            Assert.Equal(3, model.ObjectiveCoefficients["x1"]);
            Assert.Equal(5, model.ObjectiveCoefficients["x2"]);
            Assert.Equal(2, model.Constraints.Count);
            Assert.Equal("+", model.Variables.First(v => v.Name == "x1").Type);
        
        }

        [Fact]

        public void TestReadInputFile_InValid()
        {
            Assert.Throws<System.Exception>(() =>
            {
                FileHandler.ReadInputFile("tests/data/invalid_input.txt");
            });
        }

        [Fact]
        public void TestReadInputFile_AllVariableTypes()
        {
            var model = FileHandler.ReadInputFile("tests/data/variable_types.txt");

            // Checking if each variable was assigned the correct type
            Assert.Equal("+", model.Variables.First(v => v.Name == "x1").Type);
            Assert.Equal("-", model.Variables.First(v => v.Name == "x2").Type);
            Assert.Equal("urs", model.Variables.First(v => v.Name == "x3").Type);
            Assert.Equal("int", model.Variables.First(v => v.Name == "x4").Type);
            Assert.Equal("bin", model.Variables.First(v => v.Name == "x5").Type);
        }

    }
}
