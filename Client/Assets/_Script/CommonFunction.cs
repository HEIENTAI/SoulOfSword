using UnityEngine;
using System.Collections;
using System.Text;

/// <summary>
/// 共用method的class
/// </summary>
public static class CommonFunction
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

    /// <summary>
    /// 將dataList的內容轉換成字串，方便輸出
    /// </summary>
    /// <param name="dataList">待轉換成字串的List</param>
    /// <param name="listName">List前面要加的名字</param>
    /// <param name="firstIndex">第一筆資料的index</param>
    /// <returns>轉換後的字串</returns>
    public static string ListDataToString(IList dataList, string listName, int firstIndex = 0)
    {
        StringBuilder sb = new StringBuilder();
        if (dataList != null && dataList.Count > 0)
        {
            for (int index = firstIndex; index < dataList.Count; ++index)
            {
                sb.AppendFormat("{0}[{1}] =\n{2}", listName, index, dataList[index]);
            }
            sb.Append("********************************\n");
        }
        return sb.ToString();
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

    #region 單純數學相關
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

    /// <summary>
    /// 根據twoDPos座標，取得對應的3維地面座標
    /// </summary>
    /// <returns>對應的地面座標</returns>
    public static Vector3 Get3DGroundPos(Vector2 twoDPos)
    {
        return Get3DGroundPos(twoDPos.x, twoDPos.y);
    }

    /// <summary>
    /// 根據(x,y)座標，取得對應的3維地面Unity座標
    /// </summary>
    /// <returns>對應的地面座標</returns>
    public static Vector3 Get3DGroundPos(float x, float y)
    {
        Vector3 groundPos = new Vector3(x, 0, y);
        //if (Terrain.activeTerrain != null)
        {
            RaycastHit collisionHit;
            if (Physics.Raycast(groundPos + Vector3.up * GlobalConst.MAX_HEIGHT, Vector3.down, out collisionHit))
            {
                groundPos = collisionHit.point;
            }
            else
            {
                groundPos.y = Mathf.NegativeInfinity;
            }
        }
        return groundPos;
    }

    /// <summary>
    /// 依據npcID和序列號取得對應的單位Key
    /// </summary>
    /// <returns>對應的單位Key</returns>
    public static uint GetNPCUnitKey(ushort npcID, ushort serialNumber)
    {
        return npcID * GlobalConst.NPCID_TO_KEY_TIMES + serialNumber;
    }


    #region 列舉相關
    /// <summary>
    /// 取得Enum的Attribute
    /// </summary>
    /// <typeparam name="T">要取得的Attribute型別</typeparam>
    /// <param name="value">列舉值</param>
    /// <param name="outAttr">輸出的Attribute</param>
    /// <returns>是否有成功取得</returns>
    public static bool GetAttribute<T>(System.Enum value, out T outAttr) where T : System.Attribute
    {
        outAttr = default(T);
        System.Type curType = value.GetType();
        System.Reflection.FieldInfo curFieldInfo = curType.GetField(value.ToString());
        if (curFieldInfo != null)
        {
            T[] curAttrs = curFieldInfo.GetCustomAttributes(typeof(T), false) as T[];
            if (curAttrs != null && curAttrs.Length > 0)
            {
                outAttr = curAttrs[0];
                return true;
            }
        }
        return false;
    }
    #endregion
}
