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
    private float _speed = 5.0f; // 移動速度
    [SerializeField]
    private float _rotateSpeedInDegree = 0.5f;// 旋轉速度
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
        _renderObject = Instantiate(ResourceStation.Instance.GetResource("Model/PrototypeCharacter/Constructor")) as GameObject;
        _renderObject.transform.parent = transform;
        transform.position = new Vector3(66.89587f, 10.90868f, 42.0879f);
        transform.rotation = Quaternion.identity;
    }

    // 暫時，之後得改
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
    /// 沿著輸入方向移動
    /// </summary>
    /// <param name="normalizeDirect">移動方向</param>
    public void Move(Vector2 normalizeDirect)
    {   
        //float joystickAngle = Mathf.Atan2(normalizeDirect.y, normalizeDirect.x) * Mathf.Rad2Deg;
        
        _initDirectionDegree = GameMain.Instance.CameraManager.CurrentCamera.transform.rotation.eulerAngles.y;
        //_destDirectionDegree = GameMain.Instance.CameraManager.CurrentCamera.transform.rotation.eulerAngles.y + joystickAngle - 90.0f; // 和+y軸的夾角才是想要的偏移角度
        _destDirectionDegree = _initDirectionDegree + Common.Vector2ToDegreeInJoystick(normalizeDirect);
        if (_destDirectionDegree < 0.0f)
            _destDirectionDegree += 360.0f;
        if (_destDirectionDegree > 360.0f)
            _destDirectionDegree -= 360.0f;

        Common.DebugMsg(string.Format("_initAngle = {0} angle = {1} _destAngle = {2}", _initDirectionDegree,  Common.Vector2ToDegreeInJoystick(normalizeDirect), _destDirectionDegree));
        //Vector3 newPos = transform.position + _speed * Time.deltaTime * new Vector3(Mathf.Cos(_destDirectionDegree * Mathf.Deg2Rad), 0, Mathf.Sin(_destDirectionDegree * Mathf.Deg2Rad));
        Vector3 newPos = transform.position + _speed * Time.deltaTime * normalizeDirect.magnitude * new Vector3(Common.DegreeToVector2InJoystick(_destDirectionDegree).x, 0, Common.DegreeToVector2InJoystick(_destDirectionDegree).y);
        RaycastHit collisionHit;
        if (Physics.Raycast(newPos + 10000 * Vector3.up, Vector3.down, out collisionHit)) // 之後可能需要加layer
        {
            Common.DebugMsg(string.Format("cast 成功 pos = {0} ", collisionHit.point));
            transform.position = collisionHit.point + _bodyHeight * Vector3.up;
        }
        else
        {
            Common.DebugMsg(string.Format("cast 失敗 回歸原位 {0}", transform.position));
            //transform.position = newPos;
        }
    }

    /// <summary>
    /// 逐漸轉到目標角度
    /// </summary>
    public void RotateToDestDirection()
    {
        float restAngle = _destDirectionDegree - Direction.eulerAngles.y;
        if (!Mathf.Approximately(0.0f, restAngle)) // 兩者不相等才旋轉
        {
            Common.DebugMsg(string.Format("剩下移動角度 = {0} _dest = {1} _cur = {2}", restAngle, _destDirectionDegree, Direction.eulerAngles.y));
            if (restAngle * Mathf.Sign(restAngle) > _rotateSpeedInDegree)
            {
                Common.DebugMsg(string.Format("移動 _rotateSpeed 移動前角度: {0}", Direction.eulerAngles.y));
                Direction = Quaternion.Euler(0, Direction.eulerAngles.y + Mathf.Sign(restAngle) * _rotateSpeedInDegree, 0);
                Common.DebugMsg(string.Format("移動 _rotateSpeed 移動後角度: {0}", Direction.eulerAngles.y));
            }
            else
            {
                Common.DebugMsg(string.Format("移動 restAngle 移動前角度: {0}", Direction.eulerAngles.y));
                Direction = Quaternion.Euler(0, Direction.eulerAngles.y + restAngle, 0);
                Common.DebugMsg(string.Format("移動 restAngle 移動後角度: {0}", Direction.eulerAngles.y));
            }
        }
    }
}
