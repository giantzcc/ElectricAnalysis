﻿#pragma checksum "..\..\..\View\WireGraph.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "7959E429763D6C0828A461F0477F7988"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace ElectricAnalysis.View {
    
    
    /// <summary>
    /// WireGraph
    /// </summary>
    public partial class WireGraph : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 11 "..\..\..\View\WireGraph.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox SearchList;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\View\WireGraph.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ScrollViewer scroll;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\View\WireGraph.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border outerBd;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\..\View\WireGraph.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.ScaleTransform RendSize;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\View\WireGraph.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border bdMap;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\View\WireGraph.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas map;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/ElectricAnalysis;component/view/wiregraph.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\View\WireGraph.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 6 "..\..\..\View\WireGraph.xaml"
            ((ElectricAnalysis.View.WireGraph)(target)).KeyDown += new System.Windows.Input.KeyEventHandler(this.Graph_KeyDown);
            
            #line default
            #line hidden
            
            #line 6 "..\..\..\View\WireGraph.xaml"
            ((ElectricAnalysis.View.WireGraph)(target)).KeyUp += new System.Windows.Input.KeyEventHandler(this.Graph_KeyUp);
            
            #line default
            #line hidden
            return;
            case 2:
            this.SearchList = ((System.Windows.Controls.ComboBox)(target));
            
            #line 12 "..\..\..\View\WireGraph.xaml"
            this.SearchList.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.SearchList_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 13 "..\..\..\View\WireGraph.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Button_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.scroll = ((System.Windows.Controls.ScrollViewer)(target));
            return;
            case 5:
            this.outerBd = ((System.Windows.Controls.Border)(target));
            
            #line 16 "..\..\..\View\WireGraph.xaml"
            this.outerBd.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.map_MouseLeftButtonDown);
            
            #line default
            #line hidden
            
            #line 16 "..\..\..\View\WireGraph.xaml"
            this.outerBd.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.map_MouseLeftButtonUp);
            
            #line default
            #line hidden
            
            #line 17 "..\..\..\View\WireGraph.xaml"
            this.outerBd.MouseMove += new System.Windows.Input.MouseEventHandler(this.map_MouseMove);
            
            #line default
            #line hidden
            
            #line 17 "..\..\..\View\WireGraph.xaml"
            this.outerBd.MouseWheel += new System.Windows.Input.MouseWheelEventHandler(this.map_MouseWheel);
            
            #line default
            #line hidden
            
            #line 17 "..\..\..\View\WireGraph.xaml"
            this.outerBd.MouseLeave += new System.Windows.Input.MouseEventHandler(this.map_MouseLeave);
            
            #line default
            #line hidden
            return;
            case 6:
            this.RendSize = ((System.Windows.Media.ScaleTransform)(target));
            return;
            case 7:
            this.bdMap = ((System.Windows.Controls.Border)(target));
            return;
            case 8:
            this.map = ((System.Windows.Controls.Canvas)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

