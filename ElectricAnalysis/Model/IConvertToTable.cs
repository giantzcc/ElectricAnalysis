using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Model
{
    interface IConvertToTable
    {
        DataView SaveBranchesToExcel();
        DataView getTable();
    }
}
