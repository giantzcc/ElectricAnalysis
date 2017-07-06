using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    /// <summary>
    /// 原始连接节点
    /// </summary>
    public class OriginCell
    {
        private string startPart;//起始部位
        public string StartPart
        {
            get { return startPart; }
            set { startPart = value; }
        }
        private string startNum;//起始端子号
        public string StartNum
        {
            get { return startNum; }
            set { startNum = value; }
        }
        private string endPart;//末端部位
        public string EndPart
        {
            get { return endPart; }
            set { endPart = value; }
        }
        private string endNum;//末端端子号
        public string EndNum
        {
            get { return endNum; }
            set { endNum = value; }
        }
        private string lineNum;//线号
        public string LineNum
        {
            get { return lineNum; }
            set { lineNum = value; }
        }
        private bool hasIncluded = false;
        public bool HasIncluded
        {
            get { return hasIncluded; }
            set { hasIncluded = value; }
        }
    }
}
