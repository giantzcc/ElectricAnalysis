using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    class NormalTestNet : TestNetBase, IConvertToTable
    {
        #region Field Property
        private string name = "普通导通测试";
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
        /// 普通导通测试支路
        /// </summary>
        public LogicNet[] Load24VBranches(int logicMaxNetNum)
        {
            AppProject pro = AppProject.GetInstance();
            nets.Clear();
            List<TNode> cfs = pro.GetCFNodes();
            int Count = cfs.Count;
            BranchNet[] _nets = new BranchNet[Count];
            LogicNet[] pnets = new LogicNet[Count];
            for (int i = 0; i < Count; i++)
            {
                BranchNet net = new BranchNet(MaxNetNum + 1);
                this.nets.Add(net);
                LogicNet pnet = new LogicNet(++logicMaxNetNum);
                List<TestBranch> normbr = new List<TestBranch>();
                TestBranch branch = new TestBranch();
                branch.TryAddNode(cfs[i]);
                normbr.Add(branch);
                Get24VBranches(normbr, ref net, ref pnet);
                _nets[i] = net;
                pnets[i] = pnet;
            }
            pro.Nodes.ForEach(p => p.HasIncluded = false);
            return pnets;
        }
        /// <summary>
        /// 普通导通测试支路(递归广度)
        /// </summary>
        private void Get24VBranches(List<TestBranch> branches, ref BranchNet net, ref LogicNet logicNet)
        {
            AppProject pro = AppProject.GetInstance();
            List<TestBranch> nextBranches = new List<TestBranch>();
            foreach (var branch in branches)
            {
                TNode node = branch.LastNode;
                if (node.HasIncluded)
                    continue;
                foreach (var child in node.Nodes)
                {
                    TestBranch cbranch = branch.Clone();
                    if (!child.HasIncluded)
                    {
                        if ((child.PartType == "端子排" && child.TNType != TerminalType.Block)
                            || child.TNType == TerminalType.RE
                            || child.TNType == TerminalType.DiodePositive
                            || child.TNType == TerminalType.DiodeNegative
                            || child.TNType == TerminalType.Coil
                            || child.TNType == TerminalType.ContactNormalClose
                            || child.TNType == TerminalType.ContactNormalOpen
                            || child.TNType == TerminalType.ContactCom
                            || child.TNType == TerminalType.Switch
                            || child.TNType == TerminalType.Indicator
                            || child.TNType == TerminalType.BreakerContact)
                        {
                            bool success = cbranch.TryAddNode(child);
                            if (success)
                                nextBranches.Add(cbranch);
                        }
                        else if (child.PartType == "接口连接器" && child.TNType != TerminalType.Block)
                        {
                            bool success = cbranch.TryAddNode(child);
                            if (cbranch.HasBreaker || cbranch.HasLogicElement)
                            {
                                if (success)
                                    logicNet.Branches.Add(cbranch);
                            }
                            else
                            {
                                if (success)
                                    net.Branches.Add(cbranch);
                            }
                            cbranch = new TestBranch();
                            success = cbranch.TryAddNode(child);
                            if (success)
                                nextBranches.Add(cbranch);
                        }
                    }
                }
                node.HasIncluded = true;
            }
            if (nextBranches.Count != 0)
                Get24VBranches(nextBranches, ref net, ref logicNet);
        }
        #endregion
    }
}
