using ElectricAnalysis.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.DAL.Dao
{
    public class TerminalRelationsDao
    {
        /// <summary>
        /// 将继电器或接触器的端子映射信息转换为实体类
        /// </summary>
        public List<PinInfo> getRelayPins(DataTable table)
        {
            List<PinInfo> rst = new List<PinInfo>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                string mtrlNum = table.Rows[i][NamesManage.CptNumber].ToString().Trim();
                var pin = rst.Find(p => p.MaterialNum == mtrlNum);
                if (pin == null)
                {
                    PinInfo pinfo = new PinInfo();
                    pinfo.MaterialNum = mtrlNum;
                    pinfo.Model = table.Rows[i][NamesManage.Model].ToString().Trim();
                    pinfo.Pin1 = table.Rows[i][NamesManage.CoilPo].ToString().Trim();
                    pinfo.Pin2 = table.Rows[i][NamesManage.CoilNe].ToString().Trim();
                    pin = pinfo;
                    rst.Add(pinfo);
                }
                string nccom = table.Rows[i][NamesManage.NcCom].ToString().Trim();
                string nc = table.Rows[i][NamesManage.Nc].ToString().Trim();
                if (!string.IsNullOrEmpty(nccom) && !string.IsNullOrEmpty(nc))
                    pin.NcPin.Add(nccom, nc);
                string nocom = table.Rows[i][NamesManage.NoCom].ToString().Trim();
                string no = table.Rows[i][NamesManage.No].ToString().Trim();
                if (!string.IsNullOrEmpty(nocom) && !string.IsNullOrEmpty(no))
                    pin.NoPin.Add(nocom, no);
            }
            return rst;
        }
        /// <summary>
        /// 将指示灯的端子信息转换为实体类
        /// </summary>
        public List<IndicatorPinInfo> getIndicatorPins(DataTable table)
        {
            List<IndicatorPinInfo> rst = new List<IndicatorPinInfo>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                string mtrlNum = table.Rows[i][NamesManage.CptNumber].ToString().Trim();
                var pin = rst.Find(p => p.MaterialNum == mtrlNum);
                if (pin == null)
                {
                    IndicatorPinInfo pinfo = new IndicatorPinInfo();
                    pinfo.MaterialNum = mtrlNum;
                    pinfo.Model = table.Rows[i][NamesManage.Model].ToString().Trim();
                    rst.Add(pinfo);
                    pin = pinfo;
                }
                string pin1 = table.Rows[i][NamesManage.Pin1].ToString().Trim();
                string pin2 = table.Rows[i][NamesManage.Pin2].ToString().Trim();
                pin.Pins.Add(pin1, pin2);
            }
            return rst;
        }
    }
}
