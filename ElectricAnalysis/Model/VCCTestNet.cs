using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    class VCCTestNet : TestNetBase, IConvertToTable
    {
        #region Field Property
        private string name = "110V导通测试";
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
        /// 获得与总正相连接的支路
        /// </summary>
        public LogicNet Load110VBranches(int logicMaxNetNum)
        {
            AppProject pro = AppProject.GetInstance();
            nets.Clear();
            BranchNet net = new BranchNet(MaxNetNum + 1);
            nets.Add(net);
            LogicNet pnet = new LogicNet(logicMaxNetNum + 1);
            List<TNode> vccnd = pro.GetSetTerminal(p => p.Type == NamesManage.Positive);
            List<TestBranch> vccbr = new List<TestBranch>();
            vccnd.ForEach(p =>
            {
                TestBranch br = new TestBranch();
                br.TryAddNode(p);
                vccbr.Add(br);
            });
            Get110VBranches(vccbr, ref net, ref pnet);
            pro.Nodes.ForEach(p => p.HasIncluded = false);
            return pnet;
        }
        /// <summary>
        /// 获得与总正相连接的支路(递归广度)
        /// </summary>
        private void Get110VBranches(List<TestBranch> branches, ref BranchNet net, ref LogicNet positiveNet)
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
                        if (child.PartType == "端子排" && child.TNType != TerminalType.Block)
                        {
                            if (cbranch.AllTB)
                            {
                                cbranch.Clear();
                            }
                            bool success = cbranch.TryAddNode(child);
                            if (success)
                                nextBranches.Add(cbranch);
                        }
                        else if (child.TNType == TerminalType.RE
                            || child.TNType == TerminalType.BreakerContact
                            || child.TNType == TerminalType.DiodePositive
                            || child.TNType == TerminalType.DiodeNegative
                            || child.TNType == TerminalType.Coil
                            || child.TNType == TerminalType.ContactNormalClose
                            || child.TNType == TerminalType.ContactNormalOpen
                            || child.TNType == TerminalType.ContactCom
                            || child.TNType == TerminalType.Switch
                            || child.TNType == TerminalType.Indicator)
                        {
                            bool success = cbranch.TryAddNode(child);
                            if (success)
                                nextBranches.Add(cbranch);
                        }
                        else if (child.PartType == "接口连接器")
                        {
                            bool success = cbranch.TryAddNode(child);
                            /*是否含有逻辑元件*/
                            if (cbranch.HasLogicElement)
                            {
                                if (success)
                                    positiveNet.Branches.Add(cbranch);
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
                Get110VBranches(nextBranches, ref net, ref positiveNet);
        }
        #endregion
    }
}
