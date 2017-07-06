using ElectricAnalysis.DAL;
using ElectricAnalysis.Graph;
using ElectricAnalysis.Model;
using ElectricAnalysis.Model.Authority;
using ElectricAnalysis.Model.Result;
using ElectricAnalysis.ViewModel.TreeViewItemViewModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace ElectricAnalysis.ViewModel
{
    class ResourceViewModel : ViewModelBase
    {
        #region Construction
        public ResourceViewModel()
        {
            RunCmd = new RelayCommand<string>(SwitchState);
            DoubleClickCmd = new RelayCommand<Itemvm>(DoubleClick, CanDoubleClick);
            ClearCellConfigCmd = new RelayCommand(ClearCellConfig);
            ClearCellLibCmd = new RelayCommand(ClearCellLib);
            ClearLinkerCfgCmd = new RelayCommand(ClearLinkerCfg);
            ClearCptRelationListCmd = new RelayCommand(ClearCptRelationList);
            ImportCellConfigCmd = new RelayCommand(ImportCellConfig, CanImportCellConfig);
            ImportCellLibCmd = new RelayCommand(ImportCellLib, CanImportCellLib);
            ImportGraphData = new RelayCommand(ImportGraph, CanImportGraph);
            items = new ObservableCollection<Itemvm>();
            loadConfig();
        }
        #endregion

        #region Command
        public ICommand RunCmd { get; set; }//开启或中止模式
        public RelayCommand<Itemvm> DoubleClickCmd { get; set; }//目录项的双击命令
        public ICommand ClearCptRelationListCmd { get; set; }//清空清单表
        public ICommand ClearCellConfigCmd { get; set; }//清空元件配置
        public ICommand ClearCellLibCmd { get; set; }//清空元件库
        public ICommand ClearLinkerCfgCmd { get; set; }//清空接口配置
        public ICommand ImportCellConfigCmd { get; set; }//导入元件配置
        public ICommand ImportCellLibCmd { get; set; }//导入元件库
        public ICommand ImportGraphData { get; set; }//导入电路图数据
        #endregion

        #region Field Property
        private IModeFunctional manager = ModeManager.getAuthority();//当前系统的工作模式
        private IList<string> openPages = new List<string>();//启动模式之前打开的页面
        private AppProject pro = AppProject.GetInstance();
        private Resource src;//目录中的根节点项
        public bool IsDoubleClick { get; set; }//双击标志
        private ObservableCollection<Itemvm> items;//目录集合
        public ObservableCollection<Itemvm> Items
        {
            get { return items; }
            set { items = value; }
        }
        private string output;//绑定到消息框的内容
        public string Output
        {
            get { return output; }
            set
            {
                output = value;
                base.RaisePropertyChanged("Output");
            }
        }
        private IDictionary<string, Mode> modes = new Dictionary<string, Mode>();//工作模式
        public ICollection<string> ModeNames
        {
            get
            {
                return modes.Keys;
            }
        }
        #endregion

        #region Function
        /// <summary>
        /// 添加消息输出
        /// </summary>
        private void appendLog(string msg)
        {
            Output += msg;
        }
        /// <summary>
        /// 按下启动或中止按钮所执行的操作
        /// </summary>
        /// <param name="obj"></param>
        private void SwitchState(string state)
        {
            if (manager.CurrentMode == Mode.Default)
            {
                manager.CurrentMode = modes[state];
                Messenger.Default.Send<IList<string>>(openPages, "SaveOpenPages");
                Itemvm graph = Itemvm.findItem(items, typeof(GraphGenerator));
                Task task1 = new Task(() => { graph.Itemvms[0].DoubleClick(new Action<string>(appendLog)); Thread.Sleep(200); });
                Task task = task1;
                task1 = task1.ContinueWith(t => { graph.Itemvms[1].DoubleClick(new Action<string>(appendLog)); Thread.Sleep(200); });
                task1 = task1.ContinueWith(t => { graph.Itemvms[2].DoubleClick(new Action<string>(appendLog)); Thread.Sleep(200); });
                task1 = task1.ContinueWith(t => { graph.Itemvms[3].DoubleClick(new Action<string>(appendLog)); Thread.Sleep(200); });
                task1 = task1.ContinueWith(t =>
                {
                    Messenger.Default.Send<Object>(null, "GraphCanNotClose");
                    try
                    {
                        CFDisplay output = manager.prepared();//初始化仿真器的输出源
                        Messenger.Default.Send<CFDisplay>(output, "ShowOutPutValue");
                        appendLog("CF接口输出源绑定成功。\r\n");
                    }
                    catch (Exception e)
                    {
                        appendLog("CF接口输出源绑定失败。\r\n");
                        Console.WriteLine(e);
                    }
                });
                task.Start();
            }
            else
            {
                Messenger.Default.Send<object>(openPages, "ExitWorkMode");
                manager.exit();
                manager.CurrentMode = Mode.Default;
            }
            appendLog("进入" + manager.CurrentMode + "模式.\r\n");
        }
        /// <summary>
        /// 清空元件清单表
        /// </summary>
        private void ClearCptRelationList()
        {
            Itemvm cfgItem = Itemvm.findItem(items, typeof(CellListConfig));
            Itemvm item = Itemvm.findItem(items, typeof(CptRelationDisplay));
            cfgItem.Itemvms.Remove(item);
            pro.clearProject();
            appendLog("元件清单清空成功。\r\n");
        }
        /// <summary>
        /// 清空接口配置
        /// </summary>
        private void ClearLinkerCfg()
        {
            Itemvm cfgItem = Itemvm.findItem(items, typeof(Config));
            Itemvm item = Itemvm.findItem(items, typeof(LinkerCfg));
            cfgItem.Itemvms.Remove(item);
            appendLog("接口配置清空成功。\r\n");
        }
        /// <summary>
        /// 清空元件库
        /// </summary>
        private void ClearCellLib()
        {
            Itemvm cfgItem = Itemvm.findItem(items, typeof(Config));
            Itemvm item = Itemvm.findItem(items, typeof(CellLibConfig));
            item.Itemvms.Clear();
            appendLog("元件库清空成功。\r\n");
        }
        /// <summary>
        /// 清空元件配置
        /// </summary>
        private void ClearCellConfig()
        {
            Itemvm cfgItem = Itemvm.findItem(items, typeof(Config));
            Itemvm item = Itemvm.findItem(items, typeof(CellConfig));
            item.Itemvms.Clear();
            appendLog("元件配置清空成功。\r\n");
        }
        /// <summary>
        /// 加载基本的目录信息
        /// </summary>
        private void loadConfig()
        {
            items.Add(src = new Resource());
            modes.Add("仿真模式", Mode.Simulink);
            modes.Add("自动测试模式", Mode.AutoTest);
            modes.Add("手动测试模式", Mode.ManualTest);
        }
        /// <summary>
        /// 双击
        /// </summary>
        private bool CanDoubleClick(Itemvm arg)
        {
            return IsDoubleClick;
        }
        private void DoubleClick(Itemvm obj)
        {
            if (obj.Itemvms == null || obj.Itemvms.Count == 0)
                obj.DoubleClick(new Action<string>(appendLog));
        }
        private bool CanImportGraph()
        {
            Itemvm item = Itemvm.findItem(items, typeof(CellListConfig));
            return item.Itemvms.Count == 0;
        }

        private bool CanImportCellLib()
        {
            Itemvm item = Itemvm.findItem(items, typeof(CellLibConfig));
            return item.Itemvms.Count == 0;
        }

        private bool CanImportCellConfig()
        {
            Itemvm item = Itemvm.findItem(items, typeof(CellConfig));
            return item.Itemvms.Count == 0;
        }
        private void ImportGraph()
        {
            Itemvm item = Itemvm.findItem(items, typeof(CellListConfig));
            item.DoubleClick(new Action<string>(appendLog));
        }

        private void ImportCellLib()
        {
            Itemvm item = Itemvm.findItem(items, typeof(CellLibConfig));
            item.DoubleClick(new Action<string>(appendLog));
        }

        private void ImportCellConfig()
        {
            Itemvm item = Itemvm.findItem(items, typeof(CellConfig));
            item.DoubleClick(new Action<string>(appendLog));
        }
        #endregion
    }
}
