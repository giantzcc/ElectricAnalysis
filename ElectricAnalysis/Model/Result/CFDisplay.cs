using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model.Result
{
    public class CFDisplay
    {
        #region Construction
        public CFDisplay()
        {
            cfs = new Dictionary<TNode, CFPair>();
            AppProject app = AppProject.GetInstance();
            foreach(TNode node in app.CFNodes)
            {
                cfs[node] = new CFPair(node);
            }
        }
        #endregion

        #region Field
        private IDictionary<TNode, CFPair> cfs;
        public IEnumerable<CFPair> CFs
        {
            get
            {
                return cfs.Values.OrderBy(p=>p.Part);
            }
        } 
        #endregion

        #region Function
        /// <summary>
        /// 更新输出点的值
        /// </summary>
        /// <param name="nodes">高电平的cf点</param>
        /// <param name="source">指定是更新哪一个数据源</param>
        public void updateCfPairs(ICollection<TNode> nodes, SourceType source)
        {
            if (source == SourceType.Source1)
            {
                foreach (CFPair pair in cfs.Values)
                    pair.LogicValue = CFPair.LOW;
            }
            else
            {
                foreach (CFPair pair in cfs.Values)
                    pair.RealValue = CFPair.LOW;
            }
            foreach (TNode nd in nodes)
            {
                if (source == SourceType.Source1)
                    cfs[nd].LogicValue = CFPair.HIGH;
                else
                    cfs[nd].RealValue = CFPair.HIGH;
            }
        }
        /// <summary>
        /// 获取指定输出点的输出值
        /// </summary>
        /// <param name="node">输出节点</param>
        /// <param name="source">获取的值的类型</param>
        public string getValue(TNode node, SourceType source)
        {
            if (!cfs.ContainsKey(node))
                return String.Empty;
            if (source == SourceType.Source1)
                return cfs[node].LogicValue;
            else
                return cfs[node].RealValue;
        }
        #endregion
    }
}
