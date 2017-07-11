using ElectricAnalysis.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model.LogicTest
{
    public class LogicCircuit
    {
        #region Construction
        public LogicCircuit(List<INotifyComponentChanged> circuit, TNode cf, string pow)
        {
            this.circuit = circuit;
            this.cf = cf;
            this.pow = pow;
            load();
        }
        #endregion

        #region Field Property
        private List<INotifyComponentChanged> circuit;
        private List<LogicCircuit> additional = new List<LogicCircuit>();
        private TNode cf;
        private ISet<string> openContacts = new HashSet<string>();//常开的触点
        private ISet<string> closeContacts = new HashSet<string>();//常闭的触点
        private ISet<INotifyComponentChanged> components = new HashSet<INotifyComponentChanged>();//所有的被测试元件
        private ISet<string> coils = new HashSet<string>();//包含的得电线圈
        private List<string> pows;//条件支路的加电点
        private string pow;//被测支路的加电点
        public ISet<INotifyComponentChanged> Components
        {
            get { return components; }
        }
        public TNode CF
        {
            get { return cf; }
        }
        public ISet<string> OpenContacts
        {
            get { return openContacts; }
        }
        public ISet<string> CloseContacts
        {
            get { return closeContacts; }
        }
        public string Pow
        {
            get { return pow; }
        }
        public ISet<string> Coils
        {
            get { return coils; }
            set { coils = value; }
        }
        public List<string> Pows
        {
            get { return pows; }
            set { pows = value; }
        }
        public List<INotifyComponentChanged> Circuit
        {
            get { return circuit; }
        }
        public List<LogicCircuit> Additional
        {
            get { return additional; }
        }
        #endregion

        #region Function
        /// <summary>
        /// 初始化该支路包含的逻辑关系
        /// </summary>
        private void load()
        {
            foreach (INotifyComponentChanged info in circuit)
            {
                if(info.CptType == ViewModel.ComponentType.ContactOpen)
                    openContacts.Add(info.getName());
                if (info.CptType == ViewModel.ComponentType.ContactClose)
                    closeContacts.Add(info.getName());
            }
        }
        /// <summary>
        /// 从导通的回路中整理出所有测试元件
        /// </summary>
        /// <param name="conductRoutes"></param>
        public void saveTestComponents(List<List<INotifyComponentChanged>> conductRoutes)
        {
            conductRoutes.ForEach(p => p.ForEach(q => components.Add(q)));
        }
        #endregion
    }
}
