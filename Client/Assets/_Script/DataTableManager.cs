using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 管理遊戲中所有資料表
/// </summary>
public class DataTableManager
{
    //List<T> another = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(encodeToJson, settings);
    //foreach(T one in another)
    //{
    //    Common.DebugMsgFormat("decode後的結果：\n{0}", one);
    //}


    public void Load()
    {
        Common.DebugMsgFormat("resourcepath = {0}", Application.dataPath);
    }
}
