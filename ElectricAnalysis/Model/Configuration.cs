using ElectricAnalysis.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    /// <summary>
    /// 配置类
    /// </summary>
    [Serializable]
    public class Configuration
    {
        #region 单例
        private static Configuration instance = null;
        private static object syncObj = new object();
        static Configuration()
        {
            if (File.Exists(NamesManage.ConfigFileName))
            {
                instance = SerializeData<Configuration>.T_ReadBinary(NamesManage.ConfigFileName);
            }
        }
        private Configuration()
        {
            vsets = new List<VoltageSet>();
            cptlib = new List<ComponentLib>();
            tblib = new List<TBCircuit>();
            relayPins = new List<PinInfo>();
            contactorPins = new List<PinInfo>();
            indicatorPins = new List<IndicatorPinInfo>();
            linkerRelation = new LinkerConfig();
        }
        public static Configuration GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                    instance = new Configuration();
                return instance;
            }
        }
        #endregion

        #region Field Property
        private List<VoltageSet> vsets;//电压给定信息
        public List<VoltageSet> Vsets
        {
            get { return vsets; }
            set { vsets = value; }
        }
        private List<ComponentLib> cptlib;//元件配置信息
        public List<ComponentLib> Cptlib
        {
            get { return cptlib; }
            set { cptlib = value; }
        }
        private List<TBCircuit> tblib;//端子排短接表
        public List<TBCircuit> Tblib
        {
            get { return tblib; }
            set { tblib = value; }
        }
        private List<PinInfo> relayPins;//继电器的线圈与触点
        public List<PinInfo> RelayPins
        {
            get { return relayPins; }
            set { relayPins = value; }
        }
        private List<PinInfo> contactorPins;//接触器的线圈与触点
        public List<PinInfo> ContactorPins
        {
            get { return contactorPins; }
            set { contactorPins = value; }
        }
        private List<IndicatorPinInfo> indicatorPins;//指示灯端子
        public List<IndicatorPinInfo> IndicatorPins
        {
            get { return indicatorPins; }
            set { indicatorPins = value; }
        }
        private LinkerConfig linkerRelation;//接口配置
        public LinkerConfig LinkerRelation
        {
            get { return linkerRelation; }
        }
        #endregion

        #region Function
        /// <summary>
        /// 获得元件类型
        /// </summary>
        public string GetType(string abbName)
        {
            var cpt = cptlib.FirstOrDefault(p => p.AbbName == abbName);
            if (cpt != null)
                return cpt.Type;
            else
                return string.Empty;
        }
        /// <summary>
        /// 获得元件的另一个端口
        /// </summary>
        public Tuple<string, string> GetAnotherNum(TNode node, string type)
        {
            string rst = string.Empty;
            string rst2 = string.Empty;
            if (!node.Num.Contains("RE") && !node.Num.Contains("NC"))
            {
                string materialNum = GetMaterialNum(node.Part);
                List<PinInfo> source = null;
                if (type.Contains("接触器"))
                    source = contactorPins;
                else if (type.Contains("继电器"))
                    source = relayPins;
                else
                    source = null;
                if (source != null)
                {
                    var pinfo = source.Find(p => p.MaterialNum == materialNum);
                    if (pinfo != null)
                    {
                        if (pinfo.Pin1 == node.Num)
                        {
                            rst = pinfo.Pin2;
                            node.TNType = TerminalType.Coil;
                        }
                        else if (pinfo.Pin2 == node.Num)
                        {
                            rst = pinfo.Pin1;//线圈正负极接反算作正常工作
                            node.TNType = TerminalType.Coil;
                        }
                    }
                    //rst = pinfo.Pin2 == node.Num ? pinfo.Pin1 : rst;
                    if (string.IsNullOrEmpty(rst) && pinfo != null)
                    {
                        var com1 = pinfo.NoPin.FirstOrDefault(p => p.Key == node.Num);
                        var com2 = pinfo.NcPin.FirstOrDefault(p => p.Key == node.Num);
                        if (!com1.Equals(default(KeyValuePair<string, string>))
                            || !com2.Equals(default(KeyValuePair<string, string>)))
                        {
                            if (!com1.Equals(default(KeyValuePair<string, string>)))
                                rst = com1.Value;
                            if (!com2.Equals(default(KeyValuePair<string, string>)))
                                rst2 = com2.Value;
                            node.TNType = TerminalType.ContactCom;
                        }
                        else
                        {
                            var contact = pinfo.NcPin.FirstOrDefault(p => p.Value == node.Num);
                            if (!contact.Equals(default(KeyValuePair<string, string>)))
                            {
                                rst = contact.Key;
                                node.TNType = TerminalType.ContactNormalClose;
                            }
                            else
                            {
                                contact = pinfo.NoPin.FirstOrDefault(p => p.Value == node.Num);
                                if (!contact.Equals(default(KeyValuePair<string, string>)))
                                {
                                    rst = contact.Key;
                                    node.TNType = TerminalType.ContactNormalOpen;
                                }
                            }
                        }
                    }
                }
            }
            else if (node.Num.Contains("NC"))
            {
                node.TNType = TerminalType.Block;
            }
            return Tuple.Create<string, string>(rst, rst2);
        }
        /// <summary>
        /// 获得元件物料号
        /// </summary>
        public string GetMaterialNum(string abbName)
        {
            var cpt = cptlib.FirstOrDefault(p => p.AbbName == abbName);
            if (cpt != null)
                return cpt.MaterialNum;
            else
                return string.Empty;
        }
        /// <summary>
        /// 生成接口配置
        /// </summary>
        public bool GenerateLinkerConfig()
        {
            linkerRelation.LinkerRelation.Clear();
            return linkerRelation.GenerateLinkerConfig();
        }
        #endregion
    }
    /// <summary>
    /// 给定电压
    /// </summary>
    [Serializable]
    public class VoltageSet
    {
        private string type;
        public string Type
        {
            get { return type; }
            set { type = value; }
        }
        private string part;
        public string Part
        {
            get { return part; }
            set { part = value; }
        }
        private string nc;
        public string Nc
        {
            get { return nc; }
            set { nc = value; }
        }
    }
    /// <summary>
    /// 元件名称
    /// </summary>
    [Serializable]
    public class ComponentLib
    {
        private string abbName;
        public string AbbName
        {
            get { return abbName; }
            set { abbName = value; }
        }
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string materialNum;
        public string MaterialNum
        {
            get { return materialNum; }
            set { materialNum = value; }
        }
        private string type;
        public string Type
        {
            get { return type; }
            set { type = value; }
        }
    }
    /// <summary>
    /// 端子排短接表
    /// </summary>
    [Serializable]
    public class TBCircuit
    {
        private string startPart;//起始部位
        public string StartPart
        {
            get { return startPart; }
            set { startPart = value; }
        }
        private string startNum;//起始端子号
        public string StartNum
        {
            get { return startNum; }
            set { startNum = value; }
        }
        private string endPart;//末端部位
        public string EndPart
        {
            get { return endPart; }
            set { endPart = value; }
        }
        private string endNum;//末端端子号
        public string EndNum
        {
            get { return endNum; }
            set { endNum = value; }
        }
        private bool hasIncluded = false;//是否已经包含
        public bool HasIncluded
        {
            get { return hasIncluded; }
            set { hasIncluded = value; }
        }
    }
    /// <summary>
    /// 继电器、接触器端子信息
    /// </summary>
    [Serializable]
    public class PinInfo
    {
        #region Property
        public string MaterialNum { get; set; }//物料号
        public string Model { get; set; }//型号
        public string Pin1 { get; set; }//线圈+(端子1)
        public string Pin2 { get; set; }//线圈-(端子2)
        public Dictionary<string, string> NoPin { get; set; }//常开触点
        public Dictionary<string, string> NcPin { get; set; }//常闭触点
        #endregion

        #region Construction
        public PinInfo()
        {
            NoPin = new Dictionary<string, string>(); 
            NcPin = new Dictionary<string, string>();
        }
        #endregion

        #region Function
       
        #endregion
    }
    /// <summary>
    /// 指示灯端子信息
    /// </summary>
    [Serializable]
    public class IndicatorPinInfo
    {
        #region Property
        public string MaterialNum { get; set; }//物料号
        public string Model { get; set; }//型号
        public Dictionary<string, string> Pins { get; set; }//普通端口
        #endregion

        #region Construction
        public IndicatorPinInfo()
        {
            Pins = new Dictionary<string, string>();
        }
        #endregion
    }
}
