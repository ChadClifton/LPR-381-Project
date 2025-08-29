using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LPR381_Project.Models;

namespace LPR381_Project.Algorithms
{
    public static class Knapsack
    {
        /// <summary>
        /// Solves the 0-1 Knapsack IP using Branch and Bound.
        /// Assumes single constraint, binary variables.
        /// Displays all node iterations.
        /// </summary>
        public static SimplexResult SolveBranchAndBoundKnapsack(LPModel model, out List<string> allIterations)
        {
            allIterations = new List<string>();
            allIterations.Add("Branch and Bound Knapsack Algorithm Started");

            double[] values = model.ObjectiveCoefficients.ToArray();
            double[] weights = model.Constraints[0].Coeffs;
            double capacity = model.Constraints[0].RHS;
            int n = values.Length;

            List<Item> items = new List<Item>();
            for (int i = 0; i < n; i++)
                items.Add(new Item { Index = i, Value = values[i], Weight = weights[i] });

            items.Sort((a, b) => (b.Value / b.Weight).CompareTo(a.Value / b.Weight));

            Node root = new Node { Level = -1, Profit = 0, Weight = 0, Selected = new double[n] };
            root.Bound = GetBound(root, n, capacity, items);

            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(root);

            double maxProfit = 0;
            double[] bestSolution = new double[n];

            while (queue.Count > 0)
            {
                Node u = queue.Dequeue();

                string selectedStr = string.Join(",", u.Selected.Select((v, i) => v > 0 ? (i + 1).ToString() : ""));
                if (string.IsNullOrWhiteSpace(selectedStr)) selectedStr = "None";

                allIterations.Add($"Node: Level={u.Level}, Profit={Math.Round(u.Profit, 3)}, Weight={Math.Round(u.Weight, 3)}, Bound={Math.Round(u.Bound, 3)}, Selected Items: {selectedStr}");

                if (u.Level == n - 1) continue;

                Item item = items[u.Level + 1];

                // Take item
                if (u.Weight + item.Weight <= capacity)
                {
                    Node v = new Node
                    {
                        Level = u.Level + 1,
                        Profit = u.Profit + item.Value,
                        Weight = u.Weight + item.Weight,
                        Selected = (double[])u.Selected.Clone()
                    };
                    v.Selected[item.Index] = 1;
                    v.Bound = GetBound(v, n, capacity, items);

                    if (v.Profit > maxProfit)
                    {
                        maxProfit = v.Profit;
                        bestSolution = (double[])v.Selected.Clone();
                    }

                    if (v.Bound > maxProfit) queue.Enqueue(v);
                }

                // Not take item
                Node v2 = new Node
                {
                    Level = u.Level + 1,
                    Profit = u.Profit,
                    Weight = u.Weight,
                    Selected = (double[])u.Selected.Clone()
                };
                v2.Bound = GetBound(v2, n, capacity, items);

                if (v2.Bound > maxProfit) queue.Enqueue(v2);
            }

            allIterations.Add($"Best Candidate: Objective = {Math.Round(maxProfit, 3)}, Items Selected: {string.Join(",", bestSolution.Select((v, i) => v > 0 ? (i + 1).ToString() : ""))}");

            return new SimplexResult
            {
                Status = SolveStatus.Optimal,
                ObjectiveValue = maxProfit,
                PrimalSolution = bestSolution.Select(v => Math.Round(v, 3)).ToArray(),
                Iterations = allIterations
            };
        }

        private class Item
        {
            public int Index;
            public double Value;
            public double Weight;
        }

        private class Node
        {
            public int Level;
            public double Profit;
            public double Weight;
            public double Bound;
            public double[] Selected = Array.Empty<double>();
        }

        private static double GetBound(Node u, int n, double W, List<Item> items)
        {
            if (u.Weight >= W) return 0;
            double bound = u.Profit;
            int j = u.Level + 1;
            double totW = u.Weight;

            while (j < n && totW + items[j].Weight <= W)
            {
                totW += items[j].Weight;
                bound += items[j].Value;
                j++;
            }

            if (j < n) bound += (W - totW) * (items[j].Value / items[j].Weight);

            return bound;
        }
    }
}

