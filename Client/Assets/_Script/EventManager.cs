using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 事件管理器
/// </summary>
public class EventManager
{
    List<Event> _allEvent;

    /// <summary>
    /// 確認是否有事件要被觸發，有則觸發事件
    /// </summary>
    public void CheckAndTriggerEvent()
    {
        //foreach (Event evt in _allEvent)
        //{
        //    if (false) // 確認是否有事件要觸發
        //    {
        //        ;  // 觸發事件
        //    }
        //}
    }
}
