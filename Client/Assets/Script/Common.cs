using UnityEngine;
using System.Collections;

/// <summary>
/// 共用method的class
/// </summary>
public class Common
{
    public static void DebugMsg(string msg)
    {
        Debug.Log(msg);
    }


    /// <summary>
    /// 搖桿座標系(+y為0度):方向(二維向量)轉角度
    /// </summary>
    /// <returns>回傳值以度為單位，介於0~360度之間</returns>
    public static float Vector2ToDegreeInJoystick(Vector2 direction)
    {        
        float degree = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90.0f;
        if (degree < 0.0f)
            degree += 360.0f;
        return degree;
    }

    /// <summary>
    /// 搖桿座標系(+y為0度): 角度轉方向(二維向量)
    /// </summary>
    /// <param name="angle">以度為單位的角度</param>
    /// <returns>對應的二維向量</returns>
    public static Vector2 DegreeToVector2InJoystick(float angle)
    {
        return new Vector2(-Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
    }

}
