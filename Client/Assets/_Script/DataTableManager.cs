using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 讀取資料的狀態
/// </summary>
public enum ReadDataState
{
    Unload, // 未讀
    Loading, // 讀檔中
    ReadError,	//讀檔完成，但解檔失敗(原因可能為檔案格式變換或其他...)
    HaveLoad,	//讀檔完成，且解檔完成
}


/// <summary>
/// 管理遊戲中所有資料表
/// </summary>
public class DataTableManager :IStartDependency
{
    public ReadDataState[] DataState;

    #region 表格資料宣告區
    private Dictionary<GlobalConst.DataLoadTag, IList> _allDataList = new Dictionary<GlobalConst.DataLoadTag, IList>()
    {
        {GlobalConst.DataLoadTag.Event, new List<GameEventData>()}, // 事件資料表
        {GlobalConst.DataLoadTag.Plant, new List<PlantData>()},     // 種植檔資料表
        {GlobalConst.DataLoadTag.Scene, new List<SceneData>()},     // 場景資料表
        {GlobalConst.DataLoadTag.NPCTable, new List<NPCTableData>()},   // NPC資料表
    };
    #endregion


    public DataTableManager()
    {
        DataState = new ReadDataState[_allDataList.Count];
        for (int index = 0; index < DataState.Length; ++index)
        {
            DataState[index] = ReadDataState.Unload;
        }
    }

    ~DataTableManager()
    {
        _allDataList = null;
    }

