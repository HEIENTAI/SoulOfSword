﻿using System.Collections.Generic;

/// <summary>
///  * State Pattern for gameplay state
///  遊戲進行狀態 Interface declaration.
///  特定狀態進行的功能盡量寫在此處, 不要在其他的class撰寫 if (GameState==xxx) 相關的敘述
/// </summary>
///
public interface IGameState
{
    void OnChangeIn(SceneManager sceneManager); // 
    void Update();
}
/// <summary>
/// 無
/// </summary>
public class GameNone : IGameState
{
    #region Singleton
    private static GameNone _instance = null;

    public static GameNone Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameNone();
            return _instance;
        }
    }
    #endregion
    GameNone()
    {
        ;
    }

    ~GameNone()
    {
        _instance = null;
    }

    public override string ToString()
    {
        return "無";
    }

    void IGameState.OnChangeIn(SceneManager sceneManager)
    {
        
    }

    void IGameState.Update()
    {
        GameMain.Instance.ChangeGameState(GameEntered.Instance);
    }
}

/// <summary>
/// 在遊戲中
/// </summary>
public class GameEntered : IGameState
{
    #region Singleton
    private static GameEntered _instance = null;

    public static GameEntered Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameEntered();
            return _instance;
        }
    }
    #endregion
    GameEntered()
    {
        ;
    }

    ~GameEntered()
    {
        _instance = null;
    }

    public override string ToString()
    {
        return "遊戲中狀態";
    }

    void IGameState.OnChangeIn(SceneManager sceneManager)
    {
        sceneManager.ChangeScene(1);
    }

    void IGameState.Update()
    {
        //throw new System.NotImplementedException();

    }
}