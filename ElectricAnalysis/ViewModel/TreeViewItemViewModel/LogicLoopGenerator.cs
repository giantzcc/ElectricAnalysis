using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.ViewModel.TreeViewItemViewModel
{
    /// <summary>
    /// 逻辑回路
    /// </summary>
    class LogicLoopGenerator : Itemvm
    {
        public LogicLoopGenerator(Itemvm parent)
            : base("逻辑回路", null, "", parent)
        {

        }
        public override void DoubleClick(Action<string> showMsg)
        {
            Action waitForFinish = () =>
            {
                showMsg("开始生成逻辑回路...\r\n");
                DataView dt = pro.LoadLogicLoops(false);
                int count = 0;
                pro.LoopNets.Nets.ForEach(p => count += p.Branches.Count);
                showMsg(String.Format("生成逻辑回路信息{0}条。\r\n", count));
                Messenger.Default.Send<Tuple<string, DataView>>(
                    Tuple.Create<string, DataView>("逻辑回路", dt),
                    "DisplayTestBranch");
            };
            Messenger.Default.Send<Action>(waitForFinish, "WaitForFinish");
        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
