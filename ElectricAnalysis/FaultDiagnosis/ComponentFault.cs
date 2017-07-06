using ElectricAnalysis.Model;
using ElectricAnalysis.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.FaultDiagnosis
{
    enum FaultType
    {
        ShortCircuit,
        BlockCircuit
    }
    class ComponentFault
    {
        #region Field/Property
        private string faultName;//故障名
        public string FaultName
        {
            get { return faultName; }
        }
        private Tuple<TNode, TNode> nodes;//故障节点
        public Tuple<TNode, TNode> Nodes
        {
            get { return nodes; }
        }

        private FaultType type;//故障类型
        internal FaultType Type
        {
            get { return type; }
        }
        private BitArray bits;//故障情况下的CF输出
        public BitArray Bits
        {
            get { return bits; }
        }
        #endregion

        #region Construction
        public ComponentFault(ComponentFault fault)
        {
            this.faultName = fault.faultName;
            this.nodes = fault.nodes;
            this.type = fault.type;
        }
        public ComponentFault(string faultName, Tuple<TNode, TNode> nodes, FaultType type)
        {
            this.faultName = faultName;
            this.nodes = nodes;
            this.type = type;
        }
        #endregion

        #region Function
        /// <summary>
        /// 计算故障模式对应的特征向量
        /// </summary>
        public void setVector(Dijkstra dist, List<TNode> cfNodes)
        {
            bits = new BitArray(cfNodes.Count);
            for (int i=0;i<cfNodes.Count;i++)
            {
                bits.Set(i, dist.getRouteWeight(cfNodes[i].index) < Dijkstra.infinite);
            }
        }
        #endregion
    }
}
