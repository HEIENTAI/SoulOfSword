using System;
using System.Collections.Generic;

/// <summary>
/// 事件管理器
/// </summary>
// TODO :可能需要實踐觀察者，使用冠傑找到的方式？
public class GameEventManager
{
    private Dictionary<ushort, List<EventDataInGame>> _allEventDataInGame; // 利用List的index作為事件subID索引
    

    private uint _doingEvent;
    public bool DoingEvent
    {
        get { return _doingEvent > 0; }
    }

    public GameEventManager()
    {
        _allEventDataInGame = new Dictionary<ushort, List<EventDataInGame>>();
        _doingEvent = 0;
    }

    ~GameEventManager()
    {
        _allEventDataInGame = null;
    }

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("========= EventManager ============\n");
        foreach (ushort eventMainKey in _allEventDataInGame.Keys)
        {
            sb.AppendFormat("MainKey = {0}\n{1}\n", eventMainKey, Common.ListDataToString(_allEventDataInGame[eventMainKey], string.Format("allEventDataInGame[{0}]", eventMainKey), 1));
        }
        sb.Append("===================================\n");
        return sb.ToString();
    }

    public void Initialize()
    {
        List<GameEventData> allEventData = GameMain.Instance.DataTableManager.GetAllEventData();
        foreach (GameEventData ged in allEventData)
        {
            if (!_allEventDataInGame.ContainsKey(ged.MainID))
            {
                _allEventDataInGame.Add(ged.MainID, new List<EventDataInGame>());
            }
            if (ged.SubID >= _allEventDataInGame[ged.MainID].Count) // 此表示該事件還不在裡面
            {
                // 填滿list到該事件的subID前一個
                for (int i = _allEventDataInGame[ged.MainID].Count; i < ged.SubID; ++i)
                {
                    _allEventDataInGame[ged.MainID].Add(null);
                }
                _allEventDataInGame[ged.MainID].Add(new EventDataInGame(ged.MainID, ged.SubID));
            }

            if (_allEventDataInGame[ged.MainID][ged.SubID] == null)
            {
                _allEventDataInGame[ged.MainID][ged.SubID] = new EventDataInGame(ged.MainID, ged.SubID);
            }
            // 加入事件條件
            if (ged.CheckCondition != null)
            {
                bool replaceOccur = _allEventDataInGame[ged.MainID][ged.SubID].AddOrReplaceCoditionData(ged.EffectID, ged.CheckCondition);
                if (replaceOccur) { Common.DebugMsgFormat("Event({0}, {1}) 的 Condition {2} 發生資料被覆蓋！！", ged.MainID, ged.SubID, ged.EffectID); }
            }
            // 加入正效果
            if (ged.TrueEffect != null)
            {
                bool replaceOccur = _allEventDataInGame[ged.MainID][ged.SubID].AddOrReplaceTrueEffectData(ged.EffectID, ged.TrueEffect);
                if (replaceOccur) { Common.DebugMsgFormat("Event({0}, {1}) 的 TrueEffect {2} 發生資料被覆蓋！！", ged.MainID, ged.SubID, ged.EffectID); }
            }
            if (ged.FalseEffect != null)
            {
                bool replaceOccur = _allEventDataInGame[ged.MainID][ged.SubID].AddOrReplaceFalseEffectData(ged.EffectID, ged.FalseEffect);
                if (replaceOccur) { Common.DebugMsgFormat("Event({0}, {1}) 的 FalseEffect {2} 發生資料被覆蓋！！", ged.MainID, ged.SubID, ged.EffectID); }
            }
        }
        // TODO: 如果全部跑完，發覺有空的，可能得跳訊息？
        Common.DebugMsgFormat("事件資料轉換完畢：\n{0}", this);
    }


    /// <summary>
    /// 確認是否有事件要被觸發，有則觸發事件
    /// </summary>
    /// <param name="mainID">事件的主ID</param>
    public void CheckAndTriggerEvent(ushort mainID)
    {
        _doingEvent = mainID;
        Common.DebugMsgFormat("Doing Event {0}", mainID);
        // 只有事件資料包含此主事件ID才執行
        if (_allEventDataInGame.ContainsKey(mainID) )
        {
            // 取得下一個執行的子事件ID
            ushort currentSubID = GameMain.Instance.GameEventState.GetNextEventSubID(mainID);
            Common.DebugMsgFormat("get sub ID = {0}", currentSubID);
            // 如果取得的值超出資料範圍，就執行最後一個
            currentSubID = (currentSubID < _allEventDataInGame[mainID].Count) ? currentSubID : (ushort)(_allEventDataInGame[mainID].Count - 1);
            Common.DebugMsgFormat("調整後的sub ID = {0}", currentSubID);
            // 一次只觸發一個事件，事件觸發完畢就結束。
            _allEventDataInGame[mainID][currentSubID].CheckAndTriggerThisEvent();
            // 執行完畢將現在執行到的子事件ID記錄起來
            GameMain.Instance.GameEventState.SetCurrentEventSubID(mainID, currentSubID);
        }
        _doingEvent = 0;
    }
}

