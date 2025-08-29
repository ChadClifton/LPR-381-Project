using System;
using System.Linq;
using System.Text;

namespace LPR381_Project.Utils
{
    public static class MathUtils
    {
        public static double[,] Invert(double[,] M, double tolerance = 1e-10)
        {
            int n = M.GetLength(0);
            if (n != M.GetLength(1)) throw new ArgumentException("Matrix must be square");

            double[,] A = new double[n, n * 2];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    A[i, j] = M[i, j];
                    A[i, n + j] = (i == j) ? 1.0 : 0.0;
                }
            }

            for (int i = 0; i < n; i++)
            {
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                    if (Math.Abs(A[k, i]) > Math.Abs(A[maxRow, i]))
                        maxRow = k;

                if (Math.Abs(A[maxRow, i]) < tolerance)
                    throw new InvalidOperationException("Matrix is singular");

                if (maxRow != i)
                {
                    for (int j = 0; j < n * 2; j++)
                    {
                        double tmp = A[i, j];
                        A[i, j] = A[maxRow, j];
                        A[maxRow, j] = tmp;
                    }
                }

                double p = A[i, i];
                for (int j = 0; j < 2 * n; j++) A[i, j] /= p;

                for (int r = 0; r < n; r++)
                {
                    if (r == i) continue;
                    double factor = A[r, i];
                    for (int j = 0; j < 2 * n; j++) A[r, j] -= factor * A[i, j];
                }
            }

            double[,] inverse = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    inverse[i, j] = A[i, n + j];

            return inverse;
        }

        public static double[,] Submatrix(double[,] A, int[] rows, int[] cols)
        {
            double[,] R = new double[rows.Length, cols.Length];
            for (int i = 0; i < rows.Length; i++)
                for (int j = 0; j < cols.Length; j++)
                    R[i, j] = A[rows[i], cols[j]];
            return R;
        }

        public static double[] Column(double[,] A, int col)
        {
            int m = A.GetLength(0);
            double[] v = new double[m];
            for (int i = 0; i < m; i++) v[i] = A[i, col];
            return v;
        }

        public static double[] Multiply(double[,] M, double[] v)
        {
            int r = M.GetLength(0), c = M.GetLength(1);
            if (c != v.Length) throw new ArgumentException("Dimension mismatch");
            double[] res = new double[r];
            for (int i = 0; i < r; i++)
            {
                double s = 0;
                for (int j = 0; j < c; j++) s += M[i, j] * v[j];
                res[i] = s;
            }
            return res;
        }

        public static double[,] Multiply(double[,] A, double[,] B)
        {
            int r = A.GetLength(0), k = A.GetLength(1);
            int k2 = B.GetLength(0), c = B.GetLength(1);
            if (k != k2) throw new ArgumentException("Dimension mismatch");
            double[,] R = new double[r, c];
            for (int i = 0; i < r; i++)
                for (int j = 0; j < c; j++)
                {
                    double s = 0;
                    for (int t = 0; t < k; t++) s += A[i, t] * B[t, j];
                    R[i, j] = s;
                }
            return R;
        }

        public static double[,] Transpose(double[,] A)
        {
            int r = A.GetLength(0), c = A.GetLength(1);
            double[,] T = new double[c, r];
            for (int i = 0; i < r; i++)
                for (int j = 0; j < c; j++)
                    T[j, i] = A[i, j];
            return T;
        }

        public static double Dot(double[,] rowVector1xN, double[] vecN)
        {
            int n = vecN.Length;
            double s = 0;
            for (int j = 0; j < n; j++) s += rowVector1xN[0, j] * vecN[j];
            return s;
        }

        public static string PrettyMatrix(double[,] M, int dp = 3)
        {
            var sb = new StringBuilder();
            int r = M.GetLength(0), c = M.GetLength(1);
            for (int i = 0; i < r; i++)
            {
                var row = Enumerable.Range(0, c)
                    .Select(j => Math.Round(M[i, j], dp).ToString("0.000"));
                sb.AppendLine(string.Join(" ", row));
            }
            return sb.ToString().TrimEnd();
        }

        public static double[,] RowToVector1D(double[] A)
        {
            double[,] v = new double[1, A.Length];
            for (int i = 0; i < A.Length; i++) v[0, i] = A[i];
            return v;
        }
    }
}

