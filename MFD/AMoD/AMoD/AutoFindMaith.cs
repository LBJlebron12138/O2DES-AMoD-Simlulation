using System;
using System.Collections.Generic;
using System.Text;
using HungarianAlgorithm;

namespace O2DESNet.AMoD
{
    public static class Hutest
    {
        public static int[] AutoFinfAssingment(this int[,] costs)
        {
            var rows = costs.GetLength(0);
            var cols = costs.GetLength(1);
            int[,] square;
            int[] result;
            int loops = 0;
            int i;
            int j;
            if (rows == cols)
                result = costs.FindAssignments();
            else
            {

                loops = rows > cols ? rows : cols;
                square = new int[loops, loops];
                if(rows>cols)
                {
                    for(i=0;i<loops;i++)
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
                            square[i, j] =0;
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
                            square[i, j] = 0;
                        }
                    }
                }
                result = square.FindAssignments();
            }

            return result;
        }
    }
}

