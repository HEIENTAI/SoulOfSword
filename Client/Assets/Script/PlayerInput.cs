using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 處理玩家輸入
/// </summary>
public class PlayerInput : MonoBehaviour
{
    #region Singleton
    private static PlayerInput _instance;
    public static PlayerInput Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("PlayerInput");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<PlayerInput>();                
                if (GameMain.Instance != null)
                {
                    go.transform.parent = GameMain.Instance.gameObject.transform;
                }
            }
            return _instance;
        }
    }
    #endregion

    bool _isTouching = false;

    void OnEnable()
    {
        EasyJoystick.On_JoystickMove += On_JoystickMove;
        EasyJoystick.On_JoystickMoveEnd += On_JoystickMoveEnd;
    }

    void OnDisable()
    {
        EasyJoystick.On_JoystickMove -= On_JoystickMove;
        EasyJoystick.On_JoystickMoveEnd -= On_JoystickMoveEnd;
    }


    void On_JoystickMoveEnd(MovingJoystick move)
    {
        if (move.joystickName == "PlayerInputJoystick")
        {
            Common.DebugMsg("JoyStick移動結束");            
            GameMain.Instance.MyRole.CrossAnimation("idle");
        }
    }
    void On_JoystickMove(MovingJoystick move)
    {
        if (move.joystickName == "PlayerInputJoystick")
        {            
            Common.DebugMsg(string.Format("JoyStick移動中 x = {0} y = {1}", move.joystickAxis.x, move.joystickAxis.y));
            //
            if (Mathf.Abs(move.joystickAxis.y) > 0 && Mathf.Abs(move.joystickAxis.y) < 0.5)
            {
                GameMain.Instance.MyRole.CrossAnimation("walk");
            }
            else if (Mathf.Abs(move.joystickAxis.y) >= 0.5)
            {
                GameMain.Instance.MyRole.CrossAnimation("run");
            }
            GameMain.Instance.MyRole.move(move.joystickAxis * Time.deltaTime);
        }
    }

    void Awake()
    {
    }

    //void Start()
    //{
    //    EasyJoystick.On_JoystickMove += On_JoystickMove;
    //    EasyJoystick.On_JoystickMoveEnd += On_JoystickMoveEnd;
    //}
    


    //void Update()
    //{
    //    if (Input.touchCount > 0)
    //    {
    //        Debug.Log(Input.GetTouch(0).position);
    //    }
    //}

    void OnDestroy()
    {
        EasyJoystick.On_JoystickMove -= On_JoystickMove;
        EasyJoystick.On_JoystickMoveEnd -= On_JoystickMoveEnd;
        _instance = null;
    }
}
