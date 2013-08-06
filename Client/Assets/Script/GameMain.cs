using UnityEngine;
using System.Collections;

/// <summary>
/// 遊戲進入點，只有GameStart場景才可掛載此Script
/// </summary>
public class GameMain : MonoBehaviour
{
    #region Singleton
    private static GameMain _instance = null;

    public static GameMain Instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType(typeof(GameMain)) as GameMain;

            return _instance;
        }
    }
    #endregion
    /// <summary>
    /// 遊戲狀態
    /// </summary>
    private IGameState _gameState;
    private SceneManager _sceneManager;




    void Awake()
    {
        DontDestroyOnLoad(gameObject); // 換場不被刪除
    }

    /// <summary>
    /// 用來作初始化
    /// </summary>
    void Start()
    {
        _gameState = GameNone.Instance;
		_sceneManager = gameObject.AddComponent<SceneManager>();
        _sceneManager.ChangeScene(1);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
        _sceneManager = null;
        _instance = null;
    }
}
