using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Control
{
    using System;

    internal class PLCController
    {
        private readonly ActUtlType64Lib.ActUtlType64Class plc;
        private readonly int logicalStationNumber;
        private bool isConnected = false;

        /// <summary>
        /// 公开构造函数
        /// </summary>
        public PLCController(int logicalStationNumber)
        {
            this.logicalStationNumber = logicalStationNumber;
            plc = new ActUtlType64Lib.ActUtlType64Class();
            plc.ActLogicalStationNumber = logicalStationNumber;
        }

        /// <summary>
        /// 打开连接（成功返回 true）
        /// </summary>
        public bool Open()
        {
            if (isConnected)
            {
                return true;
            }

            int result = plc.Open();
            if (result == 0)
            {
                isConnected = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            if (isConnected)
            {
                plc.Close();
                isConnected = false;
            }
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        public bool ReadData(string deviceName, out int data)
        {
            data = 0;
            if (!isConnected)
            {
                return false;
            }

            int result = plc.GetDevice(deviceName, out data);
            if (result == 0)
                return true;

            return false;
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        public bool WriteData(string deviceName, int data)
        {
            if (!isConnected)
            {
                return false;
            }

            int result = plc.SetDevice(deviceName, data);
            if (result == 0)
                return true;

            return false;
        }
        public bool SendPulse(string deviceName)
        {
            if (!WriteData(deviceName, 1)) return false;
            Thread.Sleep(100);
            return WriteData(deviceName, 0);
        }
    }
}