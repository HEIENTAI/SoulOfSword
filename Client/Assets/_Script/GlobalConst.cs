using UnityEngine;
using System;
using System.IO;
using System.Collections;

public class GlobalConst
{
    #region 數值常數
    public const float DEGREE_PER_CIRCLE = 360.0f; // 一圈 = 360度
    public const float DEGREE_PER_HALF_CIRCLE = 180.0f; // 半圈 = 180度
    #endregion

    #region 路徑相關常數
    public static readonly string DIR_ROOT = Application.dataPath + Path.DirectorySeparatorChar;
    public static readonly string DIR_RESOURCES = DIR_ROOT + "Resources" + Path.DirectorySeparatorChar;

    public static readonly string DIR_DATA_ROOT = Application.streamingAssetsPath + Path.DirectorySeparatorChar;
    public static readonly string DIR_DATA_JSON = DIR_DATA_ROOT + "JSON" + Path.DirectorySeparatorChar;

    public static readonly string EXT_JSONDATA = ".json";
    #endregion

    public const string JSON_EVENT = "EventData.json";

    public static readonly DataConvertInfomation[] DataConvertList = 
    {
        new DataConvertInfomation(typeof(EventData), "EventData"),
    };

    public enum DataLoadTag
    {
        [LoadAttribute(JSON_EVENT, typeof(EventData))] Event, // 事件資料表
    };
}

/// <summary>
/// 資源載入的屬性
/// </summary>
public class LoadAttribute : System.Attribute
{
    public string FileName
    {
        get;
        protected set;
    }

    public System.Type DataType
    {
        get;
        protected set;
    }

    public LoadAttribute(string fileName, Type dataType)
    {
        FileName = fileName;
        DataType = dataType;
    }

    public static string GetFileName(Enum value)
    {
        string retFileName = default(string);
        LoadAttribute enumType;
        if (GetAttribute(value, out enumType))
        {
            retFileName = enumType.FileName;
        }
        return retFileName;
    }

    public static Type GetDataType(Enum value)
    {
        Type retDataType = default(Type);
        LoadAttribute enumType;
        if (GetAttribute(value, out enumType))
        {
            retDataType = enumType.DataType;
        }
        return retDataType;
    }

    public static bool GetAttribute<T>(Enum value, out T outAttr)
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
}

