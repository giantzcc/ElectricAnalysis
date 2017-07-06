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
    /// 电压给定表
    /// </summary>
    class VoltDisplay : Itemvm
    {
        public VoltDisplay(Itemvm parent)
            : base("电压给定表", null, "", parent)
        {

        }
        public override void DoubleClick(Action<string> showMsg)
        {
            Messenger.Default.Send<Tuple<string, List<VoltageSet>>>(
            Tuple.Create<string, List<VoltageSet>>("电压给定表", config.Vsets),
            "DisplayVoltageSet");
        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
