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
    /// 110V导通测试
    /// </summary>
    class VccTestGenerator : Itemvm
    {
        public VccTestGenerator(Itemvm parent)
            : base("110V导通测试", null, "", parent)
        {

        }
        public override void DoubleClick(Action<string> showMsg)
        {
            Action waitForFinish = () =>
            {
                showMsg("开始生成110V导通测试...\r\n");
                DataView dt = pro.Load110VBranches(true);
                showMsg(String.Format("生成110V测试支路信息{0}条。\r\n", dt.Table.Rows.Count));
                Messenger.Default.Send<Tuple<string, DataView>>(
                    Tuple.Create<string, DataView>("110V导通测试支路", dt),
                    "DisplayTestBranch");
            };
            Messenger.Default.Send<Action>(waitForFinish, "WaitForFinish");
        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
