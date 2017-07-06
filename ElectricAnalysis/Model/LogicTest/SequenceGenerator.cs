using ElectricAnalysis.Graph;
using ElectricAnalysis.Model.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model.LogicTest
{
    public class SequenceGenerator
    {
        #region Construction
        public SequenceGenerator()
        {
            Simulation sim = Simulation.getInstance();
            this.vccToGndRoutes = sim.VccToGndRoutes;
            this.cfToGndRoutes = sim.CfToGndRoutes;
            this.cfToVccRoutes = sim.CfToVccRoutes;
            load();
        }
        #endregion

        #region Field Property
        private List<List<INotifyComponentChanged>> vccToGndRoutes = new List<List<INotifyComponentChanged>>();//Vcc到Gnd的所有的回路
        private List<List<INotifyComponentChanged>> cfToGndRoutes = new List<List<INotifyComponentChanged>>();//CF到Gnd的所有的回路
        private List<List<INotifyComponentChanged>> cfToVccRoutes = new List<List<INotifyComponentChanged>>();//CF到Vcc的所有回路

        private List<LogicCircuit> testBranch = new List<LogicCircuit>();//待测的支路
        private IDictionary<string, List<List<string>>> powCoils = new Dictionary<string, List<List<string>>>();//线圈得电的电压注入点
        #endregion

        #region Function
        /// <summary>
        /// 确定并验证待测支路的可测性
        /// </summary>
        public void validate()
        {
            Simulation sim = Simulation.getInstance();
            CFDisplay cfSrc = new CFDisplay();
            sim.srcType = SourceType.Source2;
            sim.OutPuts = cfSrc;
            List<LogicCircuit> cannotTest = new List<LogicCircuit>();
            foreach (LogicCircuit cirt in testBranch)
            {
                List<List<List<string>>> conditions = new List<List<List<string>>>();
                foreach(string coilName in cirt.OpenContacts)
                {
                    if (powCoils.ContainsKey(coilName))
                        conditions.Add(powCoils[coilName]);
                    else
                        System.Windows.MessageBox.Show("缺少线圈信息。");
                }
                List<List<string>> cbs = new List<List<string>>();
                combine(conditions, 0, new List<string>(), cbs);
                bool success = false;
                foreach (List<string> pows in cbs)
                {
                    try
                    {
                        for (int i = pows.Count - 1; i >= 0; i--)
                            sim.powUp(pows[i]);
                        sim.powUp(cirt.Pow);
                        sim.updateOutPutSource();
                    }
                    catch (SimulationLogicException e)
                    {
                        continue;
                    }
                    finally
                    {
                        sim.resetSimulation();
                    }
                    string val = cfSrc.getValue(cirt.CF, sim.srcType);
                    if (val == CFPair.HIGH)
                    {
                        success = true;
                        cirt.saveTestComponents(sim.VccConductRoutes);
                        cirt.saveTestComponents(sim.CfConductRoutes);
                        break;
                    }
                }
                if (!success)
                    cannotTest.Add(cirt);
            }

        }

        private void load()
        {
            //整理出可测的支路
            loadTestBranch();
            //整理出线圈对应的加电点
            loadPowCoils();
        }
        /// <summary>
        /// 整理出可测的支路
        /// </summary>
        private void loadTestBranch()
        {
            AppProject app = AppProject.GetInstance();
            foreach (var route in cfToVccRoutes)
            {
                bool needTest = route.Exists(p => p.CptType == ViewModel.ComponentType.ContactOpen);
                if (needTest)
                    testBranch.Add(new LogicCircuit(route, route[route.Count - 1].getHeadNode(), route[0].getHeadNode().Equal));
            }
            foreach (var route in vccToGndRoutes)
            {
                var cpt = route.FirstOrDefault(p => p.CptType == ViewModel.ComponentType.ContactOpen);
                if (cpt != null)
                {
                    int index = route.IndexOf(cpt);
                    for (int i = index + 1; i < route.Count; i++)
                    {
                        TNode cf = app.IsCFEqual(route[i].getHeadNode().Equal);
                        if (cf != null && cf.Equal!="GND")
                        {
                            testBranch.Add(new LogicCircuit(route.GetRange(0, i), cf, route[0].getHeadNode().Equal));
                            break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 整理出线圈对应的加电点
        /// </summary>
        private void loadPowCoils()
        {
            ISet<List<INotifyComponentChanged>> allRts = new HashSet<List<INotifyComponentChanged>>();
            //先把直接上电就能得电的线圈的加电方式
            getNoneConditionCoil(allRts);
            //再将有依赖关系的线圈的加电方式找出来
            getConditionCoil(allRts);
        }
        private void getConditionCoil(ISet<List<INotifyComponentChanged>> allRts)
        {
            AppProject app = AppProject.GetInstance();
            bool mark = false;
            while (!mark)
            {
                mark = true;
                #region vccToGndRoutes
                foreach (var route in vccToGndRoutes)
                {
                    if (!allRts.Contains(route))
                    {
                        List<List<List<string>>> condition = new List<List<List<string>>>();
                        for (int i = route.Count - 1; i >= 0; i--)
                        {
                            if (route[i].CptType == ViewModel.ComponentType.Coil && !powCoils.ContainsKey(route[i].getName()))
                            {
                                string name = route[i].getName();
                                while (i >= 0)
                                {
                                    if (app.IsCFEqual(route[i].getHeadNode().Equal)!=null)
                                    {
                                        mark = false;
                                        string src = route[i].getHeadNode().Equal;
                                        List<List<string>> val = new List<List<string>>();
                                        combine(condition, 0, new List<string>(), val);
                                        val.ForEach(p => {
                                            if (!p.Contains(src))
                                                p.Add(src);
                                        });
                                        powCoils[name] = val;
                                        allRts.Add(route);
                                        break;
                                    }
                                    else if (route[i].CptType == ViewModel.ComponentType.ContactOpen)
                                    {
                                        if (!powCoils.ContainsKey(route[i].getName()))
                                            break;
                                        else
                                        {
                                            condition.Add(powCoils[route[i].getName()]);
                                        }
                                    }
                                    i--;
                                }
                                if (i < 0)
                                {
                                    mark = false;
                                    string src = route[0].getHeadNode().Equal;
                                    List<List<string>> val = new List<List<string>>();
                                    combine(condition, 0, new List<string>(), val);
                                    val.ForEach(p =>
                                    {
                                        if (!p.Contains(src))
                                            p.Add(src);
                                    });
                                    powCoils[name] = val;
                                    allRts.Add(route);
                                }
                                break;
                            }
                            else if (route[i].CptType == ViewModel.ComponentType.ContactOpen)
                            {
                                if (!powCoils.ContainsKey(route[i].getName()))
                                    break;
                                else
                                {
                                    condition.Add(powCoils[route[i].getName()]);
                                }
                            }
                        }
                    }
                }
                #endregion

                #region cfToGndRoutes
                foreach (var route in cfToGndRoutes)
                {
                    if (!allRts.Contains(route))
                    {
                        List<List<List<string>>> condition = new List<List<List<string>>>();
                        for (int i = 0; i < route.Count; i++)
                        {
                            if (route[i].CptType == ViewModel.ComponentType.Coil && !powCoils.ContainsKey(route[i].getName()))
                            {
                                string name = route[i].getName();
                                i++;
                                while (i < route.Count)
                                {
                                    if (app.IsCFEqual(route[i].getHeadNode().Equal)!=null)
                                    {
                                        mark = false;
                                        string src = route[i].getHeadNode().Equal;
                                        List<List<string>> val = new List<List<string>>();
                                        combine(condition, 0, new List<string>(), val);
                                        val.ForEach(p =>
                                        {
                                            if (!p.Contains(src))
                                                p.Add(src);
                                        });
                                        powCoils[name] = val;
                                        allRts.Add(route);
                                        break;
                                    }
                                    else if (route[i].CptType == ViewModel.ComponentType.ContactOpen)
                                    {
                                        if (!powCoils.ContainsKey(route[i].getName()))
                                            break;
                                        else
                                        {
                                            condition.Add(powCoils[route[i].getName()]);
                                        }
                                    }
                                    i++;
                                }
                                if (i >= route.Count)
                                {
                                    mark = false;
                                    string src = route[route.Count - 1].getHeadNode().Equal;
                                    List<List<string>> val = new List<List<string>>();
                                    combine(condition, 0, new List<string>(), val);
                                    val.ForEach(p =>
                                    {
                                        if (!p.Contains(src))
                                            p.Add(src);
                                    });
                                    powCoils[name] = val;
                                    allRts.Add(route);
                                }
                                break;
                            }
                            else if (route[i].CptType == ViewModel.ComponentType.ContactOpen)
                            {
                                if (!powCoils.ContainsKey(route[i].getName()))
                                    break;
                                else
                                {
                                    condition.Add(powCoils[route[i].getName()]);
                                }
                            }
                        }
                    }
                }
                #endregion
            }
        }
        /// <summary>
        /// 递归将一组条件排列组合进行归并
        /// </summary>
        /// <param name="conditions">所有线圈的条件</param>
        /// <param name="index">第几个线圈的条件</param>
        /// <param name="temp">当前保存的数据</param>
        /// <param name="rst">存放归并结果</param>
        private void combine(List<List<List<string>>> conditions, int index, List<string> temp, List<List<string>> rst)
        {
            if (index == conditions.Count)
            {
                rst.Add(temp);
                return;
            }
            foreach (List<string> pd in conditions[index])
            {
                List<string> cp = new List<string>(temp);
                pd.ForEach(p =>
                {
                    if (!cp.Contains(p))
                        cp.Add(p);
                });
                combine(conditions, index + 1, cp, rst);
            }
        }
        /// <summary>
        /// 获取加电后可以直接得电的线圈
        /// </summary>
        private void getNoneConditionCoil(ISet<List<INotifyComponentChanged>> allRts)
        {
            AppProject app = AppProject.GetInstance();
            foreach (var route in vccToGndRoutes)
            {
                for (int i = route.Count - 1; i >= 0; i--)
                {
                    if (route[i].CptType == ViewModel.ComponentType.Coil && !powCoils.ContainsKey(route[i].getName()))
                    {
                        string name = route[i].getName();
                        while (i >= 0)
                        {
                            if (app.IsCFEqual(route[i].getHeadNode().Equal)!=null)
                            {
                                powCoils[name] = new List<List<string>>();
                                powCoils[name].Add(new List<string>() { route[i].getHeadNode().Equal });
                                allRts.Add(route);
                                break;
                            }
                            else if (route[i].CptType == ViewModel.ComponentType.ContactOpen)
                            {
                                break;
                            }
                            i--;
                        }
                        if (i < 0)
                        {
                            powCoils[name] = new List<List<string>>();
                            powCoils[name].Add(new List<string>() { route[0].getHeadNode().Equal });
                            allRts.Add(route);
                        }
                        break;
                    }
                    else if (route[i].CptType == ViewModel.ComponentType.ContactOpen)
                        break;
                }
            }
            foreach (var route in cfToGndRoutes)
            {
                for (int i = 0; i < route.Count; i++)
                {
                    if (route[i].CptType == ViewModel.ComponentType.Coil && !powCoils.ContainsKey(route[i].getName()))
                    {
                        string name = route[i].getName();
                        i++;
                        while (i < route.Count)
                        {
                            if (app.IsCFEqual(route[i].getHeadNode().Equal)!=null)
                            {
                                powCoils[name] = new List<List<string>>();
                                powCoils[name].Add(new List<string>() { route[i].getHeadNode().Equal });
                                allRts.Add(route);
                                break;
                            }
                            else if (route[i].CptType == ViewModel.ComponentType.ContactOpen)
                            {
                                break;
                            }
                            i++;
                        }
                        if (i >= route.Count)
                        {
                            powCoils[name] = new List<List<string>>();
                            powCoils[name].Add(new List<string>() { route[route.Count - 1].getHeadNode().Equal });
                            allRts.Add(route);
                        }
                        break;
                    }
                    else if (route[i].CptType == ViewModel.ComponentType.ContactOpen)
                        break;
                }
            }
        }
        #endregion
    }
}
