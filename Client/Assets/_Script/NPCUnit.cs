using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 場景非玩家單位
/// </summary>
public class NPCUnit : BaseUnit
{
    private ushort _NPCID = 0;
    public uint NPCID
    {
        get { return _NPCID; }
        //set {_NPCID = value;}
    }
    private ushort _serialNumber;
    public ushort SerialNumber
    {
        get { return _serialNumber; }
    }

    /// <summary>
    /// NPC名字
    /// </summary>
    public string NPCName
    {
        get;
        set;
    }

    public bool DramaVisible
    {
        get;
        set;
    }

    /// <summary>
    /// 觸發的主事件ID
    /// </summary>
    public ushort EventMainID
    {
        get;
        set;
    }

    public static NPCUnit newInstance(ushort npcID, ushort serialNumber)
    {
        NPCUnit instance = null;
        GameObject obj = new GameObject(string.Format("NPC_{0:0000}_{1:000}", npcID, serialNumber));
        instance = obj.AddComponent<NPCUnit>();
        instance._NPCID = npcID;
        instance._serialNumber = serialNumber;
        return instance;
    }

    /// <summary>
    /// 依據modelName產生模型
    /// </summary>
    /// <param name="modelName">模型名稱</param>
    public void GenerateModel(string modelName)
    {
        Common.DebugMsgFormat("modelName = {0}", modelName);
        _renderObject = Instantiate(ResourceStation.Instance.GetModelResource(modelName)) as GameObject;
        _renderObject.transform.parent = transform;
    }

}
