using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace solution
{
    class loader
    {
        static List<int> input1 = new List<int>();
        static List<int> input2 = new List<int>();

        internal static void LoadDataFromFile()
        {
            var lines = System.IO.File.ReadAllLines("test1.txt");

            var t = Read(lines);

            input1 = t.Item1;
            input2 = t.Item2;
        }

        internal static void LoadDataFromSTDIN()
        {
            string stdin = null;
            if (Console.IsInputRedirected)
            {
                using (StreamReader reader = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding))
                {
                    stdin = reader.ReadToEnd();
                }
            }

            var lines = stdin.Split('\n');
            var t = Read(lines);

            input1 = t.Item1;
            input2 = t.Item2;
        }

        private static Tuple<List<int>, List<int>> Read(IEnumerable<string> lines)
        {
            List<int> list1 = new List<int>();
            List<int> list2 = new List<int>();

            foreach(var line in lines)
            {
                var ln = line.Split(' ');
                int a = Convert.ToInt32(ln[0]);
                int b = Convert.ToInt32(ln[1]);

                list1.Add(a);
                list2.Add(b);
            }

            return Tuple.Create<List<int>, List<int>>(list1,list2);
        }
    
        internal static decimal CalculateJaccardSimilarity()
        {
            decimal intersect = input1.Intersect(input2).Count();
            decimal union = input1.Union(input2).Count();

            return Math.Round(intersect/union,3);
        }

        internal static decimal CalculateNormalizedMutualInformation()
        {
            //Yet To IMPLEMENT
            return 0;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            loader.LoadDataFromFile();

            Console.WriteLine($"{loader.CalculateNormalizedMutualInformation()} {loader.CalculateJaccardSimilarity()}");
        }
    }
}
