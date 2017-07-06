using ElectricAnalysis.Graph;
using ElectricAnalysis.Model;
using ElectricAnalysis.Model.Authority;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace ElectricAnalysis.ViewModel
{
    /// <summary>
    /// 所有元器件类型
    /// </summary>
    public enum ComponentType
    {
        Breaker,
        Capacitance,
        Coil,
        ContactOpen,
        ContactClose,
        Diode,
        Indicator,
        Resistance,
        Switch,
        Terminal,
        Blank
    }
    /// <summary>
    /// 元器件的四种状态
    /// </summary>
    public enum ComponentState
    {
        Connected,//正常导通
        UnConnected,//正常未导通
        Fault0,//断路故障
        Fault1//短路故障
    }
    class ComponentViewModel : ViewModelBase, INotifyComponentChanged
    {
        #region Construction
        public ComponentViewModel(Tuple<TNode, TNode> tuple, ComponentType type)
        {
            this.CptType = type;
            this.tuple = tuple;
            this.LabelClick = new RelayCommand<string>(notifyLabelClicked);
            this.ShowAllLabels = new RelayCommand<string>(showLabels, canShowLabels);
            this.PowUp = new RelayCommand<string>(powUp, CanPowUp);
            this.PowDown = new RelayCommand<string>(powDown, CanPowDown);
            this.ManualBreak = new RelayCommand(ManualSetValue, CanManualSetValue);
            this.SetFault0 = new RelayCommand(setFault0, canSetFault0);
            this.SetFault1 = new RelayCommand(setFault1, canSetFault1);
            this.ResetFault = new RelayCommand(resetFault, canResetFault);

            if (tuple.Item1 != null)
            {
                this.Name = tuple.Item1.Part + "/" + tuple.Item1.Num;
                this.Label1 = tuple.Item1.Equal;
            }
            if (tuple.Item2 != null)
            {
                this.Name += "-" + tuple.Item2.Num;
                this.Label2 = tuple.Item2.Equal;
            }
        }
        #endregion

        #region Command
        public ICommand LabelClick { get; set; }//节点被选中
        public ICommand ShowAllLabels { get; set; }//显示所有相同编号的网络节点
        public ICommand PowUp { get; set; }//施加电压
        public ICommand PowDown { get; set; }//撤销电压
        public ICommand ManualBreak { get; set; }//手动断开
        public ICommand SetFault0 { get; set; }//插入故障0
        public ICommand SetFault1 { get; set; }//插入故障1
        public ICommand ResetFault { get; set; }//撤销故障
        #endregion

        #region Property
        private double WIDTH = 150d;
        private double HEIGHT = 60d;
        public double Width
        {
            get
            {
                return WIDTH;
            }
            set
            {
                WIDTH = value;
                base.RaisePropertyChanged("Width");
            }
        }
        public double Height
        {
            get
            {
                return HEIGHT;
            }
            set
            {
                HEIGHT = value;
                base.RaisePropertyChanged("Height");
            }
        }
        private Tuple<TNode, TNode> tuple;
        public ComponentType CptType { get; set; }
        private bool isConnect;//表示元件的连通状态
        public bool IsConnect
        {
            get { return isConnect; }
            set { this.isConnect = value; base.RaisePropertyChanged("HasAction"); }
        }
        public int HasAction
        {
            get
            {
                if (CptType == ComponentType.ContactOpen && isConnect)
                    return 1;//表示常开触点闭合
                else if (CptType == ComponentType.ContactClose && !isConnect)
                    return 2;//表示常闭触点断开
                else if ((CptType == ComponentType.Breaker || CptType == ComponentType.Switch)
                    && !isConnect)
                    return 2;//表示触点断开
                else
                    return 0;//表示触点保持初始状态
            }
        }
        public string Name { get; set; }//元件的名称
        public string Label1 { get; set; }//等电位点一
        public string Label2 { get; set; }//等电位点二
        private bool hightlightLabel1 = false;//Label1的文字颜色
        public bool HightlightLabel1
        {
            get
            {
                return hightlightLabel1;
            }
            set
            {
                this.hightlightLabel1 = value;
                base.RaisePropertyChanged("HightlightLabel1");
            }
        }
        private bool hightlightLabel2 = false;//Label2的文字颜色
        public bool HightlightLabel2
        {
            get
            {
                return hightlightLabel2;
            }
            set
            {
                this.hightlightLabel2 = value;
                base.RaisePropertyChanged("HightlightLabel2");
            }
        }
        private ComponentState state = ComponentState.UnConnected;//节点状态
        public ComponentState State
        {
            get
            {
                return state;
            }
            set
            {
                this.state = value;
                base.RaisePropertyChanged("State");
            }
        }
        private bool hasPow = false;//节点上电标志
        public bool HasPow
        {
            get { return hasPow; }
            set { hasPow = value; base.RaisePropertyChanged("HasPow"); }
        }

        #endregion

        #region Function
        /// <summary>
        /// 移除设置的故障
        /// </summary>
        private void resetFault()
        {
            Simulation sim = Simulation.getInstance();
            sim.removeNodeFault(this);
            sim.showConductRoutes();
            sim.updateOutPutSource();
        }
        private bool canResetFault()
        {
            return ModeManager.getAuthority().canOperate() && (state == ComponentState.Fault0 || state == ComponentState.Fault1);
        }
        /// <summary>
        /// 元件注入短路故障
        /// </summary>
        private void setFault1()
        {
            this.State = ComponentState.Fault1;
            Simulation sim = Simulation.getInstance();
            sim.addNodeFault(true, this);
            sim.showConductRoutes();
            sim.updateOutPutSource();
        }
        private bool canSetFault1()
        {
            return ModeManager.getAuthority().canOperate() && state != ComponentState.Fault1;
        }
        /// <summary>
        /// 元件注入断路故障
        /// </summary>
        private void setFault0()
        {
            this.State = ComponentState.Fault0;
            Simulation sim = Simulation.getInstance();
            sim.addNodeFault(false, this);
            sim.showConductRoutes();
            sim.updateOutPutSource();
        }
        private bool canSetFault0()
        {
            return ModeManager.getAuthority().canOperate() && state != ComponentState.Fault0;
        }
        /// <summary>
        /// 手动设置值
        /// </summary>
        private bool CanManualSetValue()
        {
            return ModeManager.getAuthority().canOperate() &&
                (CptType == ComponentType.Breaker || CptType == ComponentType.Switch);
        }
        private void ManualSetValue()
        {
            Simulation sim = Simulation.getInstance();
            bool connect = isConnect;
            if (connect)
                sim.blockComponent(tuple.Item1.Part, CptType);
            else
                sim.connectComponent(tuple.Item1.Part, CptType);
            sim.showConductRoutes();
            sim.updateOutPutSource();
            if (ModeManager.getAuthority().needSimComparator())
            {
                Simulation sim2 = Simulation.getAnotherSimulation();
                if (connect)
                    sim2.blockComponent(tuple.Item1.Part, CptType);
                else
                    sim2.connectComponent(tuple.Item1.Part, CptType);
                sim2.updateOutPutSource();
            }
        }
        /// <summary>
        /// 初始化元件状态
        /// </summary>
        public void initStatus(ComponentType type)
        {
            this.CptType = type;
            switch (type)
            {
                case ComponentType.Breaker:
                case ComponentType.Coil:
                case ComponentType.Diode:
                case ComponentType.Indicator:
                case ComponentType.Resistance:
                case ComponentType.Switch:
                case ComponentType.Terminal:
                case ComponentType.Blank:
                    {
                        IsConnect = true;
                        break;
                    }
                case ComponentType.Capacitance:
                    {
                        IsConnect = false;
                        break;
                    }
                case ComponentType.ContactOpen:
                    {
                        IsConnect = false;
                        break;
                    }
                case ComponentType.ContactClose:
                    {
                        IsConnect = true;
                        break;
                    }
            }
        }

        /// <summary>
        /// 列表显示所有的等效节点
        /// </summary>
        private void showLabels(string label)
        {
            ISet<TNode> nodes = AppProject.GetInstance().Equals[label];
            Messenger.Default.Send<Tuple<string, DataView>>(
                Tuple.Create<string, DataView>(label, addEqualCollection(nodes, label))
                , "DisplayTestBranch");
        }
        /// <summary>
        /// 不存在该网络标号时不执行任何操作
        /// </summary>
        private bool canShowLabels(string arg)
        {
            return !string.IsNullOrEmpty(arg);
        }
        /// <summary>
        /// 添加等电位点的所有节点名称集合
        /// </summary>
        private DataView addEqualCollection(ICollection<TNode> nodes, string label)
        {
            DataTable table = new DataTable();
            table.TableName = label;
            table.Columns.AddRange(new DataColumn[]{
                    new DataColumn("部件名", Type.GetType("System.String")),
                    new DataColumn("端子号", Type.GetType("System.String"))
            });
            foreach (TNode nd in nodes)
            {
                DataRow row = table.NewRow();
                row[0] = nd.Part;
                row[1] = nd.Num;
                table.Rows.Add(row);
            }
            return table.AsDataView();
        }
        /// <summary>
        /// 通知标号被选中
        /// </summary>
        public void notifyLabelClicked(string label)
        {
            Messenger.Default.Send<string>(label, "LabelClicked");
        }
        /// <summary>
        /// 反转网络标号的颜色
        /// </summary>
        /// <param name="label">网络标号</param>
        public void reverseNetLabelColor(string label)
        {
            if (Label1 == label)
                HightlightLabel1 = !hightlightLabel1;
            if (Label2 == label)
                HightlightLabel2 = !hightlightLabel2;
        }
        /// <summary>
        /// 反转加电标志
        /// </summary>
        /// <param name="label"></param>
        public void reversePowMark(string label)
        {
            if (Label1 == label)
                HasPow = !hasPow;
        }
        /// <summary>
        /// 反转元件的颜色
        /// </summary>
        /// <param name="node">节点</param>
        public void markNode()
        {
            if (state != ComponentState.Fault0 && state != ComponentState.Fault1)
                State = ComponentState.Connected;
        }
        public void resetNode()
        {
            State = ComponentState.UnConnected;
            HasPow = false;
            initStatus(this.CptType);
        }
        public bool isCFNode()
        {
            if (tuple.Item1 != null)
            {
                if (tuple.Item1.PartType.Contains("接口连接器"))
                    return true;
            }
            if (tuple.Item2 != null)
            {
                if (tuple.Item2.PartType.Contains("接口连接器"))
                    return true;
            }
            return false;
        }

        public void connect()
        {
            IsConnect = true;
        }

        public void block()
        {
            IsConnect = false;
        }

        public bool CanConduct(bool order)
        {
            if (state == ComponentState.Fault0)
                return false;
            else if (state == ComponentState.Fault1)
                return true;
            if (CptType == ComponentType.Diode)
            {
                if (order)
                {
                    if (tuple.Item1.TNType == TerminalType.DiodePositive && tuple.Item2.TNType == TerminalType.DiodeNegative)
                        return true;
                    else
                        return false;
                }
                else
                {
                    if (tuple.Item1.TNType == TerminalType.DiodeNegative && tuple.Item2.TNType == TerminalType.DiodePositive)
                        return true;
                    else
                        return false;
                }
            }
            else
            {
                return isConnect;
            }
        }
        /// <summary>
        /// 返回元件的名称
        /// </summary>
        public string getName()
        {
            if (tuple.Item1 != null)
                return tuple.Item1.Part;
            else
                return "";
        }
        public TNode getHeadNode()
        {
            return tuple.Item1;
        }

        public TNode getTailNode()
        {
            return tuple.Item2;
        }

        public bool CanPowUp(string label)
        {
            if (string.IsNullOrEmpty(label))
                return false;
            else
            {
                ISet<string> pows = Simulation.getInstance().getPows();
                HashSet<TNode> nodes = (HashSet<TNode>)AppProject.GetInstance().Equals[label];
                return ModeManager.getAuthority().canOperate()
                    && (!label.Equals("GND"))
                    && (label.Equals("DC110V") || nodes.Any(p => p.PartType.StartsWith("接口连接器"))) 
                    && !pows.Contains(label);
            }
        }
        private void powUp(string label)
        {
            Simulation sim = Simulation.getInstance();
            sim.powUp(label);
            sim.showConductRoutes();
            sim.updateOutPutSource();
            if (ModeManager.getAuthority().needSimComparator())
            {
                Simulation sim2 = Simulation.getAnotherSimulation();
                sim2.powUp(label);
                sim2.updateOutPutSource();
            }
        }
        private bool CanPowDown(string label)
        {
            if (string.IsNullOrEmpty(label))
                return false;
            else
            {
                ISet<string> pows = Simulation.getInstance().getPows();
                HashSet<TNode> nodes = (HashSet<TNode>)AppProject.GetInstance().Equals[label];
                return ModeManager.getAuthority().canOperate() 
                    && (label.Equals("DC110V") || nodes.Any(p => p.PartType.StartsWith("接口连接器")))
                    && pows.Contains(label);
            }
        }

        private void powDown(string label)
        {
            Simulation sim = Simulation.getInstance();
            sim.powDown(label);
            sim.showConductRoutes();
            sim.updateOutPutSource();
            if (ModeManager.getAuthority().needSimComparator())
            {
                Simulation sim2 = Simulation.getAnotherSimulation();
                sim2.powDown(label);
                sim2.updateOutPutSource();
            }
        }

        public void reverseStatus()
        {
            isConnect = !isConnect;
        }


        public INotifyComponentChanged clone()
        {
            return (INotifyComponentChanged)this.MemberwiseClone();
        }
        public static bool compare(INotifyComponentChanged a, INotifyComponentChanged b)
        {
            return (a.getHeadNode() == b.getHeadNode() && a.getTailNode() == b.getTailNode()) || (a.getHeadNode() == b.getTailNode() && a.getTailNode() == b.getHeadNode());
        }
        #endregion
    }
}
