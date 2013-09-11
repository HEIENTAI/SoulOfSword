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

    // 先宣告成list，再看怎麼處理(可能改arraylist裝list)
    #region 表格資料宣告區
    // 事件資料表
    private List<EventData> _eventDataList = new List<EventData>();
    
    #endregion


    public DataTableManager()
    {
        DataState = new ReadDataState[Enum.GetValues(typeof(GlobalConst.DataLoadTag)).Length];
        for (int index = 0; index < DataState.Length; ++index)
        {
            DataState[index] = ReadDataState.Unload;
        }
    }

    ~DataTableManager()
    {
        _eventDataList = null;
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
        string filePath = GlobalConst.DIR_DATA_JSON + loadAttr.FileName;
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
        bool isSuccess;

        try
        {
            switch (loadAttr.FileName)
            {
                case "EventData.json":
                    Common.DebugMsgFormat("In EventData");
                    object refObj = Activator.CreateInstance(typeof(List<>).MakeGenericType(loadAttr.DataType));
                    isSuccess = DeserializeObject(encodingStr, ref refObj);
                    if (isSuccess)
                    {
                        _eventDataList = refObj as List<EventData>;
                        int index = 0;
                        foreach (EventData ed in _eventDataList)
                        {
                            Common.DebugMsgFormat("EventData[{0}] = \n{1}\n", index++, ed);
                        }
                        DataState[tag] = ReadDataState.HaveLoad;
                        Common.DebugMsg("EventData讀取成功");
                    }
                    else
                    {
                        DataState[tag] = ReadDataState.ReadError;
                        Common.DebugMsg("EventData讀取失敗");
                    }
                    break;
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


    public List<EventData> GetAllEventData()
    {
        return _eventDataList;
    }

}
