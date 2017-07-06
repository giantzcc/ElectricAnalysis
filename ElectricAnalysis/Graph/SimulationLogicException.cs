using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Graph
{
    public class SimulationLogicException:Exception
    {
        public SimulationLogicException():base("加电序列存在逻辑矛盾，导致仿真异常。")
        {
            
        }
    }
}
