using ElectricAnalysis.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// TestBranchList.xaml 的交互逻辑
    /// </summary>
    public partial class TestBranchList : UserControl
    {
        public TestBranchList(TestBranchViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }

        void datatb_Loaded(object sender, RoutedEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            for (int i = 0; i < dg.Columns.Count; i++)
            {
                double width = (dg.ActualWidth-25) / dg.Columns.Count;
                if (width < 150)
                    width = 150;
                dg.Columns[i].Width = width;
            }
        }

        private void datatb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TestBranchViewModel vm = this.DataContext as TestBranchViewModel;
            vm.IsDoubleClick = true;
        }
    }
}
