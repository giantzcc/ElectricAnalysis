using ElectricAnalysis.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Util
{
    class DijkstraUtil
    {
        /// <summary>
        /// 判断是否有短路的情况
        /// </summary>
        /// <param name="negative">目标节点</param>
        /// <param name="dist">Dijkstra算法类</param>
        /// <returns></returns>
        public static bool hasShortCircuit(List<TNode> negative, Dijkstra dist)
        {
            foreach (TNode nd in negative)
            {
                if (dist.getRouteWeight(nd.index) == 0)
                {
                    Console.WriteLine("短路权重为：" + dist.getRouteWeight(nd.index));
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断是否有断路的情况
        /// </summary>
        /// <param name="negative">目标节点</param>
        /// <param name="dist">Dijkstra算法类</param>
        /// <returns></returns>
        public static bool isBlockCircuit(List<TNode> negative, Dijkstra dist)
        {
            foreach (TNode nd in negative)
            {
                if (dist.getRouteWeight(nd.index) < Dijkstra.infinite)
                    return false;
            }
            return true;
        }
    }
}
