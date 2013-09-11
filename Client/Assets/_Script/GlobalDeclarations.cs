using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/// <summary>
/// 開始就需要的資源要實作
/// </summary>
public interface IStartDependency
{
    bool IsStartDataReady { get; }
}

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
    public ushort? CheckData; // 要檢查的資料標示 (檢查條件式[A op B] 中的A）
    public byte? CheckOp; // 檢查方法（檢查條件式[A op B]中的op）
    public ushort? CheckTarget; // 檢查目標數值（檢查條件式[A op B]中的B）

    public override string ToString()
    {
        return string.Format("CheckType = {0} CheckData = {1} CheckOp = {2} CheckTarget = {3}\n", CheckType, CheckData, CheckOp, CheckTarget);
    }
}

[StructLayout(LayoutKind.Sequential)]
public class EventEffectData
{
    public ushort? EffectType; // 效果類型
    public ushort?[] EffectParameter = new ushort?[3]; // 效果參數
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("EffectType = {0}\n============\n", EffectType);
        for (int i = 0; i < EffectParameter.Length; ++i)
        {
            sb.AppendFormat("EffectParameter[{0}] = {1}\n", i,  EffectParameter[i]);
        }
        return sb.ToString();
    }
}

/// <summary>
/// 從表格來的事件資料
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public class EventData
{
    public ushort MainID; // 事件ID
    public ushort SubID;  // 子事件ID
    public byte EffectID; // 效果ID
    public EventConditionData CheckCondition; // 事件檢查條件
    public EventEffectData TrueEffect; // 正效果
    public EventEffectData FalseEffect; // 反效果

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("EventMainID = {0} EventSubID = {1} EffectID = {2}\n", MainID, SubID, EffectID);
        sb.AppendFormat("===================================\n");
        sb.AppendFormat("事件條件：\n{0}\n", CheckCondition);
        sb.AppendFormat("事件正效果：\n{0}\n", TrueEffect);
        sb.AppendFormat("事件反效果：\n{0}\n", FalseEffect);
        sb.AppendFormat("=======================================\n");

        return sb.ToString();
    }
}