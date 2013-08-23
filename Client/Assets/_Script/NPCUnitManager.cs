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

    public Dictionary<uint, NPCUnit> NPCUnits = new Dictionary<uint, NPCUnit>();


    void Awake()
    {
        // for test
        NPCUnits.Add(1, NPCUnit.newInstance(1));
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

    /// <summary>
    /// 依據參數取得對應NPC
    /// </summary>
    public NPCUnit GetNPC(uint npcID)
    {
        return NPCUnits[npcID];
    }
}
