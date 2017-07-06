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
    /// 逻辑测试
    /// </summary>
    class LogicTestGenerator : Itemvm
    {
        public LogicTestGenerator(Itemvm parent)
            : base("逻辑测试", null, "", parent)
        {

        }
        public override void DoubleClick(Action<string> showMsg)
        {
            Action waitForFinish = () =>
            {
                showMsg("开始生成逻辑测试支路...\r\n");
                DataView dt = pro.SplitLogicBranches(true);
                showMsg(String.Format("生成逻辑测试支路信息共{0}条。", dt.Table.Rows.Count));
                Tuple<int, int> counts = pro.getLogicNetCount();
                showMsg(String.Format("其中包含可测试网络{0}个，不可测试网络{1}个。\r\n", counts.Item1, counts.Item2));
                Messenger.Default.Send<Tuple<string, DataView>>(
                    Tuple.Create<string, DataView>("逻辑测试支路", dt),
                    "DisplayTestBranch");
            };
            Messenger.Default.Send<Action>(waitForFinish, "WaitForFinish");
        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
