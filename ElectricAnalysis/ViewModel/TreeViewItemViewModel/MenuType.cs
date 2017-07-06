using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.ViewModel.TreeViewItemViewModel
{
    /// <summary>
    /// 资源列表每一项的菜单项
    /// </summary>
    class MenuType
    {
        #region Fields
        private string name;
        private List<MenuType> childMenus;
        private Itemvm vm;
        #endregion

        #region Constructor
        /// <summary>
        /// 构造父节点菜单
        /// </summary>
        public MenuType(string name, Itemvm vm)
        {
            this.childMenus = new List<MenuType>();
            this.name = name;
            this.vm = vm;
        }
        #endregion

        #region Properties
        public List<MenuType> ChildMenus
        {
            get { return childMenus; }
            set { childMenus = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public Itemvm Vm
        {
            get { return vm; }
            set { vm = value; }
        }
        #endregion

        #region Methods

        #endregion
    }
}
