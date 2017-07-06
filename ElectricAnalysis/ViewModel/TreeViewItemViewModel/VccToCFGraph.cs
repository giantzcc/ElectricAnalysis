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
    /// 总正至测试点的布线图
    /// </summary>
    class VccToCFGraph : Itemvm
    {
        public VccToCFGraph(Itemvm parent)
            : base("总正至测试点", null, "", parent)
        {

        }
        public override void DoubleClick(Action<string> showMsg)
        {
            IShortBranchTreeGenerator graph = new DigraphBuilder();
            graph.buildRoutes();
            if (pro.VccToCF == null)
                pro.VccToCF = graph.getVccToCFBranch();
            showMsg("加载总正至测试点部分图纸...\r\n");
            Messenger.Default.Send<BranchView>(
                pro.VccToCF
                , "ShowGraph");
        }

        public override void MenuClick(MenuType menu)
        {
            throw new NotImplementedException();
        }
    }
}
