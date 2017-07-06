using ElectricAnalysis.Model;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.ViewModel.TreeViewItemViewModel
{
    /// <summary>
    /// 端子排短接表
    /// </summary>
    class TBDisplay : Itemvm
    {
        public TBDisplay(Itemvm parent)
            : base("端子排短接表", null, "", parent)
        {

        }
        public override void DoubleClick(Action<string> showMsg)
        {
            Messenger.Default.Send<Tuple<string, List<TBCircuit>>>(
                Tuple.Create<string, List<TBCircuit>>("端子排短接表", config.Tblib),
                "DisplayTBConnector");
        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
