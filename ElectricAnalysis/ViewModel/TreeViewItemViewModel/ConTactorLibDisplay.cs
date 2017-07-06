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
    /// 接触器库
    /// </summary>
    class ConTactorLibDisplay : Itemvm
    {
        public ConTactorLibDisplay(Itemvm parent)
            : base("接触器库", null, "", parent)
        {

        }
        public override void DoubleClick(Action<string> showMsg)
        {
            Messenger.Default.Send<Tuple<string, List<PinInfo>>>(
                Tuple.Create<string, List<PinInfo>>("接触器库", config.ContactorPins),
                "DisplayRelayPinInfo");
        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
