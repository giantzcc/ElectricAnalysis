using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    class TestBox
    {
        #region Construction
        public TestBox(Linker linker, string num, int port)
        {
            this.testBoxNum = num;
            this.testBoxPort = port;
            string[] lineChars = new string[4];
            lineChars[0] = num.Substring(1);
            lineChars[1] = ((port - 1) / 72 + 1).ToString();
            lineChars[2] = linker.LinkerPort.Substring(0, 1);
            lineChars[3] = linker.LinkerPort.Substring(1);
            this.lineNum = string.Join("--", lineChars);
        }
        public TestBox(int testBoxNum, int testBoxPort)
        {
            this.testBoxNum = "#" + testBoxNum;
            this.testBoxPort = testBoxPort;
        }
        #endregion

        #region Field Property
        private string testBoxNum = string.Empty;
        public string TestBoxNum
        {
            get { return testBoxNum; }
        }
        public int BoxNum
        {
            get
            {
                return int.Parse(testBoxNum.Substring(1));
            }
        }
        private int testBoxPort;
        public int TestBoxPort
        {
            get { return testBoxPort; }
        }
        private string lineNum = string.Empty;
        public string LineNum
        {
            get { return lineNum; }
        }
        #endregion

        #region Function
        
        #endregion
    }
}
