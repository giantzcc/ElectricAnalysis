using ElectricAnalysis.Graph;
using ElectricAnalysis.Model;
using ElectricAnalysis.ViewModel;
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
using System.Windows.Shapes;

namespace ElectricAnalysis.View.component
{
    /// <summary>
    /// VerticalLine.xaml 的交互逻辑
    /// </summary>
    public partial class VerticalLine : UserControl, ICloneComponent
    {
        public readonly INotifyComponentChanged info;
        public VerticalLine(ComponentType type, TNode node1, TNode node2)
        {
            InitializeComponent();
            INotifyComponentChanged VM = new ComponentViewModel(Tuple.Create<TNode, TNode>(node1, node2), type);
            VM.initStatus(type);
            this.info = VM;
            this.DataContext = VM;
        }
        public VerticalLine(INotifyComponentChanged info)
        {
            InitializeComponent();
            info.initStatus(ComponentType.Blank);
            this.info = info;
            this.DataContext = info;
        }
        public void setLineLength(double len)
        {
            vline.Y2 = len;
        }

        public UIElement clone()
        {
            return new VerticalLine(info);
        }
    }
}
