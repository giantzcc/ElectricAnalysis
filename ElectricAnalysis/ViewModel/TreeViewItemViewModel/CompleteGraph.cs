using ElectricAnalysis.Graph;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.ViewModel.TreeViewItemViewModel
{
    class CompleteGraph : Itemvm
    {
        public CompleteGraph(Itemvm parent)
            : base("布线总图", null, "", parent)
        {

        }
        public override void DoubleClick(Action<string> showMsg)
        {
            IShortBranchTreeGenerator graph = new DigraphBuilder();
            graph.buildRoutes();
            if (pro.CompleteBr == null)
                pro.CompleteBr = graph.getAllBranch();
            showMsg("加载布线总图...\r\n");
            Messenger.Default.Send<BranchView>(
                pro.CompleteBr
                , "ShowGraph");
        }
        public override void MenuClick(MenuType menu)
        {
            throw new NotImplementedException();
        }
    }
}
