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
    [SerializeField]
    private Vector2 _speed = new Vector2(3, 3);
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

    public void GenerateModel()
    {
        Common.DebugMsg("產生pc model");
        _renderObject = Instantiate(ResourceStation.Instance.GetResource("Model/PrototypeCharacter/Constructor")) as GameObject;
        _renderObject.transform.parent = transform;
        transform.position = new Vector3(77.55244f, 10.90868f, 54.266f);
    }

    // 暫時，之後得改
    public void CrossAnimation(string animeName)
    {
        Common.DebugMsg(string.Format("新動作為：{0}", animeName));
        _renderObject.animation.CrossFade(animeName);
    }
    
    /// <summary>
    /// 沿著輸入方向移動
    /// </summary>
    /// <param name="normalizeDirect">移動方向</param>
    public void move(Vector2 normalizeDirect)
    {
        Vector3 newPos = transform.position + new Vector3(normalizeDirect.x * _speed.x, 0, normalizeDirect.y * _speed.y);
        //transform.position += new Vector3(normalizeDirect.x * _speed.x, 0, normalizeDirect.y * _speed.y);
        RaycastHit collisionHit;
        if (Physics.Raycast(newPos + 10000 * Vector3.up, Vector3.down, out collisionHit)) // 之後可能需要加layer
        {
            Common.DebugMsg(string.Format("cast 成功 pos = {0} ", collisionHit.point));
            transform.position = collisionHit.point + _bodyHeight * Vector3.up;
        }
        else
        {
            transform.position = newPos;
        }
    }
}
