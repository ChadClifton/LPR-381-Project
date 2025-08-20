

// Chad Work:
/*
using System;
using System.Collections.Generic;
using LP381_Project.Models;

namespace LP381_Project.Analysis
{
    public class SensitivityAnalysis
    {
        private LPModel model;

        // Constructor - take solved model as input
        public SensitivityAnalysis(LPModel solvedModel)
        {
            this.model = solvedModel;
        }

        // === Variable Analysis ===

        // Get allowable increase/decrease for a non-basic variable
        public void GetNonBasicVariableRange(string variableName)
        {
            // TODO: Implement range calculation
        }

        // Apply change to non-basic variable coefficient
        public void ApplyChangeToNonBasicVariable(string variableName, double newCoefficient)
        {
            // TODO: Recalculate solution after change
        }

        // Get allowable increase/decrease for a basic variable
        public void GetBasicVariableRange(string variableName)
        {
            // TODO: Implement range calculation
        }

        // Apply change to basic variable coefficient
        public void ApplyChangeToBasicVariable(string variableName, double newCoefficient)
        {
            // TODO: Recalculate solution after change
        }

        // === Constraint Analysis ===

        // Get allowable increase/decrease for RHS of a constraint
        public void GetConstraintRHRange(string constraintName)
        {
            // TODO: Implement RHS range calculation
        }

        // Apply change to RHS value
        public void ApplyChangeToConstraintRHS(string constraintName, double newValue)
        {
            // TODO: Recalculate solution after change
        }

        // Add a new constraint to the model
        public void AddConstraint(Constraint newConstraint)
        {
            // TODO: Add constraint and re-run solver
        }

        // === Activity Analysis ===

        // Add a new decision variable/activity
        public void AddVariable(Variable newVariable)
        {
            // TODO: Add variable and re-run solver
        }

        // === Shadow Prices ===

        public void CalculateShadowPrices()
        {
            // TODO: Compute shadow prices from dual values
        }

        // === Duality ===

        public void ConstructDual()
        {
            // TODO: Construct the dual LP model
        }

        public void SolveDual()
        {
            // TODO: Call solver on dual model
        }

        public void VerifyDuality()
        {
            // TODO: Compare primal and dual objective values
        }
    }
}
*/