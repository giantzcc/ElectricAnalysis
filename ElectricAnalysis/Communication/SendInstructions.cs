using ElectricAnalysis.DAL;
using ElectricAnalysis.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Communication
{
    class SendInstructions : CanInfoAbs
    {
        #region Construction
        public static readonly SendInstructions Instance = new SendInstructions();
        private SendInstructions()
        {
            instructNum = 0;
        }
        #endregion

        #region Field Property
        private static byte instructNum;//指令序列号
        public byte Instruct
        {
            get
            {
                return (byte)(instructNum - 1);
            }
        }
        private byte InstructNum
        {
            get
            {
                return instructNum++;
            }
        }
        #endregion

        #region 发送指令
        /// <summary>
        /// 发送握手指令
        /// </summary>
        public int SendShakehand(uint deviceId)
        {
            CAN_MSG_T msg = new CAN_MSG_T();
            msg.rtr = 0;
            msg.dlen = 2;
            msg.id = deviceId;
            msg.data = new byte[8];
            msg.data[0] = (byte)(0x10 + deviceId & 0x0F);
            msg.data[1] = InstructNum;
            return CanBusApi.CANWriteFile(m_nPort, ref ready, ref msg);
        }
        /// <summary>
        /// 发送24V控制指令
        /// </summary>
        public void Send24VCtrl(Queue<CAN_MSG_T> canMsgs)
        {

        }
        /// <summary>
        /// 发送110V控制指令
        /// </summary>
        public void Send110VCtrl(Queue<CAN_MSG_T> canMsgs)
        {

        }
        public int SendStateCheck()
        {
            return 0;
        }
        #endregion


        #region 初始化指令
        /// <summary>
        /// 初始化24V控制指令
        /// </summary>
        public Queue<CAN_MSG_T> Init24VCtrl(List<TestBox> inputPts)
        {
            Queue<CAN_MSG_T> instructs = new Queue<CAN_MSG_T>();
            ILookup<int, TestBox> lookupPts = inputPts.ToLookup(p => p.BoxNum);
            CAN_MSG_T msgtemp = new CAN_MSG_T();
            msgtemp.rtr = 0;
            msgtemp.dlen = 8;
            msgtemp.data = new byte[8];
            for (int i = 1; i <= 6; i++)
            {
                /*同一块控制板上注入点的总个数*/
                int ptsCount = lookupPts[i].Count(p => true);
                /*电压点号*/
                Queue<int> pts = new Queue<int>();
                foreach (var pt in lookupPts[i])
                {
                    pts.Enqueue(pt.TestBoxPort);
                }
                /*同一块控制板的指令包个数*/
                byte msgNum = (byte)(ptsCount <= 3 ? 1 : ((ptsCount - 2) / 4) + 1);
                if (ptsCount > 0)
                {
                    /*第一包*/
                    CAN_MSG_T msg = msgtemp;
                    msg.id = (byte)(lookupPts[i].First().BoxNum + 0x80);
                    msg.data[0] = (byte)(0x20 + lookupPts[i].First().BoxNum);
                    msg.data[1] = InstructNum;
                    msg.data[2] = 1;
                    msg.data[3] = msgNum;
                    for (int j = 0; j < 3; j++)
                    {
                        if (pts.Count > 0)
                        {
                            byte num = (byte)pts.Dequeue();
                            msg.data[j + 4] = num;
                        }
                        else
                        {
                            msg.data[j + 4] = 0xEF;
                        }
                    }
                    /*校验码*/
                    msg.data[7] = (byte)(msg.data[1] ^ msg.data[2] ^ msg.data[3] ^ msg.data[4] ^ msg.data[5] ^ msg.data[6]);
                    instructs.Enqueue(msg);
                }
                /*其余几包*/
                for (int j = 1; j < msgNum; j++)
                {
                    CAN_MSG_T msg = msgtemp;
                    msg.id = (byte)(lookupPts[i].First().BoxNum + 0x80);
                    msg.data[0] = (byte)(0x20 + lookupPts[i].First().BoxNum);
                    msg.data[1] = InstructNum;
                    msg.data[2] = (byte)(j + 1);
                    for (int k = 0; k < 4; k++)
                    {
                        if (pts.Count > 0)
                        {
                            byte num = (byte)pts.Dequeue();
                            msg.data[k + 3] = num;
                        }
                        else
                        {
                            msg.data[k + 3] = 0xEF;
                        }
                    }
                    msg.data[7] = (byte)(msg.data[1] ^ msg.data[2] ^ msg.data[3] ^ msg.data[4] ^ msg.data[5] ^ msg.data[6]);
                    instructs.Enqueue(msg);
                }
            }
            return instructs;
        }
        /// <summary>
        /// 初始化110V控制指令
        /// </summary>
        public Queue<CAN_MSG_T> Init110VCtrl(List<TestBox> inputPts)
        {
            Queue<CAN_MSG_T> instructs = new Queue<CAN_MSG_T>();
            ILookup<int, TestBox> lookupPts = inputPts.ToLookup(p => p.BoxNum);
            CAN_MSG_T msgtemp = new CAN_MSG_T();
            msgtemp.rtr = 0;
            msgtemp.dlen = 8;
            msgtemp.data = new byte[8];
            for (int i = 1; i <= 6; i++)
            {
                /*同一块控制板上注入点的总个数*/
                int ptsCount = lookupPts[i].Count(p => true);
                /*电压点号*/
                Queue<int> pts = new Queue<int>();
                foreach (var pt in lookupPts[i])
                {
                    pts.Enqueue(pt.TestBoxPort);
                }
                /*同一块控制板的指令包个数*/
                byte msgNum = (byte)(ptsCount <= 3 ? 1 : ((ptsCount - 2) / 4) + 1);
                if (ptsCount > 0)
                {
                    /*第一包*/
                    CAN_MSG_T msg = msgtemp;
                    msg.id = (byte)(lookupPts[i].First().BoxNum + 0x80);
                    msg.data[0] = (byte)(0x30 + lookupPts[i].First().BoxNum);
                    msg.data[1] = InstructNum;
                    msg.data[2] = 1;
                    msg.data[3] = msgNum;
                    for (int j = 0; j < 3; j++)
                    {
                        if (pts.Count > 0)
                        {
                            byte num = (byte)pts.Dequeue();
                            msg.data[j + 4] = num;
                        }
                        else
                        {
                            msg.data[j + 4] = 0xEF;
                        }
                    }
                    /*校验码*/
                    msg.data[7] = (byte)(msg.data[1] ^ msg.data[2] ^ msg.data[3] ^ msg.data[4] ^ msg.data[5] ^ msg.data[6]);
                    instructs.Enqueue(msg);
                }
                /*其余几包*/
                for (int j = 1; j < msgNum; j++)
                {
                    CAN_MSG_T msg = msgtemp;
                    msg.id = (byte)(lookupPts[i].First().BoxNum + 0x80);
                    msg.data[0] = (byte)(0x30 + lookupPts[i].First().BoxNum);
                    msg.data[1] = InstructNum;
                    msg.data[2] = (byte)(j + 1);
                    for (int k = 0; k < 4; k++)
                    {
                        if (pts.Count > 0)
                        {
                            byte num = (byte)pts.Dequeue();
                            msg.data[k + 3] = num;
                        }
                        else
                        {
                            msg.data[k + 3] = 0xEF;
                        }
                    }
                    msg.data[7] = (byte)(msg.data[1] ^ msg.data[2] ^ msg.data[3] ^ msg.data[4] ^ msg.data[5] ^ msg.data[6]);
                    instructs.Enqueue(msg);
                }
            }
            return instructs;
        }
        /// <summary>
        /// 初始化真值表数据指令
        /// </summary>
        public Queue<CAN_MSG_T> InitTruthTableData(List<TestBox> highLvPts)
        {
            return null;
        }

        #endregion
    }
}
