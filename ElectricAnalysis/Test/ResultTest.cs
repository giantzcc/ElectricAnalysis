using ElectricAnalysis.Graph;
using ElectricAnalysis.Model.LogicTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Test
{
    public class ResultTest
    {
        private SequenceGenerator generator;
        public ResultTest(SequenceGenerator generator)
        {
            this.generator = generator;
        }
        /// <summary>
        /// 测试最后检测的覆盖率
        /// </summary>
        public void testBranchPercent()
        {
            Simulation sim = Simulation.getInstance();
            ISet<INotifyComponentChanged> cpts = new HashSet<INotifyComponentChanged>();
            foreach (var cirt in generator.TestBranch)
            {
                foreach (var node in cirt.Components)
                    cpts.Add(node);
                foreach (var node in cirt.Circuit)
                    cpts.Add(node);
            }
            Console.WriteLine(String.Format("总元件数量为：{0}\r\n可测元件数量为：{1}\r\n测试覆盖率为：{2}"
                , sim.AllCpts.Count
                ,cpts.Count
                , (double)cpts.Count / sim.AllCpts.Count));
        }
    }
}
