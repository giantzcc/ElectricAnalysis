using ElectricAnalysis.Graph;
using ElectricAnalysis.Model;
using ElectricAnalysis.View.component;
using ElectricAnalysis.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
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
using System.Windows.Resources;
using System.Windows.Shapes;

namespace ElectricAnalysis.View
{
    public interface IUpdateComponent
    {
        void updateLabel(string label);
        void showConductRoute(ISet<INotifyComponentChanged> infos);
        void resetComponents();
    }
    /// <summary>
    /// WireGraph.xaml 的交互逻辑
    /// </summary>
    public partial class WireGraph : UserControl, IUpdateComponent
    {
        private BranchView view;
        private ISet<string> cptNames = new HashSet<string>();
        private ISet<TNodeUI> allCpts = new HashSet<TNodeUI>();
        private ComponentVMObserver observer;
        /*用于元件检索*/
        private List<TNodeUI> selectedUis = new List<TNodeUI>();
        private int selectedIndex = 0;
        /*用于画布拖动缩放*/
        private bool mouseleftDown = false;
        private bool ctrlDown = false;
        private System.Windows.Point clickPoint = new System.Windows.Point(0, 0);
        public WireGraph(BranchView view, bool left, bool right)
        {
            InitializeComponent();
            this.observer = view.Observer;
            this.view = view;
            drawGraph(view.Nodes, left, right);
            setSearchSource(view.Nodes);
            Messenger.Default.Register<string>(this, "LabelClicked", updateLabel);
            Messenger.Default.Register<string>(this, "PowClicked", updatePowMark);
            Messenger.Default.Register<ISet<INotifyComponentChanged>>(this, "ShowConductRoute", showConductRoute);
        }
        private void setSearchSource(ISet<ShortBranchNode> brs)
        {
            ISet<ShortBranchNode> total = DigraphBuilder.expandBranch(brs);
            foreach (ShortBranchNode node in total)
            {
                foreach (TNodeUI ui in node.Uis)
                {
                    cptNames.Add(ui.Info.getName());
                    allCpts.Add(ui);
                }
            }
            SearchList.ItemsSource = cptNames;
        }
        /// <summary>
        /// 根据仿真中触发的动作更新布线图中元件状态
        /// </summary>
        public void showConductRoute(ISet<INotifyComponentChanged> infos)
        {
            observer.updateComponent(infos);
        }
        /// <summary>
        /// 添加电路图中的元件，并设置其位置
        /// </summary>
        private void drawGraph(ISet<ShortBranchNode> brs, bool left, bool right)
        {
            //绘制并联电路的封闭垂线
            if (view.Type)
            {
                ISet<ShortBranchNode> total = DigraphBuilder.expandBranch(brs);
                foreach (ShortBranchNode node in view.LabelToBranch.Values)
                {
                    if (node == null || node.EndName == "GND" || node.EndName == "DC110V")
                        continue;
                    var filterNodes = total.Where(p => node.EndName == p.EndName).ToList();
                    if (filterNodes.Count == 0 || filterNodes.Count == 1)
                        continue;
                    double max = filterNodes.Max(p => p.Uis[p.Uis.Count - 1].Pos.Y);
                    double min = filterNodes.Min(p => p.Uis[p.Uis.Count - 1].Pos.Y);
                    INotifyComponentChanged info = node.Nodes.First().Uis[node.Nodes.First().Uis.Count - 1].Info;
                    INotifyComponentChanged cpinfo = info.clone();
                    view.Observer.addListener(cpinfo);
                    VerticalLine line = new VerticalLine(cpinfo);
                    line.setLineLength((max - min) * 60);
                    ShortBranchNode brnode = filterNodes.FirstOrDefault(p => p.Uis[p.Uis.Count - 1].Pos.Y == min);
                    Canvas.SetLeft(line, DigraphBuilder.START_POS.X + brnode.Uis[brnode.Uis.Count - 1].Pos.X * 150 + 150);
                    Canvas.SetTop(line, DigraphBuilder.START_POS.Y + brnode.Uis[0].Pos.Y * 60 + 37.5);
                    map.Children.Add(line);
                }
            }

            //总正垂线
            if (left)
            {
                Line line = new Line();
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 1;
                line.X1 = 0; line.Y1 = 0;
                line.X2 = 0; line.Y2 = (brs.Last().Uis[0].Pos.Y - brs.First().Uis[0].Pos.Y) * 60;
                Canvas.SetLeft(line, DigraphBuilder.START_POS.X + brs.First().Uis[0].Pos.X * 150);
                Canvas.SetTop(line, DigraphBuilder.START_POS.Y + brs.First().Uis[0].Pos.Y * 60 + 37.5);
                map.Children.Add(line);
            }

            /*核心元器件*/
            List<TNodeUI> uis = new List<TNodeUI>();
            double widthMax=0, heightMax=0;
            double miny = double.MaxValue, maxy = double.MinValue, maxx = double.MinValue;
            ISet<ShortBranchNode> branchs = new HashSet<ShortBranchNode>(brs);
            while (branchs.Count != 0)
            {
                ISet<ShortBranchNode> temp = new HashSet<ShortBranchNode>();
                foreach (ShortBranchNode brNode in branchs)
                {
                    foreach (ShortBranchNode child in brNode.Nodes)
                        temp.Add(child);
                    if (brNode.Nodes.Count == 0)
                    {
                        if (brNode.Uis[0].Pos.Y < miny)
                            miny = brNode.Uis[0].Pos.Y;
                        if (brNode.Uis[0].Pos.Y > maxy)
                            maxy = brNode.Uis[0].Pos.Y;
                        if (brNode.Uis.Last().Pos.X > maxx)
                            maxx = brNode.Uis.Last().Pos.X;
                        if (brNode.EndName=="GND")
                            uis.Add(brNode.Uis.Last());
                    }
                    foreach (TNodeUI ui in brNode.Uis)
                    {
                        UIElement ele = BranchFactory.convert(ui);
                        map.Children.Add(ele);
                        ElectricAnalysis.Graph.Point realPos = ui.RealPos;
                        Canvas.SetLeft(ele, realPos.X);
                        Canvas.SetTop(ele, realPos.Y);
                        if (DigraphBuilder.START_POS.X + ui.Pos.X * 150 > widthMax)
                            widthMax = DigraphBuilder.START_POS.X + ui.Pos.X * 150;
                        if (DigraphBuilder.START_POS.Y + ui.Pos.Y * 60 > heightMax)
                            heightMax = DigraphBuilder.START_POS.Y + ui.Pos.Y * 60;
                    }
                    //绘垂线
                    if (brNode.Nodes.Count > 1)
                    {
                        INotifyComponentChanged cpinfo = brNode.Uis[brNode.Uis.Count - 1].Info.clone();
                        view.Observer.addListener(cpinfo);
                        VerticalLine line = new VerticalLine(cpinfo);
                        line.setLineLength((brNode.Nodes.Last().Uis[0].Pos.Y - brNode.Nodes.First().Uis[0].Pos.Y) * 60);
                        Canvas.SetLeft(line, DigraphBuilder.START_POS.X + brNode.Nodes.First().Uis[0].Pos.X * 150);
                        Canvas.SetTop(line, DigraphBuilder.START_POS.Y + brNode.Nodes.First().Uis[0].Pos.Y * 60 + 37.5);
                        map.Children.Add(line);
                    }
                }
                branchs = temp;
            }
            outerBd.Width = widthMax + 250;
            outerBd.Height = heightMax + 150;

            //总负垂线
            if (right)
            {
                Line line = new Line();
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 1;
                line.X1 = 0; line.Y1 = 0;
                line.X2 = 0; line.Y2 = (maxy - miny) * 60;
                Canvas.SetLeft(line, widthMax + 150);
                Canvas.SetTop(line, DigraphBuilder.START_POS.Y + brs.First().Uis[0].Pos.Y * 60 + 37.5);
                map.Children.Add(line);
                foreach(TNodeUI ui in uis){
                    for (double i = ui.Pos.X + 1; i <= maxx; i++)
                    {
                        BlankLine bk = new BlankLine(ui.Info);
                        map.Children.Add(bk);
                        Canvas.SetLeft(bk, DigraphBuilder.START_POS.X + i * 150);
                        Canvas.SetTop(bk, DigraphBuilder.START_POS.Y + ui.Pos.Y * 60);
                    }
                }
            }
        }
        public void updateLabel(string label)
        {
            observer.updateNetLabel(label);
        }
        private void updatePowMark(string label)
        {
            observer.updatePowMark(label);
        }
        private void map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseleftDown = true;
            scroll.Cursor = Cursors.ScrollAll;
            clickPoint = e.GetPosition(this);
        }

