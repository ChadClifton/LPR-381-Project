# LPR381 Project: Linear Programming Solver

## Introduction (Programming)
Operations research is a scientific approach to decision making that seeks to best design and operate a system, under conditions requiring the allocation of scarce resources. The scientific approach to decision making usually involves the use of one or more mathematical models.
If, whenever decision variables appear in the objective function and in the constraints of an optimisation model, the decision variables are always multiplied by constants and added together, such a model is a linear model.
If one or more decision variables must be integer, then we say that an optimisation model is a discrete model or an integer model.

## Outline
Tip: do the assignment first.
For the project, create a program that solves Linear Programming and Integer Programming Models and then analyses how the changes in an LP’s parameters change the optimal solution.
The source code must be a visual studio project. Any .NET programming language may be used. The project should build an executable (solve.exe) that is menu driven with the following:
The program should be able to accept an input text file with the mathematical model and export all results to an output text file.

## Minimum Requirements Criteria
Program should accept a random amount of decision variables.
Program should accept a random amount of constraints.
Use comments with programming.
Programming Best Practices should be implemented.

## Input Text File Criteria
The first line contains the following, separated by spaces:
The word max or min, to indicate whether it is a maximization or a minimization problem.
For each decision variable, an operator to represent whether the objective function coefficient is a negative or positive.
For each decision variable, a number to represent its objective function coefficient.

A line for each constraint:
The operator of the technological coefficients for the decision variables, in the same order as in the specification of the objective function in line 1, that represents whether the technological coefficient is negative or positive.
The technological coefficients for the decision variables, in the same order as in the specification of the objective function in line 1.

## Project Implementation
This repository contains the implementation of the LPR381 project in C# using .NET and Visual Studio. The program solves LP and IP models, performs sensitivity analysis, and handles duality and special cases.
### Features
Menu-driven console application.
Supports loading models from text files in the specified format.
Solvers: Primal Simplex, Revised Simplex, Branch & Bound, Cutting Plane, Knapsack.
Sensitivity analysis: Variable ranges, RHS ranges, changes, additions.
Duality: Construct and solve dual, verify duality.
Special cases: Infeasibility, unboundedness, degeneracy, multiple solutions.
Export results to text file.

## Requirements
.NET SDK (version 6.0 or higher recommended).
Visual Studio 2022 or later for development.


## Sample Input File (sample.txt)
max 2 x1 3 x2 4 x3
1 x1 2 x2 1 x3 <= 10
2 x1 1 x2 3 x3 <= 15
bin int urs