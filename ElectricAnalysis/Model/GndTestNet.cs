using ElectricAnalysis.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    class GndTestNet : TestNetBase, IConvertToTable
    {
        #region Field Property
        private string name = "地线导通测试";
        #endregion

        #region Function
        /// <summary>
        /// 将数据转换成表
        /// </summary>
        public DataView getTable()
        {
            return getTable(name);
        }
        /// <summary>
        /// 保存测试网络到excel
        /// </summary>
        public DataView SaveBranchesToExcel()
        {
            return SaveBranchesToExcel(name);
        }
        /// <summary>
        /// 获得与总负相连接的支路
        /// </summary>
        public void LoadGndBranches()
        {
            AppProject pro = AppProject.GetInstance();
            nets.Clear();
            BranchNet net = new BranchNet(MaxNetNum + 1);
            List<TNode> gndnd = pro.GetSetTerminal(p => p.Type == NamesManage.Negative);
            List<TestBranch> gndbr = new List<TestBranch>();
            gndnd.ForEach(p =>
            {
                TestBranch br = new TestBranch();
                br.TryAddNode(p);
                gndbr.Add(br);
            });
            GetGndBranches(gndbr, ref net);
            //GetGndBranches(gndnd, ref net);
            nets.Add(net);
            pro.Nodes.ForEach(p => p.HasIncluded = false);
        }
        /// <summary>
        /// 获得与总负相连接的支路(Dijkstra)
        /// </summary>
        public void GetGndBranches(List<TNode> sources, ref BranchNet net)
        {
            AppProject pro = AppProject.GetInstance();
            IReadOnlyCollection<TNode> cfs = pro.CFNodes;
            foreach (var nd in sources)
            {
                Dijkstra dist = pro.getDijkstra(nd);
                foreach (var cf in cfs)
                {
                    if (dist.getRouteWeight(cf.index) == 0)
                    {
                        List<int> paths = dist.getPath(cf.index);
                        TestBranch cbranch = new TestBranch();
                        if (paths.Count>0)
                            cbranch.TryAddNode(pro.Nodes[paths[paths.Count-1]]);
                        net.Branches.Add(cbranch);
                    }
                }
            }
        }
        /// <summary>
        /// 获得与总负相连接的支路(递归广度)
        /// </summary>
        private void GetGndBranches(List<TestBranch> branches, ref BranchNet net)
        {
            AppProject pro = AppProject.GetInstance();
            List<TestBranch> nextBranches = new List<TestBranch>();
            foreach (var branch in branches)
            {
                TNode node = branch.LastNode;
                foreach (var child in node.Nodes)
                {
                    TestBranch cbranch = branch.Clone();
                    if (!child.HasIncluded)
                    {
                        if (child.PartType == "端子排"
                            && child.TNType != TerminalType.Block
                            && !(node.TNType == TerminalType.DiodePositive && child.TNType == TerminalType.DiodeNegative))
                        {
                            if (!pro.GndTbs.Contains(child, AppProject.cmpNode))
                                pro.GndTbs.Add(child);
                            if (cbranch.AllTB)
                            {
                                cbranch.Clear();
                            }
                            cbranch.TryAddNode(child);
                            nextBranches.Add(cbranch);
                        }
                        else if (child.TNType == TerminalType.RE)
                        {
                            if (!pro.GndTbs.Contains(child, AppProject.cmpNode))
                                pro.GndTbs.Add(child);
                            cbranch.TryAddNode(child);
                            nextBranches.Add(cbranch);
                        }
                        else if (child.PartType == "接口连接器")
                        {
                            if (!pro.GndTbs.Contains(child, AppProject.cmpNode))
                                pro.GndTbs.Add(child);
                            cbranch.TryAddNode(child);
                            net.Branches.Add(cbranch);
                            cbranch = new TestBranch();
                            cbranch.TryAddNode(child);
                            nextBranches.Add(cbranch);
                        }
                    }
                }
                node.HasIncluded = true;
            }
            if (nextBranches.Count != 0)
                GetGndBranches(nextBranches, ref net);
        }
        #endregion
    }
}
