using ElectricAnalysis.Model;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.ViewModel
{
    class LinkerConfigViewModel:ViewModelBase
    {
        #region Constructor
        public LinkerConfigViewModel()
        {

        }
        #endregion

        #region Property
        private AppProject pro = AppProject.GetInstance();
        private DataView source = null;
        public DataView Source
        {
            get { return source; }
            set
            {
                source = value;
                base.RaisePropertyChanged("Source");
            }
        }
        #endregion

        #region Function

        #endregion
    }
}
