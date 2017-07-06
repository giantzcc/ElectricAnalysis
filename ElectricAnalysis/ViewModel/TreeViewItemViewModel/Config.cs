using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.ViewModel.TreeViewItemViewModel
{
    /// <summary>
    /// 配置
    /// </summary>
    class Config : Itemvm
    {
        public Config(Itemvm parent)
            : base("配置", null, "/Image/config.png", parent)
        {
            this.Itemvms.Add(new CellConfig(this));
            this.Itemvms.Add(new CellLibConfig(this));
            this.Itemvms.Add(new CellListConfig(this));
        }
        public override void DoubleClick(Action<string> showMsg)
        {

        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
