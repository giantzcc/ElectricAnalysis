using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.ViewModel.TreeViewItemViewModel
{
    /// <summary>
    /// 资源
    /// </summary>
    class Resource : Itemvm
    {
        public Resource()
            : base("资源", null, "/Image/resource.png", null)
        {
            base.Itemvms.Add(new Config(this));
            base.Itemvms.Add(new Operation(this));
        }
        public override void DoubleClick(Action<string> showMsg)
        {

        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
