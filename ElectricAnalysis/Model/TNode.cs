using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    /// <summary>
    /// 支路中的一个节点
    /// </summary>
    public class TNode
    {
        #region Construction
        public TNode(string part, string num)
        {
            this.part = part;
            this.num = num;
            TNType = TerminalType.Normal;
            HasIncluded = false;
            HasChanged = false;
        }
        #endregion

        #region Property
        private string part;
        public string Part
        {
            get { return part; }
            set { part = value; }
        }
        private string num;
        public string Num
        {
            get { return num; }
            set { num = value; }
        }
        public int index { get; set; }//在集合中此节点的位置
        public TerminalType TNType { get; set; }//端子类型
        public string PartType { get; set; }//元件类型
        public bool HasIncluded { get; set; }//是否已经寻找过
        public bool HasChanged { get; set; }//触点是否已经动作（用于分析逻辑支路是否导通）
        public string Equal { get; set; }//所属的等电位点编号
        public TNode Brother { get; set; }//兄弟节点，即成对的同属于同一元件的节点
        private List<TNode> nodes = new List<TNode>();//与本节点相连接的其他的节点
        public List<TNode> Nodes
        {
            get { return nodes; }
        }
        #endregion

        #region Function
        public TNode GetAnother()
        {
            return Brother;
        }
        public TNode clone()
        {
            TNode copy = (TNode)this.MemberwiseClone();
            copy.nodes = new List<TNode>();
            copy.Brother = null;
            return copy;
        }
        #endregion
    }
    /// <summary>
    /// 端子类型
    /// </summary>
    public enum TerminalType
    {
        Normal,//普通端子
        RE,//RE端子
        ContactNormalOpen,//常开触点
        ContactNormalClose,//常闭触点
        ContactCom,//com端口
        BreakerContact,//断路器触点
        Indicator,//指示灯模块
        Switch,//转换开关
        Coil,//线圈
        Block,//阻塞
        DiodePositive,//二极管正极
        DiodeNegative//二极管负极
    }
}
