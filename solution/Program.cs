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

        private class GroungTruthCluster
        {
            public int GTclusterid {get; set;}
            public double correctLabelCount {get; set;}
        }
        private class IdentifiedCluster
        {
            public int ClusterID;
            public List<GroungTruthCluster> GroundTruthCluster = new List<GroungTruthCluster>();
        }

        static Dictionary<int,Tuple<int,int>> RawData = new Dictionary<int, Tuple<int,int>>();
        static Dictionary<string,confusionMatrix> OrchestratedData_Jaccard = new Dictionary<string, confusionMatrix>();
        static Dictionary<int,IdentifiedCluster> OrchestratedData_NMI = new Dictionary<int, IdentifiedCluster>();

        internal static void LoadDataFromFile(string path)
        {
            var lines = System.IO.File.ReadAllLines(path);

            var t = Read(lines);

            int point = 0;
            t.ForEach(x=>RawData.Add(point++, Tuple.Create(x.Item1,x.Item2)));
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

            int point = 0;

            var t = Read(lines);
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

                    OrchestratedData_Jaccard.Add($"{points[i]}-{points[j]}",new confusionMatrix{TP=tp,FN=fn,FP=fp});
                }
            }
        }

        internal static void OrchestrateDataToIdentifiedLabels()
        {
            var groundTruthClusters = RawData.Values.Select(x=>x.Item1).Distinct();
            var identifiedClusters = RawData.Values.Select(x=>x.Item2).Distinct();

            foreach(int idc in identifiedClusters)
            {
                IdentifiedCluster i = new IdentifiedCluster();
                i.ClusterID = idc;

                foreach(int gt in groundTruthClusters)
                {
                    var pointsInidc = RawData.Where(x=>x.Value.Item2==idc).Select(p=>p.Key);
                    var pointsInigt = RawData.Where(x=>x.Value.Item1==gt).Select(p=>p.Key);

                    var common = pointsInidc.Intersect(pointsInigt).Count();

                    GroungTruthCluster GT = new GroungTruthCluster
                    {
                        GTclusterid=gt,
                        correctLabelCount=common
                    };

                    i.GroundTruthCluster.Add(GT);
                }
                OrchestratedData_NMI.Add(idc,i);
            }

            var totalPoints = OrchestratedData_NMI.Values.Sum(x=>x.GroundTruthCluster.Sum(p=>p.correctLabelCount));


            //Caclulate probability
            foreach(var idc in OrchestratedData_NMI)
            {
                var gts = idc.Value.GroundTruthCluster;

                foreach(var gt in gts)
                {
                    gt.correctLabelCount=gt.correctLabelCount/totalPoints;
                }
            }

        }


        private static List<Tuple<int,int>> Read(string[] lines)
        {
            List<Tuple<int,int>> temp = new List<Tuple<int,int>>();
            string l = string.Empty;
            foreach(string line in lines)
            {
                try
                {
                l = line.Trim();

                if(l.Length>0)
                {
                    string[] ln = l.Split(' ');
                    int a = Convert.ToInt32(ln[0].Trim().Replace(" ",""));
                    int b = Convert.ToInt32(ln[1].Trim().Replace(" ",""));
                    
                    temp.Add(Tuple.Create(a,b));
                }
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Error : {e.InnerException}. Line = {l}");
                }

            }
            return temp;
        }
    
        internal static decimal CalculateJaccardSimilarity()
        {
            decimal temp = 0;

            decimal TP = OrchestratedData_Jaccard.Values.Where(x=>x.TP==1).Count();
            decimal FP = OrchestratedData_Jaccard.Values.Where(x=>x.FP==1).Count();
            decimal FN = OrchestratedData_Jaccard.Values.Where(x=>x.FN==1).Count();

            temp = TP == 0? 0 : TP / (TP+FP+FN);

            return Math.Round(temp,3);
        }

        internal static double CalculateNormalizedMutualInformation()
        {
            //Row Level Sum
            Dictionary<int,double> sumByCluster = new Dictionary<int, double>();
            Dictionary<int,double> sumByGT = new Dictionary<int, double>();

          
            foreach(var kvp in OrchestratedData_NMI)
            {
                sumByCluster.Add(kvp.Key, kvp.Value.GroundTruthCluster.Sum(x=>x.correctLabelCount));

                foreach(var gt in kvp.Value.GroundTruthCluster)
                {
                    var id = gt.GTclusterid;

                    if(sumByGT.ContainsKey(id))
                    {
                        sumByGT[id] += gt.correctLabelCount;
                    }
                    else
                    {
                        sumByGT.Add(id,gt.correctLabelCount);
                    }
                }
            }

            double IYC = 0;
            foreach(var idc in OrchestratedData_NMI)
            {
                foreach(var gt in idc.Value.GroundTruthCluster)
                {
                    if(gt.correctLabelCount>0)
                    {
                        double lg = Math.Log10((double)gt.correctLabelCount / ((double)sumByCluster[idc.Key] * (double)sumByGT[gt.GTclusterid]));
                        IYC += gt.correctLabelCount * lg;
                    }
                }
            }
            
            double HC = 0.0;

            foreach(var kvp in sumByCluster)
            {
                var v = (double)kvp.Value;

                HC += v * Math.Log10(v) * -1;
            }

            double HY =0;

            foreach(var kvp in sumByGT)
            {
                var v = (double) kvp.Value;

                HY += v * Math.Log10(v) * -1;
            }

            return Math.Round(IYC/Math.Sqrt(HC*HY),3);
        }
    }

    class Solution
    {
        static void Main(string[] args)
        {
            loader.LoadDataFromFile(@"testdata\testfile0.txt");
            //loader.LoadDataFromSTDIN();
            
            loader.OrchestradeDataToCluster();
            loader.OrchestrateDataToIdentifiedLabels();

            var resJaccard = loader.CalculateJaccardSimilarity();
            var resNMI = loader.CalculateNormalizedMutualInformation();

            Console.WriteLine($"{resNMI,0:N3} {resJaccard,0:N3}");
        }
    }
}
