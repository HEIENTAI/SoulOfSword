using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 場景玩家單位
/// </summary>
public class PCUnit : BaseUnit
{
    private uint _PCID = 0; // fs : 或許可以不需要
    public uint PCID
    {
        get { return _PCID; }
        set { _PCID = value; }
    }
    //private Quaternion _initDirection = Quaternion.identity; // 初始朝向
    //private Quaternion _destDirection = Quaternion.identity;
    private float _initDirectionDegree = 0.0f; // 初始方向(以度為單位)
    private float _destDirectionDegree = 0.0f; // 目標方向(
    [SerializeField]
    private float _speed = 1.0f; // 移動速度
    [SerializeField]
    private float _rotateSpeedInDegree = 30.0f;// 旋轉速度
    [SerializeField]
    private float _bodyHeight = 0.85f;

    public static PCUnit newInstance(uint pcID)
    {
        PCUnit instance = null;
        GameObject obj = new GameObject(string.Format("PC_{0:0000}", pcID));
        DontDestroyOnLoad(obj);        
        instance = obj.AddComponent<PCUnit>();
        instance._PCID = pcID;
        
        return instance;
    }

    void Update()
    {
        RotateToDestDirection();
    }


    public void GenerateModel()
    {
        Common.DebugMsg("產生pc model");
        _renderObject = Instantiate(ResourceStation.Instance.GetModelResource("Constructor")) as GameObject;
        _renderObject.transform.parent = transform;
        
    }

    // TODO: 暫時，之後得改
    public void CrossAnimation(string animeName)
    {
        Common.DebugMsg(string.Format("新動作為：{0}", animeName));
        _renderObject.animation.CrossFade(animeName);
    }

    /// <summary>
    /// 停止移動
    /// </summary>
    public void StopMove()
    {
        // 將現在方向紀錄
        _initDirectionDegree = Direction.eulerAngles.y;
        _destDirectionDegree = _initDirectionDegree;
    }

    /// <summary>
    /// 沿著偏移angle角度的方向移動，速度受manitude影響
    /// </summary>
    /// <param name="angle">以度為單位的偏向角度</param>
    /// <param name="manitude">移動速度比值</param>
    public void Move(float angle, float manitude)
    {
        if (GameMain.Instance.CameraManager.CurrentCamera == null)
        {
            Common.DebugMsg("現在場景主Camera設定中，不能移動");
            return;
        }
        _initDirectionDegree = GameMain.Instance.CameraManager.CurrentCamera.transform.eulerAngles.y;
        _destDirectionDegree = Common.ToArg(_initDirectionDegree + angle);
        Vector2 moveVector = Common.DegreeToVector2InModel(_destDirectionDegree);
        Vector3 newPos = transform.position + _speed * Time.deltaTime * manitude * new Vector3(moveVector.x, 0, moveVector.y);

        // 決定高度
        Vector3 groundPos = Common.Get3DGroundPos(newPos.x, newPos.z);
        if (groundPos.y > Mathf.NegativeInfinity)
        {
            Position = groundPos; 
            //+_bodyHeight * Vector3.up;
        }

        NPCUnitManager.Instance.CheckNear();
    }

    /// <summary>
    /// 逐漸轉到目標角度
    /// </summary>
    public void RotateToDestDirection()
    {
        float restAngle = _destDirectionDegree - Direction.eulerAngles.y;        
        if (!Mathf.Approximately(0.0f, restAngle)) // 兩者不相等才旋轉
        {
            float nextAngle = Direction.eulerAngles.y;
            // 決定旋轉方向
            int clockwiseRotate = 1; // 是否為順時針旋轉（是：+1，否：-1）
            if ((restAngle > GlobalConst.DEGREE_PER_HALF_CIRCLE) || (restAngle < 0.0f && restAngle > -GlobalConst.DEGREE_PER_HALF_CIRCLE))
            {
                clockwiseRotate = -1;
            }
            // 取得實際需要的旋轉角度
            float restAngleAbs = Mathf.Abs(restAngle); // 旋轉角度的絕對值
            if (restAngleAbs > GlobalConst.DEGREE_PER_HALF_CIRCLE)
            { // 超出180度的，就只要轉比較少的就好
                restAngleAbs = GlobalConst.DEGREE_PER_CIRCLE - restAngleAbs;
            }
            // 如果實際需要的旋轉角度大於這次可旋轉角度，只旋轉可旋轉角度
            if (restAngleAbs > _rotateSpeedInDegree * Time.deltaTime)
            {
                nextAngle += (clockwiseRotate * _rotateSpeedInDegree * Time.deltaTime);
            }
            else
            {
                nextAngle = _destDirectionDegree;
            }
            Direction = Quaternion.Euler(0, nextAngle, 0);
        }
    }
}
