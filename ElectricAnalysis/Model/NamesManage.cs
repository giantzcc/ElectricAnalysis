using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    static class NamesManage
    {
        /*Excel中相关的表名*/
        public static string OriginTbName = "连接清单";
        public static string VoltTbName = "电压给定表";
        public static string CptTbName = "元件配置";
        public static string TBTbName = "端子排短接表";
        public static string ConTactorTbName = "接触器库";
        public static string RelayTbName = "继电器库";
        public static string IndicatorTbName = "指示灯模块库";
        /*清单表中各列的名称*/
        public static string SrcStartName = "起始连接部位";
        public static string SrcStartNC = "起始位置";
        public static string SrcEndName = "末端连接部位";
        public static string SrcEndNC = "末端位置";
        public static string SrcLine = "线号";
        /*电压给定表中各列的名称*/
        public static string VoltageType = "电压类型";
        public static string V_PartName = "名称";
        public static string V_NC = "端口";
        public static string Positive = "外接总正110";
        public static string Negative = "外接总负110";
        /*元件配置表中各列名称*/
        public static string CptAbbName = "缩写";
        public static string CptName = "名称";
        public static string CptNumber = "物料号";
        public static string CptType = "种类";
        /*端子排短接表中各列的名称*/
        public static string TbStartName = "起始连接部位";
        public static string TbStartNC = "起始位置";
        public static string TbEndName = "末端连接部位";
        public static string TbEndNC = "末端位置";
        /*元件库文件中各列的名称*/
        public static string Model = "型号";
        public static string CoilPo = "线圈+";
        public static string CoilNe = "线圈-";
        public static string NoCom = "NO(COM)";
        public static string No = "NO";
        public static string NcCom = "NC(COM)";
        public static string Nc = "NC";
        public static string Pin1 = "端子1";
        public static string Pin2 = "端子2";
        /*文件的名称*/
        public static string ConfigFileName = "config.fg";
    }
}
