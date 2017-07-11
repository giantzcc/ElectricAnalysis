using ElectricAnalysis.DAL;
using ElectricAnalysis.Graph;
using ElectricAnalysis.Model;
using ElectricAnalysis.Model.Result;
using ElectricAnalysis.View;
using ElectricAnalysis.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.AvalonDock.Layout;
using MIV.Bus.WPF.UIShell.Controls;
using ElectricAnalysis.ViewModel.TreeViewItemViewModel;

namespace ElectricAnalysis
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new ResourceViewModel();
            Messenger.Default.Register<Action>(this, "WaitForFinish", ShowWaitBar);
            Messenger.Default.Register<Tuple<string, DataView>>(this, "DisplayTestBranch", DisplayTestBranch);
            Messenger.Default.Register<Tuple<string, List<PinInfo>>>(this, "DisplayRelayPinInfo", DisplayRelayPinInfo);
            Messenger.Default.Register<Tuple<string, List<IndicatorPinInfo>>>(this, "DisplayIndicatorPinInfo", DisplayIndicatorPinInfo);
            Messenger.Default.Register<Tuple<string, List<TBCircuit>>>(this, "DisplayTBConnector", DisplayTBConnector);
            Messenger.Default.Register<Tuple<string, List<OriginCell>>>(this, "DisplayCptRelation", DisplayCptRelation);
            Messenger.Default.Register<Tuple<string, List<ElectricAnalysis.Model.ComponentLib>>>(this, "DisplayComponentLib", DisplayComponentLib);
            Messenger.Default.Register<Tuple<string, List<VoltageSet>>>(this, "DisplayVoltageSet", DisplayVoltageSet);
            Messenger.Default.Register<List<UIElement>>(this, "ShowBranchPicture", ShowBranch);
            Messenger.Default.Register<BranchView>(this, "ShowGraph", ShowGraph);
            Messenger.Default.Register<IList<string>>(this, "SaveOpenPages", SaveOpenPages);
            Messenger.Default.Register<Object>(this, "GraphCanNotClose", GraphCanNotClose);
            Messenger.Default.Register<object>(this, "ExitWorkMode", RecoverPages);
            Messenger.Default.Register<CFDisplay>(this, "ShowOutPutValue", ShowOutPutValue);
            Messenger.Default.Register<object>(this, "PowBox", showPowBox);
            this.Resizable();//支持窗体拉伸
        }

        private void showPowBox(object obj)
        {
            PowSystem powWnd = new PowSystem();
            powWnd.Owner = this;
            powWnd.Show();
        }

        private void DisplayCptRelation(Tuple<string, List<OriginCell>> obj)
        {
            LayoutDocument doc = new LayoutDocument();
            doc.Title = obj.Item1;
            doc.Content = new CptRelation(obj.Item2);
            ShowPanel.Children.Add(doc);
            doc.IsActive = true;
        }
        /// <summary>
        /// 显示输出CF点的列表窗体
        /// </summary>
        private void ShowOutPutValue(CFDisplay cfs)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                OutPutView view = new OutPutView(cfs);
                view.Owner = this;
                view.Show();
            }));
        }
        /// <summary>
        /// 退出工作模式后恢复到开启工作模式时的页面打开状态
        /// </summary>
        private void RecoverPages(object obj)
        {
            IList<string> pages = (IList<string>)obj;
            List<LayoutDocument> docs = new List<LayoutDocument>();
            foreach (LayoutDocument page in ShowPanel.Children)
            {
                if (pages.Contains(page.Title))
                {
                    page.CanClose = true;
                }
                else
                {
                    docs.Add(page);
                }
                if (page.Content is WireGraph)
                {
                    WireGraph graph = (WireGraph)page.Content;
                    graph.resetComponents();
                }
            }
            docs.ForEach(doc => doc.Close());
        }
        /// <summary>
        /// 将所有显示的doc页面设置为不可关闭
        /// </summary>
        private void GraphCanNotClose(object obj)
        {
            foreach (LayoutDocument page in ShowPanel.Children)
            {
                if (page.Content is WireGraph)
                {
                    page.CanClose = false;
                }
            }
        }
        /// <summary>
        /// 保存当前打开了哪些页面
        /// </summary>
        private void SaveOpenPages(IList<string> pages)
        {
            pages.Clear();
            foreach (LayoutDocument page in ShowPanel.Children)
            {
                pages.Add(page.Title);
            }
        }   
        /// <summary>
        /// 根据拓扑数据显示布线图
        /// </summary>
        private void ShowGraph(BranchView obj)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                LayoutDocument doc = new LayoutDocument();
                WireGraph graph = null;
                bool left = true, right = true;
                doc.Title = obj.GraphName;
                switch (obj.GraphType)
                {
                    case GraphType.VccToGndGraph:
                        {
                            left = true; right = true;
                            break;
                        }
                    case GraphType.VccToCFGraph:
                        {
                            left = true; right = false;
                            break;
                        }
                    case GraphType.CFToGndGraph:
                        {
                            left = true; right = false;
                            break;
                        }
                    case GraphType.CompleteGraph:
                        {
                            left = true; right = false;
                            break;
                        }
                }
                LayoutContent other = ShowPanel.Children.FirstOrDefault(p => p.Title.Equals(doc.Title));
                if (other != null)
                {
                    other.IsActive = true;
                    return;
                }
                ShowPanel.Children.Add(doc);
                doc.IsActive = true;
                if (obj.page == null)
                {
                    graph = new WireGraph(obj, left, right);
                    obj.page = graph;
                }
                else
                {
                    graph = (WireGraph)obj.page;
                }
                doc.Content = graph;
            })); 
        }

        private void ShowBranch(List<UIElement> elements)
        {
            branchPanel.Children.Clear();
            elements.ForEach(el => branchPanel.Children.Add(el));
            branchView.IsActive = true;
        }

        private void DisplayVoltageSet(Tuple<string, List<VoltageSet>> obj)
        {
            LayoutDocument doc = new LayoutDocument();
            doc.Title = obj.Item1;
            doc.Content = new VoltageSetList(obj.Item2);
            ShowPanel.Children.Add(doc);
            doc.IsActive = true;
        }

        private void DisplayComponentLib(Tuple<string, List<Model.ComponentLib>> obj)
        {
            LayoutDocument doc = new LayoutDocument();
            doc.Title = obj.Item1;
            doc.Content = new ElectricAnalysis.View.ComponentLib(obj.Item2);
            ShowPanel.Children.Add(doc);
            doc.IsActive = true;
        }

        private void DisplayTBConnector(Tuple<string, List<TBCircuit>> obj)
        {
            LayoutDocument doc = new LayoutDocument();
            doc.Title = obj.Item1;
            doc.Content = new TBConnector(obj.Item2);
            ShowPanel.Children.Add(doc);
            doc.IsActive = true;
        }

        private void DisplayIndicatorPinInfo(Tuple<string, List<IndicatorPinInfo>> obj)
        {
            LayoutDocument doc = new LayoutDocument();
            doc.Title = obj.Item1;
            doc.Content = new IndicatorLib(obj.Item2);
            ShowPanel.Children.Add(doc);
            doc.IsActive = true;
        }

        private void DisplayRelayPinInfo(Tuple<string, List<PinInfo>> obj)
        {
            LayoutDocument doc = new LayoutDocument();
            doc.Title = obj.Item1;
            doc.Content = new ConTactorLib(obj.Item2);
            ShowPanel.Children.Add(doc);
            doc.IsActive = true;
        }

        private void DisplayTestBranch(Tuple<string, DataView> tp)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                LayoutDocument doc = new LayoutDocument();
                doc.Title = tp.Item1;
                doc.Content = new TestBranchList(new TestBranchViewModel(tp.Item2));
                ShowPanel.Children.Add(doc);
                doc.IsActive = true;
            }));
        }

        private void ShowWaitBar(Action loadAction)//等待标志
        {
            WaitingBar bar = new WaitingBar();
            maingrid.Children.Add(bar);
            bar.Margin = new Thickness((ShowPanel.DockWidth.Value - bar.ActualWidth) / 2 + manager.ActualWidth,
                ShowPanel.DockHeight.Value/2-120,
                0,0);
            loadAction.BeginInvoke(
                ar => this.Dispatcher.Invoke(() => maingrid.Children.Remove(bar))
                , null);
        }
    }
}