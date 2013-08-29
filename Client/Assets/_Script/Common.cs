﻿using UnityEngine;
using System.Collections;

/// <summary>
/// 共用method的class
/// </summary>
public static class Common
{
    public static void DebugMsg(string msg)
    {
        Debug.Log(msg);
    }

    /// <summary>
    /// 依照string.Format的格式輸入參數，印出由string.Format回傳的訊息
    /// </summary>
    public static void DebugMsgFormat(string format, params object[] args)
    {
        Debug.Log(string.Format(format, args));
    }

    public static float DirectionY(this Camera cam)
    {
        return cam.transform.rotation.eulerAngles.y;
    }

    /// <summary>
    /// 測試世界座標系的點（positionInWorld）是否在攝影機（cam）的viewport內
    /// </summary>
    /// <param name="cam">要測試的攝影機</param>
    /// <param name="positionInWorld">要測試的世界座標</param>
    /// <returns>true：在該攝影機的viewport內</returns>
    public static bool WorldPositionInCameraViewPort(this Camera cam, Vector3 positionInWorld)
    {
        Vector3 positionInViewPort = cam.WorldToViewportPoint(positionInWorld);
        return (positionInViewPort.x >= 0.0f && positionInViewPort.x <= 1.0f && 
            positionInViewPort.y >= 0.0f && positionInViewPort.y <= 1.0f && 
            positionInViewPort.z > 0.0f && positionInViewPort.z <= cam.farClipPlane); // 在viewport內
    }

    #region 數學相關
    /// <summary>
    /// 取得角度對應的主輻角（介於0到360度之間），即扣除多餘的繞圈
    /// </summary>
    /// <param name="angle">待轉換的角度</param>
    /// <returns>對應的主輻角</returns>
    public static float ToArg(float angle)
    {
        float arg = angle % GlobalConst.DEGREE_PER_CIRCLE;
        if (arg < 0.0f)
            arg += GlobalConst.DEGREE_PER_CIRCLE;
        return arg;
    }
    
    /// <summary>
    /// 模型座標系(+y為0度，順時針為角度正方向):方向(二維向量)轉角度
    /// </summary>
    /// <returns>回傳值以度為單位，介於0~360度之間</returns>
    public static float Vector2ToDegreeInModel(Vector2 direction)
    {
        return Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// 模型座標系(+y為0度，順時針為角度正方向): 角度轉方向(二維向量)
    /// </summary>
    /// <param name="angle">以度為單位的角度</param>
    /// <returns>對應的二維向量</returns>
    public static Vector2 DegreeToVector2InModel(float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Sin(angleRad), Mathf.Cos(angleRad));
    }
    #endregion
}
