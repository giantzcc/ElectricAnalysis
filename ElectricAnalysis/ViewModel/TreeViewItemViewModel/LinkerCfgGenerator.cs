using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ElectricAnalysis.ViewModel.TreeViewItemViewModel
{
    /// <summary>
    /// 接口配置生成
    /// </summary>
    class LinkerCfgGenerator : Itemvm
    {
        public LinkerCfgGenerator(Itemvm parent)
            : base("接口生成", null, "", parent)
        {

        }
        public override void DoubleClick(Action<string> showMsg)
        {
            Action waitForFinish = () =>
            {
                Itemvm p = Parent;
                while (p.Parent != null)
                    p = p.Parent;
                Itemvm cfgItem = p.Itemvms.FirstOrDefault(q => q is Config);
                Itemvm lg = cfgItem.Itemvms.FirstOrDefault(q => q is LinkerCfg);
                if (lg == null && config.GenerateLinkerConfig())
                {
                    Application.Current.Dispatcher.BeginInvoke(
                        new Action(() =>
                        {
                            cfgItem.Itemvms.Add(new LinkerCfg(cfgItem));
                            showMsg(String.Format("生成连接器与插座映射信息{0}条。\r\n", config.LinkerRelation.LinkerRelation.Count));
                        }));
                }
            };
            Messenger.Default.Send<Action>(waitForFinish, "WaitForFinish");
        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
