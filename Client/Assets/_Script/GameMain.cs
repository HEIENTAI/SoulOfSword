using UnityEngine;
using System.Collections;

/// <summary>
/// 遊戲進入點，只有GameStart場景才可掛載此Script
/// </summary>
public class GameMain : MonoBehaviour
{
    #region Singleton
    GameMain()
    {
        ;
    }
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

    private DataTableManager _dataTableManager;
    public DataTableManager DataTableManager
    {
        get {return _dataTableManager; }
    }
    private SceneManager _sceneManager;
    public SceneManager SceneManager
    {
        get { return _sceneManager; }
    }
    
    private CameraManager _cameraManager;
    public CameraManager CameraManager
    {
        get { return _cameraManager; }
    }
    private EventManager _eventManager;
    public  EventManager EventManager
    {
        get { return _eventManager; }
    }

    private PCUnit _myRole;
    public PCUnit MyRole
    {
        get { return _myRole; }
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
        _dataTableManager = new DataTableManager();
        _sceneManager = new SceneManager();
        _playerInput = PlayerInput.Instance;
        _cameraManager = new CameraManager();
        _eventManager = new EventManager();
    }

    // Update is called once per frame
    void Update()
    {
        if (_gameState != null)
            _gameState.Update();
        if (_sceneManager.ChangeSceneComplete)
            _cameraManager.UpdateMainCamera();
        _eventManager.CheckAndTriggerEvent();
    }

    void OnDestroy()
    {
        _gameState = null;
        _dataTableManager = null;
        _sceneManager = null;
        _playerInput = null;
        _cameraManager = null;
        _eventManager = null;
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
        _gameState.OnChangeIn(_sceneManager);
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
