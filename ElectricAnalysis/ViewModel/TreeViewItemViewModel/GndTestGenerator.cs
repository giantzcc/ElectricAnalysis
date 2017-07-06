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
    /// 地线导通支路生成
    /// </summary>
    class GndTestGenerator : Itemvm
    {
        public GndTestGenerator(Itemvm parent)
            : base("地线导通测试", null, "", parent)
        {

        }
        public override void DoubleClick(Action<string> showMsg)
        {
            Action waitForFinish = () =>
            {
                showMsg("开始生成地线导通测试...\r\n");
                DataView dt = pro.LoadGndBranches(true);
                showMsg(String.Format("生成地线测试支路信息{0}条。\r\n", dt.Table.Rows.Count));
                Messenger.Default.Send<Tuple<string, DataView>>(
                Tuple.Create<string, DataView>("地线导通测试支路", dt),
                "DisplayTestBranch");
            };
            Messenger.Default.Send<Action>(waitForFinish, "WaitForFinish");
        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
