using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.ViewModel.TreeViewItemViewModel
{
    /// <summary>
    /// 控制箱握手命令
    /// </summary>
    class HandShake : Itemvm
    {
        public HandShake(Itemvm parent)
            : base("上电测试", null, "", parent)
        {

        }

        public override void DoubleClick(Action<string> showMsg)
        {
            Messenger.Default.Send<object>(null, "PowBox");
        }

        public override void MenuClick(MenuType menu)
        {
            
        }
    }
}
