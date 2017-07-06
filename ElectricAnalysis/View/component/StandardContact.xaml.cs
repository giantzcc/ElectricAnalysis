﻿using ElectricAnalysis.Graph;
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
    /// StandardContact.xaml 的交互逻辑
    /// </summary>
    public partial class StandardContact : UserControl, ICloneComponent
    {
        public readonly INotifyComponentChanged info;
        public StandardContact(ComponentType type, TNode node1, TNode node2)
        {
            InitializeComponent();
            if (type == ComponentType.ContactOpen)
            {
                open.Visibility = Visibility.Visible;
                close.Visibility = Visibility.Hidden;
            }
            INotifyComponentChanged VM = new ComponentViewModel(Tuple.Create<TNode, TNode>(node1, node2), type);
            VM.initStatus(type);
            this.info = (INotifyComponentChanged)VM;
            this.DataContext = VM;
        }
        public StandardContact(INotifyComponentChanged info)
        {
            InitializeComponent();
            if (info.CptType == ComponentType.ContactOpen)
            {
                open.Visibility = Visibility.Visible;
                close.Visibility = Visibility.Hidden;
            }
            info.initStatus(info.CptType);
            this.info = info;
            this.DataContext = info;
        }

        public UIElement clone()
        {
            return new StandardContact(info);
        }
    }
}
