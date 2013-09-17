using UnityEngine;
using System.Collections.Generic;

public class NPCUnitManager : MonoBehaviour
{
    #region Singleton
    private static NPCUnitManager _instance = null;
    public static NPCUnitManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("NPCUnitManager");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<NPCUnitManager>();
                if (GameMain.Instance)
                {
                    go.transform.parent = GameMain.Instance.gameObject.transform;
                }
            }
            return _instance;
        }
    }
    #endregion

    public Dictionary<uint, NPCUnit> NPCUnits = new Dictionary<uint, NPCUnit>(); // Key = NPCUnit.GetKey()

    #region Mono固定函式
    void Awake()
    {
    }

    /// <summary>
    /// 用來作初始化
    /// </summary>
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
        NPCUnits = null;
        _instance = null;
    }
    #endregion

    /// <summary>
    /// 刪除npcID對應的NPC
    /// </summary>
    /// <param name="npcID">要刪除的NPC的ID</param>
    public void DeleteOneNPC(uint key)
    {
        if (!NPCUnits.ContainsKey(key) || NPCUnits[key] == null) 
        {
            Common.DebugMsgFormat("NPC不存在，請確認資料(key = {0})", key);
            return; 
        }
        NPCUnits[key].Dispose();
        NPCUnits.Remove(key);
    }

    public void AddOneNPC(ushort npcID, ushort serialNumber)
    {
        if (NPCUnits.ContainsKey(npcID))
        {
            Common.DebugMsgFormat("已有同一隻NPC，請確認資料(npcID = {0})", npcID);
            return;
        }
        NPCTableData npcTableData;
        bool haveNPCtableData = GameMain.Instance.DataTableManager.TryGetNPCTableData(npcID, out npcTableData);
        if (!haveNPCtableData)
        {
            Common.DebugMsgFormat("該npcID（{0}）沒有NPCTableData，不做事", npcID);
            return;
        }
        NPCUnit tempNPC = NPCUnit.newInstance(npcID, serialNumber);
        tempNPC.NPCName = npcTableData.NPCName;
        tempNPC.GenerateModel(npcTableData.ModelName);
        tempNPC.Scale = (float)npcTableData.Scale / 100.0f;
        NPCUnits.Add(Common.GetNPCUnitKey(npcID, serialNumber), tempNPC);
        
    }

    /// <summary>
    /// 產生&顯示現在場景所有NPC
    /// </summary>
    /// <param name="sceneID">現在場景ID</param>
    public void CreateAndShowAllNPCInCurrentScene(ushort sceneID)
    {
        List<PlantData> oneScenePlantDatas;
        bool havePlantData = GameMain.Instance.DataTableManager.TryGetPlantDatasBySceneID(sceneID, out oneScenePlantDatas);
        if (!havePlantData)
        {
            Common.DebugMsgFormat("該sceneID = {0} 取不到任何種植檔資料，不做事", sceneID);
            return;
        }
        foreach (PlantData pd in oneScenePlantDatas)
        {
            AddOneNPC(pd.NPCID, pd.SerialNumber);
            uint key = Common.GetNPCUnitKey(pd.NPCID, pd.SerialNumber);
            if (NPCUnits.ContainsKey(key))
            {
                NPCUnit tempNPC = NPCUnits[key];
                tempNPC.Position = Common.Get3DGroundPos(pd.PosX, pd.PosY);
                tempNPC.Direction = Quaternion.Euler((float)pd.RotateX / 100.0f , (float)pd.RotateY / 100.0f, (float)pd.RotateZ / 100.0f);
                tempNPC.Visible = (pd.IsVisible == 1);
                tempNPC.DramaVisible = (pd.IsVisibleInDrama == 1);
                tempNPC.EventMainID = pd.EventMainID;
            }
        }
    }

    /// <summary>
    /// 清除現在場景所有NPC
    /// </summary>
    public void ClearAll()
    {
        StopAllCoroutines();
        if (NPCUnits != null)
        {
            foreach (NPCUnit npc in NPCUnits.Values)
            {
                if (npc != null) { npc.Dispose(); }
            }
            NPCUnits.Clear();
        }
    }


    public void CheckNear()
    {
        float minDisSqr = Mathf.Infinity; // 最短距離的平方
        
        Vector3 myRolePos = GameMain.Instance.MyRole.Position;
        NPCUnit nearestNPC = null;
        foreach (NPCUnit npcUnit in NPCUnits.Values)
        {
            float curDisSqr = new Vector2(myRolePos.x - npcUnit.Position.x, myRolePos.z - npcUnit.Position.z).sqrMagnitude;
            if (curDisSqr < minDisSqr)
            {
                minDisSqr = curDisSqr;
                nearestNPC = npcUnit;
            }
        }
        Common.DebugMsgFormat("nearestNPC = {0} disSqr = {1}", nearestNPC, minDisSqr);
        if (nearestNPC != null && minDisSqr <= GlobalConst.EVENT_TRIGGER_DIS_SQR)
        {           
            GameMain.Instance.GameEventManager.CheckAndTriggerEvent(nearestNPC.EventMainID);
        }
    }


    /// <summary>
    /// 依據參數取得對應NPC
    /// </summary>
    public NPCUnit GetNPC(uint npcID)
    {
        return NPCUnits[npcID];
    }
}
