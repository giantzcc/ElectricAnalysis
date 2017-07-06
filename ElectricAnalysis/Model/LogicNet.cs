using ElectricAnalysis.FaultDiagnosis;
using ElectricAnalysis.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    /// <summary>
    /// 逻辑测试网络
    /// </summary>
    class LogicNet : BranchNet
    {
        #region Construction
        public LogicNet(int netNum)
            : base(netNum)
        {
            AppProject pro = AppProject.GetInstance();
            loops = new List<TestBranch>();
            includedCoils = new List<string>();
            bits = new BitArray(pro.CFNodes.Count);
            bits.SetAll(false);
            faults = new List<ComponentFault>();
        }
        #endregion

        #region Field
        private static int kk = 0;//用于统计可测试的支路条数
        private List<TestBranch> loops;//逻辑支路导通的条件
        private List<string> includedCoils;//条件之路中包含的得电线圈
        private BitArray bits;//存放正常情况下CF输出点的电平
        private List<ComponentFault> faults;//各故障模式对应的CF输出电平
        #endregion

        #region Property
        public BitArray Bits
        {
            get
            {
                return bits;
            }
        }
        public List<ComponentFault> Faults
        {
            get
            {
                return faults;
            }
        }
        public List<TestBranch> Loops
        {
            get
            {
                return loops;
            }
        }
        public bool CanTest { get; set; }
        #endregion

        #region Function 寻找条件支路
        /// <summary>
        /// 分解支路网络
        /// 将含有相同逻辑元件的支路
        /// 分配进相同的网络
        /// </summary>
        public IEnumerable<LogicNet> Split(int maxNum)
        {
            List<LogicNet> newNets = new List<LogicNet>();
            branches.Sort((i, j) =>
            {
                if (i.Count > j.Count)
                    return 1;
                else if (i.Count < j.Count)
                    return -1;
                else
                    return 0;
            });
            int count = branches.Count;
            bool[] marks = new bool[count];
            for (int i = 0; i < count; i++)
            {
                marks[i] = false;
            }
            for (int i = count - 1; i >= 0; i--)
            {
                if (marks[i])
                    continue;
                LogicNet net = new LogicNet(++maxNum);
                net.branches.Add(branches[i]);
                for (int j = i - 1; j >= 0; j--)
                {
                    if (!branches[i].Equals(branches[j]) && !marks[j])
                    {
                        bool isContain = branches[j].LogicNodes.All(p =>
                            branches[i].LogicNodes.Count == branches[j].LogicNodes.Count
                            && branches[i].LogicNodes.Contains(p, AppProject.cmpNode));
                        if (isContain)
                        {
                            marks[j] = true;
                            net.branches.Add(branches[j]);
                        }
                    }
                }
                newNets.Add(net);
            }
            return newNets;
        }
        /// <summary>
        /// 逆推逻辑导通的条件
        /// 并且生成相应的故障模式
        /// 这是深度递归的入口函数
        /// </summary>
        public void GetConductCondition()
        {
            loops.Clear();
            includedCoils.Clear();
            bool Success = true;//有没有找到合适的逻辑条件
            List<TestBranch> rstbranch = new List<TestBranch>();
            List<string> destCoils = new List<string>();
            List<TNode> blockNodes = new List<TNode>();
            IEnumerable<string> foundCoils = null;
            if (this.branches.Count != 0)
                foundCoils = UpdateRelation(this.branches[0], destCoils, blockNodes);
            if (destCoils.Count == 0)
                return;
            List<string> parentCoils = new List<string>();
            foreach (var coil in foundCoils)
            {
                parentCoils.Add(coil);
                includedCoils.Add(coil);
            }
            foreach (var coil in destCoils)
            {
                List<string> parentcls = new List<string>();
                parentCoils.ForEach(p => parentcls.Add(p));
                bool success = GetConditions(rstbranch, coil, blockNodes, parentcls);
                if (!success)
                {
                    Success = false;
                    break;
                }
            }
            if (Success)
            {
                loops = rstbranch;
                CheckCondition();
            }
            else
            {
                CanTest = false;
                includedCoils.Clear();
                loops.Clear();
                Debug.WriteLine("失败解析：{0}", Num);
            }
            if (Num == 389 || Num == 393)
            {

            }

            /*如果该网络可以测试，则整理可能存在的故障模式*/
            if (CanTest)
                FaultVector();
        }
        /// <summary>
        /// 逆推逻辑导通的条件
        /// 这是深度递归函数
        /// </summary>
        /// <param name="branches">最后找到的条件支路</param>
        /// <param name="destCoil">目的线圈的部件名称</param>
        /// <param name="blockNodes">会产生矛盾，应该屏蔽的节点</param>
        /// <param name="parentCoils">当前要寻找的线圈的祖先辈线圈</param>
        /// <param name="includedCoils">已经找到的条件支路中所有的线圈</param>
        /// <returns>是否搜索到合适导通条件</returns>
        private bool GetConditions(List<TestBranch> branches, string destCoil, List<TNode> blockNodes, List<string> parentCoils)
        {
            AppProject pro = AppProject.GetInstance();
            /*没有需要得电的线圈了就停止搜索*/
            if (string.IsNullOrEmpty(destCoil))
                return true;
            /*如果寻找的线圈不在父支路中但在已经搜索过的其他支路里
             * 父支路已有的线圈如果再次出现就产生了矛盾
             * 那么不用重复寻找
             */
            if (!parentCoils.Contains(destCoil) && includedCoils.Contains(destCoil))
                return true;
            else if (parentCoils.Contains(destCoil))
                return false;
            /*遍历该网络中的每一条支路*/
            TestBranch br = null;
            if (pro.LoopNets.Nets.Count > 0)
            {
                foreach (var branch in pro.LoopNets.Nets[pro.LoopNets.Nets.Count - 1].Branches)
                {
                    if (!branch.hasBlock)
                    {
                        int num = branch.LogicNodes.Count(p => p.Part == destCoil && p.TNType == TerminalType.Coil);
                        if (num == 2)
                        {
                            if (br == null)
                                br = branch;
                            else
                            {
                                int brNcNum = br.Branch.Count(p => p.TNType == TerminalType.ContactNormalClose);
                                int NcNum = branch.Branch.Count(p => p.TNType == TerminalType.ContactNormalClose);
                                if (brNcNum > NcNum)
                                    br = branch;
                            }
                        }
                    }
                }
            }
            if (br == null)
            {
                foreach (var net in pro.LoopNets.Nets)
                {
                    foreach (var branch in net.Branches)
                    {
                        if (!branch.hasBlock)
                        {
                            int num = branch.LogicNodes.Count(p => p.Part == destCoil && p.TNType == TerminalType.Coil);
                            if (num == 2)
                            {
                                if (br == null)
                                    br = branch;
                                else
                                {
                                    int brNcNum = br.Branch.Count(p => p.TNType == TerminalType.ContactNormalClose);
                                    int NcNum = branch.Branch.Count(p => p.TNType == TerminalType.ContactNormalClose);
                                    if (brNcNum > NcNum)
                                        br = branch;
                                }
                            }
                        }
                    }
                }
            }
            else
            {

            }
            /*找到就更新屏蔽的节点、查找下一步寻找的线圈*/
            if (br != null)
            {
                List<string> destCoils = new List<string>();
                IEnumerable<string> coils = UpdateRelation(br, destCoils, blockNodes);
                if (!branches.Contains(br))
                    branches.Add(br);
                foreach (var cl in coils)
                {
                    parentCoils.Add(cl);
                    includedCoils.Add(cl);
                }
                foreach (var cl in destCoils)
                {
                    List<string> parentcls = new List<string>();
                    parentCoils.ForEach(p => parentcls.Add(p));
                    bool success = GetConditions(branches, cl, blockNodes, parentcls);
                    if (!success)
                        return false;
                }
            }
            else
            {
                /*没有找到符合条件的支路*/
                return false;
            }
            return true;
        }
        /// <summary>
        /// 从条件支路中提取出，下一次要寻找的线圈、新增的屏蔽节点
        /// </summary>
        /// <param name="br">一条条件支路</param>
        /// <param name="destCoils">存放下一次要寻找的线圈</param>
        /// <param name="blockNodes">屏蔽节点的集合</param>
        /// <returns>本条件支路中包含的线圈的名称</returns>
        private IEnumerable<string> UpdateRelation(TestBranch br, List<string> destCoils, List<TNode> blockNodes)
        {
            AppProject pro = AppProject.GetInstance();
            List<string> includedCoils = new List<string>();
            foreach (var node in br.LogicNodes)
            {
                var another = node.GetAnother();
                if (br.LogicNodes.Contains(another) && node.TNType == TerminalType.ContactNormalOpen)
                {
                    /*常开触点的线圈作为寻找的目标*/
                    var query = pro.Nodes.Where(p =>
                        p.Part == node.Part && p.TNType == TerminalType.Coil);
                    foreach (var coil in query)
                    {
                        if (!destCoils.Contains(coil.Part))
                            destCoils.Add(coil.Part);
                    }
                    /*常开触点对应的常闭触点需要屏蔽*/
                    query = pro.Nodes.Where(p =>
                        p.Part == node.Part && p.TNType == TerminalType.ContactNormalClose);
                    foreach (var nc in query)
                    {
                        TNode athr = nc.GetAnother();
                        if (athr != null && !blockNodes.Contains(nc))
                        {
                            blockNodes.Add(nc);
                            blockNodes.Add(athr);
                        }
                    }
                }
                if (br.LogicNodes.Contains(another) && node.TNType == TerminalType.ContactNormalClose)
                {
                    /*常闭触点的线圈需要屏蔽*/
                    var query = pro.Nodes.Where(p =>
                        p.Part == node.Part && p.TNType == TerminalType.Coil);
                    foreach (var coil in query)
                    {
                        blockNodes.Add(coil);
                    }
                    /*常闭触点对应的常开触点需要屏蔽*/
                    query = pro.Nodes.Where(p =>
                        p.Part == node.Part && p.TNType == TerminalType.ContactNormalOpen);
                    foreach (var no in query)
                    {
                        TNode athr = no.GetAnother();
                        if (athr != null && !blockNodes.Contains(no))
                        {
                            blockNodes.Add(no);
                            blockNodes.Add(athr);
                        }
                    }
                }
                if (node.TNType == TerminalType.Coil)
                {
                    var coilpair = br.LogicNodes.Where(p => p.TNType == TerminalType.Coil && p.Part == node.Part).ToList();
                    if (coilpair.Count == 2)
                    {
                        foreach (var coil in coilpair)
                        {
                            if (!blockNodes.Contains(coil))
                                blockNodes.Add(coil);
                            if (!includedCoils.Contains(coil.Part))
                                includedCoils.Add(coil.Part);
                        }
                        var query = pro.Nodes.Where(p =>
                            p.Part == node.Part && p.TNType == TerminalType.ContactNormalClose);
                        foreach (var nd in query)
                        {
                            TNode ather = nd.GetAnother();
                            if (ather != null && !blockNodes.Contains(nd))
                            {
                                blockNodes.Add(nd);
                                blockNodes.Add(ather);
                            }
                        }
                    }
                }
            }
            /*屏蔽不符合的支路*/
            BlockBranches(blockNodes);
            return includedCoils;
        }
        /// <summary>
        /// 标记包含屏蔽节点的回路
        /// </summary>
        /// <param name="blockNodes">屏蔽节点集合</param>
        private void BlockBranches(List<TNode> blockNodes)
        {
            AppProject pro = AppProject.GetInstance();
            /*屏蔽不符合的支路*/
            var nodes = blockNodes.Where(p => p.TNType == TerminalType.ContactNormalClose
                || p.TNType == TerminalType.ContactNormalOpen).ToList();
            var coils = blockNodes.Where(p => p.TNType == TerminalType.Coil).ToList();
            foreach (var net in pro.LoopNets.Nets)
            {
                foreach (var branch in net.Branches)
                {
                    /*屏蔽常开、常闭触点*/
                    if (branch.hasBlock)
                        break;
                    foreach (var nd in nodes)
                    {
                        TNode athr = nd.GetAnother();
                        if (branch.LogicNodes.Contains(nd) && branch.LogicNodes.Contains(athr))
                        {
                            branch.hasBlock = true;
                            break;
                        }
                    }
                    /*屏蔽线圈*/
                    if (branch.hasBlock)
                        break;
                    foreach (var cl in coils)
                    {
                        TNode athr = coils.FirstOrDefault(p => p.Part == cl.Part && p.TNType == cl.TNType && !p.Equals(cl));
                        if (branch.LogicNodes.Contains(cl) && branch.LogicNodes.Contains(athr))
                        {
                            branch.hasBlock = true;
                            break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 检查已找到的逻辑导通条件是否成立
        /// </summary>
        public void CheckCondition()
        {
            AppProject pro = AppProject.GetInstance();
            /*将所有条件注入点相关的所有回路都添加到relativeBrs集合中*/
            List<TestBranch> relativeBrs = new List<TestBranch>();
            foreach (var lp in loops)
            {
                BranchNet net = pro.LoopNets.Nets.FirstOrDefault(p => p.Branches.Contains(lp));
                net.Branches.ForEach(p =>
                {
                    if (!relativeBrs.Contains(p))
                        relativeBrs.Add(p);
                });
            }
            /*初始化所有节点的HasChanged属性为false
                        触点没有发生动作*/
            pro.Nodes.ForEach(p => p.HasChanged = false);

            /*将条件支路中得电的线圈对应的触点标记为动作*/
            var query = pro.Nodes.Where(p => includedCoils.Contains(p.Part)
                && (p.TNType == TerminalType.ContactNormalOpen || p.TNType == TerminalType.ContactNormalClose));
            foreach (var nd in query)
            {
                TNode another = nd.GetAnother();
                if (another != null)
                {
                    nd.HasChanged = true;
                    another.HasChanged = true;
                }
            }
            /*将所有通电的线圈的触点标记为动作*/
            List<TestBranch> rstBrs = new List<TestBranch>();
            CheckCondition(relativeBrs, rstBrs);
            /*最后检查一下loops里的支路是否依然满足导通的条件*/
            bool success = true;//没有问题
            rstBrs.ForEach(p => loops.Add(p));
            loops.Add(branches[0]);//将被测支路一起加入导通支路里检查

            int[,] weight = new int[pro.Nodes.Count, pro.Nodes.Count];
            Array.Copy(pro.weight, weight, weight.Length);//拷贝权重矩阵

            foreach (var br in loops)
            {
                var noQuery = br.Branch.Where(p => p.TNType == TerminalType.ContactNormalOpen);
                var ncQuery = br.Branch.Where(p => p.TNType == TerminalType.ContactNormalClose);
                foreach (var no in noQuery)
                {
                    TNode another = no.GetAnother();
                    if (br.Branch.Contains(another))
                    {
                        if (!no.HasChanged)
                        {
                            success = false;
                            weight[no.index, another.index] = 0;
                            weight[another.index, no.index] = 0;
                        }
                    }
                }
                if (success)
                {
                    foreach (var nc in ncQuery)
                    {
                        TNode another = nc.GetAnother();
                        if (br.Branch.Contains(another))
                        {
                            if (nc.HasChanged)
                            {
                                success = false;
                                weight[nc.index, another.index] = Dijkstra.infinite;
                                weight[another.index, nc.index] = Dijkstra.infinite;
                            }
                        }
                    }
                }
                //判断回路中是否可能存在短路情况
                if (success)
                {
                    Dijkstra dist = new Dijkstra(weight, br.Branch[0].index);
                    List<TNode> negative = pro.GetSetTerminal(p => p.Type == NamesManage.Negative);
                    success = !DijkstraUtil.hasShortCircuit(negative, dist);
                }

                if (!success)
                {
                    break;
                }
            }

            loops.Remove(branches[0]);//检查完毕后从条件支路中移除

            CanTest = success;
            if (!success)
            {
                loops.Clear();
                includedCoils.Clear();
                Debug.WriteLine("失败解析：{0}", Num);
            }
            else
            {
                Debug.WriteLine("成功解析：{0}", Num);
                kk += branches.Count;
                Debug.WriteLine(kk);
                Debug.WriteLine("条件支路个数：{0}", loops.Count);
            }
        }
        /// <summary>
        /// 找出所有导通的回路
        /// </summary>
        /// <param name="relativeBrs">所有可能导通的回路</param>
        /// <param name="rstBrs">导通的回路</param>
        /// <param name="weight">权重矩阵</param>
        private void CheckCondition(List<TestBranch> relativeBrs, List<TestBranch> rstBrs)
        {
            AppProject pro = AppProject.GetInstance();
            /*筛选出依然导通的回路*/
            SelectLoops(ref rstBrs);
            /*遍历relativeBrs中的回路是否闭合*/
            foreach (var br in relativeBrs)
            {
                if (loops.Contains(br) || rstBrs.Contains(br))
                    continue;
                else
                {
                    /*判断常开、常闭触点的当前状态，如果包含线圈，还要进一步判断线圈造成的影响*/
                    var noQuery = br.Branch.Where(p => p.TNType == TerminalType.ContactNormalOpen);
                    var ncQuery = br.Branch.Where(p => p.TNType == TerminalType.ContactNormalClose);
                    var coilQuery = br.Branch.Where(p => p.TNType == TerminalType.Coil).Select(p => p.Part).Distinct();

                    bool nomark = true;
                    bool ncmark = true;
                    foreach (var no in noQuery)
                    {
                        TNode another = no.GetAnother();
                        if (!br.Branch.Contains(another))
                            continue;
                        else
                        {
                            if (!no.HasChanged)
                            {
                                nomark = false;
                                break;
                            }
                        }
                    }
                    if (nomark)
                    {
                        foreach (var nc in ncQuery)
                        {
                            TNode another = nc.GetAnother();
                            if (!br.Branch.Contains(another))
                                continue;
                            else
                            {
                                if (nc.HasChanged)
                                {
                                    ncmark = false;
                                    break;
                                }
                            }
                        }
                    }
                    if (nomark && ncmark)
                    {
                        rstBrs.Add(br);
                        /*导通的回路中若还有线圈应该重新查找*/
                        bool mark = false;
                        foreach (var cl in coilQuery)
                        {
                            int num = br.Branch.Count(p => p.Part == cl && p.TNType == TerminalType.Coil);
                            if (num == 2)
                            {
                                mark = true;
                                var query = pro.Nodes.Where(p => p.Part == cl &&
                                    (p.TNType == TerminalType.ContactNormalOpen || p.TNType == TerminalType.ContactNormalClose));
                                foreach (var nd in query)
                                {
                                    TNode another = nd.GetAnother();
                                    if (another != null)
                                    {
                                        nd.HasChanged = true;
                                        another.HasChanged = true;
                                    }
                                }
                            }
                        }
                        /*如果含有线圈需要重新核查*/
                        if (mark)
                        {
                            CheckCondition(relativeBrs, rstBrs);
                            break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 筛选出所有导通的支路
        /// </summary>
        /// <param name="rstBrs">支路集合</param>
        private void SelectLoops(ref List<TestBranch> rstBrs)
        {
            AppProject pro = AppProject.GetInstance();
            List<TestBranch> rst = new List<TestBranch>();
            /*遍历rstBrs中的回路是否闭合*/
            foreach (var br in rstBrs)
            {
                /*判断常开、常闭触点的当前状态*/
                var noQuery = br.Branch.Where(p => p.TNType == TerminalType.ContactNormalOpen);
                var ncQuery = br.Branch.Where(p => p.TNType == TerminalType.ContactNormalClose);

                bool nomark = true;
                bool ncmark = true;
                foreach (var no in noQuery)
                {
                    TNode another = no.GetAnother();
                    if (!br.Branch.Contains(another))
                        continue;
                    else
                    {
                        if (!no.HasChanged)
                        {
                            nomark = false;
                            break;
                        }
                    }
                }
                if (nomark)
                {
                    foreach (var nc in ncQuery)
                    {
                        TNode another = nc.GetAnother();
                        if (!br.Branch.Contains(another))
                            continue;
                        else
                        {
                            if (nc.HasChanged)
                            {
                                ncmark = false;
                                break;
                            }
                        }
                    }
                }
                if (nomark && ncmark)
                {
                    rst.Add(br);
                }
            }
            rstBrs = rst;
        }
        #endregion

        #region Function 计算条件支路全部加电时的特征向量
        /// <summary>
        /// 计算各故障模式的特征向量
        /// </summary>
        private void FaultVector()
        {
            if (CanTest)
            {
                AppProject pro = AppProject.GetInstance();
                int[,] weight = new int[pro.Nodes.Count, pro.Nodes.Count];
                Array.Copy(pro.weight, weight, weight.Length);//拷贝权重矩阵

                List<TestBranch> circuits = new List<TestBranch>(loops);
                circuits.Add(branches[0]);
                foreach (var br in circuits)
                {
                    var coil = br.Branch.Where(p => p.TNType == TerminalType.Coil);
                    ISet<string> names = new HashSet<string>();
                    foreach (var nd in coil)
                    {
                        if (names.Contains(nd.Part))
                            continue;
                        else
                        {
                            names.Add(nd.Part);
                            TNode another = nd.GetAnother();
                            if (br.Branch.Contains(another))
                            {
                                pro.Nodes.ForEach(p =>
                                {
                                    if (p.Part == nd.Part && p.TNType == TerminalType.ContactNormalOpen)
                                    {
                                        TNode ather = p.GetAnother();
                                        weight[p.index, ather.index] = 0;
                                        weight[ather.index, p.index] = 0;
                                    }
                                    else if (p.Part == nd.Part && p.TNType == TerminalType.ContactNormalClose)
                                    {
                                        TNode ather = p.GetAnother();
                                        weight[p.index, ather.index] = Dijkstra.infinite;
                                        weight[ather.index, p.index] = Dijkstra.infinite;
                                    }
                                });
                            }
                        }
                    }
                }
                /*无故障情况下的特征向量*/
                Dijkstra dist = new Dijkstra(weight, branches[0].Branch[0].index);
                for (int i = 0; i < pro.CFNodes.Count;i++ )
                {
                    bits.Set(i, dist.getRouteWeight(pro.CFNodes[i].index) < Dijkstra.infinite);
                }
                /*有故障情况下的特征向量*/
                faults.Clear();
                ISet<TNode> pairs = new HashSet<TNode>();
                foreach (var br in branches)
                {
                    foreach (var nd in br.Branch)
                        pairs.Add(nd);
                }
                List<TNode> linkerNodes = new List<TNode>();
                foreach (var nd in pairs)
                {
                    TNode another = nd.GetAnother();
                    if (another == null||linkerNodes.Contains(nd))
                        continue;
                    if ((nd.TNType == TerminalType.BreakerContact && pairs.Contains(another))
                        || (nd.TNType == TerminalType.Coil && pairs.Contains(another))
                        || (nd.TNType == TerminalType.ContactNormalClose && pairs.Contains(another))
                        || (nd.TNType == TerminalType.ContactNormalOpen && pairs.Contains(another))
                        || ((nd.TNType == TerminalType.DiodePositive && another.TNType == TerminalType.DiodeNegative && pairs.Contains(another))
                        || (nd.TNType == TerminalType.DiodeNegative && another.TNType == TerminalType.DiodePositive && pairs.Contains(another)))
                        || (nd.TNType == TerminalType.Indicator && pairs.Contains(another)))
                    {
                        linkerNodes.Add(nd);
                        linkerNodes.Add(another);
                    }
                }
                int pairCount = linkerNodes.Count/2;
                for (int j = 0; j < pairCount; j++)
                {
                    TNode nd1 = linkerNodes[j];
                    TNode nd2 = linkerNodes[j+1];
                    ComponentFault fault1 = new ComponentFault(nd1.Part + "短路", Tuple.Create<TNode, TNode>(nd1, nd2), FaultType.ShortCircuit);
                    ComponentFault fault2 = new ComponentFault(nd1.Part + "断路", Tuple.Create<TNode, TNode>(nd1, nd2), FaultType.BlockCircuit);
                    faults.Add(fault1);
                    faults.Add(fault2);
                }
            }
        }
        #endregion
    }
}