    /// <summary>
    /// 將所有資料List內容輸出
    /// </summary>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("DataTableManager:\n");
        sb.Append(CommonFunction.ListDataToString(_allDataList[GlobalConst.DataLoadTag.Event] as IList, "_allDataList[Event]"));
        sb.Append(CommonFunction.ListDataToString(_allDataList[GlobalConst.DataLoadTag.Plant] as IList, "_allDataList[Plant]"));
        sb.Append(CommonFunction.ListDataToString(_allDataList[GlobalConst.DataLoadTag.Scene] as IList, "_allDataList[Scene]"));
        sb.Append(CommonFunction.ListDataToString(_allDataList[GlobalConst.DataLoadTag.NPCTable] as IList, "_allDataList[NPCTable]"));
        sb.Append("End Of DataTableManager\n");
        return sb.ToString();
    }


    bool IStartDependency.IsStartDataReady
    {
        get
        {
            int index = -1;
            foreach (ReadDataState ds in DataState)
            {
                ++index;
                if (ds != ReadDataState.HaveLoad) 
                {
                    CommonFunction.DebugMsgFormat("index = {0} is false (ds = {1})", index, ds);
                    return false; 
                }
            }
            return true;
        }
    }

    bool DeserializeObject(string encodingString, ref object refObj)
    {
        try
        {
            Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings();
            settings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            settings.CheckAdditionalContent = false;
            Newtonsoft.Json.JsonConvert.PopulateObject(encodingString, refObj); // 將encoding的資料填充到refObj內
            return true;
        }
        catch (Exception e)
        {
            CommonFunction.DebugMsgFormat("JSONDeserializeObject error!(type = {0})\n{1}\n{2}\n", refObj.GetType().ToString(), e.Message, e.StackTrace);
            return false;
        }
    }

    // 得處理一下非同步狀況處理
    IEnumerator Load(int tag, EnumClassValue loadAttr)
    {
        string filePath = GlobalConst.DIR_DATA_JSON + loadAttr.FileName + GlobalConst.EXT_JSONDATA;
        string encodingStr = string.Empty;
        if (filePath.Contains("://"))
        {
            WWW www = new WWW(filePath);
            yield return www;
            encodingStr = www.text;
        }
        else
        {
            encodingStr = File.ReadAllText(filePath);
        }
        try
        {
            CommonFunction.DebugMsgFormat("讀取 {0} 的 Data", (GlobalConst.DataLoadTag)tag);
            object refObj = Activator.CreateInstance(typeof(List<>).MakeGenericType(loadAttr.DataType));
            bool isSuccess = DeserializeObject(encodingStr, ref refObj);
            if (isSuccess)
            {
                _allDataList[(GlobalConst.DataLoadTag)tag] = refObj as IList;
                DataState[tag] = ReadDataState.HaveLoad;
                // TODO: 讀取事件資料完成用觀察者？
                if (tag == (int)GlobalConst.DataLoadTag.Event)
                {
                    GameMain.Instance.GameEventManager.Initialize();
                }
                CommonFunction.DebugMsgFormat("{0}讀取成功", (GlobalConst.DataLoadTag)tag);
            }
            else
            {
                DataState[tag] = ReadDataState.ReadError;
                CommonFunction.DebugMsgFormat("{0}讀取失敗", (GlobalConst.DataLoadTag)tag);
            }


        }
        catch (Exception e)
        {
            CommonFunction.DebugMsgFormat("EventData Read Error:\n");
            CommonFunction.DebugMsgFormat("StackTrace:\n{0}\n", e.StackTrace);
            CommonFunction.DebugMsgFormat("Msg:\n{0}\n", e.Message);
            DataState[tag] = ReadDataState.ReadError;
        }
    }

    /// <summary>
    /// 讀取所有的table資料
    /// </summary>
    public void LoadAllTable()
    {
        int tag;
        GlobalConst.DataLoadTag dataLoadTag;
        EnumClassValue loadAttr;
        Type enumType = typeof(GlobalConst.DataLoadTag);
        Array enumValues = Enum.GetValues(enumType);
        for (int i = 0; i < enumValues.Length; ++i)
        {
            dataLoadTag = (GlobalConst.DataLoadTag)enumValues.GetValue(i);
            if (!EnumClassValue.GetAttribute(dataLoadTag, out loadAttr)) { continue; }
            tag = (int)dataLoadTag;
            if (tag >= 0 && tag < enumValues.Length)
            {
                if (DataState[tag] == ReadDataState.Unload)
                {
                    DataState[tag] = ReadDataState.Loading;  // 需要先設，免得load跑太快使得已讀完的flag被設回
                    GameMain.Instance.StartCoroutine(Load(tag, loadAttr));
                }
            }

        }
    }

    #region 取得EventData相關
    /// <summary>
    /// 取得所有事件資料
    /// </summary>
    /// <returns>取得的事件資料</returns>
    public List<GameEventData> GetAllEventData()
    {
        return _allDataList[GlobalConst.DataLoadTag.Event] as List<GameEventData>;
    }


    #endregion
    #region 取得PlantData相關
    /// <summary>
    /// 取得所有種植資料
    /// </summary>
    /// <returns>取得的種植資料</returns>
    public List<PlantData> GetAllPlantData()
    {
        return _allDataList[GlobalConst.DataLoadTag.Plant] as List<PlantData>;
    }

    /// <summary>
    /// 依據場景ID嘗試取得該場景所有的種植資料
    /// </summary>
    /// <param name="sceneID">要取得種植資料的場景ID</param>
    /// <param name="data">取得的種植資料存放處</param>
    /// <returns>是否有取得資料</returns>
    public bool TryGetPlantDatasBySceneID(ushort sceneID, out List<PlantData> data)
    {
        data = new List<PlantData>();
        if (DataState[(int)GlobalConst.DataLoadTag.Plant] != ReadDataState.HaveLoad) { return false; }
        if (_allDataList[GlobalConst.DataLoadTag.Plant] == null) { return false; }

        data = (_allDataList[GlobalConst.DataLoadTag.Plant] as List<PlantData>).FindAll(pd => pd.SceneID == sceneID);

        return data.Count > 0; // data.Count == 0 表示找不到資料
    }

    #endregion

    #region 取得SceneData相關
    /// <summary>
    /// 取得所有場景資料
    /// </summary>
    /// <returns>取得的場景資料</returns>
    public List<SceneData> GetAllSceneData()
    {
        return _allDataList[GlobalConst.DataLoadTag.Scene] as List<SceneData>;
    }

    /// <summary>
    /// 依據場景ID嘗試取得一筆場景資料
    /// </summary>
    /// <param name="sceneID">要取得場景資料的場景ID</param>
    /// <param name="data">取得的場景資料存放處</param>
    /// <returns>是否有取得場景資料</returns>
    public bool TryGetOneSceneData(ushort sceneID, out SceneData data)
    {
        data = default(SceneData);
        if (DataState[(int)GlobalConst.DataLoadTag.Scene] != ReadDataState.HaveLoad) { return false; }
        if (_allDataList[GlobalConst.DataLoadTag.Scene] == null) {return false;}

        data = (_allDataList[GlobalConst.DataLoadTag.Scene] as List<SceneData>).Find(sd => sd.SceneID == sceneID);

        return data != default(SceneData); // 若data == default(SceneData) 表示沒有搜尋到資料
    }
    #endregion
    #region 取得NPC Table資料相關
    /// <summary>
    /// 取得所有NPC表格資料
    /// </summary>
    /// <returns>取得的NPC表格資料</returns>
    public List<NPCTableData> GetAllNPCTableData()
    {
        return _allDataList[GlobalConst.DataLoadTag.NPCTable] as List<NPCTableData>;
    }

    /// <summary>
    /// 依據npcID嘗試取得一筆NPC表格資料
    /// </summary>
    /// <param name="npcID">要取得npcID的</param>
    /// <param name="data">NPC表格資料存放處</param>
    /// <returns>是否有取得NPC表格資料</returns>
    public bool TryGetNPCTableData(ushort npcID, out NPCTableData data)
    {
        data = default(NPCTableData);
        if (DataState[(int)GlobalConst.DataLoadTag.NPCTable] != ReadDataState.HaveLoad) { return false; }
        if (_allDataList[GlobalConst.DataLoadTag.NPCTable] == null) { return false; }

        data = (_allDataList[GlobalConst.DataLoadTag.NPCTable] as List<NPCTableData>).Find(nt => nt.NPCID == npcID);

        return data != default(NPCTableData); // 若data == default(NPCTableData) 表示沒有搜尋到資料
    }
    #endregion
}
