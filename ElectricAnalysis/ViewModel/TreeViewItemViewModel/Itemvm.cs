using ElectricAnalysis.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.ViewModel.TreeViewItemViewModel
{
    /// <summary>
    /// 解决方案目录管理类
    /// </summary>
    abstract class Itemvm
    {
        #region Construction
        /// <summary>
        /// 创建非根节点
        /// </summary>
        /// <param name="type">新建目录项的类型</param>
        /// <param name="parcatalog">新建目录项的父目录项</param>
        public Itemvm(string name, List<MenuType> mns, string icon, Itemvm parcatalog)
        {
            this._parent = parcatalog;
            this._name = name;
            this.Menus = mns;
            this.Icon = icon;
            this._itemvms = new ObservableCollection<Itemvm>();
        }
        #endregion

        #region Members
        private string _name;//名称
        private Itemvm _parent;//父节点
        private string _icon;//文件图标
        private ObservableCollection<Itemvm> _itemvms;//子节点
        private List<MenuType> _menus;//菜单项
        protected AppProject pro = AppProject.GetInstance();
        protected Configuration config = Configuration.GetInstance();
        private bool isSelected;
        #endregion

        #region Properties
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Icon
        {
            get { return _icon; }
            set { _icon = value; }
        }
        public Itemvm Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }
        public ObservableCollection<Itemvm> Itemvms
        {
            get { return _itemvms; }
            set { _itemvms = value; }
        }
        public List<MenuType> Menus
        {
            get { return _menus; }
            set { _menus = value; }
        }
        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; }
        }
        #endregion

        #region Function
        public abstract void DoubleClick(Action<string> showMsg);
        public abstract void MenuClick(MenuType menu);
        public static Itemvm findItem(Collection<Itemvm> src, Type type)
        {
            Itemvm rst = null;
            foreach (Itemvm vm in src)
            {
                if (vm.GetType() == type)
                {
                    rst = vm;
                    break;
                }
                else
                {
                    rst = findItem(vm.Itemvms, type);
                }
                if (rst != null)
                    break;
            }
            return rst;
        }
        #endregion
    }
}
