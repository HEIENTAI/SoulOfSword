using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 場景顯示用的基本單位
/// </summary>
public class BaseUnit : IDisposable   // : MonoBehaviour
{
    protected GameObject _gameObject = null; // 在_renderObject上一層的物件
    protected GameObject _renderObject = null; // 顯示用的實體物件

    protected Quaternion _direction = Quaternion.identity; // 面向
    // 設定、取得面向
    public Quaternion Direction
    {
        get { return _direction; }
        set
        {
            _direction = value;
            //transform.rotation = value;
            if (_gameObject != null) { _gameObject.transform.rotation = value; }
        }
    }

    public Vector3 Position
    {
        //get { return gameObject.transform.position; }
        get { return (_gameObject == null) ? Vector3.zero : _gameObject.transform.position; }
        //set { gameObject.transform.position = value; }
        set { if (_gameObject != null) { _gameObject.transform.position = value; } }
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
            //gameObject.SetActive(value);
            if (_gameObject != null) {_gameObject.SetActive(value);}
        }
        get { return _renderObject == null ? false : _visible; }
    }


    #region Dispose -- 資源釋放
    /* 直接將物件設為null，使其呼叫解構式來釋放資源會有「CompareBaseObjectsInternal can only be called from the main thread.」問題
     * 為了解決「CompareBaseObjectsInternal can only be called from the main thread.」問題，必須在main thread執行釋放資源，
     * 所以需要繼承IDisposable並實作Disposec函式。
     * http://msdn.microsoft.com/zh-tw/library/fs2xkftw(v=vs.90).aspx 有提供撰寫範例。
     * 但是呼叫端在刪除此物件時，需記得呼叫 Dispose()函式。
     */
    private bool _disposed = false;
    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_renderObject != null)
                {
                    GameObject.Destroy(_renderObject);
                }
                if (_gameObject != null)
                {
                    GameObject.Destroy(_gameObject);
                }
            }
            _renderObject = null;
            _gameObject = null;
            _disposed = true;
        }
    }
    #endregion

    //void OnDestroy()
    //{
    //    if (_renderObject != null)
    //    {
    //        Destroy(_renderObject);
    //        _renderObject = null;
    //    }
    //    Destroy(gameObject);
    //}
}
