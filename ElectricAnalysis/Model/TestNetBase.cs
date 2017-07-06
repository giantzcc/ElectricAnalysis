using ElectricAnalysis.DAL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    class TestNetBase
    {
        #region Construction
        public TestNetBase()
        {
            nets = new List<BranchNet>();
        }
        #endregion

        #region Field Property
        protected List<BranchNet> nets;//测试支路网络
        internal List<BranchNet> Nets
        {
            get { return nets; }
        }
        public int MaxNetNum//最大网络号
        {
            get
            {
                if (nets.Count != 0)
                    return nets.Max(p => p.Num);
                else
                    return 0;
            }
        }
        #endregion

        #region Function
        /// <summary>
        /// 根据网络中最长的支路计算
        /// 转换成Table后应该有多少列
        /// </summary>
        public int getColumnCount()
        {
            if (nets.Count() == 0)
                return 0;
            int columns = nets.Max(p =>
            {
                if (p.Branches.Count != 0)
                    return p.Branches.Max(q => q.Branch.Count);
                else
                    return 0;
            });
            return columns;
        }
        /// <summary>
        /// 将网络中的支路信息填充到表格里
        /// </summary>
        public void fillTable(DataTable table)
        {
            foreach (var net in nets)
            {
                for (int i = 0; i < net.Branches.Count; i++)
                {
                    DataRow row = table.NewRow();
                    var branch = net.Branches[i].Branch;
                    row[0] = net.Num;
                    for (int j = 0; j < branch.Count; j++)
                    {
                        row[1 + j * 3] = branch[j].Part;
                        row[2 + j * 3] = branch[j].Num;
                        row[3 + j * 3] = branch[j].TNType;
                    }
                    table.Rows.Add(row);
                }
            }
        }
        /// <summary>
        /// 转化成表格返回
        /// </summary>
        protected DataView getTable(string tbName)
        {
            DataTable tb = new DataTable(tbName);
            int columns = getColumnCount();
            for (int i = 0; i < columns; i++)
            {
                tb.Columns.AddRange(new DataColumn[]{
                    new DataColumn("部位"+i,Type.GetType("System.String")),
                    new DataColumn("端子号"+i,Type.GetType("System.String")),
                    new DataColumn("端口类型"+i,Type.GetType("System.String"))
                });
            }
            fillTable(tb);
            return tb.AsDataView();
        }
        /// <summary>
        /// 保存转化成的表格到Excel中
        /// </summary>
        protected DataView SaveBranchesToExcel(string tbName)
        {
            AppProject pro = AppProject.GetInstance();
            if (nets.Count() == 0)
                return null;
            int columns = getColumnCount();
            string cmd = "CREATE TABLE " + tbName + " (";
            cmd += "网络号 VARCHAR(100),";
            for (int i = 0; i < columns; i++)
            {
                cmd += string.Format("部位{0} VARCHAR(100),端子号{1} VARCHAR(100),端口类型{2} VARCHAR(100),", i, i, i);
            }
            cmd = cmd.Substring(0, cmd.Length - 1) + ")";
            using (OleDbConnection cnn = AccessAndExcelOp.GetExcelConnect(pro.OriginFileName + "(测试支路).xls"))
            {
                List<string> tbnames = AccessAndExcelOp.GetTableNames(cnn);
                string deleteCmd = "Drop Table " + tbName;
                if (tbnames.Contains(tbName))
                    AccessAndExcelOp.ExecuteNoQueryCmd(cnn, deleteCmd);
                AccessAndExcelOp.CreateTable(cnn, cmd);
                DataTable table = AccessAndExcelOp.GetTable(cnn, tbName);
                fillTable(table);
                AccessAndExcelOp.UpdateSourceTable(cnn, table);
                return table.AsDataView();
            }
        }
        #endregion
    }
}
