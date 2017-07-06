using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    class Linker
    {
        #region Construction
        public Linker(string linkerName, int linkerPort)
        {
            this.linkerName = linkerName;
            this.linkerPort = DoFormatter(linkerPort);
        }
        #endregion

        #region Field Property
        private string linkerName = string.Empty;
        public string LinkerName
        {
            get { return linkerName; }
        }
        private string linkerPort = string.Empty;
        public string LinkerPort
        {
            get { return linkerPort; }
        }
        #endregion

        #region Function
        /// <summary>
        /// 获得连接器的最大端口号
        /// </summary>
        /// <param name="nodes">节点集</param>
        /// <returns>端口号的整数形式</returns>
        public static int GetMaxLinkerPort(IEnumerable<TNode> nodes, string name)
        {
                return nodes.Where(p => p.Part == name && p.PartType == "接口连接器" && p.TNType == TerminalType.Normal).Max(q =>
                {
                    if (q.Num.Length < 3)
                    {
                        string initialChar = q.Num.Substring(0, 1);
                        string Num = q.Num.Substring(1);
                        int qee = (Convert.ToInt32(initialChar, 16) - 10) * 12 + int.Parse(Num);
                        return qee;
                    }
                    else
                    {
                        return -1;
                    }
                    //System.Diagnostics.Debug.WriteLine(q.Num);
                });
        }
        /// <summary>
        /// 将整形端口号格式化
        /// </summary>
        /// <param name="num">整形端口号</param>
        /// <returns>格式化结果</returns>
        private string DoFormatter(int num)
        {
            int remain = num % 12;
            int serialNum = (num - 1) / 12;
            string serialChar = string.Empty;
            switch (serialNum)
            {
                case 0:
                    {
                        serialChar = "A";
                        break;
                    }
                case 1:
                    {
                        serialChar = "B";
                        break;
                    }
                case 2:
                    {
                        serialChar = "C";
                        break;
                    }
                case 3:
                    {
                        serialChar = "D";
                        break;
                    }
                case 4:
                    {
                        serialChar = "E";
                        break;
                    }
                case 5:
                    {
                        serialChar = "F";
                        break;
                    }
            }
            if (!string.IsNullOrEmpty(serialChar))
                serialChar += remain != 0 ? remain.ToString() : "12";
            return serialChar;
        }
        #endregion
    }
}
