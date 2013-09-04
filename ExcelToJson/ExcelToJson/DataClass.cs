using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ExcelToJson
{
    public class GlobalConst
    {
        public static readonly DataConvertInfomation[] DataConvertList = 
        {
            new DataConvertInfomation(typeof(EventData), "EventData"),
        };

    }
    public class Common
    {
        public static void DebugMsg(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// 依照string.Format的格式輸入參數，印出由string.Format回傳的訊息
        /// </summary>
        public static void DebugMsgFormat(string format, params object[] args)
        {
            Console.WriteLine(string.Format(format, args));
        }
    }
    #region GlobalDeclaration
    /// <summary>
    /// 描述資料轉換用的資訊用的class，描述資料結構和檔名之間的對應
    /// </summary>
    public class DataConvertInfomation
    {
        public System.Type DataType
        {
            get;
            protected set;
        }
        public string FileName
        {
            get;
            protected set;
        }

        public DataConvertInfomation(System.Type setDataType, string setFileName)
        {
            DataType = setDataType;
            FileName = setFileName;
        }
    }

    /// <summary>
    /// 事件檢查條件資料
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class EventConditionData
    {
        public ushort? CheckType; // 檢查類型
        public ushort? CheckCondition1; // 檢查條件1
        public ushort? CheckCondition2; // 檢查條件2
        /// <summary>
        /// 描述此class內容的字串
        /// </summary>
        public override string ToString()
        {
            return string.Format("CheckType = {0} CheckCondition1 = {1} CheckCondition2 = {2}\n", CheckType, CheckCondition1, CheckCondition2);
        }
    }
    /// <summary>
    /// 事件效果資料
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class EventEffectData
    {
        public ushort? EffectType; // 效果類型
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public ushort?[] EffectParameter = new ushort?[3]; // 效果參數
        /// <summary>
        /// 描述此class內容的字串
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("EffectType = {0}\n============\n", EffectType);
            for (int i = 0; i < EffectParameter.Length; ++i)
            {
                sb.AppendFormat("EffectParameter[{0}] = {1}\n", i, EffectParameter[i]);
            }
            return sb.ToString();
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public class EventData
    {
        public string testStr; // 測試用資料
        public byte? testByte;
        public uint? testUINT;
        public ushort? EventMainID;                 // 事件ID
        public ushort? EventSubID;                  // 子事件ID
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public EventConditionData[] EventCheckCondition = new EventConditionData[3]; // 事件檢查條件
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public EventEffectData[] TrueEffect = new EventEffectData[3];                // 正效果
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public EventEffectData[] FalseEffect = new EventEffectData[3];               // 反效果
        /// <summary>
        /// 描述此class內容的字串
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("For Test testStr = {0} testByte = {1} testUINT = {2}\n", testStr, testByte, testUINT);
            sb.AppendFormat("EventMainID = {0} EventSubID = {1}\n", EventMainID, EventSubID);
            sb.AppendFormat("===================================\n");
            sb.AppendFormat("事件條件：\n");

            for (int i = 0; i < EventCheckCondition.Length; ++i)
            {
                sb.AppendFormat("EventCheckCondition[{0}] = \n{1}\n", i, EventCheckCondition[i]);
            }
            sb.AppendFormat("事件正效果：\n");
            for (int i = 0; i < TrueEffect.Length; ++i)
            {
                sb.AppendFormat("TrueEffect[{0}] = \n{1}\n", i, TrueEffect[i]);
            }
            sb.AppendFormat("事件反效果：\n");
            for (int i = 0; i < FalseEffect.Length; ++i)
            {
                sb.AppendFormat("FalseEffect[{0}] = \n{1}\n", i, FalseEffect[i]);
            }
            sb.AppendFormat("=======================================\n");

            return sb.ToString();
        }

    }
#endregion
}