/// <summary>
/// 遊戲中儲存事件的內容
/// </summary>
class EventDataInGame
{
    /// <summary>
    /// 主事件ID
    /// </summary>
    public ushort MainID
    {
        get;
        set;
    }
    /// <summary>
    /// 子事件ID
    /// </summary>
    public ushort SubID
    {
        get;
        set;
    }

    private List<GameEventCondition> _conditionList = new List<GameEventCondition>();
    private List<GameEventEffect> _trueEffect = new List<GameEventEffect>();
    private List<GameEventEffect> _falseEffect = new List<GameEventEffect>();
    
    public EventDataInGame(ushort mainID, ushort subID)
    {
        MainID = mainID;
        SubID = subID;
    }

    ~EventDataInGame()
    {
        _conditionList = null;
        _trueEffect = null;
        _falseEffect = null;
    }

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("========= EventDataInGame ============\n");
        sb.AppendFormat("主事件ID = {0}\n", MainID);
        sb.AppendFormat("子事件ID = {0}\n", SubID);
        sb.Append(Common.ListDataToString(_conditionList, "_conditionList", 1));
        sb.Append(Common.ListDataToString(_trueEffect, "_trueEffect", 1));
        sb.Append(Common.ListDataToString(_falseEffect, "_falseEffect", 1));
        sb.Append("===================================\n");
        return sb.ToString();
    }

    /// <summary>
    /// 將index位置的Condition資料替換成oneData，如果list數量不足，填null直到index指示的位置。
    /// </summary>
    /// <param name="index">要替換的資料位置</param>
    /// <param name="oneData">新資料</param>
    /// <returns>是否有資料被覆蓋</returns>
    public bool AddOrReplaceCoditionData(int index, GameEventConditionData oneData)
    {
        if (index >= _conditionList.Count)
        {
            for (int i = _conditionList.Count; i <= index; ++i)
            {
                _conditionList.Add(GameEventCondition.CreateGameEventCondition(null));
            }
        }
        bool haveData = (_conditionList[index].GetType() != typeof(GameEventCondition)); // 型別不等於base型別，表示有資料會被蓋掉
        _conditionList[index] = GameEventCondition.CreateGameEventCondition(oneData);
        return haveData;
    }


    /// <summary>
    /// 將index位置的TrueEffect資料替換成oneData，如果list數量不足，填null直到index指示的位置。
    /// </summary>
    /// <param name="index">要替換的資料位置</param>
    /// <param name="oneData">新資料</param>
    /// <returns>是否有資料被覆蓋</returns>
    public bool AddOrReplaceTrueEffectData(int index, GameEventEffectData oneData)
    {
        if (index >= _trueEffect.Count)
        {
            for (int i = _trueEffect.Count; i <= index; ++i)
            {
                _trueEffect.Add(GameEventEffect.CreateGameEventEffect(null));
            }
        }
        bool haveData = (_trueEffect[index].GetType() != typeof(GameEventEffect)); // 型別不等於base型別，表示有資料會被蓋掉
        _trueEffect[index] = GameEventEffect.CreateGameEventEffect(oneData);
        return haveData;
    }

    /// <summary>
    /// 將index位置的FalseEffect資料替換成oneData，如果list數量不足，填null直到index指示的位置。
    /// </summary>
    /// <param name="index">要替換的資料位置</param>
    /// <param name="oneData">新資料</param>
    /// <returns>是否有資料被覆蓋</returns>
    public bool AddOrReplaceFalseEffectData(int index, GameEventEffectData oneData)
    {
        if (index >= _falseEffect.Count)
        {
            for (int i = _falseEffect.Count; i <= index; ++i)
            {
                _falseEffect.Add(GameEventEffect.CreateGameEventEffect(null));
            }
        }

        bool haveData = (_falseEffect[index].GetType() != typeof(GameEventEffect)); // 型別不等於base型別，表示有資料會被蓋掉
        _falseEffect[index] = GameEventEffect.CreateGameEventEffect(oneData);
        return haveData;
    }

    /// <summary>
    /// 判斷此事件是否可觸發
    /// </summary>
    bool CanTrigger()
    {
        for (int index = 1;index < _conditionList.Count; ++index)
        {
            if(!_conditionList[index].CheckCondition())
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 執行正效果
    /// </summary>
    void RunTrueEffect()
    {   // 依序執行事件效果
        for (int index = 1; index < _trueEffect.Count; ++index)
        {            
            _trueEffect[index].RunEffect();
        }
    }

    /// <summary>
    /// 執行反效果
    /// </summary>
    void RunFalseEffect()
    {   // 依序執行事件效果
        for (int index = 1; index < _falseEffect.Count; ++index)
        {
            _falseEffect[index].RunEffect();
        }
    }

    /// <summary>
    /// 確認是否有此事件要被觸發，有則觸發事件
    /// </summary>
    public void CheckAndTriggerThisEvent()
    {
        if (CanTrigger())
        {
            RunTrueEffect();
        }
        else
        {
            RunFalseEffect();
        }
    }
}