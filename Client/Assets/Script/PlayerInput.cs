using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 處理玩家輸入
/// </summary>
public class PlayerInput : MonoBehaviour
{
    bool _isTouching = false;

    void Awake()
    {
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Debug.Log(Input.GetTouch(0).position);
        }
    }
}
