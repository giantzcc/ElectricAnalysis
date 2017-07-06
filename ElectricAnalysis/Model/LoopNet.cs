using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    class LoopNet : TestNetBase, IConvertToTable
    {
        #region Field Property
        private string name = "测试回路";
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
        /// 获得包含逻辑元件的回路
        /// </summary>
        public void LoadLogicLoops()
        {
            AppProject pro = AppProject.GetInstance();
            nets.Clear();
            /*获得与CF点相连接、包含逻辑元件的回路*/
            var cfs = pro.Nodes.Where(p =>
                p.PartType == "接口连接器"
                && p.TNType == TerminalType.Normal
                && !pro.GndTbs.Contains(p, AppProject.cmpNode)
                ).ToList();
            int Count = cfs.Count;
            for (int i = 0; i < Count; i++)
            {
                pro.Nodes.ForEach(p => p.HasIncluded = false);
                int netnum = MaxNetNum;
                BranchNet net = new BranchNet(++netnum);
                nets.Add(net);
                List<TestBranch> loopbr = new List<TestBranch>();
                TestBranch branch = new TestBranch();
                branch.TryAddNode(cfs[i]);
                loopbr.Add(branch);
                GetLogicRelations(loopbr, ref net);
            }
            /*获得与总正相连接、包含逻辑器件的回路*/
            var vccs = pro.GetSetTerminal(p => p.Type == NamesManage.Positive);
            pro.Nodes.ForEach(p => p.HasIncluded = false);
            BranchNet vnet = new BranchNet(1 + MaxNetNum);
            nets.Add(vnet);
            for (int i = 0; i < vccs.Count; i++)
            {
                List<TestBranch> loopbr = new List<TestBranch>();
                TestBranch branch = new TestBranch();
                branch.TryAddNode(vccs[i]);
                loopbr.Add(branch);
                GetLogicRelations(loopbr, ref vnet);
            }
        }
        /// <summary>
        /// 获得与CF点相连接、包含逻辑元件的回路
        /// </summary>
        private void GetLogicRelations(List<TestBranch> branches, ref BranchNet net)
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
                        if (child.TNType != TerminalType.Block
                            && child.PartType != "设备连接器"
                            && !pro.GndTbs.Contains(child, AppProject.cmpNode))
                        {
                            bool success = cbranch.TryAddNode(child);
                            if (success)
                                nextBranches.Add(cbranch);
                        }
                        else if (pro.GndTbs.Contains(child, AppProject.cmpNode) && cbranch.HasLogicElement)
                        {
                            cbranch.TryAddNode(child);
                            bool condict = cbranch.BranchContradict();
                            if (!condict)
                                net.Branches.Add(cbranch);
                        }
                    }
                }
                node.HasIncluded = true;
            }
            if (nextBranches.Count != 0)
                GetLogicRelations(nextBranches, ref net);
        }
        #endregion
    }
}
