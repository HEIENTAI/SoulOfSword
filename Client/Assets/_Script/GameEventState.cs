using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 遊戲事件狀態，之後可能會變更為存檔資料的集中地
/// </summary>
public class GameEventState
{
    private Dictionary<ushort, ushort> _currentEventSubID; // 記錄每個主事件執行到的子事件ID為何，0表未開始，最後一個表示執行完畢

    public GameEventState()
    {
        _currentEventSubID = new Dictionary<ushort, ushort>();
    }

    ~GameEventState()
    {
        _currentEventSubID = null;
    }

    /// <summary>
    /// 初始化，從存檔中取得資料，目前無作用
    /// </summary>
    public void Initialize()
    {
    }

    /// <summary>
    /// 取得主事件的下一個應該執行的子事件ID，如果給予
    /// </summary>
    /// <param name="eventMainID">要查詢的主事件ID</param>
    /// <returns>下一個應該執行的子事件ID，接收者要自行處理超過最後的子事件ID的問題</returns>
    public ushort GetNextEventSubID(ushort eventMainID)
    {
        // 如果不包含此事件，回傳應該執行子事件ID＝1
        if (!_currentEventSubID.ContainsKey(eventMainID)) { return (ushort)1; }

        return (_currentEventSubID[eventMainID] == ushort.MaxValue) ? _currentEventSubID[eventMainID] : (ushort)(_currentEventSubID[eventMainID] + 1);
    }

    /// <summary>
    /// 設定主事件的現在執行到的子事件ID為何
    /// </summary>
    /// <param name="eventMainID">準備設定的主事件ID</param>
    /// <param name="eventSubID">要設定成的子事件ID的值</param>
    public void SetCurrentEventSubID(ushort eventMainID, ushort eventSubID)
    {
        _currentEventSubID[eventMainID] = eventSubID; // 不存在會自動新增，存在會將值覆寫
    }

}
