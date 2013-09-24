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
/// 事件檢查條件資料
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public class GameEventConditionData
{
    public ushort? CheckType;   // 檢查類型
    public ushort? CheckData;   // 要檢查的資料標示 (檢查條件式[A op B] 中的A）
    public byte?   CheckOp;     // 檢查方法（檢查條件式[A op B]中的op）
    public ushort? CheckTarget; // 檢查目標數值（檢查條件式[A op B]中的B）

    public override string ToString()
    {
        return string.Format("CheckType = {0} CheckData = {1} CheckOp = {2} CheckTarget = {3}\n", CheckType, CheckData, CheckOp, CheckTarget);
    }
}

[StructLayout(LayoutKind.Sequential)]
public class GameEventEffectData
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
/// 從表格來的遊戲事件資料
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public class GameEventData
{
    public ushort MainID;   // 事件ID
    public ushort SubID;    // 子事件ID
    public byte   EffectID; // 效果ID
    public GameEventConditionData CheckCondition; // 事件檢查條件
    public GameEventEffectData TrueEffect;        // 正效果
    public GameEventEffectData FalseEffect;       // 反效果

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

/// <summary>
/// 從表格來的NPC表資料
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public class NPCTableData
{
    public ushort NPCID;     // NPC ID
    public string NPCName;   // NPC 名字
    public string ModelName; // 模型名字
    public ushort Scale;     // 放大倍率（乘以100後的數值，程式中使用要除以100）
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("========= NPCTableData ============\n");
        sb.AppendFormat("NPC ID = {0}\n", NPCID);
        sb.AppendFormat("NPC 名字 = {0}\n", NPCName);
        sb.AppendFormat("模型名字 = {0}\n", ModelName);
        sb.AppendFormat("放大倍率(*100之後的數值) = {0}\n", Scale);
        sb.Append("===================================\n");
        return sb.ToString();
    }
}

/// <summary>
/// 從表格來的種植檔資料
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public class PlantData
{
    public ushort SceneID; // 場景ID
    public ushort NPCID;   // NPC ID
    public byte   SerialNumber; // 序列號
    public ushort PosX; // 座標X
    public ushort PosY; // 座標Y
    public ushort RotateX; // 對X軸的旋轉值（乘以100後的數值，程式中使用要除以100）
    public ushort RotateY; // 對Y軸的旋轉值（乘以100後的數值，程式中使用要除以100）
    public ushort RotateZ; // 對Z軸的旋轉值（乘以100後的數值，程式中使用要除以100）
    public ushort EventMainID; // 主事件ID (該NPC會觸發的事件)
    public byte IsVisible; // 是否出現
    public byte IsVisibleInDrama; // 是否在動畫中可見

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("=========== PlantData =============\n");
        sb.AppendFormat("場景ID = {0}\n", SceneID);
        sb.AppendFormat("NPC ID = {0}\n", NPCID);
        sb.AppendFormat("位置 = ({0}, {1})\n", PosX, PosY);
        sb.AppendFormat("旋轉 = ({0}, {1}, {2})(三者都是乘以100之後的數值)\n", RotateX, RotateY, RotateZ);
        sb.AppendFormat("主事件ID = {0}\n", EventMainID);
        sb.AppendFormat("是否出現 = {0}\n", IsVisible);
        sb.AppendFormat("是否在動畫中出現 = {0}\n", IsVisibleInDrama);
        sb.Append("===================================\n");
        return sb.ToString();
    }
}

/// <summary>
/// 從表格來的場景資料
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public class SceneData
{
    public ushort SceneID;       // 場景ID
    public string SceneName;     // 場景名稱（中地圖or剛換到該場景顯示的名稱）
    public string SceneFileName; // 場景檔案名稱

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("=========== SceneData =============\n");
        sb.AppendFormat("場景ID = {0}\n", SceneID);
        sb.AppendFormat("場景名稱 = {0}\n", SceneName);
        sb.AppendFormat("場景檔案名稱 = {0}\n", SceneFileName);
        sb.Append("===================================\n");
        return sb.ToString();
    }
}