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
    /// 元件清单表
    /// </summary>
    class CptRelationDisplay : Itemvm
    {
        public CptRelationDisplay(Itemvm parent)
            : base("元件清单表", null, "", parent)
        {

        }
        public override void DoubleClick(Action<string> showMsg)
        {
            Messenger.Default.Send<Tuple<string, List<OriginCell>>>(
                Tuple.Create<string, List<OriginCell>>("元件清单表", pro.Cells),
                "DisplayCptRelation");
        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
