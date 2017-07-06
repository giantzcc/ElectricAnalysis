using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ElectricAnalysis.DAL
{
    public class AccessAndExcelOp
    {
        /// <summary>
        /// 建立mdb数据库文件
        /// </summary>
        /// <param name="pathandname">完整路径</param>
        public static void CreateMdbFile(string pathandname)
        {
            ADOX.CatalogClass cls = new ADOX.CatalogClass();
            cls.Create("Provider=Microsoft.Jet.OLEDB.4.0;" +
                   "Data Source=" + pathandname + ";" +
                   "Jet OLEDB:Engine Type=5");
            cls = null;
        }
        /// <summary>
        /// 连接excel
        /// </summary>
        /// <param name="path">excel文件位置</param>
        public static OleDbConnection GetExcelConnect(string path)
        {
            string extension = Path.GetExtension(path);
            string connstr = "";
            switch (extension)
            {
                case ".xls":
                    connstr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=2;'";
                    break;
                case ".xlsx":
                    connstr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties='Excel 12.0;HDR=Yes;IMEX=2;'";
                    break;
                default:
                    connstr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=2;'";
                    break;
            }
            OleDbConnection connection = new OleDbConnection(connstr);
            return connection;
        }
        /// <summary>
        /// 连接到access数据库
        /// </summary>
        /// <param name="path">access数据库文件位置</param>
        public static OleDbConnection GetAccessConnect(string path)
        {
            string connstr = "Provider=Microsoft.Jet.OLEDB.4.0 ;Data Source=" + path;
            OleDbConnection connection = new OleDbConnection(connstr);
            return connection;
        }
        private static OleDbDataReader SearchData(OleDbConnection conn, string command)
        {
            OleDbDataReader reader;
            OleDbCommand cmd = new OleDbCommand(command, conn);
            conn.Open();
            reader = cmd.ExecuteReader();
            return reader;
        }
        /// <summary>
        /// 更新数据库
        /// </summary>
        public static void UpdateSourceTable(OleDbConnection conn,DataTable tb)
        {
            OleDbDataAdapter data = new OleDbDataAdapter("Select * From [" + tb.TableName + "]", conn);
            OleDbCommandBuilder cb = new OleDbCommandBuilder(data);
            cb.RefreshSchema();
            data.Update(tb);
        }
        /// <summary>
        ///执行SQL不返回值 
        /// </summary>
        public static void ExecuteNoQueryCmd(OleDbConnection conn, string command)
        {
            conn.Open();
            OleDbCommand cmd = new OleDbCommand(command, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        /// <summary>
        /// 执行SQL语句返回一个值
        /// </summary>
        public static object ExecuteScalar(OleDbConnection conn, string command)
        {
            conn.Open();
            OleDbCommand cmd = new OleDbCommand(command, conn);
            object rst = cmd.ExecuteScalar();
            conn.Close();
            return rst;
        }
        public static void CreateTable(OleDbConnection conn, string command)
        {
            conn.Open();
            OleDbCommand cmd = new OleDbCommand(command, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        /// <summary>
        /// 将数据库中的表拷贝到新建的excel表中
        /// </summary>
        /// <param name="conn">数据库连接对象</param>
        /// <param name="accesstb">数据库表格</param>
        /// <param name="exceldb">excel文件</param>
        /// <param name="exceltb">excel中的表</param>
        public static void AccessToExcel(OleDbConnection conn, string accesstb, string exceldb, string exceltb)
        {
            conn.Open();
            string sql_str = "Select * Into [Excel 8.0;database=" + exceldb + ";]." + "[" + exceltb + "] From " + "[" + accesstb + "]";
            OleDbCommand cmd = new OleDbCommand(sql_str, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        /// <summary>
        /// 获得放置在DataSet中的DataTable
        /// </summary>
        /// <returns>DataSet</returns>
        public static DataSet GetTable(OleDbConnection conn, string command, string tableName)
        {
            System.Data.DataSet mydataset = new System.Data.DataSet();
            conn.Open();
            OleDbDataAdapter adapter = new OleDbDataAdapter();
            string sqlstr = command;
            adapter.SelectCommand = new OleDbCommand(sqlstr, conn);
            adapter.Fill(mydataset, tableName);
            conn.Close();
            return mydataset;
        }
        /// <summary>
        /// 获得一张表
        /// </summary>
        /// <topNum>返回的最大记录数</topNum>
        /// <returns>DataTable</returns>
        public static DataTable GetTable(OleDbConnection conn, string tableName, int topNum = int.MaxValue)
        {
            System.Data.DataSet mydataset = new System.Data.DataSet();
            conn.Open();
            OleDbDataAdapter adapter = new OleDbDataAdapter();
            string sqlstr;
            if (topNum == int.MaxValue)
                sqlstr = "Select * From[" + tableName + "]";
            else
                sqlstr = "Select Top " + topNum + " * From[" + tableName + "]";
            adapter.SelectCommand = new OleDbCommand(sqlstr, conn);
            adapter.Fill(mydataset, tableName);
            conn.Close();
            return mydataset.Tables[tableName];
        }
        public static void AddTable(OleDbConnection conn, string command, string tableName, DataSet ds)
        {
            conn.Open();
            OleDbDataAdapter adapter = new OleDbDataAdapter();
            string sqlstr = command;
            adapter.SelectCommand = new OleDbCommand(sqlstr, conn);
            adapter.Fill(ds, tableName);
            conn.Close();
        }
        public static void AddTable(OleDbConnection conn, string tableName, DataSet ds)
        {
            conn.Open();
            OleDbDataAdapter adapter = new OleDbDataAdapter();
            string sqlstr = "Select * From [" + tableName + "]";
            adapter.SelectCommand = new OleDbCommand(sqlstr, conn);
            adapter.Fill(ds, tableName);
            conn.Close();
        }
        /// <summary>
        /// 获得所有表名
        /// </summary>
        /// <param name="conn">连接对象</param>
        /// <returns>表名集合</returns>
        public static List<string> GetTableNames(OleDbConnection conn)
        {
            conn.Open();
            List<string> names = new List<string>();
            DataTable shema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            foreach (DataRow dr in shema.Rows)
            {
                names.Add((string)dr["TABLE_NAME"]);
            }
            conn.Close();
            return names;
        }
        /// <summary>
        /// 通过对话框从excel文件中获得表
        /// </summary>
        public static Tuple<DataSet,string> GetDataFromExcelByConn(params string[] tbNames)
        {
            DataSet myds = new DataSet();//实例化数据集对象
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Excel文件|*.xls;*.xlsx";//设置打开文件筛选器
            openFile.Title = "选择Excel文件";//设置打开对话框标题
            openFile.Multiselect = false;//设置打开对话框中只能单选
            if (openFile.ShowDialog() == DialogResult.OK)//判断是否选择了文件
            {
                //连接Excel数据库
                string extension = Path.GetExtension(openFile.FileName);
                string strConn = "";
                switch (extension)
                {
                    case ".xls":
                        strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + openFile.FileName + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";
                        break;
                    case ".xlsx":
                        strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + openFile.FileName + ";Extended Properties='Excel 12.0;HDR=Yes;IMEX=1;'";
                        break;
                    default:
                        strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + openFile.FileName + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";
                        break;
                }
                try
                {
                    using (OleDbConnection olecon = new OleDbConnection(strConn))
                    {
                        olecon.Open();//打开数据库连接
                        foreach (var name in tbNames)
                        {
                            OleDbDataAdapter oledbda = new OleDbDataAdapter();//从工作表中查询数据
                            oledbda.SelectCommand = new OleDbCommand("select * from [" + name + "$]", olecon);
                            oledbda.Fill(myds,name);//填充数据集
                        }
                        olecon.Close();
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return null;
                }
            }
            else
            {
                return null;
            }
            string filename = Path.GetFileNameWithoutExtension(openFile.FileName);
            return Tuple.Create<DataSet, string>(myds, filename);
        }
    }
}
