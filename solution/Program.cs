using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace solution
{
    class loader
    {
        class confusionMatrix
        {
            internal int TP = 0;
            internal int FN = 0;
            internal int FP = 0;
        }

        static Dictionary<int,Tuple<int,int>> RawData = new Dictionary<int, Tuple<int, int>>();
        static Dictionary<string,confusionMatrix> OrchestratedData = new Dictionary<string, confusionMatrix>();


        static List<int> input1 = new List<int>();
        static List<int> input2 = new List<int>();

        internal static void LoadDataFromFile(string path)
        {
            var lines = System.IO.File.ReadAllLines(path);

            var t = Read(lines);

            int point = 0;
            t.ForEach(x=>RawData.Add(point++,Tuple.Create(x.Item1,x.Item2)));
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

            int point = 0;
            t.ForEach(x=>RawData.Add(point++,Tuple.Create(x.Item1,x.Item2)));            
        }

        internal static void OrchestradeDataToCluster()
        {
            var points = RawData.Keys.ToArray();

            for(int i=0; i<points.Length-1;i++)
            {
                for(int j=i+1; j<points.Length;j++)
                {
                    var GTi = RawData[points[i]].Item1;
                    var GTj = RawData[points[j]].Item1;

                    var PRi = RawData[points[i]].Item2;
                    var PRj = RawData[points[j]].Item2;

                    int tp=0,fp=0,fn=0;
                    if(GTi==GTj && PRi==PRj)
                        tp=1;
                    else if(GTi==GTj && PRi!=PRj)
                        fn=1;
                    else if(GTi!=GTj && PRi==PRj)
                        fp=1;

                    OrchestratedData.Add($"{points[i]}-{points[j]}",new confusionMatrix{TP=tp,FN=fn,FP=fp});
                }
            }
        }

        private static List<Tuple<int, int>> Read(IEnumerable<string> lines)
        {
            List<Tuple<int, int>> temp = new List<Tuple<int, int>>();

            foreach(var line in lines)
            {
                var ln = line.Split(' ');
                int a = Convert.ToInt32(ln[0]);
                int b = Convert.ToInt32(ln[1]);

                temp.Add(Tuple.Create<int,int>(a,b));
            }

            return temp;
        }
    
        internal static decimal CalculateJaccardSimilarity()
        {
            decimal temp = 0;

            decimal TP = OrchestratedData.Values.Where(x=>x.TP==1).Count();
            decimal FP = OrchestratedData.Values.Where(x=>x.FP==1).Count();
            decimal FN = OrchestratedData.Values.Where(x=>x.FN==1).Count();

            temp = TP == 0? 0 : TP / (TP+FP+FN);

            return Math.Round(temp,3);
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
            loader.LoadDataFromFile(@"testdata\testfile1.txt");
            loader.OrchestradeDataToCluster();
            var resJaccard = loader.CalculateJaccardSimilarity();

            Console.WriteLine($"{resJaccard}");
            Console.ReadKey();
        }
    }
}
