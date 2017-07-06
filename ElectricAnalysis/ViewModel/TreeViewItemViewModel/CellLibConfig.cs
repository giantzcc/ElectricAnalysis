using ElectricAnalysis.DAL;
using ElectricAnalysis.DAL.Dao;
using ElectricAnalysis.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.ViewModel.TreeViewItemViewModel
{
    /// <summary>
    /// 元件库文件
    /// </summary>
    class CellLibConfig : Itemvm
    {
        public CellLibConfig(Itemvm parent)
            : base("元件库文件", null, "/Image/lamp.png", parent)
        {
            if (config.ContactorPins != null && config.ContactorPins.Count != 0)
                base.Itemvms.Add(new ConTactorLibDisplay(this));
            if (config.RelayPins != null && config.RelayPins.Count != 0)
                base.Itemvms.Add(new RelayLibDisplay(this));
            if (config.IndicatorPins != null && config.IndicatorPins.Count != 0)
                base.Itemvms.Add(new IndicatorLibDisplay(this));
        }
        public override void DoubleClick(Action<string> showMsg)
        {
            Tuple<DataSet, string> tp = AccessAndExcelOp.GetDataFromExcelByConn(NamesManage.ConTactorTbName, NamesManage.RelayTbName, NamesManage.IndicatorTbName);
            if (tp != null)
            {
                TerminalRelationsDao dao = new TerminalRelationsDao();//Table到实体类的映射工具
                /*接触器库*/
                DataTable table = tp.Item1.Tables[NamesManage.ConTactorTbName];
                config.ContactorPins = dao.getRelayPins(table);
                showMsg(String.Format("加载接触器端子信息{0}条。\r\n", config.ContactorPins.Count));
                base.Itemvms.Add(new ConTactorLibDisplay(this));
                /*继电器库*/
                table = tp.Item1.Tables[NamesManage.RelayTbName];
                config.RelayPins = dao.getRelayPins(table);
                showMsg(String.Format("加载继电器端子信息{0}条。\r\n", config.RelayPins.Count));
                base.Itemvms.Add(new RelayLibDisplay(this));
                /*指示灯模块库*/
                table = tp.Item1.Tables[NamesManage.IndicatorTbName];
                config.IndicatorPins = dao.getIndicatorPins(table);
                showMsg(String.Format("加载指示灯模块端子信息{0}条。\r\n", config.IndicatorPins.Count));
                base.Itemvms.Add(new IndicatorLibDisplay(this));
                /*保存本地*/
                SerializeData<Configuration>.T_WriteBinary(config, NamesManage.ConfigFileName);
            }
        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