/*
namespace LPR381_Project.Algorithms
{
    public static class Knapsack
    {
        public static SimplexResult SolveBranchAndBoundKnapsack(LPModel model, out List<string> allIterations)
        {
            allIterations = new List<string>();
            allIterations.Add("Branch and Bound Knapsack Algorithm Started");

            double[] values = model.ObjectiveCoefficients.ToArray();
            double[] weights = model.Constraints[0].Coeffs;
            double capacity = model.Constraints[0].RHS;
            int n = values.Length;

            List<Item> items = new List<Item> ();
            for (int i = 0; i < n; i++)
                items.Add(new Item { Index = i, Value = values[i], Weight = weights[i] });

            items.Sort((a, b) => (b.Value / b.Weight).CompareTo(a.Value / a.Weight));

            Node root = new Node { Level = -1, Profit = 0, Weight = 0, Selected = new double[n] };
            root.Bound = GetBound(root, n, capacity, items);

            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(root);

            double maxProfit = 0;
            double[] bestSolution = new double[n];

            while (queue.Count > 0)
            {
                Node u = queue.Dequeue();

                // Build a string showing selected items in this node
                string selectedStr = string.Join(",", u.Selected.Select((v, i) => v > 0 ? (i + 1).ToString() : ""));
                if (string.IsNullOrWhiteSpace(selectedStr)) selectedStr = "None";

                allIterations.Add($"Node: Level={u.Level}, Profit={Math.Round(u.Profit, 3)}, Weight={Math.Round(u.Weight, 3)}, Bound={Math.Round(u.Bound, 3)}, Selected Items: {selectedStr}");

                if (u.Level == n - 1) continue;

                Item item = items[u.Level + 1];

                // Take the item (left branch)
                if (u.Weight + item.Weight <= capacity)
                {
                    Node v = new Node
                    {
                        Level = u.Level + 1,
                        Profit = u.Profit + item.Value,
                        Weight = u.Weight + item.Weight,
                        Selected = (double[])u.Selected.Clone()
                    };
                    v.Selected[item.Index] = 1;
                    v.Bound = GetBound(v, n, capacity, items);

                    if (v.Profit > maxProfit)
                    {
                        maxProfit = v.Profit;
                        bestSolution = (double[])v.Selected.Clone();
                    }

                    if (v.Bound > maxProfit) queue.Enqueue(v);
                }

                // Do not take the item (right branch)
                Node v2 = new Node
                {
                    Level = u.Level + 1,
                    Profit = u.Profit,
                    Weight = u.Weight,
                    Selected = (double[])u.Selected.Clone()
                };
                v2.Bound = GetBound(v2, n, capacity, items);

                if (v2.Bound > maxProfit) queue.Enqueue(v2);
            }

            allIterations.Add($"Best Candidate: Objective = {Math.Round(maxProfit, 3)}, Items Selected: {string.Join(",", bestSolution.Select((v, i) => v > 0 ? (i + 1).ToString() : ""))}");

            return new SimplexResult
            {
                Status = SolveStatus.Optimal,
                ObjectiveValue = maxProfit,
                PrimalSolution = bestSolution.Select(v => Math.Round(v, 3)).ToArray()
            };
        }

        private class Item { 
            public int Index; 
            public double Value; 
            public double Weight; 
        }
        private class Node { 
            public int Level; 
            public double Profit;
            public double Weight; 
            public double Bound; 
            public double[] Selected = Array.Empty<double>(); 
        }

        private static double GetBound(Node u, int n, double W, List<Item> items)
        {
            if (u.Weight >= W) return 0;
            double bound = u.Profit;
            int j = u.Level + 1;
            double totW = u.Weight;

            while (j < n && totW + items[j].Weight <= W)
            {
                totW += items[j].Weight;
                bound += items[j].Value;
                j++;
            }

            if (j < n) bound += (W - totW) * (items[j].Value / items[j].Weight);

            return bound;
        }
    }
}
*/
