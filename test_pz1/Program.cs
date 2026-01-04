using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace pz_1dn 
{
  
    public class Matrix
    {
        public int Rows { get; private set; }
        public int Cols { get; private set; }
        private int[,] _data;

        public Matrix(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            _data = new int[rows, cols];
        }

        public int this[int r, int c]
        {
            get { return _data[r, c]; }
            set { _data[r, c] = value; }
        }

        public static Matrix Random(int rows, int cols)
        {
            var matrix = new Matrix(rows, cols);
            var rand = new Random();
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    matrix[i, j] = rand.Next(1, 10);
            return matrix;
        }
    }

    
    public class MatrixCalculator
    {
      
        public Matrix AddSingleThread(Matrix A, Matrix B)
        {
            if (A.Rows != B.Rows || A.Cols != B.Cols)
                throw new ArgumentException("Розміри матриць не співпадають");

            var result = new Matrix(A.Rows, A.Cols);
            for (int i = 0; i < A.Rows; i++)
            {
                for (int j = 0; j < A.Cols; j++)
                {
                    result[i, j] = A[i, j] + B[i, j];
                }
            }
            return result;
        }

        
        public Matrix AddParallel(Matrix A, Matrix B, int threadCount)
        {
            if (A.Rows != B.Rows || A.Cols != B.Cols)
                throw new ArgumentException("Розміри матриць не співпадають");

            var result = new Matrix(A.Rows, A.Cols);
            var threads = new List<Thread>();

            
            int gridRows = 1;
            int gridCols = 1;

            for (int i = (int)Math.Sqrt(threadCount); i >= 1; i--)
            {
                if (threadCount % i == 0)
                {
                    gridRows = i;
                    gridCols = threadCount / i;
                    break;
                }
            }

            int rowsPerBlock = A.Rows / gridRows;
            int colsPerBlock = A.Cols / gridCols;

            for (int r = 0; r < gridRows; r++)
            {
                for (int c = 0; c < gridCols; c++)
                {
                    int startRow = r * rowsPerBlock;
                    int endRow = (r == gridRows - 1) ? A.Rows : (r + 1) * rowsPerBlock;

                    int startCol = c * colsPerBlock;
                    int endCol = (c == gridCols - 1) ? A.Cols : (c + 1) * colsPerBlock;

                    Thread t = new Thread(() =>
                    {
                        AddBlock(A, B, result, startRow, endRow, startCol, endCol);
                    });

                    threads.Add(t);
                    t.Start();
                }
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            return result;
        }

        private void AddBlock(Matrix A, Matrix B, Matrix Res, int rStart, int rEnd, int cStart, int cEnd)
        {
            for (int i = rStart; i < rEnd; i++)
            {
                for (int j = cStart; j < cEnd; j++)
                {
                    Res[i, j] = A[i, j] + B[i, j];
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Laboratory Work No.1 - Task 5b*");

           
            Console.Write("Rows: ");
            int rows = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Cols: ");
            int cols = int.Parse(Console.ReadLine() ?? "0");

            Console.Write("Threads: ");
            int threads = int.Parse(Console.ReadLine() ?? "1");

            if (rows <= 0 || cols <= 0 || threads <= 0)
            {
                Console.WriteLine("Invalid input.");
                return;
            }

            Console.WriteLine("Generating matrices...");
            var matA = Matrix.Random(rows, cols);
            var matB = Matrix.Random(rows, cols);
            var calculator = new MatrixCalculator();

            Console.WriteLine("Running Single Thread...");
            Stopwatch sw = Stopwatch.StartNew();
            var resSingle = calculator.AddSingleThread(matA, matB);
            sw.Stop();
            long timeSingle = sw.ElapsedMilliseconds;
            Console.WriteLine($"Single Thread Time: {timeSingle} ms");

            Console.WriteLine($"Running Parallel ({threads} threads)...");
            sw.Restart();
            var resMulti = calculator.AddParallel(matA, matB, threads);
            sw.Stop();
            long timeMulti = sw.ElapsedMilliseconds;
            Console.WriteLine($"Parallel Time: {timeMulti} ms");

            double speedup = timeMulti > 0 ? (double)timeSingle / timeMulti : 0;
            Console.WriteLine($"Speedup: {speedup:F2}x");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}