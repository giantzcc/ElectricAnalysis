using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    class LogicTestNet : TestNetBase, IConvertToTable
    {
        #region Field Property
        private string name = "逻辑测试";
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
        /// 分解初步理出来的逻辑网络
        /// </summary>
        public void SplitLogicBranches(LoopNet lnet)
        {
            var logicnets = new List<LogicNet>();
            nets.ForEach(p => logicnets.Add((LogicNet)p));
            foreach (var net in logicnets)
            {
                nets.Remove(net);
                var childnets = (net as LogicNet).Split(MaxNetNum);
                foreach (var child in childnets)
                {
                    bool norCondict = child.Branches.All(p => !p.BranchContradict());
                    if (norCondict)
                    {
                        nets.Add(child);
                        /*分析逻辑导通条件*/
                        child.GetConductCondition();
                        lnet.Nets.ForEach(p => p.Branches.ForEach(q => q.hasBlock = false));
                    }
                }
            }
        }

        public int getCanTestNetCount()
        {
            return nets.Count(p => (p as LogicNet).CanTest);
        }
        #endregion
    }
}
