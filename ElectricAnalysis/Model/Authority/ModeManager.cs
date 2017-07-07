using ElectricAnalysis.Graph;
using ElectricAnalysis.Model.LogicTest;
using ElectricAnalysis.Model.Result;
using ElectricAnalysis.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model.Authority
{
    public enum Mode
    {
        Default,//待机模式
        Simulink,//仿真模式
        AutoTest,//自动测试模式
        ManualTest//手动测试模式
    }
    public interface IModeFunctional
    {
        Mode CurrentMode { get; set; }//当前的工作模式
        bool canOperate();//是否允许用户操作
        bool needSimComparator();//是否启用双仿真器
        CFDisplay prepared();//切换状态后要作一些初始化的工作
        void exit();//退出工作模式时作的复位工作
    }
    public class ModeManager : IModeFunctional
    {
        #region 单例
        private static volatile ModeManager manager;
        private static object synLock = new object();
        private ModeManager()
        {
            CurrentMode = Mode.Default;
        }
        public static IModeFunctional getAuthority()
        {
            if (manager == null)
            {
                lock (synLock)
                {
                    if (manager == null)
                        manager = new ModeManager();
                }
            }
            return manager;
        }
        #endregion

        #region field/property
        public Mode CurrentMode { get; set; }//当前的工作模式
        #endregion

        public bool canOperate()
        {
            return CurrentMode == Mode.Simulink || CurrentMode == Mode.ManualTest;
        }

        public bool needSimComparator()
        {
            return CurrentMode == Mode.Simulink;
        }


        public CFDisplay prepared()
        {
            CFDisplay cfSrc = null;
            Simulation sim = Simulation.getInstance();
            sim.resetSimulation();
            switch (CurrentMode)
            {
                case Mode.Simulink:
                    {
                        cfSrc = new CFDisplay();
                        sim.OutPuts = cfSrc;
                        sim.srcType = Result.SourceType.Source2;
                        Simulation simcp = Simulation.getAnotherSimulation();
                        simcp.OutPuts = cfSrc;
                        simcp.srcType = Result.SourceType.Source1;
                        break;
                    }
                case Mode.AutoTest:
                    {
                        SequenceGenerator squence = new SequenceGenerator();
                        squence.generate();
                        squence.compressTestBranch();
                        ResultTest test = new ResultTest(squence);
                        test.testBranchPercent();
                        break;
                    }
                case Mode.ManualTest:
                    {
                        break;
                    }
            }
            return cfSrc;
        }
        public void exit()
        {
            Simulation sim = Simulation.getInstance();
            sim.resetSimulation();
            if (CurrentMode == Mode.Simulink)
                Simulation.getAnotherSimulation().resetSimulation();
        }
    }
}