/*
namespace LP381_Project.Utils
{
    public static class MathUtils
    {
        //inverts a square matrix using the Gauss-Jordan elimination mehtod
        //throws if the matrix is singular or nearly singular
        //tolerance is used for numerical stability and can be set
        //can be called like normal: Invert(matrix);
        public static double[,] Invert(double[,] M, double tolerance = 1e-10) 
        {
            int n = M.GetLength(0);
            if (n != M.GetLength(1))
                throw new ArgumentException("Matrix must be square");

            // augmented matrix [M|I]
            double[,] A = new double[n, n * 2];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    A[i, j] = M[i, j];
                    A[i, 1 + n] = (i == j) ? 1.0 : 0.0;
                }
            }             

            // gauss-jordan elimination
            for (int i = 0; i < n; i++)
            {
                //partial pivot
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(A[k, i]) > Math.Abs(A[maxRow, i]))
                        maxRow = k;
                }

                if (Math.Abs(A[maxRow, i]) < tolerance)
                    throw new InvalidOperationException("Matrix is singular or nearly singular");

                //swap rows
                if (maxRow != i)
                {
                    for (int j = 0; j < n * 2; j++)
                    {
                        double tmp = A[i, j];
                        A[i,j] = A[maxRow, j];
                        A[maxRow,j] = tmp;
                    }
                }

                // normalise piviot row
                double p = A[i, i];
                for (int j = 0; j < 2 * n; j++)
                    A[i, j] /= p;

                //eliminate other rows
                for (int r = 0; r < n; r++)
                {
                    if (r == i) continue;
                    double factor = A[r, i];
                    for (int j = 0; j < 2 * n; j++)
                        A[r, j] -= factor * A[i, j];
                }
            }

            //extract the inverse matrix from the augmented matrix
            double[,] inverse = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    inverse[i, j] = A[i, j + n];

            return inverse;
        }

        public static double[,] Submatrix(double[,] A, int[] rows, int[] cols)
        {
            var R = new double[rows.Length, cols.Length];
            for (int i = 0; i < rows.Length; i++)
                for (int j = 0; j < cols.Length; j++)
                    R[i, j] = A[rows[i], cols[j]];
            return R;
        }

        public static double[] Column(double[,] A, int col)
        {
            int m = A.GetLength(0);
            var v = new double[m];
            for (int i = 0; i < m; i++) v[i] = A[i, col];
            return v;
        }

        public static double[] Multiply(double[,] M, double[] v)
        {
            int r = M.GetLength(0), c = M.GetLength(1);
            if (c != v.Length) throw new ArgumentException("Dimension mismatch");
            var res = new double[r];
            for (int i = 0; i < r; i++)
            {
                double s = 0;
                for (int j = 0; j < c; j++) s += M[i, j] * v[j];
                res[i] = s;
            }
            return res;
        }

        public static double[,] Multiply(double[,] A, double[,] B)
        {
            int r = A.GetLength(0), k = A.GetLength(1);
            int k2 = B.GetLength(0), c = B.GetLength(1);
            if (k != k2) throw new ArgumentException("Dimension mismatch");
            var R = new double[r, c];
            for (int i = 0; i < r; i++)
                for (int j = 0; j < c; j++)
                {
                    double s = 0;
                    for (int t = 0; t < k; t++) s += A[i, t] * B[t, j];
                    R[i, j] = s;
                }
            return R;
        }

        public static double[,] Transpose(double[,] A)
        {
            int r = A.GetLength(0), c = A.GetLength(1);
            var T = new double[c, r];
            for (int i = 0; i < r; i++)
                for (int j = 0; j < c; j++)
                    T[j, i] = A[i, j];
            return T;
        }

        public static double Dot(double[,] rowVector1xN, double[] vecN)
        {
            int n = vecN.Length;
            if (rowVector1xN.GetLength(0) != 1 || rowVector1xN.GetLength(1) != n)
                throw new ArgumentException("Not a 1xN row vector");
            double s = 0;
            for (int j = 0; j < n; j++) s += rowVector1xN[0, j] * vecN[j];
            return s;
        }

        public static string PrettyMatrix(double[,] M, int dp = 3)
        {
            var sb = new StringBuilder();
            int r = M.GetLength(0), c = M.GetLength(1);
            for (int i = 0; i < r; i++)
            {
                var row = Enumerable.Range(0, c)
                    .Select(j => Math.Round(M[i, j], dp).ToString("0.000"));
                sb.AppendLine(string.Join(" ", row));
            }
            return sb.ToString().TrimEnd();
        }

        public static double[] RowToVector2D(double[,] A, int row) //2D array
        {
            int c = A.GetLength(1);
            var v = new double[c];
            for (int j = 0; j < c; j++) v[j] = A[row, j];
            return v;
        }

        public static double[,] RowToVector1D(double[] A) //1D array
        {
            var v = new double[1, A.Length];
            for (int i = 0; i < A.Length; i++)
                v[0, i] = A[i];
            return v;
        }
    }
}
*/