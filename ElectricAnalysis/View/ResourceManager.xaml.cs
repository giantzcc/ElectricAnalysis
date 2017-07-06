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
using System.Windows.Shapes;

namespace ElectricAnalysis.View
{
    /// <summary>
    /// ResourceManager.xaml 的交互逻辑
    /// </summary>
    public partial class ResourceManager : UserControl
    {
        public ResourceManager()
        {
            InitializeComponent();
        }
        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)//双击响应命令
        {
            ResourceViewModel data = this.DataContext as ResourceViewModel;
            if (e.ClickCount == 2)
                data.IsDoubleClick = true;
            else
                data.IsDoubleClick = false;
        }
    }
}
