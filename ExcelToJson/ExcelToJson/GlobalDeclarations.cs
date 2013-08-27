using System.Collections;


public struct Test
{
    int aa;
    float bb;

    public string ToString()
    {
        return string.Format("aa = {0} bb = {1}", aa, bb);
    }
}

public class EventData
{
    public ushort EventMainID;                 // 事件ID
    public ushort EventSubID;                  // 子事件ID
    public ushort[] CheckType = new ushort[3]; // 檢查類型(之後轉成Enum)
    public ushort[] CheckCondition1 = new ushort[3]; // 檢查條件1
    public ushort[] CheckCondition2 = new ushort[3]; // 檢查條件2
    public ushort[] TrueEffectType = new ushort[3];  // 正效果類型（之後轉成Enum）
    public ushort[] TrueEffectParameter1 = new ushort[3]; // 正效果欄位1
    public ushort[] TrueEffectParameter2 = new ushort[3]; // 正效果欄位2
    public ushort[] TrueEffectParameter3 = new ushort[3]; // 正效果欄位3
    public ushort[] FalseEffectType = new ushort[3]; // 反效果類型（之後轉成Enum）
    public ushort[] FalseEffectParameter1 = new ushort[3]; // 反效果欄位1
    public ushort[] FalseEffectParameter2 = new ushort[3]; // 反效果欄位2
    public ushort[] FalseEffectParameter3 = new ushort[3]; // 反效果欄位3
}