using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Util
{
    public class Dijkstra
    {
        public const int infinite = 65536;
        private int[,] weight;
        private int[] dist;
        private int[] route;
        private bool[] mark;
        private int src;
        public Dijkstra(int[,] weight, int src)
        {
            this.weight = weight;
            this.src = src;
            this.dist = new int[weight.GetLength(0)];
            this.route = new int[weight.GetLength(0)];
            this.mark = new bool[weight.GetLength(0)];
            init();
            caculate(src);
        }

        private void init()
        {
            for (int i = 0; i < dist.Length; i++)
            {
                dist[i] = weight[src,i];
                route[i] = -1;
                mark[i] = false;
            }
        }

        private void caculate(int start)
        {
            if (!mark[start])
            {
                int min = infinite; int index = -1;
                mark[start] = true;
                for (int i = 0; i < dist.Length; i++)
                {
                    if (!mark[i])
                    {
                        int len = dist[start] + weight[start,i];
                        if (len <= dist[i])
                        {
                            dist[i] = len;
                            route[i] = start;
                        }
                        if(min>dist[i])
                        {
                            min = dist[i];
                            index = i;
                        }
                    }
                }
                if (min < infinite)
                    caculate(index);
            }
        }

        private void getPath(List<int> path, int dest)
        {
            if (dest != src)
            {
                if (dest == -1)
                    return;
                getPath(path, route[dest]);
                path.Add(dest);
            }
            else
                path.Add(src);
        }

        public List<int> getPath(int dest)
        {
            List<int> paths = new List<int>();
            getPath(paths, dest);
            return paths;
        }

        public int getRouteWeight(int dest)
        {
            return dist[dest];
        }
    }
}
