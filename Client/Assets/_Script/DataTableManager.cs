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
    /// 輔助輸出List內容
    /// </summary>
    /// <returns></returns>
    string ListDataToString(IList dataList, string listName)
    {
        StringBuilder sb = new StringBuilder();
        if (dataList != null && dataList.Count > 0)
        {
            for (int index = 0; index < dataList.Count; ++index)
            {
                sb.AppendFormat("{0}[{1}] =\n{2}", listName, index, dataList[index]);
            }
            sb.Append("********************************\n");
        }
        return sb.ToString();
    }

    /// <summary>
    /// 將所有資料List內容輸出
    /// </summary>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("DataTableManager:\n");
        sb.Append(ListDataToString(_allDataList[GlobalConst.DataLoadTag.Event] as IList, "_allDataList[Event]"));
        sb.Append(ListDataToString(_allDataList[GlobalConst.DataLoadTag.Plant] as IList, "_allDataList[Plant]"));
        sb.Append(ListDataToString(_allDataList[GlobalConst.DataLoadTag.Scene] as IList, "_allDataList[Scene]"));
        sb.Append(ListDataToString(_allDataList[GlobalConst.DataLoadTag.NPCTable] as IList, "_allDataList[NPCTable]"));
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
                    Common.DebugMsgFormat("index = {0} is false (ds = {1})", index, ds);
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
            Common.DebugMsgFormat("JSONDeserializeObject error!(type = {0})\n{1}\n{2}\n", refObj.GetType().ToString(), e.Message, e.StackTrace);
            return false;
        }
    }

    // 得處理一下非同步狀況處理
    IEnumerator Load(int tag, LoadAttribute loadAttr)
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
            Common.DebugMsgFormat("讀取 {0} 的 Data", (GlobalConst.DataLoadTag)tag);
            object refObj = Activator.CreateInstance(typeof(List<>).MakeGenericType(loadAttr.DataType));
            bool isSuccess = DeserializeObject(encodingStr, ref refObj);
            if (isSuccess)
            {
                _allDataList[(GlobalConst.DataLoadTag)tag] = refObj as IList;
                DataState[tag] = ReadDataState.HaveLoad;
                Common.DebugMsgFormat("{0}讀取成功", (GlobalConst.DataLoadTag)tag);
            }
            else
            {
                DataState[tag] = ReadDataState.ReadError;
                Common.DebugMsgFormat("{0}讀取失敗", (GlobalConst.DataLoadTag)tag);
            }


        }
        catch (Exception e)
        {
            Common.DebugMsgFormat("EventData Read Error:\n");
            Common.DebugMsgFormat("StackTrace:\n{0}\n", e.StackTrace);
            Common.DebugMsgFormat("Msg:\n{0}\n", e.Message);
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
        LoadAttribute loadAttr;
        Type enumType = typeof(GlobalConst.DataLoadTag);
        Array enumValues = Enum.GetValues(enumType);
        for (int i = 0; i < enumValues.Length; ++i)
        {
            dataLoadTag = (GlobalConst.DataLoadTag)enumValues.GetValue(i);
            if (!LoadAttribute.GetAttribute(dataLoadTag, out loadAttr)) { continue; }
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

    #region 取得table資料表

    public List<GameEventData> GetAllEventData()
    {
        return _allDataList[GlobalConst.DataLoadTag.Event] as List<GameEventData>;
    }

    public List<PlantData> GetAllPlantData()
    {
        return _allDataList[GlobalConst.DataLoadTag.Plant] as List<PlantData>;
    }
    #endregion
}
