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
    /// 元件名称表
    /// </summary>
    class CptDisplay : Itemvm
    {
        public CptDisplay(Itemvm parent)
            : base("元件名称表", null, "", parent)
        {

        }
        public override void DoubleClick(Action<string> showMsg)
        {
            Messenger.Default.Send<Tuple<string, List<ComponentLib>>>(
                Tuple.Create<string, List<ComponentLib>>("元件名称表", config.Cptlib),
                "DisplayComponentLib");
        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