        private void map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouseleftDown = false;
            scroll.Cursor = Cursors.Arrow;
        }

        private void Graph_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl && !ctrlDown)
            {
                ctrlDown = true;
                StreamResourceInfo sri = System.Windows.Application.GetResourceStream(new Uri(@"pack://application:,,,/Image/mag.ico", UriKind.RelativeOrAbsolute));
                scroll.Cursor = new Cursor(sri.Stream);
            }
        }

        private void Graph_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.LeftCtrl && ctrlDown)
            {
                ctrlDown = false;
                scroll.Cursor = Cursors.Arrow;
            }
        }

        private void map_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ctrlDown && !mouseleftDown)
            {
                if (e.Delta > 0)
                {
                    double x = scroll.HorizontalOffset;
                    double y = scroll.VerticalOffset;
                    RendSize.ScaleX += 0.01;
                    RendSize.ScaleY += 0.01;
                }
                else
                {
                    RendSize.ScaleX -= 0.01;
                    RendSize.ScaleY -= 0.01;
                }
                e.Handled = true;
            }
        }

        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseleftDown && !ctrlDown)
            {
                System.Windows.Point current = e.GetPosition(this);
                double dlatx = current.X - clickPoint.X;
                double dlaty = current.Y - clickPoint.Y;
                clickPoint = current;
                scroll.ScrollToHorizontalOffset(scroll.HorizontalOffset - dlatx);
                scroll.ScrollToVerticalOffset(scroll.VerticalOffset - dlaty);
            }
        }

        private void SearchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedUis.Clear();
            if (SearchList.SelectedItem != null)
            {
                string name = SearchList.SelectedItem as string;
                foreach (TNodeUI ui in allCpts)
                {
                    if (ui.Info.getName().Equals(name))
                        selectedUis.Add(ui);
                }
            }
            Button_Click(null, null);
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (selectedUis.Count != 0)
            {
                selectedIndex %= selectedUis.Count;
                TNodeUI ui = selectedUis[selectedIndex++];
                scroll.ScrollToHorizontalOffset(ui.RealPos.X * RendSize.ScaleX - scroll.ActualWidth / 2d);
                scroll.ScrollToVerticalOffset(ui.RealPos.Y * RendSize.ScaleY - scroll.ActualHeight / 2d);
            }
        }

        private void map_MouseLeave(object sender, MouseEventArgs e)
        {
            mouseleftDown = false;
        }
        /// <summary>
        /// 将元件状态重置
        /// </summary>
        public void resetComponents()
        {
            observer.resetAllComponents();
        }
    }
}
