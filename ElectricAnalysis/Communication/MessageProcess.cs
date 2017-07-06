using ElectricAnalysis.DAL;
using ElectricAnalysis.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ElectricAnalysis.Communication
{
    /// <summary>
    /// 接收处理Can总线上的数据
    /// </summary>
    class MessageProcess : CanInfoAbs
    {
        #region Construction
        public static readonly MessageProcess Instance = new MessageProcess();
        private MessageProcess()
        {
            InitData();
        }
        #endregion

        #region Field Property
        private Queue<CAN_MSG_T>[] receiveData;//接收的指令集
        #endregion

        #region Function
        /// <summary>
        /// 初始化接收的指令集集合
        /// </summary>
        private void InitData()
        {
            receiveData = new Queue<CAN_MSG_T>[16];
            for (int i = 0; i < 16; i++)
            {
                receiveData[i] = new Queue<CAN_MSG_T>();
            }
        }
        /// <summary>
        /// 开启监听Can总线的线程
        /// </summary>
        public void BeginListen()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                CAN_MSG_T cMsg = new CAN_MSG_T();
                cMsg.data = new byte[8];
                int returnNum = 0;
                CanBusApi.CANReadFile(m_nPort, 1, ref cMsg, ref returnNum);
                if (returnNum > 0)
                {
                    receiveData[(cMsg.data[0] >> 4) - 1].Enqueue(cMsg);
                }
            });
        }
        /// <summary>
        /// 处理握手状态检测回告指令
        /// </summary>
        public byte ProcessShakehand(byte serialNum)
        {
            if (receiveData[0].Count != 0)
            {
                CAN_MSG_T msg = receiveData[0].Dequeue();
                if (msg.data[1] == serialNum)
                {
                    return msg.data[2];
                }
                else
                {
                    return 0;
                }
            }
            return 0;
        }
        /// <summary>
        /// 处理24V控制指令回告指令
        /// </summary>
        public byte Process24VCtrl(byte serialNum, byte packageNum)
        {
            if (receiveData[1].Count != 0)
            {
                CAN_MSG_T msg = receiveData[1].Dequeue();
                if (msg.data[1] == serialNum && msg.data[2] == packageNum)
                {
                    return msg.data[3];
                }
                else
                {
                    return 0;
                }
            }
            return 0;
        }
        /// <summary>
        /// 处理110V控制指令回告指令
        /// </summary>
        public byte Process110VCtrl(byte serialNum, byte packageNum)
        {
            if (receiveData[2].Count != 0)
            {
                CAN_MSG_T msg = receiveData[2].Dequeue();
                if (msg.data[1] == serialNum && msg.data[2] == packageNum)
                {
                    return msg.data[3];
                }
                else
                {
                    return 0;
                }
            }
            return 0;
        }
        /// <summary>
        /// 处理真值表接收回告指令
        /// </summary>
        public byte ProcessReceiveTruthTable(byte serialNum, byte packageNum)
        {
            if (receiveData[3].Count != 0)
            {
                CAN_MSG_T msg = receiveData[3].Dequeue();
                if(msg.data[1]==serialNum&&msg.data[2]==packageNum)
                {
                    return msg.data[3];
                }
                else
                {
                    return 0;
                }
            }
            return 0;
        }
        /// <summary>
        /// 处理运行前状态回告
        /// </summary>
        public byte ProcessReturnState(byte serialNum)
        {
            if (receiveData[4].Count != 0)
            {
                CAN_MSG_T msg = receiveData[4].Dequeue();
                if (msg.data[1] == serialNum)
                {
                    return msg.data[2];
                }
                else
                {
                    return 0;
                }
            }
            return 0;
        }
        /// <summary>
        /// 处理启动回告
        /// </summary>
        public byte ProcessStartUp(byte serialNum)
        {
            if (receiveData[5].Count != 0)
            {
                CAN_MSG_T msg = receiveData[5].Dequeue();
                if (msg.data[1] == serialNum)
                {
                    return msg.data[2];
                }
                else
                {
                    return 0;
                }
            }
            return 0;
        }
        /// <summary>
        /// 闭合查询正确回告
        /// </summary>
        public byte ProcessCloseCheckForTrue(byte serialNum)
        {
            if (receiveData[6].Count != 0)
            {
                CAN_MSG_T msg = receiveData[6].Dequeue();
                if (msg.data[1] == serialNum)
                {
                    return msg.data[2];
                }
                else
                {
                    return 0;
                }
            }
            return 0;
        }
        /// <summary>
        /// 处理闭合查询错误回告
        /// </summary>
        public IEnumerable<TestBox> ProcessCloseCheckForFalse()
        {
            List<TestBox> boxes = new List<TestBox>();
            if (receiveData[7].Count != 0)
            {
                CAN_MSG_T msg = receiveData[7].Dequeue();
                if(msg.data[2]==0x01)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        TestBox box = new TestBox(msg.data[0] & 0x0f, msg.data[4+i]);
                        boxes.Add(box);
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        TestBox box = new TestBox(msg.data[0] & 0x0f, msg.data[3 + i]);
                        boxes.Add(box);
                    }
                }
                return boxes;
            }
            return null;
        }
        /// <summary>
        /// 处理真值表正确信息反馈
        /// </summary>
        public byte ProcessTruthTableForTrue(byte serialNum)
        {
            if (receiveData[8].Count != 0)
            {
                CAN_MSG_T msg = receiveData[8].Dequeue();
                if (msg.data[1] == serialNum)
                {
                    return msg.data[2];
                }
                else
                {
                    return 0;
                }
            }
            return 0;
        }
        /// <summary>
        /// 处理真值表错误信息反馈
        /// </summary>
        public Tuple<byte, byte[]> ProcessTruthTableForFalse()
        {
            if (receiveData[9].Count != 0)
            {
                CAN_MSG_T msg = receiveData[9].Dequeue();
                return Tuple.Create<byte, byte[]>((byte)(msg.data[0] & 0x0f), new byte[4]{
                msg.data[3],msg.data[4],msg.data[5],msg.data[6]});
            }
            return null;
        }
        /// <summary>
        /// 处理程序停止指令
        /// </summary>
        public bool ProcessStopApplication()
        {
            if (receiveData[10].Count != 0)
            {
                CAN_MSG_T msg = receiveData[10].Dequeue();
                if (msg.data[1] == 0xC8)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 处理故障终止指令
        /// </summary>
        public bool ProcessFaultEnd()
        {
            if (receiveData[14].Count != 0)
            {
                CAN_MSG_T msg = receiveData[14].Dequeue();
                if (msg.data[1] == 0xCC)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 处理屏蔽指令回告
        /// </summary>
        public byte ProcessBlind(byte serialNum)
        {
            if (receiveData[11].Count != 0)
            {
                CAN_MSG_T msg = receiveData[11].Dequeue();
                if (msg.data[1] == serialNum)
                {
                    return msg.data[2];
                }
                else
                {
                    return 0;
                }
            }
            return 0;
        }
        /// <summary>
        /// 处理测试类型指令回告
        /// </summary>
        public byte ProcessSelectTestType(byte type)
        {
            if (receiveData[12].Count != 0)
            {
                CAN_MSG_T msg = receiveData[12].Dequeue();
                if (msg.data[1] == type)
                {
                    return msg.data[2];
                }
                else
                {
                    return 0;
                }
            }
            return 0;
        }
        #endregion
    }
}
