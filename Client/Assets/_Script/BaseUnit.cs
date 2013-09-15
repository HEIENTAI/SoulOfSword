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

    public Vector3 Position
    {
        get { return gameObject.transform.position; }
        set { gameObject.transform.position = value; }
    }

    private float _scale = 1.0f;
    public float Scale
    {
        set 
        { 
            _scale = value;
            if (_renderObject != null)
            {
                _renderObject.transform.localScale = new Vector3(value, value, value);
            }
        }
        get { return _scale; }
    }


    private bool _visible;
    public bool Visible
    {
        set
        {
            _visible = value;
            gameObject.SetActive(value);
        }
        get { return _renderObject == null ? false : _visible; }
    }


    void OnDestroy()
    {
        if (_renderObject != null)
        {
            Destroy(_renderObject);
            _renderObject = null;
        }
        Destroy(gameObject);
    }
}
