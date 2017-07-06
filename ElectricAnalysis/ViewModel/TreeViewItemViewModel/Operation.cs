using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.ViewModel.TreeViewItemViewModel
{
    /// <summary>
    /// 操作
    /// </summary>
    class Operation : Itemvm
    {
        public Operation(Itemvm parent)
            : base("命令", null, "/Image/download.png", parent)
        {
            base.Itemvms.Add(new GndTestGenerator(this));
            base.Itemvms.Add(new VccTestGenerator(this));
            base.Itemvms.Add(new NormalTestGenerator(this));
            base.Itemvms.Add(new LogicLoopGenerator(this));
            base.Itemvms.Add(new LogicTestGenerator(this));
            base.Itemvms.Add(new LinkerCfgGenerator(this));
            base.Itemvms.Add(new GraphGenerator(this));
        }
        public override void DoubleClick(Action<string> showMsg)
        {

        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
