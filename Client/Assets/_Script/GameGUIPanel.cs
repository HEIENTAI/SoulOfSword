using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
///  Debug功能集中區
/// </summary>
public class GameGUIPanel : MonoBehaviour
{
    #region Singleton
    private static GameGUIPanel _instance;
    public static GameGUIPanel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(GameGUIPanel)) as GameGUIPanel;
                if (_instance == null)
                {
                    GameObject guiObject = new GameObject("DebugGUIPanel");
                    DontDestroyOnLoad(guiObject);
                    _instance = guiObject.AddComponent<GameGUIPanel>();
                    if (GameMain.Instance)
                    {
                        _instance.transform.parent = GameMain.Instance.transform;
                    }
                }
            }
            return _instance;
        }
    }
    #endregion

    private static bool _debugWindowVisible = false; // Debug視窗顯示開關，預設不顯示

    private Rect _debugWindowRect;

    Dictionary<int, DebugWindowInfo> _windowTable;


    // Use this for initialization
	void Start () 
    {
        _debugWindowVisible = true; // 暫訂使其為true

        InitDebugWindow();
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    void OnGUI()
    {
        GUILayout.BeginVertical();

        if (_debugWindowVisible)
        {
            if (_windowTable != null)
            {
                GUILayout.BeginVertical();
                GUILayout.Box("Debug Windows");
                foreach (var window in _windowTable)
                {
                    window.Value.Enabled = GUILayout.Toggle(window.Value.Enabled, window.Value.Title);
                    DrawWindow(window.Key, window.Value);
                }
                GUILayout.EndVertical();
            }
        }
        GUILayout.EndVertical();
    }


    void OnDestroy()
    {
        _windowTable = null;
        _instance = null;
    }

    /// <summary>
    /// Debug視窗初始化
    /// </summary>
    void InitDebugWindow()
    {
        _windowTable = new Dictionary<int, DebugWindowInfo>
        {
            {0, new DebugWindowInfo("讀取資料Debug", new Rect(100, 70, Screen.width - 70, Screen.height-70), DebugReadDataWindow, false)},
            {1, new DebugWindowInfo("測試NPC增減", new Rect(100, 20, 50, 50), TestNPCAddOrDelete, false)},
        };
    }

    /// <summary>
    /// 繪製視窗
    /// </summary>
    /// <param name="windowID">視窗ID</param>
    /// <param name="info">相關資訊</param>
    void DrawWindow(int windowID, DebugWindowInfo info)
    {
        if (info == null) { return; }
        if (info.Enabled)
        {
            info.Rect = GUILayout.Window(windowID, info.Rect, info.DrawFunc, info.Title);
        }
        else if (info.DisableFunc != null)
        {
            info.DisableFunc(windowID);
        }
    }

    /// <summary>
    /// 繪製關閉按鈕
    /// </summary>
    GUIStyle closeButton;
    void DrawCloseButton(int windowID)
    {
        if (closeButton == null)
        {
            closeButton = new GUIStyle(GUI.skin.button) { padding = new RectOffset(0, 0, 0, 0) };
        }
        if (GUI.Button(new Rect(_windowTable[windowID].Rect.width - 18, 0, 18, 18), "X", closeButton))
        {
            _windowTable[windowID].Enabled = false;
        }
    }

    // 讀取資料的偵錯訊息視窗
    private Vector2 _readDataScrollPosition = default(Vector2);
    string _testString = string.Empty;
    public void DebugReadDataWindow(int windowID)
    {
        DrawCloseButton(windowID);

        GUILayout.BeginVertical();
        GUILayout.Box("讀取資料");

        _readDataScrollPosition = GUILayout.BeginScrollView(_readDataScrollPosition); // 加入捲軸

        List<GameEventData> test = GameMain.Instance.DataTableManager.GetAllEventData();
        string filePath = GlobalConst.DIR_DATA_JSON + "EventData" + GlobalConst.EXT_JSONDATA;
        _testString = string.Format("filePath = {1} test.count = {0} streamingAssetsPath = {2}\n", (test == null) ? 0 : test.Count, filePath, Application.streamingAssetsPath);
        if (!System.IO.File.Exists(filePath)) { _testString = string.Format("{0}Can't find {1}\n", _testString, filePath); }
        _testString = _testString + GameMain.Instance.DataTableManager.ToString();

        GUILayout.TextArea(_testString, GUILayout.ExpandHeight(true)); // 自動伸縮捲軸
        GUILayout.EndScrollView();

        GUILayout.EndVertical();
        GUI.DragWindow();
    }

    // 測試增減NPC
    public void TestNPCAddOrDelete(int windowID)
    {
        DrawCloseButton(windowID);
        GUILayout.BeginVertical();
        if (GUILayout.Button("Add NPC", GUILayout.Width(50)))
        {
            NPCUnitManager.Instance.AddOneNPC(2, 1);
        }
        if (GUILayout.Button("Delete NPC", GUILayout.Width(50)))
        {
            NPCUnitManager.Instance.DeleteOneNPC(Common.GetNPCUnitKey(2, 1));
        }
        GUILayout.EndVertical();
        GUI.DragWindow();
    }
}


class DebugWindowInfo
{
    public bool Enabled = true;
    public Rect Rect = new Rect();
    public GUI.WindowFunction DrawFunc;
    public GUI.WindowFunction DisableFunc;
    public string Title;

    public DebugWindowInfo(string title, Rect rect, GUI.WindowFunction drawFunc)
    {
        Title = title;
        Rect = rect;
        DrawFunc = drawFunc;
    }
    public DebugWindowInfo(string title, Rect rect, GUI.WindowFunction drawFunc, bool enabled)
    {
        Title = title;
        Rect = rect;
        DrawFunc = drawFunc;
        Enabled = enabled;
    }
    public DebugWindowInfo(string title, Rect rect, GUI.WindowFunction drawFunc, bool enabled, GUI.WindowFunction disableFunc)
    {
        Title = title;
        Rect = rect;
        DrawFunc = drawFunc;
        Enabled = enabled;
        DisableFunc = disableFunc;
    }
}