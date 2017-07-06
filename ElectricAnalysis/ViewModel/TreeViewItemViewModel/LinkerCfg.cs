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
    /// 接口配置
    /// </summary>
    class LinkerCfg : Itemvm
    {
        public LinkerCfg(Itemvm parent)
            : base("接口配置", null, "/Image/linker.png", parent)
        {

        }

        public override void DoubleClick(Action<string> showMsg)
        {
            DataView dv = config.LinkerRelation.getTable();
            Messenger.Default.Send<Tuple<string, DataView>>(
                Tuple.Create<string, DataView>("接口配置", config.LinkerRelation.getTable()),
                "DisplayTestBranch");
            showMsg(String.Format("加载连接器配置{0}条。\r\n", dv.Table.Rows.Count));
        }

        public override void MenuClick(MenuType menu)
        {
            throw new NotImplementedException();
        }
    }
}
