using ElectricAnalysis.Model;
using ElectricAnalysis.View.component;
using ElectricAnalysis.ViewModel;
using ElectricAnalysis.ViewModel.TreeViewItemViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ElectricAnalysis.Graph
{
    /// <summary>
    /// 返回布线图的不同部分结构
    /// </summary>
    public interface IShortBranchTreeGenerator
    {
        BranchView getVccToGndBranch();
        BranchView getVccToCFBranch();
        BranchView getCFToGndBranch();
        BranchView getAllBranch();
        void buildRoutes();
    }
    /// <summary>
    /// 视图
    /// </summary>
    public class BranchView
    {
        public UIElement page { get; set; }//缓存起来的画布对象
        public GraphType GraphType { get; set; }//布线图类型
        public string GraphName { get; set; }//布线图名称
        private bool type = false;//是否开启并联视图
        private IDictionary<string, ShortBranchNode> labelToBranch;
        private ISet<ShortBranchNode> nodes;
        private ComponentVMObserver observer;
        public BranchView(IDictionary<string, ShortBranchNode> labelToBranch, ISet<ShortBranchNode> nodes, ComponentVMObserver observer)
        {
            this.nodes = nodes;
            this.observer = observer;
            this.labelToBranch = labelToBranch;
        }
        public bool Type
        {
            get { return type; }
            set { type = value; }
        }
        public IDictionary<string, ShortBranchNode> LabelToBranch
        {
            get { return labelToBranch; }
            set { labelToBranch = value; }
        }
        public ISet<ShortBranchNode> Nodes
        {
            get { return nodes; }
            set { nodes = value; }
        }
        public ComponentVMObserver Observer
        {
            get { return observer; }
            set { observer = value; }
        }
    }
    /// <summary>
    /// 拼接短支路时短支路组成的树结构
    /// </summary>
    public class ShortBranchNode
    {
        private string endName;
        private List<TNodeUI> uis;
        private ShortBranchNode parent;
        private ISet<ShortBranchNode> nodes;
        public ShortBranchNode(List<TNodeUI> uis, ShortBranchNode parent, string endName)
        {
            this.uis = uis;
            this.parent = parent;
            this.endName = endName;
            this.nodes = new HashSet<ShortBranchNode>();
        }
        public string EndName
        {
            get { return endName; }
        }
        public ISet<ShortBranchNode> Nodes
        {
            get { return nodes; }
        }
        public List<TNodeUI> Uis
        {
            get
            {
                return uis;
            }
        }
        public ShortBranchNode Parent
        {
            get
            {
                return parent;
            }
        }
        public void remove()
        {
            if (parent != null)
            {
                parent.Nodes.Remove(this);
            }
        }

    }

    public class TNodeUIPair
    {
        private TNodeUI item1;
        private TNodeUI item2;
        public TNodeUIPair(TNodeUI item1, TNodeUI item2)
        {
            this.item1 = item1;
            this.item2 = item2;
        }

        public TNodeUI Item1
        {
            get
            {
                return item1;
            }
        }
        public TNodeUI Item2
        {
            get
            {
                return item2;
            }
        }
    }
    /// <summary>
    /// 节点
    /// </summary>
    public class Point
    {
        public double X;
        public double Y;

        public Point(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
    }
    /// <summary>
    /// 元件UI类
    /// </summary>
    public class TNodeUI
    {
        #region Property
        private bool reverse;//如果是二极管这类有向元件需要标识方向
        private INotifyComponentChanged info;//节点信息
        private Point pos;//元件的位置
        public INotifyComponentChanged Info
        {
            get { return info; }
            set { info = value; }
        }
        public Point RealPos//具体的像素位置
        {
            get
            {
                return new Point(DigraphBuilder.START_POS.X + pos.X * 150, DigraphBuilder.START_POS.Y + pos.Y * 60);
            }
        }
        public Point Pos
        {
            get
            {
                return pos;
            }
            set
            {
                pos = value;
            }
        }
        public bool Reverse
        {
            get { return reverse; }
        }
        #endregion

        #region Construction
        public TNodeUI(INotifyComponentChanged cpInfo, bool reverse, IObserver observer)
        {
            this.info = cpInfo;
            this.reverse = reverse;
            if (observer != null)
                observer.addListener(cpInfo);
        }
        #endregion
    }
    /// <summary>
    /// 电路布线图生成类
    /// </summary>
    public class DigraphBuilder : IShortBranchTreeGenerator
    {
        #region Construction
        public DigraphBuilder()
        {
            routes = new List<TestBranch>();
            endNode = new List<TNode>();
            remainNodes = new List<TNode>();
            elements = new List<TNodeUI>();
            h_lines = new List<Tuple<Point, Point>>();
            v_lines = new List<Tuple<Point, Point>>();
            poly_lines = new List<Tuple<Point, Point>>();
            nodePos = new Dictionary<TNode, Point>();
            observer = new ComponentVMObserver();
        }
        #endregion

        #region Property
        public static Point START_POS = new Point(100, 100);//从图纸的哪个点开始计算位置

        private List<TNode> nodes = new List<TNode>();//从有向图转化成的无向图节点

        private List<TestBranch> routes;//广度遍历后的所有路径
        private List<TNode> endNode;//路径最后中断的已标记节点
        private List<TNode> vccNodes;//总正相连的节点
        private List<TNode> gndNodes;//总负相连的节点
        private List<TNode> remainNodes;//从总正出发广度遍历没有遍历到的节点
        private List<TNodeUI> elements;//包含位置信息的ui节点
        private List<Tuple<Point, Point>> h_lines;//水平连线
        private List<Tuple<Point, Point>> v_lines;//垂直连线
        private List<Tuple<Point, Point>> poly_lines;//折线
        private Dictionary<TNode, Point> nodePos;//已确定的TNode节点的位置
        private ComponentVMObserver observer;//ui节点观察者

        public IDictionary<string, Point> labelPos = new Dictionary<string, Point>();//等电位点的位置
        private List<TNodeUIPair> pairs = new List<TNodeUIPair>();//垂直连线
        private List<TNodeUIPair> hpairs = new List<TNodeUIPair>();//水平连线

        public List<TNodeUIPair> Hpairs
        {
            get { return hpairs; }
        }

        public List<TNodeUIPair> Pairs
        {
            get { return pairs; }
        }

        public ComponentVMObserver Observer
        {
            get
            {
                return observer;
            }
        }
        public List<TNodeUI> Elements
        {
            get
            {
                return elements;
            }
        }
        #endregion

        #region Function
        /// <summary>
        /// 建立所有非重复路径
        /// </summary>
        public void buildRoutes()
        {
            nodes = AppProject.GetInstance().Nodes;
            init();
        }
        /// <summary>
        /// 初始化搜索的起点和终点
        /// 并标记所有的Vcc起点
        /// </summary>
        private void init()
        {
            vccNodes = AppProject.GetSetTerminal(p => p.Type == NamesManage.Positive, nodes);
            gndNodes = AppProject.GetSetTerminal(p => p.Type == NamesManage.Negative, nodes);
            nodes.ForEach(p => p.HasIncluded = false);
            vccNodes.ForEach(p => p.HasIncluded = true);
        }
        /// <summary>
        /// 获得从总正出发，总负截止的所有拼接支路
        /// </summary>
        private BranchView getNodes(string headEqual, Func<string, bool> filter)
        {
            ComponentVMObserver _observer = new ComponentVMObserver();
            AppProject pro = AppProject.GetInstance();
            ((List<ShortBranch>)pro.Shorts).ForEach(p => p.HasIncluded = false);
            LinkedList<string> labels = new LinkedList<string>();
            IDictionary<string, ShortBranchNode> allLabels = new Dictionary<string, ShortBranchNode>();
            labels.AddLast(headEqual);

            ShortBranchNode head = new ShortBranchNode(null, null, headEqual);
            allLabels.Add(headEqual, head);

            #region 广度遍历得到短支路的树结构
            while (labels.First != null)
            {
                string label = labels.First.Value;
                labels.RemoveFirst();
                if (filter(label))
                    continue;
                TNodeUI item1 = null, item2 = null;
                foreach (ShortBranch br in pro.Shorts)
                {
                    if (!br.HasIncluded)
                    {
                        if (br.HeadEqualNum == label || br.TailEqualNum == label)
                        {
//                             if (br.HeadEqualNum == "Net203" || br.TailEqualNum == "Net203")
//                             {
// 
//                             }
                            List<TNodeUI> uis = new List<TNodeUI>();
                            List<TNode> brNodes = br.Branch;
                            if (br.TailEqualNum == label)
                                brNodes.Reverse();
                            int i = 0;
                            while (i < brNodes.Count)
                            {
                                TNodeUI ui = null;
                                if (i + 1 < brNodes.Count && brNodes[i].Brother == brNodes[i + 1])
                                {
                                    ui = BranchFactory.convert(brNodes[i], brNodes[i + 1], _observer);
                                    if (i == 0)
                                        item2 = ui;
                                    uis.Add(ui);
                                    i += 2;
                                }
                                else
                                {
                                    ui = BranchFactory.convert(brNodes[i], null, _observer);
                                    if (i == 0)
                                        item2 = ui;
                                    uis.Add(ui);
                                    i++;
                                }
                                if (ui != null && item1 == null)
                                    item1 = ui;
                            }
                            ShortBranchNode bnode = new ShortBranchNode(uis, allLabels[label], brNodes[brNodes.Count - 1].Equal);
                            allLabels[label].Nodes.Add(bnode);
                            if (!allLabels.ContainsKey(brNodes[brNodes.Count - 1].Equal))
                            {
                                allLabels.Add(brNodes[brNodes.Count - 1].Equal, bnode);
                                labels.AddLast(brNodes[brNodes.Count - 1].Equal);
                            }
                            br.HasIncluded = true;
                        }
                    }
                }
            }
            #endregion

            #region 返回顶节点
//             ISet<ShortBranchNode> branchNodes = new HashSet<ShortBranchNode>();
//             foreach (ShortBranchNode brNode in allLabels.Values)
//             {
//                 if (brNode.Parent == head)
//                     branchNodes.Add(brNode);
//             }
            return new BranchView(allLabels, head.Nodes, _observer);
            #endregion
        }

        private void moveRight(ISet<ShortBranchNode> children, double offset)
        {
            foreach (ShortBranchNode node in children)
            {
                foreach (TNodeUI ui in node.Uis)
                {
                    ui.Pos.X += offset;
                }
                moveRight(node.Nodes, offset);
            }
        }

        private void alignBranchNode(ISet<ShortBranchNode> expandNodes, BranchView view)
        {
            foreach (ShortBranchNode node in view.LabelToBranch.Values)
            {
                if (node == null)
                    continue;
                var filterNodes = expandNodes.Where(p => node.EndName == p.EndName).ToList();
                if (filterNodes.Count == 0 || filterNodes.Count == 1)
                    continue;
                double maxWidth = double.MinValue;
                foreach (ShortBranchNode br in filterNodes)
                {
                    if (br.Uis[br.Uis.Count - 1].Pos.X > maxWidth)
                        maxWidth = br.Uis[br.Uis.Count - 1].Pos.X;
                }
                foreach (ShortBranchNode br in filterNodes)
                {
                    double start = br.Uis[br.Uis.Count - 1].Pos.X;
                    TNode head = br.Uis[br.Uis.Count - 1].Info.getHeadNode();
                    TNode tail = br.Uis[br.Uis.Count - 1].Info.getTailNode();
                    for (double x = start; x < maxWidth; x++)
                    {
                        INotifyComponentChanged info = new ComponentViewModel(Tuple.Create<TNode, TNode>(head, tail), ComponentType.Blank);
                        TNodeUI ui = new TNodeUI(info, false, view.Observer);
                        ui.Pos = new Point(x + 1, br.Uis[br.Uis.Count - 1].Pos.Y);
                        br.Uis.Add(ui);
                    }
                    moveRight(br.Nodes, maxWidth - start);
                }
                double maxy = filterNodes.Max(p => p.Uis[p.Uis.Count - 1].Pos.Y);
                double miny = filterNodes.Min(p => p.Uis[p.Uis.Count - 1].Pos.Y);
                TNode tm = AppProject.GetInstance().Equals[node.EndName].First();
                INotifyComponentChanged _info = new ComponentViewModel(Tuple.Create<TNode, TNode>(tm, null), ComponentType.Blank);
                TNodeUI nui = new TNodeUI(_info, false, view.Observer);
                nui.Pos = new Point(node.Uis[node.Uis.Count - 1].Pos.X + 1, node.Uis[node.Uis.Count - 1].Pos.Y);
                List<TNodeUI> uis = new List<TNodeUI>() { nui };
                ShortBranchNode nnode = new ShortBranchNode(uis, node, node.EndName);
                foreach (ShortBranchNode nd in node.Nodes)
                    nnode.Nodes.Add(nd);
                moveRight(node.Nodes, 1);
                node.Nodes.Clear();
                node.Nodes.Add(nnode);
            }
        }
        /// <summary>
        /// 计算各个节点的位置
        /// </summary>
        public int CalculatePosition(ISet<ShortBranchNode> data, int x, int y)
        {
            if (data.Count == 0)
                return y;
            foreach (ShortBranchNode brNode in data)
            {
                int posx = x;
                foreach (TNodeUI ui in brNode.Uis)
                    ui.Pos = new Point(posx++, y);
                y = CalculatePosition(brNode.Nodes, posx, y);
                if(brNode!=data.Last())
                    y++;
            }
            return y;
        }
        /// <summary>
        /// 过滤出节点
        /// </summary>
        private void filterNodes(ISet<ShortBranchNode> total, Func<string, bool> filter)
        {
            ISet<string> remain = new HashSet<string>();
            foreach (ShortBranchNode node in total)
            {
                if (node.EndName == "GND")
                {
                    ShortBranchNode temp = node;
                    while (temp != null)
                    {
                        remain.Add(temp.EndName);
                        temp = temp.Parent;
                    }
                }
            }


            bool next = true;
            while (next)
            {
                next = false;
                List<ShortBranchNode> temp = new List<ShortBranchNode>();
                foreach (ShortBranchNode node in total)
                {
                    if (node.Nodes.Count == 0 && filter(node.EndName))//node.EndName != "GND"
                    {
                        if (!remain.Contains(node.EndName))
                        {
                            node.remove();
                            temp.Add(node);
                            next = true;
                        }
                        else
                        {
                            ShortBranchNode tp = node;
                            while (tp != null)
                            {
                                remain.Add(tp.EndName);
                                tp = tp.Parent;
                            }
                        }
                    }
                }
                foreach (ShortBranchNode node in temp)
                    total.Remove(node);
            }
        }
        /// <summary>
        /// 将树状的节点展开后放入一维集合中
        /// </summary>
        public static ISet<ShortBranchNode> expandBranch(ISet<ShortBranchNode> brs)
        {
            ISet<ShortBranchNode> total = new HashSet<ShortBranchNode>(brs);
            int count = 0;
            while (total.Count != count)
            {
                count = total.Count;
                ISet<ShortBranchNode> temp = new HashSet<ShortBranchNode>();
                foreach (ShortBranchNode node in brs)
                {
                    foreach (ShortBranchNode child in node.Nodes)
                    {
                        temp.Add(child);
                        total.Add(child);
                    }
                }
                brs = temp;
            }
            return total;
        }
        /// <summary>
        /// 获取以ShortBranchNode为单位的所有路径组合
        /// </summary>
        public static List<List<ShortBranchNode>> getAllRoutes(ISet<ShortBranchNode> heads)
        {
            List<List<ShortBranchNode>> rts = new List<List<ShortBranchNode>>();
            foreach (ShortBranchNode tnode in heads)
                rts.Add(new List<ShortBranchNode>() { tnode });
            bool mark = false;
            while (!mark)
            {
                mark = true;
                List<List<ShortBranchNode>> tempRts = new List<List<ShortBranchNode>>();
                foreach (List<ShortBranchNode> rt in rts)
                {
                    if (rt[rt.Count - 1].Nodes.Count >= 1)
                    {
                        mark = false;
                        foreach (ShortBranchNode next in rt[rt.Count - 1].Nodes)
                        {
                            List<ShortBranchNode> temp = new List<ShortBranchNode>(rt);
                            temp.Add(next);
                            tempRts.Add(temp);
                        }
                    }
                    else
                    {
                        tempRts.Add(rt);
                    }
                }
                rts = tempRts;
            }
            return rts;
        }
        #endregion

        #region 接口实现
        public BranchView getVccToGndBranch()
        {
            #region 将树状结构的节点放到一维集合中
            BranchView rst = getNodes(vccNodes[0].Equal, label => label == "GND");
            ISet<ShortBranchNode> brs = rst.Nodes;
            ShortBranchNode head = brs.First().Parent;
            ISet<ShortBranchNode> total = expandBranch(brs);
            rst.Type = true;
            #endregion

            #region 过滤出所有总正到总负的通路
            filterNodes(total, name => name != "GND");
            #endregion

            #region 为短支路树计算位置信息
            ISet<ShortBranchNode> data = new HashSet<ShortBranchNode>(total.Where(p => p.Parent == head).ToList());
            CalculatePosition(data, 0, 0);
            if(rst.Type)
                alignBranchNode(total, rst);
            rst.Nodes = data;
            #endregion
            rst.GraphType = GraphType.VccToGndGraph;
            rst.GraphName = "总正至总负布线图";
            return rst;
        }

        public BranchView getVccToCFBranch()
        {
            #region 将树状结构的节点放到一维集合中
            BranchView rst = getNodes(vccNodes[0].Equal, label => label == "GND");
            ISet<ShortBranchNode> brs = rst.Nodes;
            ShortBranchNode head = brs.First().Parent;
            ISet<ShortBranchNode> total = expandBranch(brs);
            #endregion

            #region 过滤出所有总正到测试点的通路
            AppProject pro = AppProject.GetInstance();
            ISet<string> remain = new HashSet<string>();
            foreach (ShortBranchNode node in total)
            {
                if (node.Nodes.Count == 0)
                {
                    bool mark = false;
                    foreach (TNode tnode in pro.Equals[node.EndName])
                    {
                        if (tnode.PartType.StartsWith("接口连接器"))
                        {
                            mark = true;
                            break;
                        }
                    }
                    if (mark)
                    {
                        ShortBranchNode tp = node;
                        while (tp != null)
                        {
                            remain.Add(tp.EndName);
                            tp = tp.Parent;
                        }
                    }
                }
            }
            //先把叶节点中末端是测试点的标注出来
            bool next = true;
            while (next)
            {
                next = false;
                List<ShortBranchNode> temp = new List<ShortBranchNode>();
                foreach (ShortBranchNode node in total)
                {
                    if (node.Nodes.Count == 0)
                    {
                        bool mark = false;
                        foreach (TNode tnode in pro.Equals[node.EndName])
                        {
                            if (tnode.PartType.StartsWith("接口连接器"))
                            {
                                mark = true;
                                if (!node.Uis[node.Uis.Count - 1].Info.isCFNode())
                                {
                                    INotifyComponentChanged info = new ComponentViewModel(Tuple.Create<TNode, TNode>(tnode, null), ComponentType.Terminal);
                                    node.Uis.Add(new TNodeUI(info, false, rst.Observer));
                                }
                                break;
                            }
                        }
                        if (!mark)
                        {
                            if (!remain.Contains(node.EndName))
                            {
                                node.remove();
                                temp.Add(node);
                                next = true;
                            }
                            else
                            {
                                ShortBranchNode tp = node;
                                while (tp != null)
                                {
                                    remain.Add(tp.EndName);
                                    tp = tp.Parent;
                                }
                            }
                        }
                    }
                }
                foreach (ShortBranchNode node in temp)
                    total.Remove(node);
            }
            rst.Nodes = removeRedundentRoute(rst.Nodes, total);//去除冗余的支路
            #endregion

            #region 为短支路树计算位置信息
            ISet<ShortBranchNode> data = new HashSet<ShortBranchNode>(total.Where(p => p.Parent == head).ToList());
            CalculatePosition(data, 0, 0);
            if (rst.Type)
                alignBranchNode(total, rst);
            rst.Nodes = data;
            #endregion
            rst.GraphType = GraphType.VccToCFGraph;
            rst.GraphName = "总正至测试点布线图";
            return rst;
        }

        public BranchView getCFToGndBranch()
        {
            #region 将树状结构的节点放到一维集合中
            BranchView rst = getNodes(gndNodes[0].Equal, label => label == "DC110V");
            ISet<ShortBranchNode> brs = rst.Nodes;
            ShortBranchNode head = brs.First().Parent;
            ISet<ShortBranchNode> total = expandBranch(brs);
            #endregion

            #region 过滤出所有总负到测试点的通路
            AppProject pro = AppProject.GetInstance();
            ISet<string> remain = new HashSet<string>();
            foreach (ShortBranchNode node in total)
            {
                if (node.Nodes.Count == 0)
                {
                    bool mark = false;
                    foreach (TNode tnode in pro.Equals[node.EndName])
                    {
                        if (tnode.PartType.StartsWith("接口连接器"))
                        {
                            mark = true;
                            break;
                        }
                    }
                    if (mark)
                    {
                        ShortBranchNode tp = node;
                        while (tp != null)
                        {
                            remain.Add(tp.EndName);
                            tp = tp.Parent;
                        }
                    }
                }
            }
            //先把叶节点中末端是测试点的标注出来
            bool next = true;
            while (next)
            {
                next = false;
                List<ShortBranchNode> temp = new List<ShortBranchNode>();
                foreach (ShortBranchNode node in total)
                {
                    if (node.Nodes.Count == 0)
                    {
                        bool mark = false;
                        foreach (TNode tnode in pro.Equals[node.EndName])
                        {
                            if (tnode.PartType.StartsWith("接口连接器"))
                            {
                                mark = true;
                                if (!node.Uis[node.Uis.Count - 1].Info.isCFNode())
                                {
                                    INotifyComponentChanged info = new ComponentViewModel(Tuple.Create<TNode, TNode>(tnode, null), ComponentType.Terminal);
                                    node.Uis.Add(new TNodeUI(info, false, rst.Observer));
                                }
                                break;
                            }
                        }
                        if (!mark)
                        {
                            if (!remain.Contains(node.EndName))
                            {
                                node.remove();
                                temp.Add(node);
                                next = true;
                            }
                            else
                            {
                                ShortBranchNode tp = node;
                                while (tp != null)
                                {
                                    remain.Add(tp.EndName);
                                    tp = tp.Parent;
                                }
                            }
                        }
                    }
                }
                foreach (ShortBranchNode node in temp)
                    total.Remove(node);
            }
            rst.Nodes = removeRedundentRoute(rst.Nodes, total);//去除冗余的支路
            #endregion

            #region 为短支路树计算位置信息
            ISet<ShortBranchNode> data = new HashSet<ShortBranchNode>(total.Where(p => p.Parent == head).ToList());
            CalculatePosition(data, 0, 0);
            if (rst.Type)
                alignBranchNode(total, rst);
            rst.Nodes = data;
            #endregion
            rst.GraphType = GraphType.CFToGndGraph;
            rst.GraphName = "测试点至总负布线图";
            return rst;
        }

        public BranchView getAllBranch()
        {
            BranchView rst = getNodes(vccNodes[0].Equal, label => false);
            CalculatePosition(rst.Nodes, 0, 0);
            if (rst.Type)
                alignBranchNode(expandBranch(rst.Nodes), rst);
            rst.GraphType = GraphType.CompleteGraph;
            rst.GraphName = "布线总图";
            return rst;
        }
        /// <summary>
        /// 去除掉已经在VccToGnd中出现过的支路
        /// </summary>
        private ISet<ShortBranchNode> removeRedundentRoute(ISet<ShortBranchNode> nodes, ISet<ShortBranchNode> total)
        {
            List<List<ShortBranchNode>> routes = getAllRoutes(nodes);
            AppProject app = AppProject.GetInstance();
            if (app.VccToGnd == null)
                app.VccToGnd = getVccToGndBranch();
            ISet<ShortBranchNode> allNodes = expandBranch(app.VccToGnd.Nodes);
            ISet<INotifyComponentChanged> allcpts = new HashSet<INotifyComponentChanged>();
            foreach (ShortBranchNode brNode in allNodes)
                foreach (TNodeUI nd in brNode.Uis)
                    allcpts.Add(nd.Info);
            List<ShortBranchNode> bans = new List<ShortBranchNode>();
            foreach (List<ShortBranchNode> rt in routes)
            {
                List<INotifyComponentChanged> infos = new List<INotifyComponentChanged>();
                rt.ForEach(p => p.Uis.ForEach(ui => infos.Add(ui.Info)));
                infos.RemoveAt(infos.Count-1);
                if (infos.All(p => 
                    allcpts.FirstOrDefault(cpt => ComponentViewModel.compare(cpt, p)) != null
                    ))
                {
                    bans.Add(rt[0]);
                    rt.ForEach(p => total.Remove(p));
                }
            }
            bans.ForEach(p => nodes.Remove(p));
            return nodes;
        }
        #endregion
    }
}
