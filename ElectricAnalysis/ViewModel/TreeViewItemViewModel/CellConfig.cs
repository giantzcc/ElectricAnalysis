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
    /// 元件配置
    /// </summary>
    class CellConfig : Itemvm
    {
        public CellConfig(Itemvm parent)
            : base("元件配置", null, "/Image/component.png", parent)
        {
            if (config.Vsets != null && config.Vsets.Count != 0)
                base.Itemvms.Add(new VoltDisplay(this));
            if (config.Cptlib != null && config.Cptlib.Count != 0)
                base.Itemvms.Add(new CptDisplay(this));
            if (config.Tblib != null && config.Tblib.Count != 0)
                base.Itemvms.Add(new TBDisplay(this));
        }
        public override void DoubleClick(Action<string> showMsg)
        {
            base.Itemvms.Clear();
            Tuple<DataSet, string> tp = AccessAndExcelOp.GetDataFromExcelByConn(NamesManage.VoltTbName, NamesManage.CptTbName, NamesManage.TBTbName);
            if (tp != null)
            {
                ComponentConfigDao dao = new ComponentConfigDao();//元件配置表实体转换类
                /*电压给定表*/
                DataTable table = tp.Item1.Tables[NamesManage.VoltTbName];
                config.Vsets = dao.getVccConfig(table);
                showMsg(String.Format("加载电压给定表{0}条。\r\n", config.Vsets.Count));
                base.Itemvms.Add(new VoltDisplay(this));
                /*元件配置表*/
                table = tp.Item1.Tables[NamesManage.CptTbName];
                config.Cptlib = dao.getComponentInfo(table);
                showMsg(String.Format("加载元件名称表{0}条。\r\n", config.Cptlib.Count));
                base.Itemvms.Add(new CptDisplay(this));
                /*端子排短接表*/
                table = tp.Item1.Tables[NamesManage.TBTbName];
                config.Tblib = dao.getTermialRelations(table);
                showMsg(String.Format("加载端子排短接表{0}条。\r\n", config.Tblib.Count));
                base.Itemvms.Add(new TBDisplay(this));
            }
            /*保存本地*/
            try
            {
                SerializeData<Configuration>.T_WriteBinary(Configuration.GetInstance(), NamesManage.ConfigFileName);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
