using System;
using System.Collections.Generic;
using System.Text;

namespace SA
{
   
        public class SimulatedAnnealing_
        {

            public int[] SAFindAssingment(int[,] costs)
            {
                double T = 100;
                double t = 0.01;
                double alpha = 0.90;
                int looptimes = 1000;


                int NumP = costs.GetLength(0);
                int NumV = costs.GetLength(1);
                int dimension;
                if (NumP >= NumV)
                    dimension = NumP;
                else
                    dimension = NumV;
                costs = ExtentionArray(costs);
                Random r = new Random();
                var start = THERandom(NumV, NumP);
                var current_solution = start;
                var current_cost = Caculatecost(current_solution, costs);
                int[] new_solution = new int[dimension];
                for (int i = 0; i < current_solution.Length; i++)
                    new_solution[i] = current_solution[i];


                while (T > t)
                {
                    for (int j = 0; j < looptimes; j++)
                    {
                        int[] k = new int[2];
                        while (true)
                        {

                            for (int i = 0; i < 2; i++)
                                k[i] = r.Next(0, dimension);
                            if (k[0] != k[1])
                                break;
                        }

                        var temp = new_solution[k[0]];
                        new_solution[k[0]] = new_solution[k[1]];
                        new_solution[k[1]] = temp;

                        var new_cost = Caculatecost(new_solution, costs);
                        if (new_cost < current_cost)
                        {
                            current_cost = new_cost;
                            for (int i = 0; i < current_solution.Length; i++)
                                current_solution[i] = new_solution[i];
                        }

                        else
                        {
                            var delta = new_cost - current_cost;
                            double probabiity = Math.Exp(-(delta / T));
                            double lambda = ((double)(r.Next(0, 100000)) / 100000);
                            if (probabiity > lambda)
                            {
                                current_cost = new_cost;
                                for (int i = 0; i < current_solution.Length; i++)
                                    current_solution[i] = new_solution[i];

                            }
                            else
                            {

                                for (int i = 0; i < current_solution.Length; i++)
                                    new_solution[i] = current_solution[i];
                            }
                        }

                    }
                    T = T * alpha;

                }

                
                return current_solution;







            }

            public int[] THERandom(int V, int P)
            {
                int d;
                if (V >= P)
                    d = V;
                else
                    d = P;


                int[] index = new int[d];
                for (int i = 0; i < d; i++)
                    index[i] = i;
                Random r = new Random();
                //用来保存随机生成的不重复的10个数 
                int[] result = new int[d];
                int site = d;//设置上限 
                int id;
                for (int j = 0; j < d; j++)
                {
                    id = r.Next(0, site - 1);
                    //在随机位置取出一个数，保存到结果数组 
                    result[j] = index[id];
                    //最后一个数复制到当前位置 
                    index[id] = index[site - 1];
                    //位置的上限减少一 
                    site--;
                }
                return result;
            }

            public int Caculatecost(int[] current_solution, int[,] cost)
            {
                int allcost = 0;
                for (int i = 0; i < current_solution.Length; i++)
                {
                    allcost += cost[i, current_solution[i]];
                }
                return allcost;
            }

            public int[,] ExtentionArray(int[,] costs)
            {
                var rows = costs.GetLength(0);
                var cols = costs.GetLength(1);
                int[,] square;

                int loops = 0;
                int i;
                int j;
                if (rows == cols)
                    return costs;

                else
                {

                    loops = rows > cols ? rows : cols;
                    square = new int[loops, loops];
                    if (rows > cols)
                    {
                        for (i = 0; i < loops; i++)
                        {
                            for (j = 0; j < cols; j++)
                            {
                                square[i, j] = costs[i, j];
                            }
                        }
                        for (i = 0; i < loops; i++)
                        {
                            for (j = cols; j < loops; j++)
                            {
                                square[i, j] = 200000;
                            }
                        }
                    }
                    else
                    {
                        for (i = 0; i < rows; i++)
                        {
                            for (j = 0; j < cols; j++)
                            {
                                square[i, j] = costs[i, j];
                            }
                        }
                        for (i = rows; i < loops; i++)
                        {
                            for (j = 0; j < loops; j++)
                            {
                                square[i, j] = 200000;
                            }
                        }
                    }

                }

                return square;
            }

        }
    }

