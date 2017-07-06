using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    /// <summary>
    /// 节点比较
    /// </summary>
    class CompareTNode:IEqualityComparer<TNode>
    {
        public bool Equals(TNode x, TNode y)
        {
            if (x.Part == y.Part && x.Num == y.Num)
                return true;
            else
                return false;
        }
        public int GetHashCode(TNode obj)
        {
            return obj.GetHashCode();
        }
    }
    /// <summary>
    /// 连接器比较
    /// </summary>
    [Serializable]
    class CompareLinker:IEqualityComparer<Linker>
    {
        public bool Equals(Linker x, Linker y)
        {
            if(x.LinkerName==y.LinkerName&&x.LinkerPort==y.LinkerPort)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(Linker obj)
        {
            return obj.GetHashCode();
        }
    }
}
