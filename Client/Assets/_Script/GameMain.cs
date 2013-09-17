using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    private GameGUIPanel _gameGUIPanel;
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
    private GameEventManager _gameEventManager;
    public  GameEventManager GameEventManager
    {
        get { return _gameEventManager; }
    }
    private GameEventState _gameEventState;
    public GameEventState GameEventState
    {
        get { return _gameEventState; }
    }

    private NPCUnitManager _npcUnitManager;


    private PCUnit _myRole;
    public PCUnit MyRole
    {
        get { return _myRole; }
    }
    private PlayerInput _playerInput;


    private List<IStartDependency> _startDependencies = new List<IStartDependency>();
    /// <summary>
    /// 進入遊戲前的資料準備好沒
    /// </summary>
    public bool StartDataReady
    {
        get
        {
            foreach (IStartDependency sd in _startDependencies)
            {
                if (!sd.IsStartDataReady) { return false; }
            }
            return true;
        }
    }


    void Awake()
    {
        DontDestroyOnLoad(gameObject); // 換場不被刪除
    }

    /// <summary>
    /// 用來作初始化
    /// </summary>
    void Start()
    {
        _gameGUIPanel = GameGUIPanel.Instance;

        _gameState = GameNone.Instance;
        _dataTableManager = new DataTableManager();
        _sceneManager = new SceneManager();
        _playerInput = PlayerInput.Instance;
        _cameraManager = new CameraManager();
        _gameEventManager = new GameEventManager();
        _gameEventState = new GameEventState();

        _npcUnitManager = NPCUnitManager.Instance;

        // 進入遊戲前需處理好的class，加載位置可能要換
        _startDependencies.Add(_dataTableManager);
    }

    // Update is called once per frame
    void Update()
    {
        if (_gameState != null) { _gameState.Update(); }
        if (_sceneManager.ChangeSceneComplete) { _cameraManager.UpdateMainCamera(); }
        if (_myRole != null) { _myRole.Update(); }
    }

    void OnDestroy()
    {
        _gameGUIPanel = null;
        _gameState = null;
        _dataTableManager = null;
        _sceneManager = null;
        if (_playerInput != null)
        {
            Destroy(_playerInput);
            _playerInput = null;
        }
        _cameraManager = null;
        _gameEventManager = null;
        _gameEventState = null;
        if (_npcUnitManager != null)
        {
            Destroy(_npcUnitManager);
            _npcUnitManager = null;
        }
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

    /// <summary>
    /// 準備自己角色
    /// </summary>
    /// <param name="twoDPos">應該在的位置</param>
    public void PrepareMyRole(Vector2 twoDPos)
    {
        Common.DebugMsg("準備pc");
        if (_myRole == null)
        {
            _myRole = PCUnit.newInstance(1);
            _myRole.GenerateModel();
        }
        _myRole.Position = Common.Get3DGroundPos(twoDPos);
        _myRole.Direction = Quaternion.identity;
    }

    /// <summary>
    /// 讀進下個場景之前要做的事情（清除npc...等
    /// </summary>
    public void LoadSceneBefore()
    {
        _npcUnitManager.ClearAll();
    }

    /// <summary>
    /// 場景切換完畢之後要做的事情
    /// </summary>
    public void LoadSceneOver(Vector2 myRoleNewPos)
    {
        PrepareMyRole(myRoleNewPos);
        _npcUnitManager.CreateAndShowAllNPCInCurrentScene(_sceneManager.CurrentSceneID);
        _cameraManager.RefreshSceneCameras();
    }
}
