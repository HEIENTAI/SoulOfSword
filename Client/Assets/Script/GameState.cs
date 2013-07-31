using System.Collections.Generic;

/// <summary>
///  * State Pattern for gameplay state
///  遊戲進行狀態 Interface declaration.
///  特定狀態進行的功能盡量寫在此處, 不要在其他的class撰寫 if (GameState==xxx) 相關的敘述
/// </summary>
///
public interface IGameState
{
    void OnChangeIn();
    void Update();
}

public class GameNone : IGameState
{
    private static GameNone _instance = null;

    GameNone()
    {
        ;
    }

    ~GameNone()
    {
        _instance = null;
    }
    

    void IGameState.OnChangeIn()
    {
        throw new System.NotImplementedException();
    }

    void IGameState.Update()
    {
        throw new System.NotImplementedException();
    }

    public static GameNone Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameNone();
            return _instance;
        }
    }
}
