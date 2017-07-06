using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model.Result
{
    public enum SourceType
    {
        Source1,
        Source2
    }
    public class CFPair:ViewModelBase
    {
        #region Construction
        public CFPair(TNode node)
        {
            this.node = node;
        }
        #endregion

        #region Field/Property
        public static readonly string HIGH = "1";
        public static readonly string LOW = "0";
        private TNode node;
        public string Part
        {
            get
            {
                return node.Part;
            }
        }
        public string Num
        {
            get
            {
                return node.Num;
            }
        }
        public string Equal
        {
            get
            {
                return node.Equal;
            }
        }
        private string logicValue = LOW;//正常逻辑值
        public string LogicValue
        {
            get
            {
                return logicValue;
            }
            set
            {
                logicValue = value;
                base.RaisePropertyChanged("LogicValue");
                base.RaisePropertyChanged("ComparedValue");
            }
        }
        private string realValue = LOW;//实测逻辑值
        public string RealValue
        {
            get
            {
                return realValue;
            }
            set
            {
                realValue = value;
                base.RaisePropertyChanged("RealValue");
                base.RaisePropertyChanged("ComparedValue");
            }
        }
        public string ComparedValue
        {
            get
            {
                return logicValue == realValue ? "√" : "×";
            }
        }
        #endregion
    }
}
