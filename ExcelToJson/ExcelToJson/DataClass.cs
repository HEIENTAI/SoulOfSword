using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ExcelToJson
{
    public class GlobalConst
    {
        
        #region 檔名相關常數（無副檔名）
        public const string FILENAME_EVENT = "EventData"; // 事件資料
        #endregion

        public enum DataLoadTag
        {
            [EnumClassValue(typeof(EventData), FILENAME_EVENT)]
            Event = 0, // 事件
        }
    }
    public class CommonFunction
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

        #region 列舉相關
        /// <summary>
        /// 取得Enum的Attribute
        /// </summary>
        /// <typeparam name="T">要取得的Attribute型別</typeparam>
        /// <param name="value">列舉值</param>
        /// <param name="outAttr">輸出的Attribute</param>
        /// <returns>是否有成功取得</returns>
        public static bool GetAttribute<T>(System.Enum value, out T outAttr) where T : System.Attribute
        {
            outAttr = default(T);
            System.Type curType = value.GetType();
            System.Reflection.FieldInfo curFieldInfo = curType.GetField(value.ToString());
            if (curFieldInfo != null)
            {
                T[] curAttrs = curFieldInfo.GetCustomAttributes(typeof(T), false) as T[];
                if (curAttrs != null && curAttrs.Length > 0)
                {
                    outAttr = curAttrs[0];
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
    #region GlobalDeclaration
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

    /// <summary>
    ///  自定義屬性宣告, 綁定檔案名稱與類別
    /// </summary>
    public class EnumClassValue : System.Attribute
    {
        protected string _fileName;  // assetbundle(or others) 檔名, 依據小寫
        protected Type _classType;    // class 類別W
        public EnumClassValue(Type classType, string fileName)
        {
            _fileName = fileName.ToLower();
            _classType = classType;
        }
        public EnumClassValue(Type classType)
        {
            _fileName = "";
            _classType = classType;
        }
        public EnumClassValue() { }

        public string FileName
        {
            get { return _fileName; }
        }

        public Type ClassType
        {
            get { return _classType; }
        }

        /// <summary>
        ///  從 Enum 取得 name
        /// </summary>
        public static string GetFileName(Enum value)
        {
            string output = null;
            EnumClassValue enumType = null;
            if (Retrieve(value, out enumType))
                output = enumType.FileName;
            return output;
        }

        /// <summary>
        ///  從 Enum 取得 class 類別
        /// </summary>
        public static Type GetClassType(Enum value)
        {
            Type output = null;
            EnumClassValue enumType = null;
            if (Retrieve(value, out enumType))
                output = enumType.ClassType;
            return output;
        }

        /// <summary>
        /// 將 Enum 復原為 EnumClassValue
        /// </summary>
        private static bool Retrieve(Enum value, out EnumClassValue output)
        {
            output = null;
            Type type = value.GetType();
            FieldInfo fi = type.GetField(value.ToString());
            EnumClassValue[] attrs = fi.GetCustomAttributes(typeof(EnumClassValue), false) as EnumClassValue[]; // Retrieve to self-def object
            if (attrs.Length > 0)
            {
                output = attrs[0];
                return true;
            }
            return false;
        }
    }
#endregion
}
