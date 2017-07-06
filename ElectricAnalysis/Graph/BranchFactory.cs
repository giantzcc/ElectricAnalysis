using ElectricAnalysis.Model;
using ElectricAnalysis.View.component;
using ElectricAnalysis.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ElectricAnalysis.Graph
{
    class BranchFactory
    {
        /// <summary>
        /// 用作短支路的显示
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static List<UIElement> convertToUIElement(List<TNode> nodes)
        {
            List<UIElement> elements = new List<UIElement>();
            for (int i=0;i<nodes.Count-1;i++)
            {
                if (nodes[i].PartType.Contains("端子排") && nodes[i].TNType == TerminalType.DiodePositive && nodes[i+1].TNType == TerminalType.DiodeNegative)
                {
                    elements.Add(new Diode(ComponentType.Diode, nodes[i], nodes[i + 1], false));
                    i++;
                }
                else if (nodes[i].PartType.Contains("端子排") && nodes[i].TNType == TerminalType.DiodeNegative && nodes[i + 1].TNType == TerminalType.DiodePositive)
                {
                    elements.Add(new Diode(ComponentType.Diode, nodes[i], nodes[i + 1], true));
                    i++;
                }
                else if (nodes[i].PartType.Contains("断路器") && nodes[i + 1].PartType.Contains("断路器") && nodes[i].TNType==TerminalType.BreakerContact)
                {
                    elements.Add(new Breaker(ComponentType.Breaker, nodes[i], nodes[i + 1]));
                    i++;
                }
                else if (nodes[i].PartType.Contains("转换开关") && nodes[i + 1].PartType.Contains("转换开关") && nodes[i].TNType == TerminalType.Switch)
                {
                    elements.Add(new Switch(ComponentType.Switch, nodes[i], nodes[i + 1]));
                    i++;
                }
                else if ((nodes[i].PartType.Contains("继电器") && nodes[i+1].PartType.Contains("继电器")) || (nodes[i].PartType.Contains("接触器")&&nodes[i+1].PartType.Contains("接触器")))
                {
                    if (nodes[i].TNType == TerminalType.Coil && nodes[i + 1].TNType == TerminalType.Coil)
                    {
                        elements.Add(new Coil(ComponentType.Coil, nodes[i], nodes[i + 1]));
                        i++;
                    }
                    else if ((nodes[i].TNType == TerminalType.ContactNormalOpen && nodes[i + 1].TNType == TerminalType.ContactCom) 
                        || (nodes[i].TNType == TerminalType.ContactCom && nodes[i + 1].TNType == TerminalType.ContactNormalOpen))
                    {
                        elements.Add(new StandardContact(ComponentType.ContactOpen, nodes[i], nodes[i + 1]));
                        i++;
                    }
                    else if ((nodes[i].TNType == TerminalType.ContactNormalClose && nodes[i + 1].TNType == TerminalType.ContactCom)
                        || (nodes[i].TNType == TerminalType.ContactCom && nodes[i + 1].TNType == TerminalType.ContactNormalClose))
                    {
                        elements.Add(new StandardContact(ComponentType.ContactClose, nodes[i], nodes[i + 1]));
                        i++;
                    }
                }
                else if (nodes[i].PartType.Contains("指示灯") && nodes[i + 1].PartType.Contains("指示灯") && nodes[i].TNType == TerminalType.Indicator)
                {
                    elements.Add(new Indicator(ComponentType.Indicator, nodes[i], nodes[i + 1]));
                    i++;
                }
                else
                {
                    elements.Add(new Terminal(ComponentType.Terminal, nodes[i]));
                }
            }
            if (nodes[nodes.Count - 1].PartType.Contains("接口连接器"))
                elements.Add(new Terminal(ComponentType.Terminal, nodes[nodes.Count - 1]));
            return elements;
        }

        /// <summary>
        /// 将一对节点转换成UI元素
        /// 用于布线图生成中
        /// upOrDown用于确定元件向上连接还是向下连接
        /// true:up
        /// false:down
        /// </summary>
        public static TNodeUI convert(TNode item1, TNode item2, IObserver observer = null)
        {
            if (item1 != null)
            {
                if(item2 != null){
                    if(item1.Part == item2.Part){
                        if (item1.TNType == TerminalType.DiodePositive && item2.TNType == TerminalType.DiodeNegative){
                            INotifyComponentChanged info = new ComponentViewModel(Tuple.Create<TNode, TNode>(item1, item2), ComponentType.Diode);
                            return new TNodeUI(info, false, observer);
                        }
                        else if (item2.TNType == TerminalType.DiodePositive && item1.TNType == TerminalType.DiodeNegative){
                            INotifyComponentChanged info = new ComponentViewModel(Tuple.Create<TNode, TNode>(item1, item2), ComponentType.Diode);
                            return new TNodeUI(info, true, observer);
                        }
                        else if (item1.TNType == TerminalType.BreakerContact && item2.TNType == TerminalType.BreakerContact){
                            INotifyComponentChanged info = new ComponentViewModel(Tuple.Create<TNode, TNode>(item1, item2), ComponentType.Breaker);
                            return new TNodeUI(info, false, observer);
                        }
                        else if (item1.TNType == TerminalType.Switch && item2.TNType == TerminalType.Switch){
                            INotifyComponentChanged info = new ComponentViewModel(Tuple.Create<TNode, TNode>(item1, item2), ComponentType.Switch);
                            return new TNodeUI(info, false, observer);
                        }
                        else if (item1.TNType == TerminalType.Coil && item2.TNType == TerminalType.Coil){
                            INotifyComponentChanged info = new ComponentViewModel(Tuple.Create<TNode, TNode>(item1, item2), ComponentType.Coil);
                            return new TNodeUI(info, false, observer);
                        }
                        else if ((item1.TNType == TerminalType.ContactNormalOpen && item2.TNType == TerminalType.ContactCom) ||
                            (item1.TNType == TerminalType.ContactCom && item2.TNType == TerminalType.ContactNormalOpen)){
                            INotifyComponentChanged info = new ComponentViewModel(Tuple.Create<TNode, TNode>(item1, item2), ComponentType.ContactOpen);
                            return new TNodeUI(info, false, observer);
                        }
                        else if ((item1.TNType == TerminalType.ContactNormalClose && item2.TNType == TerminalType.ContactCom) ||
                            (item1.TNType == TerminalType.ContactCom && item2.TNType == TerminalType.ContactNormalClose)){
                            INotifyComponentChanged info = new ComponentViewModel(Tuple.Create<TNode, TNode>(item1, item2), ComponentType.ContactClose);
                            return new TNodeUI(info, false, observer);
                        }
                        else if (item1.TNType == TerminalType.Indicator && item2.TNType == TerminalType.Indicator){
                            INotifyComponentChanged info = new ComponentViewModel(Tuple.Create<TNode, TNode>(item1, item2), ComponentType.Indicator);
                            return new TNodeUI(info, false, observer);
                        }
                        else if (item1.PartType.Contains("电容") && item1.TNType == TerminalType.Normal){
                            INotifyComponentChanged info = new ComponentViewModel(Tuple.Create<TNode, TNode>(item1, item2), ComponentType.Capacitance);
                            return new TNodeUI(info, false, observer);
                        }
                        else if (item1.PartType.Contains("电阻") && item1.TNType == TerminalType.Normal)
                        {
                            INotifyComponentChanged info = new ComponentViewModel(Tuple.Create<TNode, TNode>(item1, item2), ComponentType.Resistance);
                            return new TNodeUI(info, false, observer);
                        }
                    }
                }
                else
                {
                    INotifyComponentChanged info = new ComponentViewModel(Tuple.Create<TNode, TNode>(item1, item2), ComponentType.Terminal);
                    return new TNodeUI(info, false, observer);
                }
            }
            if (item1 == null)
                return null;
            else
            {
                INotifyComponentChanged info = new ComponentViewModel(Tuple.Create<TNode, TNode>(item1, item2), ComponentType.Terminal);
                return new TNodeUI(info, false, observer);
            }
        }

        public static UIElement convert(TNodeUI node)
        {
            UIElement rst = null;
            switch (node.Info.CptType)
            {
                case ComponentType.Terminal:
                    {
                        rst = new Terminal(node.Info);
                        break;
                    }
                case ComponentType.Switch:
                    {
                        rst = new Switch(node.Info);
                        break;
                    }
                case ComponentType.Resistance:
                    {
                        rst = new Resistance(node.Info);
                        break;
                    }
                case ComponentType.Indicator:
                    {
                        rst = new Indicator(node.Info);
                        break;
                    }
                case ComponentType.Diode:
                    {
                        rst = new Diode(node.Info, node.Reverse);
                        break;
                    }
                case ComponentType.ContactClose:
                case ComponentType.ContactOpen:
                    {
                        rst = new StandardContact(node.Info);
                        break;
                    }
                case ComponentType.Coil:
                    {
                        rst = new Coil(node.Info);
                        break;
                    }
                case ComponentType.Capacitance:
                    {
                        rst = new Capacitance(node.Info);
                        break;
                    }
                case ComponentType.Breaker:
                    {
                        rst = new Breaker(node.Info);
                        break;
                    }
                case ComponentType.Blank:
                    {
                        rst = new BlankLine(node.Info);
                        break;
                    }
            }
            return rst;
        }
            
    }
}
