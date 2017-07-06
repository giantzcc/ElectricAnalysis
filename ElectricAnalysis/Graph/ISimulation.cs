using ElectricAnalysis.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Graph
{
    public interface ISimulation
    {
        void resetSimulation();
        void powUp(string label);
        void powDown(string label);
        bool containsPow(string label);
        ISet<string> getPowCoils();
        ISet<string> getBrokenSwitches();
        void blockComponent(string name, ComponentType type);
        void connectComponent(string name, ComponentType type);
    }
}
