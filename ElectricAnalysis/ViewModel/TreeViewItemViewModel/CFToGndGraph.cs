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
    /// 测试点至总负的布线图
    /// </summary>
    class CFToGndGraph : Itemvm
    {
        public CFToGndGraph(Itemvm parent)
            : base("测试点至总负", null, "", parent)
        {

        }
        public override void DoubleClick(Action<string> showMsg)
        {
            IShortBranchTreeGenerator graph = new DigraphBuilder();
            graph.buildRoutes();
            if (pro.GndToCF == null)
                pro.GndToCF = graph.getCFToGndBranch();
            showMsg("加载测试点至总负部分图纸...\r\n");
            Messenger.Default.Send<BranchView>(
                pro.GndToCF
                , "ShowGraph");
        }
        public override void MenuClick(MenuType menu)
        {
            throw new NotImplementedException();
        }
    }
}
