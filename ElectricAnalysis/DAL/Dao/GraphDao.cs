using ElectricAnalysis.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.DAL.Dao
{
    /// <summary>
    /// 元件拓扑表的实体映射
    /// </summary>
    public class GraphDao
    {
        public List<OriginCell> getComponentRelations(DataTable table)
        {
            List<OriginCell> rst = new List<OriginCell>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                OriginCell cell = new OriginCell();
                cell.StartPart = table.Rows[i][NamesManage.SrcStartName].ToString().Trim();
                cell.StartNum = table.Rows[i][NamesManage.SrcStartNC].ToString().Trim();
                cell.EndPart = table.Rows[i][NamesManage.SrcEndName].ToString().Trim();
                cell.EndNum = table.Rows[i][NamesManage.SrcEndNC].ToString().Trim();
                cell.LineNum = table.Rows[i][NamesManage.SrcLine].ToString().Trim();
                rst.Add(cell);
            }
            return rst;
        }
    }
}
