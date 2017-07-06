using ElectricAnalysis.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    /// <summary>
    /// 网络类型
    /// </summary>
    /*enum NetType
    {
        Gnd,
        Vcc,
        Normal,
        Logic,
        Loop
    }*/
    /// <summary>
    /// 测试支路
    /// </summary>
    public class TestBranch
    {
        #region Construction
        public TestBranch()
        {
            branch = new List<TNode>();
            cmp = AppProject.cmpNode;
            logicNodes = null;
            hasBlock = false;
        }
        #endregion

        #region Field
        private List<TNode> branch;
        private CompareTNode cmp;
        private ReadOnlyCollection<TNode> logicNodes;//网络中的逻辑节点
        #endregion

        #region Property
        public int Count//支路长度
        {
            get
            {
                return branch.Count;
            }
        }
        public List<TNode> Branch//支路中的节点
        {
            get
            {
                return branch;
            }
        }
        public TNode LastNode//末节点
        {
            get
            {
                return branch.Last();
            }
        }
        public bool AllTB//是否支路中全是端子排
        {
            get
            {
                bool alltb = branch.All(p => p.PartType == "端子排");
                return alltb;
            }
        }
        public bool HasLogicElement//支路中是否含有逻辑元件
        {
            get
            {
                var logicnd = branch.Where(p => (p.PartType == "继电器" || p.PartType == "接触器") && p.TNType != TerminalType.RE && p.TNType != TerminalType.Block).ToList();
                for (int i = 0; i < logicnd.Count; i++)
                {
                    for (int j = 0; j < logicnd.Count; j++)
                    {
                        if (i != j)
                        {
                            if (logicnd[i].Part == logicnd[j].Part)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }
        public bool HasBreaker//支路中是否含有断路器
        {
            get
            {
                var logicnd = branch.Where(p => (p.PartType == "断路器") && p.TNType != TerminalType.RE && p.TNType != TerminalType.Block).ToList();
                for (int i = 0; i < logicnd.Count; i++)
                {
                    for (int j = 0; j < logicnd.Count; j++)
                    {
                        if (i != j)
                        {
                            if (logicnd[i].Part == logicnd[j].Part && logicnd[i].TNType == logicnd[j].TNType)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }
        public ReadOnlyCollection<TNode> LogicNodes
        {
            get
            {
                if (logicNodes == null)
                {
                    logicNodes = branch.Where(p =>
                            (p.TNType == TerminalType.ContactNormalOpen)
                            || (p.TNType == TerminalType.ContactNormalClose)
                            || (p.TNType == TerminalType.ContactCom)
                            || (p.TNType == TerminalType.Coil)).ToList().AsReadOnly();
                }
                return logicNodes;
            }
        }
        public bool hasBlock { get; set; }//是否被屏蔽
        #endregion

        #region Function
        /// <summary>
        /// 为支路添加下一个节点
        /// </summary>
        public bool TryAddNode(TNode node)
        {
            if (!branch.Contains(node))
            {
                branch.Add(node);
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 清空支路节点
        /// </summary>
        public void Clear()
        {
            branch.Clear();
        }
        /// <summary>
        /// 浅度复制一个测试支路
        /// </summary>
        public TestBranch Clone()
        {
            TestBranch br = new TestBranch();
            foreach (var node in branch)
            {
                br.TryAddNode(node);
            }
            return br;
        }
        /// <summary>
        /// 测试一条支路中的逻辑是否矛盾
        /// </summary>
        public bool BranchContradict()
        {
            var parts = LogicNodes.Select(p => p.Part).Distinct();
            foreach (var name in parts)
            {
                var noNd = LogicNodes.FirstOrDefault(p => p.Part == name && p.TNType == TerminalType.ContactNormalOpen);
                var ncNd = LogicNodes.FirstOrDefault(p => p.Part == name && p.TNType == TerminalType.ContactNormalClose);
                TNode noCom = null;
                TNode ncCom = null;
                if (noNd != null)
                    noCom = noNd.GetAnother();
                if (ncNd != null)
                    ncCom = ncNd.GetAnother();
                int coilNum = LogicNodes.Count(p => p.Part == name && p.TNType == TerminalType.Coil);
                if (noCom != null
                    && LogicNodes.Contains(noCom)
                    && ncCom != null
                    && LogicNodes.Contains(ncCom))
                {
                    if (LogicNodes.Contains(noNd.GetAnother()) && LogicNodes.Contains(ncNd.GetAnother()))
                        return true;
                }
                if (coilNum == 2 && ncCom != null)
                {
                    if (LogicNodes.Contains(ncNd.GetAnother()))
                        return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 测试该支路加正电压会不会短路
        /// </summary>
        public bool hasShortCircuit()
        {
            AppProject pro = AppProject.GetInstance();
            if (branch.Count != 0)
            {
                Dijkstra dist = pro.getDijkstra(branch[0]);
                List<TNode> negative = pro.GetSetTerminal(p => p.Type == NamesManage.Negative);
                foreach(TNode nd in negative){
                    if (dist.getRouteWeight(nd.index) == 0)
                        return true;
                }
            }
            return false;
        }
        #endregion
    }
}
