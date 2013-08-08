using UnityEngine;
using System.Collections;

/// <summary>
/// 管理場景讀取切換
/// FelesSong@SoulOfSword
/// </summary>
public class SceneManager : MonoBehaviour
{
    
    #region Singleton
    private static SceneManager _instance;
    public static SceneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SceneManger");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<SceneManager>();
                if (GameMain.Instance)
                {
                    go.transform.parent = GameMain.Instance.gameObject.transform;
                }
            }
            return _instance;
        }
    }
    #endregion
    // Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

    void OnDestroy()
    {
        _instance = null;
    }

    /// <summary>
    /// 切換場景
    /// </summary>
    /// <param name="newSceneID">新場景ID</param>
	public void ChangeScene(int newSceneID)
    {
        StartCoroutine(ChangeSceneIEnumerator(newSceneID));
    }
    /// <summary>
    /// 切換場景
    /// </summary>
    /// <param name="newSceneID">新場景ID</param>
    IEnumerator ChangeSceneIEnumerator(int newSceneID)
    {
        Resources.UnloadUnusedAssets();
        yield return Application.LoadLevelAsync("Auction");

        NPCUnit npcTemp = NPCUnitManager.Instance.GetNPC(1);
        npcTemp.GenerateModel();

        Debug.Log("test");
    }
}
