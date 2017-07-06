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
    /// 继电器库
    /// </summary>
    class RelayLibDisplay : Itemvm
    {
        public RelayLibDisplay(Itemvm parent)
            : base("继电器库", null, "", parent)
        {

        }
        public override void DoubleClick(Action<string> showMsg)
        {
            Messenger.Default.Send<Tuple<string, List<PinInfo>>>(
                Tuple.Create<string, List<PinInfo>>("继电器库", config.RelayPins),
                "DisplayRelayPinInfo");
        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
