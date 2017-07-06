using ElectricAnalysis.Graph;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.ViewModel.TreeViewItemViewModel
{
    /// <summary>
    /// 总正到总负的布线图
    /// </summary>
    class VccToGndGraph : Itemvm
    {
        public VccToGndGraph(Itemvm parent)
            : base("总正至总负", null, "", parent)
        {

        }
        public override void DoubleClick(Action<string> showMsg)
        {
            IShortBranchTreeGenerator graph = new DigraphBuilder();
            graph.buildRoutes();
            if (pro.VccToGnd == null)
                pro.VccToGnd = graph.getVccToGndBranch();
            showMsg("加载总正至总负部分图纸...\r\n");
            Messenger.Default.Send<BranchView>(
                pro.VccToGnd
                , "ShowGraph");
        }

        public override void MenuClick(MenuType menu)
        {
            throw new NotImplementedException();
        }
    }
}
