using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 場景顯示用的基本單位
/// </summary>
public class BaseUnit : MonoBehaviour
{    
    protected GameObject _renderObject = null; // 顯示用的實體物件

    protected Quaternion _direction = Quaternion.identity; // 面向
    // 設定、取得面向
    public Quaternion Direction
    {
        get { return _direction; }
        set
        {
            _direction = value;
            transform.rotation = value;
        }
    }


    void OnDestroy()
    {
        _renderObject = null;
    }
}
