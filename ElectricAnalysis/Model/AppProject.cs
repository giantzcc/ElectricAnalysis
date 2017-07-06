using ElectricAnalysis.Graph;
using ElectricAnalysis.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    /// <summary>
    /// 全局单例对象
    /// </summary>
    class AppProject
    {
        #region Signle Instance
        private volatile static AppProject instance = null;
        private static object syncObj = new object();
        private AppProject()
        {
            cells = new List<OriginCell>();
            nodes = new List<TNode>();
            gndTbs = new List<TNode>();
            gndNet = new GndTestNet();
            vccNet = new VCCTestNet();
            normalNets = new NormalTestNet();
            lgicNets = new LogicTestNet();
            loopNets = new LoopNet();
            equals = new Dictionary<string, ISet<TNode>>();
            shorts = new List<ShortBranch>();
        }
        public static AppProject GetInstance()
        {
            if (instance == null)
            {
                lock (syncObj)
                {
                    if (instance == null)
                        instance = new AppProject();
                }
            }
            return instance;
        }
        #endregion

        #region Field Property
        public static CompareTNode cmpNode = new CompareTNode();
        public string OriginFileName;
        public ReadOnlyCollection<TNode> CFNodes;
        private List<OriginCell> cells;
        internal List<OriginCell> Cells
        {
            get { return cells; }
            set { cells = value; }
        }
        private List<TNode> nodes;
        internal List<TNode> Nodes
        {
            get { return nodes; }
            set { nodes = value; }
        }

        private IDictionary<string, ISet<TNode>> equals;//所有的等电位组合点
        public IDictionary<string, ISet<TNode>> Equals
        {
            get
            {
                return equals;
            }
        }
        private IList<ShortBranch> shorts;//所有的短支路
        public IList<ShortBranch> Shorts
        {
            get
            {
                return shorts;
            }
        }
        private BranchView vccToGnd;
        public BranchView VccToGnd
        {
            get { return vccToGnd; }
            set { vccToGnd = value; }
        }
        private BranchView vccToCF;
        public BranchView VccToCF
        {
            get { return vccToCF; }
            set { vccToCF = value; }
        }
        private BranchView gndToCF;
        public BranchView GndToCF
        {
            get { return gndToCF; }
            set { gndToCF = value; }
        }
        private BranchView completeBr;
        public BranchView CompleteBr
        {
            get { return completeBr; }
            set { completeBr = value; }
        }

        private GndTestNet gndNet;//地线导通测试网络
        private VCCTestNet vccNet;//110V导通测试
        private NormalTestNet normalNets;//普通导通测试
        private LogicTestNet lgicNets;//逻辑测试
        private LoopNet loopNets;//回路网络
        public int[,] weight;//有向图的权重矩阵
        internal LoopNet LoopNets
        {
            get { return loopNets; }
        }
        private List<TNode> gndTbs;//所有与总负相连接的端子排
        internal List<TNode> GndTbs
        {
            get { return gndTbs; }
        }
        #endregion

        #region Function
        /// <summary>
        /// 清空工程信息
        /// </summary>
        public void clearProject()
        {
            cells.Clear();
            nodes.Clear();
            gndTbs.Clear();
            equals.Clear();
            shorts.Clear();
        }
        /// <summary>
        /// 将继电器与接触器的公共触点
        /// 分裂成两个节点
        /// </summary>
        private void splitSharedTermial()
        {
            Configuration config = Configuration.GetInstance();
            foreach (ComponentLib compo in config.Cptlib)
            {
                if (compo.Type.Contains("继电器") || compo.Type.Contains("接触器"))
                {
                    PinInfo pin = null;
                    IDictionary<string, List<Tuple<TerminalType, string>>> reflect = new Dictionary<string, List<Tuple<TerminalType, string>>>();
                    if (compo.Type.Contains("继电器"))
                        pin = config.RelayPins.Find(p => p.MaterialNum == compo.MaterialNum);
                    else
                        pin = config.ContactorPins.Find(p => p.MaterialNum == compo.MaterialNum);
                    if (pin == null || string.IsNullOrEmpty(pin.Pin1) || string.IsNullOrEmpty(pin.Pin2))
                        continue;
                    reflect.Add(pin.Pin1, new List<Tuple<TerminalType, string>>() { Tuple.Create<TerminalType, string>(TerminalType.Coil, pin.Pin2) });
                    reflect.Add(pin.Pin2, new List<Tuple<TerminalType, string>>() { Tuple.Create<TerminalType, string>(TerminalType.Coil, pin.Pin1) });
                    foreach (KeyValuePair<string, string> pair in pin.NoPin)
                    {
                        if (reflect.ContainsKey(pair.Key))
                            reflect[pair.Key].Add(Tuple.Create<TerminalType, string>(TerminalType.ContactCom, pair.Value));
                        else
                            reflect.Add(pair.Key, new List<Tuple<TerminalType, string>>() { Tuple.Create<TerminalType, string>(TerminalType.ContactCom, pair.Value) });
                        if (reflect.ContainsKey(pair.Value))
                            reflect[pair.Value].Add(Tuple.Create<TerminalType, string>(TerminalType.ContactNormalOpen, pair.Key));
                        else
                            reflect.Add(pair.Value, new List<Tuple<TerminalType, string>>() { Tuple.Create<TerminalType, string>(TerminalType.ContactNormalOpen, pair.Key) });
                    }
                    foreach (KeyValuePair<string, string> pair in pin.NcPin)
                    {
                        if (reflect.ContainsKey(pair.Key))
                            reflect[pair.Key].Add(Tuple.Create<TerminalType, string>(TerminalType.ContactCom, pair.Value));
                        else
                            reflect.Add(pair.Key, new List<Tuple<TerminalType, string>>() { Tuple.Create<TerminalType, string>(TerminalType.ContactCom, pair.Value) });
                        if (reflect.ContainsKey(pair.Value))
                            reflect[pair.Value].Add(Tuple.Create<TerminalType, string>(TerminalType.ContactNormalClose, pair.Key));
                        else
                            reflect.Add(pair.Value, new List<Tuple<TerminalType, string>>() { Tuple.Create<TerminalType, string>(TerminalType.ContactNormalClose, pair.Key) });
                    }
                    int count = reflect.Count(p => p.Value.Count > 1);
                    if (count > 1)
                    {
                        var repeat = reflect.Where(p => p.Value.Count > 1).ToList();
                        foreach (KeyValuePair<string, List<Tuple<TerminalType, string>>> pair in repeat)
                        {
                            TNode dest = nodes.FirstOrDefault(p => p.Part == compo.AbbName && p.Num == pair.Key);
                            if (dest != null)
                            {
                                for (int i = 1; i < pair.Value.Count; i++)
                                {
                                    if (nodes.Exists(p => p.Part == compo.AbbName && p.Num == pair.Value[i].Item2))
                                    {
                                        string num = dest.Num;
                                        for (int j = 0; j <= i; j++)
                                            num += "'";
                                        TNode nnd = new TNode(dest.Part, num);
                                        dest.Nodes.Add(nnd);
                                        nnd.Nodes.Add(dest);
                                        nodes.Add(nnd);
                                        reflect.Add(num, new List<Tuple<TerminalType, string>>() { pair.Value[i] });
                                        reflect[pair.Value[i].Item2] = new List<Tuple<TerminalType, string>>() { Tuple.Create<TerminalType, string>(reflect[pair.Value[i].Item2][0].Item1, num) };
                                    }
                                }
                                pair.Value.RemoveRange(1, pair.Value.Count - 1);
                            }
                        }
                    }
                    foreach (string key in reflect.Keys)
                    {
                        Tuple<TerminalType, string> tp = reflect[key][0];
                        TNode node1 = nodes.FirstOrDefault(p => p.Part == compo.AbbName && p.Num == key);
                        TNode node2 = nodes.FirstOrDefault(p => p.Part == compo.AbbName && p.Num == tp.Item2);
                        if (node1 != null && node2 != null)
                        {
                            if (!node1.Nodes.Contains(node2))
                                node1.Nodes.Add(node2);
                        }
                        if (node1 != null)
                        {
                            node1.Brother = node2;
                            node1.TNType = tp.Item1;
                            node1.PartType = compo.Type;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 构造节点图结构
        /// 并初始化权重矩阵
        /// </summary>
        public void BuildGraphData()
        {
            Configuration config = Configuration.GetInstance();
            /*不包含节点关系的图*/
            foreach (var cell in cells)
            {
                TNode fnode = new TNode(cell.StartPart, cell.StartNum);//首节点
                TNode snode = new TNode(cell.EndPart, cell.EndNum);//末节点
                if (!nodes.Contains(fnode, AppProject.cmpNode))
                {
                    nodes.Add(fnode);
                    fnode.index = nodes.Count - 1;
                }
                if (!nodes.Contains(snode, AppProject.cmpNode))
                {
                    nodes.Add(snode);
                    snode.index = nodes.Count - 1;
                }
            }
            /*初始化权重矩阵*/
            weight = new int[nodes.Count, nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = 0; j < nodes.Count; j++)
                {
                    if (i == j)
                        weight[i, j] = 0;
                    else
                        weight[i, j] = Dijkstra.infinite;
                }
            }
            /*添加清单表中直接的节点关系*/
            foreach (var cell in cells)
            {
                var fnode = nodes.Find(p => p.Part == cell.StartPart && p.Num == cell.StartNum);
                var snode = nodes.Find(p => p.Part == cell.EndPart && p.Num == cell.EndNum);
                if (!fnode.Nodes.Contains(snode, AppProject.cmpNode))
                {
                    fnode.Nodes.Add(snode);
                    weight[fnode.index, snode.index] = 0;
                }
                if (!snode.Nodes.Contains(fnode, AppProject.cmpNode))
                {
                    snode.Nodes.Add(fnode);
                    weight[snode.index, fnode.index] = 0;
                }
            }
            /*继电器与接触器的公共端子需要分裂成两个节点
             同时添加上继电器和接触器的触点连接关系*/
            splitSharedTermial();
            /*添加二极管、断路器、继电器等元件的节点关系*/
            foreach (var node in nodes)
            {
                string partType = config.GetType(node.Part);
                node.PartType = partType;
                if (node.Num.Contains("NC"))
                {
                    node.TNType = TerminalType.Block;
                }
                else if (node.Num.Contains("RE"))
                {
                    node.TNType = TerminalType.RE;
                }
                /*二极管节点*/
                else if (partType.Contains("端子排") && node.Num.EndsWith("A"))
                {
                    string num = node.Num;
                    num = num.Replace("A", "K");
                    var dioK = nodes.Find(p => p.Part == node.Part && p.Num == num);
                    //                     if (!dioK.Nodes.Contains(node))
                    //                         dioK.Nodes.Add(node);
                    if (dioK != null && !node.Nodes.Contains(dioK, cmpNode))
                    {
                        node.Brother = dioK; dioK.Brother = node;
                        node.Nodes.Add(dioK);
                        weight[node.index, dioK.index] = 0;
                        node.TNType = TerminalType.DiodePositive;
                        dioK.TNType = TerminalType.DiodeNegative;
                    }
                    else
                    {
                        if (dioK != null)
                            System.Windows.MessageBox.Show("已连接二极管正负极节点");
                    }
                }
                /*断路器触点*/
                else if (partType.Contains("断路器"))
                {
                    string num = node.Num;
                    TNode cbNd = null;
                    if (num.StartsWith("L"))
                    {
                        num = num.EndsWith("I") ? num.Replace("I", "O") : num.Replace("O", "I");
                        cbNd = nodes.Find(p => p.Part == node.Part && p.Num == num);
                    }
                    else
                    {
                        int nNum;
                        bool success = int.TryParse(num, out nNum);
                        if (success)
                        {
                            string anotherNum = nNum % 2 == 0 ? (nNum - 1).ToString() : (nNum + 1).ToString();
                            cbNd = nodes.Find(p => p.Part == node.Part && p.Num == anotherNum);
                        }
                    }
                    if (cbNd != null && !node.Nodes.Contains(cbNd, cmpNode))
                    {
                        node.Brother = cbNd; cbNd.Brother = node;
                        node.Nodes.Add(cbNd);
                        weight[node.index, cbNd.index] = 1;
                        node.TNType = TerminalType.BreakerContact;
                    }
                }
                else if (partType.Contains("指示灯"))
                {
                    Regex reg = new Regex(@"^(\w)([0-9]+)$");
                    Match mt = reg.Match(node.Num);
                    int nValue = 0;
                    bool success = false;
                    if (mt.Success)
                    {
                        success = int.TryParse(mt.Groups[2].Value, out nValue);
                    }
                    if (success)
                    {
                        int Num = nValue % 2 == 0 ? nValue - 1 : nValue + 1;
                        string anotherNum = mt.Groups[1].Value + Num;
                        var cbNd = nodes.Find(p => p.Part == node.Part && p.Num == anotherNum);
                        if (cbNd != null && !node.Nodes.Contains(cbNd, cmpNode))
                        {
                            node.Brother = cbNd; cbNd.Brother = node;
                            node.Nodes.Add(cbNd);
                            node.TNType = TerminalType.Indicator;
                            weight[node.index, cbNd.index] = 1;
                        }
                    }
                }
                else if (partType.Contains("转换开关"))
                {
                    int nValue = 0;
                    bool success = int.TryParse(node.Num, out nValue);
                    if (success)
                    {
                        int Num = nValue % 2 == 0 ? nValue - 1 : nValue + 1;
                        var cbNd = nodes.Find(p => p.Part == node.Part && p.Num == Num.ToString());
                        if (cbNd != null && !node.Nodes.Contains(cbNd, cmpNode))
                        {
                            node.Brother = cbNd; cbNd.Brother = node;
                            node.Nodes.Add(cbNd);
                            node.TNType = TerminalType.Switch;
                            weight[node.index, cbNd.index] = 1;
                        }
                    }
                }
            }
            /*添加端子排互相短接的节点*/
            foreach (var Tb in config.Tblib)
            {
                var fNd = nodes.Find(p => p.Part == Tb.StartPart && p.Num == Tb.StartNum);
                var sNd = nodes.Find(p => p.Part == Tb.EndPart && p.Num == Tb.EndNum);
                if (fNd != null && sNd != null)
                {
                    if (!fNd.Nodes.Contains(sNd, cmpNode))
                    {
                        fNd.Nodes.Add(sNd);
                        weight[fNd.index, sNd.index] = 0;
                    }
                    if (!sNd.Nodes.Contains(fNd, cmpNode))
                    {
                        sNd.Nodes.Add(fNd);
                        weight[sNd.index, fNd.index] = 0;
                    }
                }
            }
            /*将总正的多个端子互相连接起来，总负的多个端子也互相连接*/
            List<TNode> vcc = GetSetTerminal(p => p.Type == NamesManage.Positive);
            for (int i = 0; i < vcc.Count - 1; i++)
            {
                if (!vcc[i].Nodes.Contains(vcc[i + 1]))
                    vcc[i].Nodes.Add(vcc[i + 1]);
                if (!vcc[i + 1].Nodes.Contains(vcc[i]))
                    vcc[i + 1].Nodes.Add(vcc[i]);
            }
            List<TNode> gnd = GetSetTerminal(p => p.Type == NamesManage.Negative);
            for (int i = 0; i < gnd.Count - 1; i++)
            {
                if (!gnd[i].Nodes.Contains(gnd[i + 1]))
                    gnd[i].Nodes.Add(gnd[i + 1]);
                if (!gnd[i + 1].Nodes.Contains(gnd[i]))
                    gnd[i + 1].Nodes.Add(gnd[i]);
            }
            /*筛选出所有的CF测试节点*/
            CFNodes = GetAllCFNodes().AsReadOnly();
            /*电阻电容从一个节点应当分裂为两个节点*/
            List<TNode> split = new List<TNode>();
            foreach (TNode nd in nodes)
            {
                if (nd.PartType.Contains("电容") || nd.PartType.Contains("电阻"))
                {
                    if (nd.TNType == TerminalType.Normal && nd.Nodes.Count == 2)
                        split.Add(nd);
                }
            }
            foreach (TNode nd in split)
            {
                TNode cp = new TNode(nd.Part, nd.Num);
                cp.PartType = nd.PartType;
                cp.TNType = nd.TNType;
                nd.Nodes[0].Nodes.Remove(nd);
                nd.Nodes[0].Nodes.Add(cp);
                cp.Nodes.Add(nd.Nodes[0]);
                cp.Nodes.Add(nd);
                nd.Nodes.RemoveAt(0);
                nd.Nodes.Add(cp);
                nodes.Add(cp);
                nd.Brother = cp; cp.Brother = nd;
            }
            /*标记所有的等电位点*/
            getAllEquipotential();
            /*找到所有的短支路*/
            getAllShortBranch();
            /*修改总正与总负的等电位点名称分别为DC110V和GND*/
            markVccAndGnd();
        }
        /// <summary>
        /// 标记总正总负节点
        /// </summary>
        private void markVccAndGnd()
        {
            List<TNode> vcc = GetSetTerminal(p => p.Type == NamesManage.Positive);
            foreach (TNode nd in vcc)
            {
                string key = nd.Equal;
                foreach (TNode tnd in equals[key])
                    tnd.Equal = "DC110V";
                equals.Add("DC110V", equals[key]);
                equals.Remove(key);
                break;
            }
            List<TNode> gnd = GetSetTerminal(p => p.Type == NamesManage.Negative);
            foreach (TNode nd in gnd)
            {
                string key = nd.Equal;
                foreach (TNode tnd in equals[key])
                    tnd.Equal = "GND";
                equals.Add("GND", equals[key]);
                equals.Remove(key);
                break;
            }
        }
        /// <summary>
        /// 获得所有等电位点的组合
        /// 即初始化equalNodes这个成员map
        /// </summary>
        private void getAllEquipotential()
        {
            int netNum = 0;
            nodes.ForEach(p => p.HasIncluded = false);
            foreach (TNode nd in nodes)
            {
                if (!nd.HasIncluded)
                {
                    ISet<TNode> total = new HashSet<TNode>();
                    ISet<TNode> eq = new HashSet<TNode>();
                    eq.Add(nd);
                    total.Add(nd);
                    nd.HasIncluded = true;
                    while (eq.Count != 0)
                    {
                        ISet<TNode> temp = new HashSet<TNode>();
                        foreach (TNode e in eq)
                        {
                            foreach (TNode p in e.Nodes)
                            {
                                if ((p != e.Brother) && !total.Contains(p))
                                {
                                    temp.Add(p);
                                    total.Add(p);
                                    p.HasIncluded = true;
                                }
                            }
                        }
                        eq = temp;
                    }
                    foreach (TNode n in total)
                        n.Equal = "Net" + netNum;
                    equals.Add("Net" + netNum, total);
                    netNum++;
                }
            }
            nodes.ForEach(p => p.HasIncluded = false);
        }
        /// <summary>
        /// 获得所有的短支路
        /// 短支路即两个等电位点之间直接相连的支路
        /// </summary>
        private void getAllShortBranch()
        {
            nodes.ForEach(p => p.HasIncluded = false);
            foreach (ISet<TNode> set in equals.Values)
            {
                foreach (TNode nd in set)
                    nd.HasIncluded = true;
                foreach (TNode nd in set)
                {
                    if (nd.Brother != null && !nd.Brother.HasIncluded)
                    {
                        ShortBranch sbr = new ShortBranch();
                        sbr.Branch.Add(nd);
                        TNode next = nd.Brother;
                        sbr.Branch.Add(next);
                        next.HasIncluded = true;
                        while (next.Nodes.Count <= 2)
                        {
                            TNode last = next;
                            if (next.Brother != null && !next.Brother.HasIncluded)
                            {
                                next = next.Brother;
                            }
                            else
                            {
                                foreach (TNode tnd in next.Nodes)
                                {
                                    if (!tnd.HasIncluded)
                                    {
                                        next = tnd;
                                        break;
                                    }
                                }
                            }
                            if (last == next)
                                break;
                            next.HasIncluded = true;
                            sbr.Branch.Add(next);
                        }
                        if (sbr.LastNode.Brother != null && !sbr.Branch.Contains(sbr.LastNode.Brother))
                        {
                            sbr.LastNode.HasIncluded = false;
                            sbr.Branch.Remove(sbr.LastNode);
                        }
                        shorts.Add(sbr);
                    }
                }
            }
            nodes.ForEach(p => p.HasIncluded = false);
        }
        /// <summary>
        /// 获取指定源点的最短路径信息
        /// </summary>
        public Dijkstra getDijkstra(TNode src)
        {
            return new Dijkstra(weight, src.index);
        }
        /// <summary>
        /// 获得总负或者总正端子
        /// </summary>
        public static List<TNode> GetSetTerminal(Func<VoltageSet, bool> PosOrNeg, List<TNode> list)
        {
            Configuration config = Configuration.GetInstance();
            List<TNode> nds = new List<TNode>();
            config.Vsets.ForEach(p =>
            {
                bool mark = PosOrNeg(p);
                if (mark)
                {
                    var gndNode = list.Find(q => q.Part == p.Part && q.Num == p.Nc);
                    if (gndNode != null)
                    {
                        nds.Add(gndNode);
                    }
                }
            });
            return nds;
        }
        public List<TNode> GetSetTerminal(Func<VoltageSet, bool> PosOrNeg)
        {
            return GetSetTerminal(PosOrNeg, nodes);
        }
        /// <summary>
        /// 判断是否该等效位置是否包含CF点
        /// </summary>
        public TNode IsCFEqual(string label)
        {
            HashSet<TNode> nodes = (HashSet<TNode>)Equals[label];
            return nodes.FirstOrDefault(p => p.PartType.StartsWith("接口连接器"));
        } 
        /// <summary>
        /// 获得所有的连接器端口
        /// </summary>
        private List<TNode> GetAllCFNodes()
        {
            List<TNode> cfs = new List<TNode>();
            nodes.ForEach(
                node =>
                {
                    if (node.PartType == "接口连接器" && node.TNType != TerminalType.Block)
                    {
                        cfs.Add(node);
                    }
                }
            );
            return cfs;
        }
        /// <summary>
        /// 获得没有测试过的连接器端口
        /// </summary>
        public List<TNode> GetCFNodes()
        {
            List<TNode> cfs = new List<TNode>();
            nodes.ForEach(
                node =>
                {
                    if (node.PartType == "接口连接器" && node.TNType != TerminalType.Block)
                    {
                        cfs.Add(node);
                    }
                }
                );
            gndNet.Nets.ForEach(p => p.Branches.ForEach(q => q.Branch.ForEach(nd => cfs.Remove(nd))));
            vccNet.Nets.ForEach(p => p.Branches.ForEach(q => q.Branch.ForEach(nd => cfs.Remove(nd))));
            return cfs;
        }
        /// <summary>
        /// 导出地线测试网络
        /// </summary>
        public DataView LoadGndBranches(bool save)
        {
            gndNet.LoadGndBranches();
            if (save)
                return gndNet.SaveBranchesToExcel();
            else
                return null;
        }
        /// <summary>
        /// 导出110V测试网络
        /// </summary>
        public DataView Load110VBranches(bool save)
        {
            LogicNet rst = vccNet.Load110VBranches(lgicNets.MaxNetNum);
            lgicNets.Nets.Add(rst);
            if (save)
                return vccNet.SaveBranchesToExcel();
            else
                return null;
        }
        /// <summary>
        /// 普通导通测试网络
        /// </summary>
        public DataView Load24VBranches(bool save)
        {
            LogicNet[] rst = normalNets.Load24VBranches(lgicNets.MaxNetNum);
            lgicNets.Nets.AddRange(rst);
            if (save)
                return normalNets.SaveBranchesToExcel();
            else
                return null;
        }
        /// <summary>
        /// 逻辑测试网络
        /// </summary>
        public DataView SplitLogicBranches(bool save)
        {
            lgicNets.SplitLogicBranches(loopNets);
            if (save)
                return lgicNets.SaveBranchesToExcel();
            else
                return null;
        }
        /// <summary>
        /// 逻辑回路
        /// </summary>
        public DataView LoadLogicLoops(bool save)
        {
            loopNets.LoadLogicLoops();
            if (save)
                return loopNets.SaveBranchesToExcel();
            else
                return null;
        }
        /// <summary>
        /// 返回地线测试的指定支路
        /// </summary>
        public TestBranch getGndBranch(int index)
        {
            foreach (BranchNet net in gndNet.Nets)
            {
                if (index - net.Branches.Count < 0)
                    return net.Branches[index];
                else
                    index -= net.Branches.Count;
            }
            return null;
        }
        /// <summary>
        /// 返回110V测试的指定支路
        /// </summary>
        public TestBranch getVccBranch(int index)
        {
            foreach (BranchNet net in vccNet.Nets)
            {
                if (index - net.Branches.Count < 0)
                    return net.Branches[index];
                else
                    index -= net.Branches.Count;
            }
            return null;
        }
        /// <summary>
        /// 返回普通测试的指定支路
        /// </summary>
        public TestBranch getNormalBranch(int index)
        {
            foreach (BranchNet net in normalNets.Nets)
            {
                if (index - net.Branches.Count < 0)
                    return net.Branches[index];
                else
                    index -= net.Branches.Count;
            }
            return null;
        }
        /// <summary>
        /// 返回指定回路
        /// </summary>
        public TestBranch getLoopBranch(int index)
        {
            foreach (BranchNet net in loopNets.Nets)
            {
                if (index - net.Branches.Count < 0)
                    return net.Branches[index];
                else
                    index -= net.Branches.Count;
            }
            return null;
        }
        /// <summary>
        /// 返回逻辑测试指定回路
        /// </summary>
        public TestBranch getLogicBranch(int index)
        {
            foreach (BranchNet net in lgicNets.Nets)
            {
                if (index - net.Branches.Count < 0)
                    return net.Branches[index];
                else
                    index -= net.Branches.Count;
            }
            return null;
        }
        public Tuple<int, int> getLogicNetCount()
        {
            if (lgicNets != null)
            {
                int canTest = lgicNets.Nets.Count(p => (p as LogicNet).CanTest);
                return Tuple.Create<int, int>(canTest, lgicNets.Nets.Count - canTest);
            }
            else
            {
                return Tuple.Create<int, int>(0, 0);
            }
        }
        #endregion
    }
    /// <summary>
    /// 等电位之间互相连接的短支路
    /// </summary>
    class ShortBranch
    {
        #region Construction
        public ShortBranch()
        {
            branch = new List<TNode>();
        }
        #endregion

        #region Field
        private List<TNode> branch;//短支路节点
        public List<TNode> Branch
        {
            get
            {
                return branch;
            }
        }
        public string HeadEqualNum//短支路首节点电位标号
        {
            get
            {
                if (branch.Count != 0)
                    return branch[0].Equal;
                else
                    return string.Empty;
            }
        }
        public string TailEqualNum//短支路尾节点电位标号
        {
            get
            {
                if (branch.Count != 0)
                    return branch[branch.Count - 1].Equal;
                else
                    return string.Empty;
            }
        }
        public TNode LastNode
        {
            get { return branch[branch.Count - 1]; }
        }
        public TNode FirstNode
        {
            get { return branch[0]; }
        }
        public bool HasIncluded { get; set; }
        #endregion
    }
}
