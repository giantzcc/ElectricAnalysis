using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.ViewModel.TreeViewItemViewModel
{
    /// <summary>
    /// 生成电路拓扑图
    /// </summary>
    class GraphGenerator : Itemvm
    {
        public GraphGenerator(Itemvm parent)
            : base("布线图生成", null, "/Image/graph.png", parent)
        {
            base.Itemvms.Add(new CompleteGraph(this));
            base.Itemvms.Add(new VccToGndGraph(this));
            base.Itemvms.Add(new VccToCFGraph(this));
            base.Itemvms.Add(new CFToGndGraph(this));
        }
        public override void DoubleClick(Action<string> showMsg)
        {

        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
