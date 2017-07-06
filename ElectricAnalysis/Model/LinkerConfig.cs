using ElectricAnalysis.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    /// <summary>
    /// 接口配置类
    /// </summary>
    [Serializable]
    public class LinkerConfig : IConvertToTable
    {
        #region Construction
        public LinkerConfig()
        {
            linkerRelation = new Dictionary<Linker, TestBox>(new CompareLinker());
        }
        #endregion

        #region Field Property
        private Dictionary<Linker, TestBox> linkerRelation;//接口配置
        internal Dictionary<Linker, TestBox> LinkerRelation
        {
            get { return linkerRelation; }
            set { linkerRelation = value; }
        }
        #endregion

        #region Function
        /// <summary>
        /// 生成接口配置
        /// </summary>
        public bool GenerateLinkerConfig()
        {
            AppProject pro = AppProject.GetInstance();
            int boxnum = 1;
            int boxpoint = 1;
            var linkerNames = pro.Nodes.Where(p => p.PartType == "接口连接器" && p.TNType == TerminalType.Normal).Select(q => q.Part).Distinct();
            foreach (var name in linkerNames)
            {
                int port = Linker.GetMaxLinkerPort(pro.Nodes, name);
                if (port <= 56)
                {
                    /*小于56可以分配在任意插头上*/
                    bool FindEmpty = false;
                    for (int j = 1; j <= 5; j++)
                    {
                        var box = linkerRelation.Values.FirstOrDefault(p => p.TestBoxNum == ("#" + j) && p.TestBoxPort > 144);
                        if (box == null)
                        {
                            boxnum = j;
                            boxpoint = 145;
                            FindEmpty = true;
                            break;
                        }
                    }
                    if (!FindEmpty)
                    {
                        for (int j = 1; j <= 5; j++)
                        {
                            var box = linkerRelation.Values.FirstOrDefault(p => p.TestBoxNum == ("#" + j) && p.TestBoxPort <= 72);
                            if (box == null)
                            {
                                FindEmpty = true;
                                boxnum = j;
                                boxpoint = 1;
                                break;
                            }
                            else
                            {
                                box = linkerRelation.Values.FirstOrDefault(p => p.TestBoxNum == ("#" + j) && p.TestBoxPort > 72 && p.TestBoxPort <= 144);
                                if (box == null)
                                {
                                    FindEmpty = true;
                                    boxnum = j;
                                    boxpoint = 73;
                                    break;
                                }
                            }
                        }
                    }
                    if (!FindEmpty)
                        return false;
                }
                else
                {
                    /*大于56必须分配在每个箱子的前两个插头上*/
                    bool FindEmpty = false;
                    for (int j = 1; j <= 5; j++)
                    {
                        var box = linkerRelation.Values.FirstOrDefault(p => p.TestBoxNum == ("#" + j) && p.TestBoxPort <= 72);
                        if (box == null)
                        {
                            FindEmpty = true;
                            boxnum = j;
                            boxpoint = 1;
                            break;
                        }
                        else
                        {
                            box = linkerRelation.Values.FirstOrDefault(p => p.TestBoxNum == ("#" + j) && p.TestBoxPort > 72 && p.TestBoxPort <= 144);
                            if (box == null)
                            {
                                FindEmpty = true;
                                boxnum = j;
                                boxpoint = 73;
                                break;
                            }
                        }
                    }
                    if (!FindEmpty)
                        return false;
                }
                for (int i = 1; i <= port; i++)
                {
                    Linker link = new Linker(name, i);
                    TestBox box = new TestBox(link, "#" + boxnum, boxpoint++);
                    linkerRelation.Add(link, box);
                }
            }
            return true;
        }
        /// <summary>
        /// 保存到excel
        /// </summary>
        public System.Data.DataView SaveBranchesToExcel()
        {
            AppProject pro = AppProject.GetInstance();
            string tbName = "接口配置";
            if (linkerRelation.Count() == 0)
                return null;
            string cmd = "CREATE TABLE 接口配置 (连接器 VARCHAR(100),连接器端口 VARCHAR(100),测试箱 VARCHAR(100),测试箱端口 VARCHAR(100),线号 VARCHAR(100))";
            using (OleDbConnection cnn = AccessAndExcelOp.GetExcelConnect(pro.OriginFileName + "(测试支路).xls"))
            {
                List<string> tbnames = AccessAndExcelOp.GetTableNames(cnn);
                if (tbnames.Contains(tbName))
                {
                    string deleteCmd = "Drop Table " + tbName;
                    AccessAndExcelOp.ExecuteNoQueryCmd(cnn, deleteCmd);
                }
                AccessAndExcelOp.CreateTable(cnn, cmd);
                DataTable table = AccessAndExcelOp.GetTable(cnn, tbName);
                foreach (var link in linkerRelation)
                {
                    DataRow row = table.NewRow();
                    row[0] = link.Key.LinkerName;
                    row[1] = link.Key.LinkerPort;
                    row[2] = link.Value.TestBoxNum;
                    row[3] = link.Value.TestBoxPort;
                    row[4] = link.Value.LineNum;
                    table.Rows.Add(row);
                }
                AccessAndExcelOp.UpdateSourceTable(cnn, table);
                return table.AsDataView();
            }
        }
        /// <summary>
        /// 将数据转换成表
        /// </summary>
        public DataView getTable()
        {
            DataTable tb = new DataTable("接口配置");
            tb.Columns.AddRange(new DataColumn[]{
                new DataColumn("连接器", Type.GetType("System.String")),
                new DataColumn("连接器端口", Type.GetType("System.String")),
                new DataColumn("测试箱", Type.GetType("System.String")),
                new DataColumn("测试箱端口", Type.GetType("System.String")),
                new DataColumn("测试箱线号", Type.GetType("System.String"))
            });
            foreach (var link in linkerRelation)
            {
                DataRow row = tb.NewRow();
                row[0] = link.Key.LinkerName;
                row[1] = link.Key.LinkerPort;
                row[2] = link.Value.TestBoxNum;
                row[3] = link.Value.TestBoxPort;
                row[4] = link.Value.LineNum;
                tb.Rows.Add(row);
            }
            return tb.AsDataView();
        }
        #endregion
    }
}
