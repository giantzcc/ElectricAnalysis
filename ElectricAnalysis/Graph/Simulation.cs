using ElectricAnalysis.Model;
using ElectricAnalysis.Model.Result;
using ElectricAnalysis.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Graph
{
    public class Simulation : ISimulation
    {
        #region 将单例改成双例
        private Simulation()
        {
            AppProject app = AppProject.GetInstance();
            IShortBranchTreeGenerator graph = new DigraphBuilder();
            graph.buildRoutes();
            if (app.VccToGnd == null)
                this.VccToGndView = graph.getVccToGndBranch();
            else
                this.VccToGndView = app.VccToGnd;
            if (app.GndToCF == null)
                this.CFToGndView = graph.getCFToGndBranch();
            else
                this.CFToGndView = app.GndToCF;
            if (app.VccToCF == null)
                this.CFToVccView = graph.getVccToCFBranch();
            else
                this.CFToVccView = app.VccToCF;
            load();
        }
        public static Simulation getInstance()
        {
            if (instance == null)
            {
                lock (sync)
                {
                    if (instance == null)
                    {
                        instance = new Simulation();
                    }
                }
            }
            return instance;
        }
        public static Simulation getAnotherSimulation()
        {
            if (instance1 == null)
            {
                lock (sync1)
                {
                    if (instance1 == null)
                    {
                        instance1 = Simulation.getInstance().clone();
                    }
                }
            }
            return instance1;
        }
        #endregion

        #region Field
        private static object sync = new object();//锁持有的对象
        private volatile static Simulation instance;//实例
        private static object sync1 = new object();//锁持有的对象
        private volatile static Simulation instance1;//实例
        private BranchView VccToGndView;//总正至总负电路拓扑
        private BranchView CFToGndView;//测试点至总负电路拓扑
        private BranchView CFToVccView;//测试点至总正电路拓扑
        private ISet<string> pows = new HashSet<string>();//所有的加电点
        private ISet<INotifyComponentChanged> allCpts = new HashSet<INotifyComponentChanged>();//所有的元件
        private ISet<string> coils = new HashSet<string>();//得电线圈
        private ISet<string> switches = new HashSet<string>();//手动断开的开关
        private ISet<INotifyComponentChanged> faultPoints = new HashSet<INotifyComponentChanged>();//设置的故障点
        private List<List<INotifyComponentChanged>> vccToGndRoutes = new List<List<INotifyComponentChanged>>();//Vcc到Gnd的所有的回路
        private List<List<INotifyComponentChanged>> cfToGndRoutes = new List<List<INotifyComponentChanged>>();//CF到Gnd的所有的回路
        private List<List<INotifyComponentChanged>> cfToVccRoutes = new List<List<INotifyComponentChanged>>();//CF到Vcc的所有回路
        private List<List<INotifyComponentChanged>> vccConductRoutes = new List<List<INotifyComponentChanged>>();//总正导通的支路
        private List<List<INotifyComponentChanged>> cfConductRoutes = new List<List<INotifyComponentChanged>>();//测试点导通的支路
        public ISet<INotifyComponentChanged> AllCpts
        {
            get { return allCpts; }
        }
        public List<List<INotifyComponentChanged>> VccToGndRoutes
        {
            get
            {
                List<List<INotifyComponentChanged>> rst = new List<List<INotifyComponentChanged>>();
                vccToGndRoutes.ForEach(p => rst.Add(new List<INotifyComponentChanged>(p)));
                return rst;
            }
        }
        public List<List<INotifyComponentChanged>> CfToGndRoutes
        {
            get
            {
                List<List<INotifyComponentChanged>> rst = new List<List<INotifyComponentChanged>>();
                cfToGndRoutes.ForEach(p => rst.Add(new List<INotifyComponentChanged>(p)));
                return rst;
            }
        }
        public List<List<INotifyComponentChanged>> CfToVccRoutes
        {
            get
            {
                List<List<INotifyComponentChanged>> rst = new List<List<INotifyComponentChanged>>();
                cfToVccRoutes.ForEach(p => rst.Add(new List<INotifyComponentChanged>(p)));
                return rst;
            }
        }
        public List<List<INotifyComponentChanged>> VccConductRoutes
        {
            get
            {
                List<List<INotifyComponentChanged>> rst = new List<List<INotifyComponentChanged>>();
                vccConductRoutes.ForEach(p => rst.Add(new List<INotifyComponentChanged>(p)));
                return rst;
            }
        }
        public List<List<INotifyComponentChanged>> CfConductRoutes
        {
            get
            {
                List<List<INotifyComponentChanged>> rst = new List<List<INotifyComponentChanged>>();
                cfConductRoutes.ForEach(p => rst.Add(new List<INotifyComponentChanged>(p)));
                return rst;
            }
        }
        public CFDisplay OutPuts { get; set; }//测试点的输出值
        public SourceType srcType { get; set; }//指定输出值到CFDisplay的哪个源中
        #endregion

        #region Function
        #region 更新所有的CF输出点
        public void updateOutPutSource()
        {
            ISet<TNode> sets = getPowedNodes();
            OutPuts.updateCfPairs(sets, srcType);
        }
        private ISet<TNode> getPowedNodes()
        {
            ISet<TNode> cfs = new HashSet<TNode>();
            ISet<string> labels = new HashSet<string>();
            Action<INotifyComponentChanged> action = p =>
            {
                labels.Add(p.getHeadNode().Equal);
                if (p.getTailNode() != null)
                    labels.Add(p.getTailNode().Equal);
            };
            addPowedLabels(vccToGndRoutes, action);
            addPowedLabels(cfToGndRoutes, action);
            addPowedLabels(cfToVccRoutes, action);
            AppProject app = AppProject.GetInstance();
            foreach (string label in labels)
            {
                if(label!="GND"){
                    var query = app.Equals[label].Where(p => p.PartType.Contains("接口连接器") && p.TNType == TerminalType.Normal);
                    foreach (var nd in query)
                        cfs.Add(nd);
                }
            }
            return cfs;
        }
        private void addPowedLabels(List<List<INotifyComponentChanged>> src, Action<INotifyComponentChanged> addAction)
        {
            foreach (List<INotifyComponentChanged> cmps in src)
            {
                for (int i = 0; i < cmps.Count;i++ )
                {
                    int down = -1, up = cmps.Count;
                    if (pows.Contains(cmps[i].getHeadNode().Equal))
                    {
                        down = i - 1;
                        up = i;
                    }
                    else if (cmps[i].getTailNode() != null && pows.Contains(cmps[i].getTailNode().Equal))
                    {
                        down = i;
                        up = i + 1;
                    }
                    while (down >= 0)
                    {
                        if (cmps[down].CanConduct(false))
                            addAction(cmps[down]);
                        else
                            break;
                        down--;
                    }
                    while (up < cmps.Count)
                    {
                        if (cmps[up].CanConduct(true))
                            addAction(cmps[up]);
                        else
                            break;
                        up++;
                    }
                }
            }
        }
        #endregion

        #region 初始化第二个辅助仿真器
        /// <summary>
        /// 克隆一个仿真器
        /// </summary>
        private Simulation clone()
        {
            Simulation cp = (Simulation)this.MemberwiseClone();
            cp.pows = new HashSet<string>(pows);
            cp.coils = new HashSet<string>(coils);
            cp.switches = new HashSet<string>(switches);
            cp.vccToGndRoutes = new List<List<INotifyComponentChanged>>();
            cp.cfToGndRoutes = new List<List<INotifyComponentChanged>>();
            cp.cfToVccRoutes = new List<List<INotifyComponentChanged>>();
            cp.vccConductRoutes = new List<List<INotifyComponentChanged>>();
            cp.cfConductRoutes = new List<List<INotifyComponentChanged>>();
            vccToGndRoutes.ForEach(p => cp.vccToGndRoutes.Add(new List<INotifyComponentChanged>(p)));
            cfToGndRoutes.ForEach(p => cp.cfToGndRoutes.Add(new List<INotifyComponentChanged>(p)));
            cfToVccRoutes.ForEach(p => cp.cfToVccRoutes.Add(new List<INotifyComponentChanged>(p)));
            vccConductRoutes.ForEach(p=>cp.vccConductRoutes.Add(new List<INotifyComponentChanged>(p)));
            cfConductRoutes.ForEach(p=>cp.cfConductRoutes.Add(new List<INotifyComponentChanged>(p)));
            ISet<INotifyComponentChanged> cpts = new HashSet<INotifyComponentChanged>();
            foreach (INotifyComponentChanged one in allCpts)
            {
                INotifyComponentChanged ncp = one.clone();
                replaceElement(cp.vccToGndRoutes, one, ncp);
                replaceElement(cp.cfToGndRoutes, one, ncp);
                replaceElement(cp.cfToVccRoutes, one, ncp);
                replaceElement(cp.vccConductRoutes, one, ncp);
                replaceElement(cp.cfConductRoutes, one, ncp);
                cpts.Add(ncp);
            }
            cp.allCpts = cpts;
            return cp;
        }
        private void replaceElement(List<List<INotifyComponentChanged>> src, INotifyComponentChanged old, INotifyComponentChanged cp)
        {
            List<INotifyComponentChanged> col = src.FirstOrDefault(p => p.Contains(old));
            if (col != null)
            {
                col[col.IndexOf(old)] = cp;
            }
        }
        #endregion
        /// <summary>
        /// 获取注电点
        /// </summary>
        public ISet<string> getPows()
        {
            return new HashSet<string>(pows);
        }
        /// <summary>
        /// 获取得电的线圈
        /// </summary>
        public ISet<string> getPowCoils()
        {
            return new HashSet<string>(coils);
        }
        /// <summary>
        /// 获取被手动断开的开关
        /// </summary>
        public ISet<string> getBrokenSwitches()
        {
            return new HashSet<string>(switches);
        }
        /// <summary>
        /// 获取设置的故障节点
        /// </summary>
        public ISet<INotifyComponentChanged> getFaultComponents()
        {
            return new HashSet<INotifyComponentChanged>(faultPoints);
        }
        /// <summary>
        /// 判断某个点有没有有效上电
        /// </summary>
        /// <param name="label">等电位标号</param>
        public bool containsPow(string label)
        {
            return pows.Contains(label);
        }
        /// <summary>
        /// 恢复至未仿真状态
        /// </summary>
        public void resetSimulation()
        {
            pows.Clear();
            coils.Clear();
            switches.Clear();
            vccConductRoutes.Clear();
            cfConductRoutes.Clear();
            foreach (INotifyComponentChanged cpt in allCpts)
                cpt.resetNode();
        }
        /// <summary>
        /// 将ShortBranchNode表示的支路展开成INotifyComponentChanged表示的支路
        /// </summary>
        private List<List<INotifyComponentChanged>> convertToNodes(List<List<ShortBranchNode>> rts)
        {
            List<List<INotifyComponentChanged>> rst = new List<List<INotifyComponentChanged>>();
            foreach (List<ShortBranchNode> rt in rts)
            {
                List<INotifyComponentChanged> infos = new List<INotifyComponentChanged>();
                foreach (ShortBranchNode brNode in rt)
                {
                    infos.AddRange(brNode.Uis.Select(p => p.Info).ToList());
                }
                rst.Add(infos);
            }
            return rst;
        }
        /// <summary>
        /// 整理出所有的回路
        /// </summary>
        private void load()
        {
            //Vcc至Gnd的通路
            ISet<ShortBranchNode> allNodes = DigraphBuilder.expandBranch(VccToGndView.Nodes);
            foreach (ShortBranchNode node in allNodes)
            {
                node.Uis.ForEach(p => allCpts.Add(p.Info));
                if (node.Nodes.Count == 0 && node.EndName != "GND")
                {
                    foreach (var child in VccToGndView.LabelToBranch[node.EndName].Nodes)
                        node.Nodes.Add(child);
                }
            }
            List<List<ShortBranchNode>> rts1 = DigraphBuilder.getAllRoutes(VccToGndView.Nodes);
            vccToGndRoutes.AddRange(convertToNodes(rts1));
            //CF至Gnd通路
            allNodes = DigraphBuilder.expandBranch(CFToGndView.Nodes);
            foreach (ShortBranchNode node in allNodes)
            {
                node.Uis.ForEach(p => allCpts.Add(p.Info));
                if (node.Nodes.Count == 0)
                {
                    if (CFToGndView.LabelToBranch.ContainsKey(node.EndName))
                    {
                        foreach (var child in CFToGndView.LabelToBranch[node.EndName].Nodes)
                            node.Nodes.Add(child);
                    }
                }
            }
            List<List<ShortBranchNode>> rts2 = DigraphBuilder.getAllRoutes(CFToGndView.Nodes);
            cfToGndRoutes.AddRange(convertToNodes(rts2));
            //Vcc至CF的通路
            allNodes = DigraphBuilder.expandBranch(CFToVccView.Nodes);
            foreach (ShortBranchNode node in allNodes)
            {
                node.Uis.ForEach(p => allCpts.Add(p.Info));
                if (node.Nodes.Count == 0)
                {
                    if (CFToVccView.LabelToBranch.ContainsKey(node.EndName))
                    {
                        foreach (var child in CFToVccView.LabelToBranch[node.EndName].Nodes)
                            node.Nodes.Add(child);
                    }
                }
            }
            List<List<ShortBranchNode>> rts3 = DigraphBuilder.getAllRoutes(CFToVccView.Nodes);
            cfToVccRoutes.AddRange(convertToNodes(rts3));
        }

        #region 模拟某节点发生故障
        /// <summary>
        /// 注入故障
        /// </summary>
        /// <param name="faultType">true:固定1故障 false:固定0故障</param>
        /// <param name="node">故障节点</param>
        public void addNodeFault(bool faultType, INotifyComponentChanged node)
        {
            this.faultPoints.Add(node.clone());
            if (faultType)
                modifyComponent(node, p =>
                    p.State = ComponentState.Fault1);
            else
                modifyComponent(node, p =>  
                    p.State = ComponentState.Fault0);
        }
        public void removeNodeFault(INotifyComponentChanged node)
        {
            this.faultPoints.Remove(this.faultPoints.FirstOrDefault(p=>
                ComponentViewModel.compare(p, node)));
            modifyComponent(node, p =>
                    p.State = ComponentState.UnConnected);
        }
        private void modifyComponent(INotifyComponentChanged node, Action<INotifyComponentChanged> action)
        {
            foreach (INotifyComponentChanged cpt in allCpts)
            {
                if (ComponentViewModel.compare(cpt, node))
                    action(cpt);
            }
            ISet<string> ele = new HashSet<string>();
            while (!powUpRoute(ele)) ;
        }
        #endregion

        #region 模拟开关或者断路器切换
        public void blockComponent(string name, ComponentType type)
        {
            switches.Add(name);
            modifyComponent(name, type, p => p.block());
        }
        public void connectComponent(string name, ComponentType type)
        {
            switches.Remove(name);
            modifyComponent(name, type, p => p.connect());
        }
        private void modifyComponent(string name, ComponentType type, Action<INotifyComponentChanged> action)
        {
            foreach (INotifyComponentChanged cpt in allCpts)
            {
                if (cpt.CptType == type && cpt.getName() == name)
                    action(cpt);
            }
            ISet<string> ele = new HashSet<string>();
            while (!powUpRoute(ele)) ;
        }
        #endregion

        public void powUp(string label)
        {
            pows.Add(label);
            ISet<string> ele = new HashSet<string>();
            while (!powUpRoute(ele)) ;
        }

        public void powDown(string label)
        {
            pows.Remove(label);
            ISet<string> ele = new HashSet<string>();
            if (!powDownRoute(label))
            {
                while (!powUpRoute(ele)) ;
                while (!powDownRoute())
                {
                    while (!powUpRoute(ele)) ;
                }
            }
        }

        public void showConductRoutes()
        {
            ISet<INotifyComponentChanged> total = new HashSet<INotifyComponentChanged>();
            foreach (List<INotifyComponentChanged> rt in vccConductRoutes)
                rt.ForEach(p => total.Add(p));
            foreach (List<INotifyComponentChanged> rt in cfConductRoutes)
                rt.ForEach(p => total.Add(p));
            Messenger.Default.Send<ISet<INotifyComponentChanged>>(total, "ShowConductRoute");
        }
        private bool powDownRoute()
        {
            //检测剩下的导通的回路里是否满足导通条件
            List<List<INotifyComponentChanged>> vccbroken = getBreakRoute(vccConductRoutes, true);
            List<List<INotifyComponentChanged>> cfbroken = getBreakRoute(cfConductRoutes, false);
            vccbroken.ForEach(p => vccConductRoutes.Remove(p));
            cfbroken.ForEach(p => cfConductRoutes.Remove(p));
            //取出断开回路中的线圈
            ISet<string> _coils = new HashSet<string>();
            foreach (List<INotifyComponentChanged> rt in vccbroken)
            {
                foreach (INotifyComponentChanged info in rt)
                {
                    if (info.CptType == ViewModel.ComponentType.Coil)
                        _coils.Add(info.getName());
                }
            }
            foreach (List<INotifyComponentChanged> rt in cfbroken)
            {
                foreach (INotifyComponentChanged info in rt)
                {
                    if (info.CptType == ViewModel.ComponentType.Coil)
                        _coils.Add(info.getName());
                }
            }
            //将失电线圈的触点状态更新
            bool mark = true;
            foreach (string name in _coils)
            {
                if (coils.Contains(name) && needUpdate(name))
                {
                    mark = false;
                    coils.Remove(name);
                    applyAll(
                        info => (info.CptType == ViewModel.ComponentType.ContactOpen || info.CptType == ViewModel.ComponentType.ContactClose) 
                            && info.getName() == name,
                        p => { p.reverseStatus(); });
                }
            }
            return mark;
        }

        private bool powDownRoute(string label)
        {
            //将与撤销的加电点的导通回路移除
            ISet<string> _coils = new HashSet<string>();
            //总正被断开
            List<List<INotifyComponentChanged>> temp = new List<List<INotifyComponentChanged>>();
            foreach (List<INotifyComponentChanged> rt in vccConductRoutes)
            {
                if (rt[0].getHeadNode().Equal == label)
                {
                    temp.Add(rt);
                    foreach (INotifyComponentChanged info in rt)
                    {
                        if (info.CptType == ViewModel.ComponentType.Coil)
                            _coils.Add(info.getName());
                    }
                }

            }
            temp.ForEach(p => vccConductRoutes.Remove(p));

            //测试点被断开
            temp = new List<List<INotifyComponentChanged>>();
            foreach (List<INotifyComponentChanged> rt in cfConductRoutes)
            {
                if ((rt[rt.Count - 1].getTailNode() == null && rt[rt.Count - 1].getHeadNode().Equal == label)
                    || (rt[rt.Count - 1].getTailNode() != null && rt[rt.Count - 1].getTailNode().Equal == label))
                {
                    temp.Add(rt);
                    foreach (INotifyComponentChanged info in rt)
                    {
                        if (info.CptType == ViewModel.ComponentType.Coil)
                            _coils.Add(info.getName());
                    }
                }
            }
            temp.ForEach(p => cfConductRoutes.Remove(p));
            //判断哪些线圈会失电
            bool mark = true;
            foreach (string name in _coils)
            {
                if (coils.Contains(name) && needUpdate(name))
                {
                    mark = false;
                    coils.Remove(name);
                    applyAll(
                        info => (info.CptType == ViewModel.ComponentType.ContactOpen || info.CptType == ViewModel.ComponentType.ContactClose)
                            && info.getName() == name,
                        p => { p.reverseStatus(); });
                }
            }
            return mark;
        }

        private bool powUpRoute(ISet<string> getElectric)
        {
            //整理出所有导通的支路
            ISet<string> _coils = new HashSet<string>();
            collectRoutes(vccToGndRoutes, vccConductRoutes, true, _coils);
            collectRoutes(cfToGndRoutes, cfConductRoutes, false, _coils);
            //检测已经导通的支路会不会被互斥断开
            bool breakMark = true;
            int checkrst = checkConduct(vccConductRoutes, true, getElectric);
            if (checkrst == 0)
            {
                resetSimulation();
                throw new SimulationLogicException();
            }
            else if (checkrst == 1)
            {
                breakMark = false;
            }
            checkrst = checkConduct(cfConductRoutes, false, getElectric);
            if (checkrst == 0)
            {
                resetSimulation();
                throw new SimulationLogicException();
            }
            else if (checkrst == 1)
            {
                breakMark = false;
            }
            //导通的线圈对应的触点动作
            bool mark = true;
            foreach (string name in _coils)
            {
                if (!coils.Contains(name))
                {
                    mark = false;
                    coils.Add(name);
                    getElectric.Add(name);
                    applyAll(
                        info => (info.CptType == ViewModel.ComponentType.ContactOpen || info.CptType == ViewModel.ComponentType.ContactClose)
                            && info.getName() == name,
                        p => { p.reverseStatus(); });
                }
            }
            return mark & breakMark;
        }
        /// <summary>
        /// 将所有元件中满足一定条件的元件，执行一定的处理
        /// </summary>
        /// <param name="condition">筛选条件</param>
        /// <param name="action">处理的内容</param>
        private void applyAll(Func<INotifyComponentChanged, bool> condition, Action<INotifyComponentChanged> action)
        {
            foreach (INotifyComponentChanged info in allCpts)
            {
                if (condition(info))
                    action(info);
            }
        }
        private List<List<INotifyComponentChanged>> getBreakRoute(List<List<INotifyComponentChanged>> rts, bool direction)
        {
            List<List<INotifyComponentChanged>> rst = new List<List<INotifyComponentChanged>>();
            foreach (List<INotifyComponentChanged> rt in rts)
            {
                foreach (INotifyComponentChanged cpt in rt)
                {
                    if (!cpt.CanConduct(direction))
                        rst.Add(rt);
                }
            }
            return rst;
        }
        /// <summary>
        /// 检测已经导通的支路会不会被互斥断开
        /// </summary>
        /// <param name="rts">已导通的支路</param>
        /// <param name="direction">导通方向</param>
        private int checkConduct(List<List<INotifyComponentChanged>> rts, bool direction, ISet<string> getElectric)
        {
            bool lost = false;
            List<List<INotifyComponentChanged>> temp = new List<List<INotifyComponentChanged>>();
            foreach (List<INotifyComponentChanged> rt in rts)
            {
                foreach (INotifyComponentChanged cpt in rt)
                {
                    if (!cpt.CanConduct(direction))
                    {
                        temp.Add(rt);
                        break;
                    }
                }
            }
            foreach (List<INotifyComponentChanged> rt in temp)
            {
                rts.Remove(rt);
                foreach (INotifyComponentChanged cpt in rt)
                {
                    if (cpt.CptType == ViewModel.ComponentType.Coil)
                    {
                        string name = cpt.getName();
                        bool update = needUpdate(name);
                        if (getElectric.Contains(name) && update)
                            return 0;//在本次事件中得电的线圈又将要失电，表示出现了逻辑冲突
                        if (coils.Contains(name) && update)
                        {
                            lost = true;
                            coils.Remove(name);
                            applyAll(
                                info => (info.CptType == ViewModel.ComponentType.ContactOpen || info.CptType == ViewModel.ComponentType.ContactClose)
                                    && info.getName() == name,
                                p => { p.reverseStatus(); });
                        }
                    }
                }
            }
            if (lost)
                return 1;//存在线圈失电的情况
            else
                return 2;//不存在线圈失电的情况
        }
        /// <summary>
        /// 整理出所有导通的支路
        /// </summary>
        /// <param name="src">回路集</param>
        /// <param name="dest">筛选出的导通回路集</param>
        /// <param name="direction">导通方向</param>
        /// <param name="_coils">导通回路中的得电线圈</param>
        private void collectRoutes(List<List<INotifyComponentChanged>> src, List<List<INotifyComponentChanged>> dest, bool direction, ISet<string> _coils)
        {
            foreach (List<INotifyComponentChanged> rt in src)
            {
                if (!dest.Contains(rt))
                {
                    List<INotifyComponentChanged> subRt = null;
                    if (direction)
                    {
                        int index = rt.Count;
                        for (int i = rt.Count - 1; i >= 0; i--)
                        {
                            if (!rt[i].CanConduct(direction))
                                break;
                            if (pows.Contains(rt[i].getHeadNode().Equal))
                                index = i;
                        }
                        subRt = rt.GetRange(index, rt.Count - index);
                    }
                    else
                    {
                        int index = -1;
                        for (int i = 0; i < rt.Count; i++)
                        {
                            if (pows.Contains(rt[i].getHeadNode().Equal))
                                index = i - 1;
                            if (!rt[i].CanConduct(direction))
                                break;
                        }
                        subRt = rt.GetRange(0, index + 1);

                    }
                    if (subRt.Count != 0)
                    {
                        bool hasInclude = dest.Exists(p => p.All(q => subRt.Contains(q)) && p.Count == subRt.Count);
                        if (!hasInclude)
                        {
                            dest.Add(subRt);
                            foreach (INotifyComponentChanged cpt in subRt)
                            {
                                if (cpt.CptType == ViewModel.ComponentType.Coil)
                                    _coils.Add(cpt.getName());
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 判断撤销掉某加电点后指定线圈还能否导通
        /// </summary>
        /// <param name="coilName">线圈元件的名称</param>
        /// <returns>是否线圈失电</returns>
        private bool needUpdate(string coilName)
        {
            foreach (List<INotifyComponentChanged> rt in vccConductRoutes)
            {
                foreach (INotifyComponentChanged info in rt)
                {
                    if (info.CptType == ViewModel.ComponentType.Coil && info.getName() == coilName)
                        return false;
                }
            }
            foreach (List<INotifyComponentChanged> rt in cfConductRoutes)
            {
                foreach (INotifyComponentChanged info in rt)
                {
                    if (info.CptType == ViewModel.ComponentType.Coil && info.getName() == coilName)
                        return false;
                }
            }
            return true;
        }
        #endregion
    }
}
