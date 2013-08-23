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
            GameMain.Instance.MyRole.StopMove();
        }
    }
    void On_JoystickMove(MovingJoystick move)
    {
        if (move.joystickName == "PlayerInputJoystick")
        {            
            Common.DebugMsg(string.Format("JoyStick移動中 ({0})", move.joystickAxis));
            //

            if (move.joystickAxis.sqrMagnitude >= 0.25)
            {
                GameMain.Instance.MyRole.CrossAnimation("run");
            }
            else if (move.joystickAxis.sqrMagnitude > 0)
            {
                GameMain.Instance.MyRole.CrossAnimation("walk");
            }
            GameMain.Instance.MyRole.Move(move.Axis2Angle(true), move.joystickAxis.magnitude);
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
