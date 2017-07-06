using ElectricAnalysis.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.DAL.Dao
{
    public class ComponentConfigDao
    {
        /// <summary>
        /// 将总正信息转换成实体类
        /// </summary>
        public List<VoltageSet> getVccConfig(DataTable table)
        {
            List<VoltageSet> rst = new List<VoltageSet>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                VoltageSet vset = new VoltageSet();
                vset.Type = table.Rows[i][NamesManage.VoltageType].ToString().Trim();
                vset.Part = table.Rows[i][NamesManage.V_PartName].ToString().Trim();
                vset.Nc = table.Rows[i][NamesManage.V_NC].ToString().Trim();
                rst.Add(vset);
            }
            return rst;
        }
        /// <summary>
        /// 将元件的名称类别信息转换成实体类
        /// </summary>
        public List<ComponentLib> getComponentInfo(DataTable table)
        {
            List<ComponentLib> rst = new List<ComponentLib>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                ComponentLib vset = new ComponentLib();
                vset.AbbName = table.Rows[i][NamesManage.CptAbbName].ToString().Trim();
                vset.Name = table.Rows[i][NamesManage.CptName].ToString().Trim();
                vset.MaterialNum = table.Rows[i][NamesManage.CptNumber].ToString().Trim();
                vset.Type = table.Rows[i][NamesManage.CptType].ToString().Trim();
                rst.Add(vset);
            }
            return rst;
        }
        /// <summary>
        /// 将端子排短接表转换成实体类
        /// </summary>
        public List<TBCircuit> getTermialRelations(DataTable table)
        {
            List<TBCircuit> rst = new List<TBCircuit>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                TBCircuit tb = new TBCircuit();
                tb.StartPart = table.Rows[i][NamesManage.TbStartName].ToString().Trim();
                tb.StartNum = table.Rows[i][NamesManage.TbStartNC].ToString().Trim();
                tb.EndPart = table.Rows[i][NamesManage.TbEndName].ToString().Trim();
                tb.EndNum = table.Rows[i][NamesManage.TbEndNC].ToString().Trim();
                rst.Add(tb);
            }
            return rst;
        }
    }
}
