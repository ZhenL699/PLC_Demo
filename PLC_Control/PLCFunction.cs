using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Control
{

    internal static class PLCFunction
    {
        // 连接 PLC（仅封装，非必须）
        public static bool OpenPLC(PLCController plc) => plc.Open();

        // 设定原点：发出 M0 脉冲 → 梯形图执行 RST M1/M2、停止脉冲、清零当前位置、SET M10
        public static bool SetZeroPoint(PLCController plc) => plc.SendPulse("M0");

        // 绝对定位移动：先写目标值 D0，再发 M1 脉冲
        public static bool Move(PLCController plc, int freq,int step)
        {
            // 1. 确保原点已经设定（M10=ON）
            if (step < 0)
                return false;

            // 2. 写入目标位置
            if (!plc.WriteData("D0", freq))
                return false;
            if (!plc.WriteData("D1", step))
                return false;
            // 3. 发出 M1 脉冲
            return plc.SendPulse("M1");
        }

        // 回原点：发 M2 脉冲 → DRVA K0
        public static bool BackMove(PLCController plc, int freq, int step)
        {
            // 1. 确保原点已经设定（M10=ON）
            if (step < 0)
                return false;

            // 2. 写入目标位置
            if (!plc.WriteData("D0", freq))
                return false;
            if (!plc.WriteData("D1", step))
                return false;
            // 3. 发出 M2 脉冲
            return plc.SendPulse("M2");
        }

        // 可选：清除原点标志（触发 M3）
        public static bool ResetZeroPointFlag(PLCController plc) => plc.SendPulse("M3");
    }
}