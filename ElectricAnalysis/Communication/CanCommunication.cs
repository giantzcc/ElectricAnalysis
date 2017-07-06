using ElectricAnalysis.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ElectricAnalysis.Communication
{
    /// <summary>
    /// 管理整个通信
    /// </summary>
    class CanCommunication : CanInfoAbs
    {
        #region Signle Instance
        private static CanCommunication instance = null;
        private static object syncObj = new object();
        private CanCommunication(){}
        public static CanCommunication GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                    instance = new CanCommunication();
                return instance;
            }
        }
        #endregion

        #region Field Property
        
        #endregion

        #region Function
        /// <summary>
        /// Can通讯初始化
        /// </summary>
        public void CanInit(IntPtr hwnd)
        {
            m_nBTR0 = 0x03;
            m_nBTR1 = 0x1C;
            m_nAcpCode = 0;
            m_nAcpMask = 255;
            m_nIntMask = 0;
            m_nOutCtrlCode = 250;


            byte[] DeviceName = Encoding.ASCII.GetBytes(m_strDevName);

            CanBusApi.CANPortClose(m_nPort);
            if (CanBusApi.CANSelectDevice(hwnd, 0, ref m_nDevNum, DeviceName) != 0)
            {
                
            }

            if (CanBusApi.CANPortOpen((UInt16)m_nDevNum, ref m_nPort, ref m_nHostID, ref m_nPreBaudRate) != 0)
            {
                MessageBox.Show("CAN port open error!", "Error");
            }
            if (CanBusApi.CANSetProtocolType(m_nPort, 0) != 0)
            {
                MessageBox.Show("CAN Set Protocol Type error!", "Error");
                CanBusApi.CANPortClose(m_nPort);
            }


            if (CanBusApi.CANHwReset(m_nPort) != 0)
            {
                MessageBox.Show("CAN port HW Reset error!", "Error");
                CanBusApi.CANPortClose(m_nPort);
            }

            if (CanBusApi.CANInit(m_nPort, m_nBTR0, m_nBTR1, (byte)m_nIntMask) != 0)
            {
                MessageBox.Show("CAN port init error!", "Error");
                CanBusApi.CANPortClose(m_nPort);
            }

            if (CanBusApi.CANSetOutCtrl(m_nPort, m_nOutCtrlCode) != 0)
            {
                MessageBox.Show("CAN set out ctrl code error!", "Error");
                CanBusApi.CANPortClose(m_nPort);
            }

            if (CanBusApi.CANSetAcp(m_nPort, m_nAcpCode, m_nAcpMask) != 0)
            {
                MessageBox.Show("CAN set acp code error!", "Error");
                CanBusApi.CANPortClose(m_nPort);
            }

            if (CanBusApi.CANSetBaud(m_nPort, m_nBTR0, m_nBTR1) != 0)
            {
                MessageBox.Show("CAN set baud rate error!", "Error");
                CanBusApi.CANPortClose(m_nPort);
            }

            if (CanBusApi.CANSetNormal(m_nPort) != 0)
            {
                MessageBox.Show("CAN set normal error!", "Error");
                CanBusApi.CANPortClose(m_nPort);
            }
            CanBusApi.CANEnableEvent(m_nPort, 1);
            CanBusApi.CANEnableRxInt(m_nPort);

            m_strDevName = Encoding.ASCII.GetString(DeviceName);
        }
        #endregion
    }
}
