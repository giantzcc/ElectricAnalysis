using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Communication
{
    abstract class CanInfoAbs
    {
        #region Field Property
        protected ushort m_nBTR0;
        protected ushort m_nBTR1;
        protected ushort m_nAcpCode;
        protected ushort m_nAcpMask;
        protected ushort m_nIntMask;
        protected ushort m_nOutCtrlCode;

        protected uint m_nDevNum = 0;
        protected ushort m_nPort = 0;
        protected ushort m_nHostID = 0;
        protected ushort m_nPreBaudRate = 0;

        protected int ready = 0;

        protected string m_strDevName = " 000 : {CAN1 I/O=da000H  Interrupt=5}";
        #endregion
    }
}
