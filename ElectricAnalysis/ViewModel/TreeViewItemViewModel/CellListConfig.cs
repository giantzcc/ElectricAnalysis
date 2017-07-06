using ElectricAnalysis.DAL;
using ElectricAnalysis.DAL.Dao;
using ElectricAnalysis.Model;
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
    /// 清单表
    /// </summary>
    class CellListConfig : Itemvm
    {
        public CellListConfig(Itemvm parent)
            : base("元件清单", null, "/Image/list.png", parent)
        {
            
        }
        public override void DoubleClick(Action<string> showMsg)
        {
            Tuple<DataSet, string> tp = AccessAndExcelOp.GetDataFromExcelByConn(NamesManage.OriginTbName);
            Action waitForFinish = () =>
            {
                if (tp != null)
                {
                    GraphDao dao = new GraphDao();
                    DataTable table = tp.Item1.Tables[NamesManage.OriginTbName];
                    pro.Cells = dao.getComponentRelations(table);
                    pro.OriginFileName = tp.Item2;
                    showMsg(String.Format("加载元件清单信息{0}条。\r\n", pro.Cells.Count));
                    pro.BuildGraphData();
                    showMsg(String.Format("电路图拓扑结构导入完成，共包含{0}个节点。\r\n", pro.Nodes.Count));
                }
            };
            Messenger.Default.Send<Action>(waitForFinish, "WaitForFinish");
            base.Itemvms.Add(new CptRelationDisplay(this));
        }
        public override void MenuClick(MenuType menu)
        {

        }
    }
}
