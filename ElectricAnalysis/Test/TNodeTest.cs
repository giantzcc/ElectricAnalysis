using ElectricAnalysis.Graph;
using ElectricAnalysis.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Test
{
    public class TNodeTest
    {
        public static void testNodesCount()
        {
            var brs = DigraphBuilder.expandBranch(AppProject.GetInstance().CompleteBr.Nodes);
            ISet<TNode> nodes = new HashSet<TNode>();
            foreach (var br in brs)
                br.Uis.ForEach(p =>
                {
                    nodes.Add(p.Info.getHeadNode());
                    if (p.Info.getTailNode() != null)
                        nodes.Add(p.Info.getTailNode());
                });
            Console.Write(string.Format("总图包含节点数:{0},总结点数为：{1}\r\n", nodes.Count, AppProject.GetInstance().Nodes.Count));
            nodes.Clear();
            brs = DigraphBuilder.expandBranch(AppProject.GetInstance().VccToGnd.Nodes);
            foreach (var br in brs)
                br.Uis.ForEach(p =>
                {
                    nodes.Add(p.Info.getHeadNode());
                    if (p.Info.getTailNode() != null)
                        nodes.Add(p.Info.getTailNode());
                });
            brs = DigraphBuilder.expandBranch(AppProject.GetInstance().GndToCF.Nodes);
            foreach (var br in brs)
                br.Uis.ForEach(p =>
                {
                    nodes.Add(p.Info.getHeadNode());
                    if (p.Info.getTailNode() != null)
                        nodes.Add(p.Info.getTailNode());
                });
            Console.Write(string.Format("两个子图包含节点数:{0}\r\n", nodes.Count));
            HashSet<TNode> tnodes = new HashSet<TNode>();
            foreach (ShortBranch br in AppProject.GetInstance().Shorts)
                br.Branch.ForEach(p => tnodes.Add(p));
            Console.Write(string.Format("所有短支路中包含节点数为：{0}\r\n", tnodes.Count));
            nodes.Clear();
            foreach (var list in AppProject.GetInstance().Equals.Values)
                foreach (var one in list)
                    nodes.Add(one);
            Console.Write(string.Format("所有等电位点包含的节点数为：{0}\r\n", nodes.Count));

            foreach (var nd in tnodes)
                nodes.Remove(nd);
            foreach (var nd in nodes)
            {
                if (nd.TNType != TerminalType.Block && nd.TNType != TerminalType.RE && !nd.PartType.Contains("设备连接") && !nd.PartType.Contains("端子排") && !nd.PartType.Contains("接口连接"))
                {

                }

            }
        }
    }
}
