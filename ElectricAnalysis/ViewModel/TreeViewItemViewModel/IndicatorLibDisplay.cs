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
    /// 指示灯模块库
    /// </summary>
    class IndicatorLibDisplay : Itemvm
    {
        public IndicatorLibDisplay(Itemvm parent)
            : base("指示灯模块库", null, "", parent)
        {

        }
        public override void DoubleClick(Action<string> showMsg)
        {
            Messenger.Default.Send<Tuple<string, List<IndicatorPinInfo>>>(
                Tuple.Create<string, List<IndicatorPinInfo>>("指示灯模块库", config.IndicatorPins),
                "DisplayIndicatorPinInfo");
        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
