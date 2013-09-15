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
        _testRect = new Rect(170, 70, Screen.width - 170, Screen.height - 70);

        _gameState = GameNone.Instance;
        _dataTableManager = new DataTableManager();
        _sceneManager = new SceneManager();
        _playerInput = PlayerInput.Instance;
        _cameraManager = new CameraManager();
        _npcUnitManager = new NPCUnitManager();
        _gameEventManager = new GameEventManager();

        _npcUnitManager = NPCUnitManager.Instance;

        // 進入遊戲前需處理好的class，加載位置可能要換
        _startDependencies.Add(_dataTableManager);
    }

    // Update is called once per frame
    void Update()
    {
        if (_gameState != null)
            _gameState.Update();
        if (_sceneManager.ChangeSceneComplete)
            _cameraManager.UpdateMainCamera();
        //_eventManager.CheckAndTriggerEvent();
    }

    void OnDestroy()
    {
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

    Rect _testRect;
    private Vector2 _testScrollPosition;
    string _testString = string.Empty;
    void OnGUI()
    {
        _testRect = GUI.Window(1, _testRect, TestWindow, "Debug Window");
    }


    public void TestWindow(int windowID)
    {
        _testScrollPosition = GUILayout.BeginScrollView(_testScrollPosition); // 加入捲軸
        
        List<GameEventData> test = _dataTableManager.GetAllEventData();
        string filePath = GlobalConst.DIR_DATA_JSON + "EventData" + GlobalConst.EXT_JSONDATA;
        _testString = string.Format("filePath = {1} test.count = {0} streamingAssetsPath = {2}\n", (test == null) ? 0 : test.Count, filePath, Application.streamingAssetsPath);
        if (!System.IO.File.Exists(filePath)) { _testString = string.Format("{0}Can't find {1}\n",_testString, filePath); }
        _testString = _testString + _dataTableManager.ToString();
        GUILayout.TextArea(_testString, GUILayout.ExpandHeight(true)); // 自動伸縮捲軸
        GUILayout.EndScrollView();
        GUI.DragWindow();
    }
}
