using ElectricAnalysis.Model;
using ElectricAnalysis.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Graph
{
    public interface IObserver
    {
        void addListener(INotifyComponentChanged listener);
        void removeListener(INotifyComponentChanged listener);
    }
    /// <summary>
    /// UI元件的观察者类
    /// </summary>
    public class ComponentVMObserver : IObserver
    {
        #region Construction
        public ComponentVMObserver()
        {
            listeners = new HashSet<INotifyComponentChanged>();
        }
        #endregion

        #region Field
        private ISet<INotifyComponentChanged> listeners;
        #endregion

        #region Function
        public void addListener(INotifyComponentChanged listener)
        {
            listeners.Add(listener);
        }
        public void removeListener(INotifyComponentChanged listener)
        {
            listeners.Remove(listener);
        }

        public void updateNetLabel(string label)
        {
            foreach (INotifyComponentChanged notify in listeners)
                notify.reverseNetLabelColor(label);
        }
        public void updatePowMark(string label)
        {
            foreach (INotifyComponentChanged notify in listeners)
                notify.reversePowMark(label);
        }
        public void updateFaultNode(INotifyComponentChanged node)
        {
            foreach (INotifyComponentChanged notify in listeners)
            {
                if (ComponentViewModel.compare(notify, node))
                    notify.State = node.State;
            }
        }
        public void resetAllComponents()
        {
            foreach (INotifyComponentChanged notify in listeners)
                notify.resetNode();
        }
        public void updateComponent(ISet<INotifyComponentChanged> infos)
        {
            /*恢复初始状态*/
            resetAllComponents();
            /*标记加电点*/
            ISet<string> pows = Simulation.getInstance().getPows();
            foreach (string label in pows)
                updatePowMark(label);
            /*标记故障点*/
            ISet<INotifyComponentChanged> faults = Simulation.getInstance().getFaultComponents();
            foreach (INotifyComponentChanged nd in faults)
                updateFaultNode(nd);
            /*将导通的回路标记出来*/
            foreach (INotifyComponentChanged info in infos)
            {
                foreach (INotifyComponentChanged notify in listeners)
                {
                    if (ComponentViewModel.compare(info, notify))
                        notify.markNode();
                }
            }
            /*将得电的线圈对应的触点标记为动作，手动断开的开关显示断开*/
            ISet<string> coils = Simulation.getInstance().getPowCoils();
            ISet<string> switches = Simulation.getInstance().getBrokenSwitches();
            foreach (INotifyComponentChanged notify in listeners)
            {
                if (notify.CptType == ComponentType.ContactOpen && coils.Contains(notify.getHeadNode().Part))
                    notify.connect();
                else if (notify.CptType == ComponentType.ContactClose && coils.Contains(notify.getHeadNode().Part))
                    notify.block();
                else if ((notify.CptType == ComponentType.Breaker || notify.CptType == ComponentType.Switch)
                    && switches.Contains(notify.getHeadNode().Part))
                    notify.block();
            }
        }
        #endregion
    }
}
