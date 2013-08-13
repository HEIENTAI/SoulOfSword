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
    private CameraManager _cameraManager;

    private PCUnit _myRole;
    public PCUnit MyRole
    {
        get { return _myRole;}
    }
    private PlayerInput _playerInput;

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
        _playerInput = PlayerInput.Instance;
        _cameraManager = new CameraManager();
    }

    // Update is called once per frame
    void Update()
    {
        if (_gameState != null)
            _gameState.Update();
        if (SceneManager.Instance.ChangeSceneComplete)
            _cameraManager.UpdateMainCamera();
    }

    void OnDestroy()
    {
        _gameState = null;
        _instance = null;
    }

    #region 遊戲狀態相關
    public void ChangeGameState(IGameState newGameState)
    {
        if (newGameState == _gameState)
            return;
        Common.DebugMsg(string.Format("遊戲狀態改變 從 {0} -> {1}", _gameState, newGameState));
        _gameState = null;
        _gameState = newGameState;
        _gameState.OnChangeIn();
    }
    #endregion


    public void PrepareMyRole()
    {
        if (_myRole != null)
            return;
        Common.DebugMsg("準備pc");
        _myRole = PCUnit.newInstance(1);
        _myRole.GenerateModel();
    }
    /// <summary>
    /// 場景切換完畢之後要做的事情
    /// </summary>
    public void LoadSceneOver()
    {
        PrepareMyRole();
        _cameraManager.RefreshCurrentSceneCameras();
    }

}
