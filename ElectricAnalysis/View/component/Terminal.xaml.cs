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
    /// Terminal.xaml 的交互逻辑
    /// </summary>
    public partial class Terminal : UserControl, ICloneComponent
    {
        public readonly INotifyComponentChanged info;
        public Terminal(ComponentType type, TNode node)
        {
            InitializeComponent();
            INotifyComponentChanged VM = new ComponentViewModel(Tuple.Create<TNode, TNode>(node, null), type);
            VM.initStatus(type);
            this.info = (INotifyComponentChanged)VM;
            this.DataContext = VM;
        }
        public Terminal(INotifyComponentChanged info)
        {
            InitializeComponent();
            info.initStatus(info.CptType);
            this.info = info;
            this.DataContext = info;
        }

        public UIElement clone()
        {
            return new Terminal(info);
        }
    }
}
