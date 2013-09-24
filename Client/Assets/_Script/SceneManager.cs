using UnityEngine;
using System.Collections;

/// <summary>
/// 管理場景讀取切換
/// FelesSong@SoulOfSword
/// </summary>
public class SceneManager
    //: MonoBehaviour
{

    //#region Singleton
    //private static SceneManager _instance;
    //public static SceneManager Instance
    //{
    //    get
    //    {
    //        if (_instance == null)
    //        {
    //            GameObject go = new GameObject("SceneManger");
    //            SceneManager temp = new SceneManager();
    //            DontDestroyOnLoad(go);
    //            _instance = go.AddComponent<SceneManager>();
    //            if (GameMain.Instance != null)
    //            {
    //                go.transform.parent = GameMain.Instance.gameObject.transform;
    //            }
    //        }
    //        return _instance;
    //    }
    //}
    //#endregion
    private bool _changeSceneComplete;
    public bool ChangeSceneComplete
    {
        get { return _changeSceneComplete; }
    }

    private ushort _currentSceneID = 0;
    public ushort CurrentSceneID
    {
        get { return _currentSceneID; }
    }

    private ushort _targetSceneID = 0;
    private Vector2 _targetSceneRoleXY = Vector2.zero; // 若為Vector2.zero，不改座標


    // Use this for initialization
    //void Start () 
    //{
        
    //}
	
    //// Update is called once per frame
    //void Update () 
    //{
	
    //}

    //void OnDestroy()
    //{
    //    _instance = null;
    //}

    /// <summary>
    /// 切換場景
    /// </summary>
    /// <param name="newSceneID">新場景ID</param>
    /// <param name="x">在新場景的座標x</param>
    /// <param name="y">在新場景的座標y</param>
	public void ChangeScene(ushort newSceneID, ushort x, ushort y)
    {
        _changeSceneComplete = false;
        _targetSceneRoleXY.Set((float)x, (float)y);
        GameMain.Instance.StartCoroutine(ChangeSceneIEnumerator(newSceneID));
    }
    /// <summary>
    /// 切換場景
    /// </summary>
    /// <param name="newSceneID">新場景ID</param>
    IEnumerator ChangeSceneIEnumerator(ushort newSceneID)
    {
        Resources.UnloadUnusedAssets();
        PlayerInput.Instance.enabled = false;
        SceneData nextSceneData;
        bool haveNextSceneData = GameMain.Instance.DataTableManager.TryGetOneSceneData(newSceneID, out nextSceneData);
        if (!haveNextSceneData)
        {
            CommonFunction.DebugMsgFormat("場景編號 {0} 不存在，不做事", newSceneID);
            yield break;
        }

        GameMain.Instance.LoadSceneBefore(); // 讀取新場景前需要做的事情
        yield return Application.LoadLevelAsync(nextSceneData.SceneFileName);


        _currentSceneID = newSceneID; // 換場OK，將現在場景ID切到新的
        GameMain.Instance.LoadSceneOver(_targetSceneRoleXY); // 處理換完場要弄得事情
        _targetSceneRoleXY = Vector2.zero;
        PlayerInput.Instance.enabled = true;
        Debug.Log("test");

        _changeSceneComplete = true;
    }
}

