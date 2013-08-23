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
    private uint _NPCID = 0;
    public uint NPCID
    {
        get { return _NPCID; }
        set {_NPCID = value;}
    }

    public static NPCUnit newInstance(uint npcID)
    {
        NPCUnit instance = null;
        GameObject obj = new GameObject(string.Format("NPC_{0:0000}", npcID));
        instance = obj.AddComponent<NPCUnit>();
        instance._NPCID = npcID;
        return instance;
    }

    public void GenerateModel()
    {
        _renderObject = Instantiate(ResourceStation.Instance.GetResource("Model/PrototypeCharacter/Constructor")) as GameObject;
        _renderObject.transform.position = new Vector3(77.55244f, 10.90868f, 54.266f);
    }
}
