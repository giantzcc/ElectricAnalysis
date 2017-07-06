using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.ViewModel.TreeViewItemViewModel
{
    /// <summary>
    /// 布线图的分类
    /// </summary>
    public enum GraphType
    {
        VccToGndGraph,
        VccToCFGraph,
        CFToGndGraph,
        CompleteGraph
    }
}
