using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Traffic
{
    class Floyd
    {
        public List<Tuple<int,int,double>> Edges { get; private set; }
        public double[,] Adjacency { get; private set; }
        public int[,] Prenode { get; private set; }
        public int Nnode { get; private set; }

        public double[,] Splength { get; private set; }
        public int[,] SpPrenode { get; private set; }
        
        public Floyd(List<Tuple<int,int,double>> edges)
        {
            Edges = edges;
            Nnode = Edges.Select(edge => edge.Item1).Concat(Edges.Select(edge => edge.Item2)).Distinct().Max() + 1;
            Adjacency = new double[Nnode, Nnode];
            Prenode = new int[Nnode, Nnode];
            for (int i = 0; i < Nnode; i++)
                for (int j = 0; j < Nnode; j++)
                    if (i == j)
                    { Adjacency[i, j] = 0; Prenode[i, j] = i; }
                    else
                    { Adjacency[i, j] = double.PositiveInfinity; Prenode[i, j] = -1; }

            foreach (var edge in Edges)
            {
                Adjacency[edge.Item1, edge.Item2] = edge.Item3;
                Prenode[edge.Item1, edge.Item2] = edge.Item1;
            }

            Splength = Adjacency;
            SpPrenode = Prenode;
            for (int k = 0; k < Nnode; k++)
                for (int i = 0; i < Nnode; i++)
                    for (int j = 0; j < Nnode; j++)
                        if (Splength[i, j] > Splength[i, k] + Splength[k, j])
                        {
                            Splength[i, j] = Splength[i, k] + Splength[k, j];
                            SpPrenode[i, j] = SpPrenode[k, j];
                        }
        }

        public List<int> ShortestPath(int source,int sink)
        {
            List<int> path = new List<int> { sink };
            while(SpPrenode[source,sink]!=source)
            {
                path.Add(SpPrenode[source, sink]);
                sink = SpPrenode[source, sink];
            }
            path.Add(source);
            path.Reverse();
            return path;
        }
    }


    class Executor
    {
        static void Main()
        {
            List<Tuple<int, int, double>> edges = new List<Tuple<int, int, double>>
            {
                new Tuple<int,int,double>(0,1,5),
                new Tuple<int, int, double>(0,2,12),
                new Tuple<int,int,double>(1,0,10),
                new Tuple<int, int, double>(1,2,5),
                new Tuple<int, int, double>(2,3,3),
                new Tuple<int, int, double>(3,1,5),
                new Tuple<int, int, double>(3,2,12),
            };

            Floyd Floyd = new Floyd(edges);
            for (int i = 0; i < Floyd.Nnode; i++)
            {
                for (int j = 0; j < Floyd.Nnode; j++) Console.Write("{0},{1}\t", Floyd.Splength[i, j], Floyd.Prenode[i, j]);
                Console.WriteLine();
            }
            foreach(var v in Floyd.ShortestPath(3, 0))Console.Write("{0}\t",v);Console.WriteLine(Floyd.Splength[3, 0]);
            Console.ReadKey();

        }
    }
}
