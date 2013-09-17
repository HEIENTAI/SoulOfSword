using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// fs: 此處放置和遊戲事件的檢查條件以及效果相關的程式碼

#region 遊戲事件檢查部分
/// <summary>
/// 遊戲事件檢查觸發類型
/// </summary>
enum GameEventConditionType : ushort
{
    None = 0, // 不做檢查
    Mark = 1, // 檢查標記
    Item = 2, // 檢查物品
    STR  = 3, // 檢查力量
    Level = 22, // 檢查等級
    Money = 25, // 檢查金錢
}

/// <summary>
/// 遊戲事件判斷基本類別
/// </summary>
class GameEventCondition
{

    public override string ToString()
    {
        return "不做檢查\n";
    }


    /// <summary>
    /// 產生一個判斷遊戲事件條件式的物件
    /// </summary>
    /// <param name="data">遊戲事件條件資料</param>
    /// <returns></returns>
    public static GameEventCondition CreateGameEventCondition(GameEventConditionData data)
    {
        GameEventConditionType gameEventConditionType;
        // 決定對應到的GameEventCondtionType
        if (data == null)
        {
            gameEventConditionType = GameEventConditionType.None;
        }
        else
        {
            if (data.CheckType.HasValue && Enum.IsDefined(typeof(GameEventConditionType), data.CheckType.Value))
            {
                gameEventConditionType = (GameEventConditionType)data.CheckType.Value;
            }
            else
            {
                gameEventConditionType = GameEventConditionType.None;
            }
        }
        switch (gameEventConditionType)
        {
            case GameEventConditionType.Mark:
                return new GameEventCondition_Mark(data.CheckData, data.CheckOp, data.CheckTarget);
            case GameEventConditionType.Level:
                return new GameEventCondition_Level(data.CheckData, data.CheckOp, data.CheckTarget);
            //case GameEventConditionType.Money:
            //    return 
            default:
                return new GameEventCondition();
        }
    }
    /// <summary>
    /// 檢查條件是否成立
    /// </summary>
    public virtual bool CheckCondition()
    {
        return true;
    }

}

/// <summary>
/// 檢查條件 — 標記
/// </summary>
class GameEventCondition_Mark : GameEventCondition
{
    ushort _mark;  // 標記
    byte _checkOP; // 檢查方式
    ushort _markNumber; // 標記數值

    /// <summary>
    /// 建構式
    /// </summary>
    /// <param name="setMark">要設定的標記</param>
    /// <param name="setOPType">要設定的檢查方式</param>
    /// <param name="setMarkNumber">標記數值</param>
    public GameEventCondition_Mark(ushort? setMark, byte? setOPType, ushort? setMarkNumber)
    {
        _mark = setMark ?? (ushort)0;
        _checkOP = setOPType ?? (byte)0;
        _markNumber = setMarkNumber ?? (ushort)0;
    }

    public override string ToString()
    {
        return string.Format("檢查標記：標記 = {0} op(0:!=,1:=,2:>,3:<) = {1}, 標記數值 = {2}\n", _mark, _checkOP, _markNumber);
    }

    public override bool CheckCondition()
    {
        if (_mark == 0) {return false;} // 資料錯誤，無條件檢查失敗
        // TODO：　改成依照參數檢查標記
        return true;
    }
}

/// <summary>
/// 檢查條件 — 等級
/// </summary>
class GameEventCondition_Level : GameEventCondition
{
    ushort _checkWho; // 檢查誰的等級
    byte _checkOp; // 檢查方式
    ushort _checkNum; // 檢查數值

    /// <summary>
    /// 建構式
    /// </summary>
    /// <param name="setCheckWho">要檢查等級的角色</param>
    /// <param name="setCheckOp">檢查方式</param>
    /// <param name="setCheckNum">檢查數值</param>
    public GameEventCondition_Level(ushort? setCheckWho, byte? setCheckOp, ushort? setCheckNum)
    {
        _checkWho = setCheckWho ?? (ushort)0;
        _checkOp = setCheckOp ?? (byte)0;
        _checkNum = setCheckNum ?? (ushort)0;
    }

    public override string ToString()
    {
        return string.Format("檢查等級：檢查對象(1.李幕 ； 2.蘇以雪 ； 3.周小彤 ； 4.上官黛兒 ； 5.隊伍全部) = {0} Op(0:!=,1:=,2:>,3:<) = {1}, 數值 = {2}\n", _checkWho, _checkOp, _checkNum);
    }

    public override bool CheckCondition()
    {
        if (_checkWho == 0) { return false; } // 資料錯誤，無條件檢查失敗
        // TODO: 改成依照參數檢查等級
        return true;
    }
}

#endregion

#region 遊戲事件效果部分
enum GameEventEffectType : ushort
{
    None = 0,            // 無效果
    JumpToSubEvent = 1,  // 跳到特定子事件（企劃那邊寫「連接子事件」）
    JumpToMainEvent = 2, // 跳到特定主事件（企劃那邊寫「連接主事件」）
    Transport = 3,       // 傳送
}

class GameEventEffect
{
    public static GameEventEffect CreateGameEventEffect(GameEventEffectData data) // ushort? type, ushort?[] para)
    {
        GameEventEffectType gameEventEffectType;

        if (data == null)
        {
            gameEventEffectType = GameEventEffectType.None;
        }
        else
        {
            if (data.EffectType.HasValue && Enum.IsDefined(typeof(GameEventEffectType), data.EffectType.Value))
            {
                gameEventEffectType = (GameEventEffectType)data.EffectType.Value;
            }
            else
            {
                gameEventEffectType = GameEventEffectType.None;
            }
        }
        switch (gameEventEffectType)
        {
            case GameEventEffectType.Transport:
                return new GameEventEffect_Transport(data.EffectParameter);
            default:
                return new GameEventEffect();
        }
    }

    /// <summary>
    /// 執行事件效果
    /// </summary>
    public virtual void RunEffect()
    {
        ;
    }

    public override string ToString()
    {
        return "無效果\n";
    }
}

class GameEventEffect_Transport : GameEventEffect
{
    ushort newSceneID;
    ushort x;
    ushort y;


    public GameEventEffect_Transport(ushort?[] para)
    {
        if (para == null || para.Length < 3)
        {
            newSceneID = (ushort)0;
            Common.DebugMsg("GameEventEffect_Transport建構式傳入參數有誤");
            return;
        }
        newSceneID = para[0] ?? (ushort)0;
        x = para[1] ?? (ushort)0;
        y = para[2] ?? (ushort)0;
    }

    /// <summary>
    /// 執行事件效果
    /// </summary>
    public override void RunEffect()
    {
        if (newSceneID > 0)
        {
            GameMain.Instance.SceneManager.ChangeScene(newSceneID, x, y);
        }
        else
        {
            Common.DebugMsgFormat("newSceneID == 0，不做事[x = {0} y = {1}]", x, y);
        }
    }

    public override string ToString()
    {
        return string.Format("傳送效果：newSceneID = {0} (x, y) = ({1}, {2})\n", newSceneID, x, y);
    }
}

#endregion