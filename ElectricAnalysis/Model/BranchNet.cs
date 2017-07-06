using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    /// <summary>
    /// 支路网络
    /// </summary>
    class BranchNet
    {
        #region Construction
        public BranchNet(int netNum)
        {
            this.Num = netNum;
            branches = new List<TestBranch>();
            logicNodes = null;
        }
        #endregion

        #region Field
        protected List<TestBranch> branches;//网络支路
        private List<TNode> logicNodes;//网络中的逻辑节点
        #endregion

        #region Property
        public int Num { get; set; }//网络号
        internal List<TestBranch> Branches
        {
            get { return branches; }
            set { branches = value; }
        }
        public ReadOnlyCollection<TNode> LogicNodes
        {
            get
            {
                if (logicNodes == null)
                {
                    logicNodes = new List<TNode>();
                    foreach (var br in branches)
                    {
                        var query = br.Branch.Where(p =>
                            (p.TNType == TerminalType.ContactNormalOpen)
                            || (p.TNType == TerminalType.ContactNormalClose)
                            || (p.TNType == TerminalType.ContactCom)
                            || (p.TNType == TerminalType.Coil));
                        foreach (var node in query)
                        {
                            if (!logicNodes.Contains(node, AppProject.cmpNode))
                                logicNodes.Add(node);
                        }
                    }
                }
                return logicNodes.AsReadOnly();
            }
        }
        #endregion

        #region Function

        #endregion
    }
}
