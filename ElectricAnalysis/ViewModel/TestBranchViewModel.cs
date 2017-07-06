using ElectricAnalysis.Graph;
using ElectricAnalysis.Model;
using ElectricAnalysis.View.component;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ElectricAnalysis.ViewModel
{
    public class TestBranchViewModel
    {
        #region Constructor
        public TestBranchViewModel(DataView dt)
        {
            this.source = dt;
            DoubleClickCmd = new RelayCommand(showRoute, canShowRoute);
        }
        #endregion

        #region command
        public RelayCommand DoubleClickCmd { get; set; }//双击命令
        #endregion

        #region Field Property
        public bool IsDoubleClick { get; set; }//双击标志
        public int SelectedIndex { get; set; }//选中项序号
        private DataView source;//视图数据
        public DataView Source
        {
            get { return source; }
            set { source = value; }
        }
        #endregion

        #region function
        private void showRoute()
        {
            IsDoubleClick = false;
            AppProject pro = AppProject.GetInstance();
            TestBranch br = null;
            if (source.Table.TableName == "地线导通测试")
                br = pro.getGndBranch(SelectedIndex);
            else if (source.Table.TableName == "110V导通测试")
                br = pro.getVccBranch(SelectedIndex);
            else if (source.Table.TableName == "普通导通测试")
                br = pro.getNormalBranch(SelectedIndex);
            else if (source.Table.TableName == "测试回路")
                br = pro.getLoopBranch(SelectedIndex);
            else if (source.Table.TableName == "逻辑测试")
                br = pro.getLogicBranch(SelectedIndex);
            if (br != null)
            {
                List<UIElement> elements = BranchFactory.convertToUIElement(br.Branch);
                Messenger.Default.Send<List<UIElement>>(elements, "ShowBranchPicture");
            }
            Console.WriteLine(source.Table.TableName);
        }

        private bool canShowRoute()
        {
            return IsDoubleClick;
        }
        #endregion
    }
}
