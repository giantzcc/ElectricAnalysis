using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ElectricAnalysis.View.component
{
    public interface ICloneComponent
    {
        UIElement clone();
    }
}
