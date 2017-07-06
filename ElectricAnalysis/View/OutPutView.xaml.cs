using ElectricAnalysis.Model.Result;
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
    /// OutPutView.xaml 的交互逻辑
    /// </summary>
    public partial class OutPutView : Window
    {
        private bool manualClose = false;
        public OutPutView(CFDisplay cfData)
        {
            InitializeComponent();
            this.DataContext = cfData;
            Messenger.Default.Register<object>(this, "ExitWorkMode", close);
        }

        private void close(object obj)
        {
            Messenger.Default.Unregister(this);
            manualClose = true;
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!manualClose)
            {
                e.Cancel = true;
                MessageBox.Show("请退出模式以关闭输出窗体。");
            }
            else
            {
                manualClose = false;
            }
        }
    }
}
