using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

/// <summary>
/// 此 scirpt 會被編譯為 pc standalone 版本執行檔
/// 將Excel檔案轉換成json檔案的工具
/// </summary>
public class ExcelToJson : MonoBehaviour 
{
    class DataConvertAttribute
    {
        public Type DataType
        {
            get;
            protected set;
        }
        public string FileName
        {
            get;
            protected set;
        }

        public DataConvertAttribute(Type setDataType, string setFileName)
        {
            DataType = setDataType;
            FileName = setFileName;
        }
    }

    private static DataConvertAttribute[] DataConvertList = 
    {
        new DataConvertAttribute(typeof(EventData), "EventData"),
    };

    const string START_OF_TABLE = "#"; // 表示表格開始的識別字
    const string END_OF_COLUMN = "EOC";// 表示為最後column（不包含此column）的識別字
    const string END_OF_ROW = "EOR";   // 表示為最後row（不包含此row）的識別字

    Dictionary<Type, string> _baseTypeString = new Dictionary<Type, string>()
    {
        {typeof(byte), "BYTE"},
        {typeof(ushort), "USHORT"},
        {typeof(uint), "UINT"},
        {typeof(string), "STRING"},
    };

    Dictionary<Type, string> _baseTypeTransferString = new Dictionary<Type,string>() // 型別和字串轉該型別的對應
    {
        {typeof(byte), "ToByte"},
        {typeof(ushort), "ToUInt16"},
        {typeof(uint), "ToUInt32"},
    };

    int _columnCount; // table的column數
    List<string> _allType; // table中所有欄位的type字串
    List<int> _dontNeedColumnIndexes; // table不需要的欄位Index

    

	// Use this for initialization
	void Start () 
    {
        //string path = @"D:\Work\jingjang\SoulOfSword\ExcelToJson\ExcelToJson\bin\Debug";
        //string[] xlsxFiles = Directory.GetFiles(path, "*.xlsx");

        //int fileCount = 0;
        //foreach (string curFileName in xlsxFiles)
        //{
        //    Common.DebugMsg(string.Format("第 {0} file Name = {1}", ++fileCount, curFileName));
        //    ReadOneExcelData(curFileName);
        //}
        bool isSuccess;
        List<object> allData;
        foreach (DataConvertAttribute oneData in DataConvertList)
        {
            InitTableVariable();
            isSuccess = ReadOneExcelFile(oneData, out allData);
        }
        Test testEncode = new Test();
        testEncode.testByte = null;
        testEncode.testInt = 5;
        string testJSon = Newtonsoft.Json.JsonConvert.SerializeObject(testEncode);
        Common.DebugMsgFormat("testJSon = {0}", testEncode);

        Test testDeCode = Newtonsoft.Json.JsonConvert.DeserializeObject(testJSon, typeof(Test)) as Test;
        Common.DebugMsgFormat("testDecode = {0}", testDeCode);
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    /// <summary>
    /// 將讀取excel內table時會用到的變數初始化
    /// </summary>
    void InitTableVariable()
    {
        _columnCount = 0;
        _allType = new List<string>();
        _dontNeedColumnIndexes = new List<int>();
    }

    /// <summary>
    /// 讀取一個excel檔案，讀到的資料存在allData，回傳是否成功
    /// </summary>
    bool ReadOneExcelFile(DataConvertAttribute excelAndTypeData, out List<object> allData)
    {
        allData = new List<object>();
        using (FileStream fs = File.Open("Assets" + Path.DirectorySeparatorChar + excelAndTypeData.FileName + ".xlsx", FileMode.Open, FileAccess.Read))
        {
            #region ExcelDataReader
            // 1. Reading from a OpenXml Excel file (2007 format; *.xlsx)
            // 下面會在結束時自動dispose
            using (Excel.ExcelOpenXmlReader excelReader = Excel.ExcelReaderFactory.CreateOpenXmlReader(fs) as Excel.ExcelOpenXmlReader) 
            {
                if (excelReader != null)
                {
                    // 2. Data Reader methods
                    #region 確認此Excel是否有所需Table
                    bool hasContent = false;
                    while (excelReader.Read())
                    {
                        if (excelReader.GetString(0).Equals(START_OF_TABLE)) 
                        { 
                            hasContent = true;
                            break;
                        }
                    }
                    if (!hasContent)
                    {
                        Common.DebugMsg(string.Format("{0} 裡面沒有所需內容", excelAndTypeData.FileName));
                        return false;
                    }
                    #endregion
                    #region 計算Table中的Column數
                    if (excelReader.Read())
                    {
                        if (CheckEndOfRow(excelReader))
                        {
                            Common.DebugMsgFormat("{0} 太早遇到列結尾符號 {1}", excelAndTypeData.FileName, END_OF_ROW);
                            return false;
                        }
                        try
                        {
                            while (!excelReader.GetString(_columnCount).Equals(END_OF_COLUMN.ToUpper())) 
                            {
                                ++_columnCount; 
                            }
                        }
                        catch (Exception e)
                        {
                            _columnCount = 0;
                            Common.DebugMsgFormat("{0} 內找不到行結尾符號 {1}", excelAndTypeData.FileName, END_OF_COLUMN);
                            Common.DebugMsgFormat("{0}\n{1}", e.Message, e.StackTrace);
                            return false;    
                        }
                    }
                    if (_columnCount == 0)
                    {
                        Common.DebugMsg(string.Format("{0} 內的表格欄位數是0", excelAndTypeData.FileName));
                        return false;
                    }
                    Common.DebugMsg(string.Format("{0} 內的表格欄位數是{1}", excelAndTypeData.FileName, _columnCount));
                    #endregion
                    #region 取得各欄位的類型
                    if (excelReader.Read())
                    {
                        if (CheckEndOfRow(excelReader))
                        {
                            Common.DebugMsgFormat("{0} 太早遇到列結尾符號 {1}", excelAndTypeData.FileName, END_OF_ROW);
                            return false;
                        }
                        for (int col = 0; col < _columnCount; ++col)
                        {
                            _allType.Add(excelReader.GetString(col));
                        }
                    }
                    if (_allType.Count != _columnCount)
                    {
                        Common.DebugMsg(string.Format("{0} 內的欄位型別資料不足", excelAndTypeData.FileName));
                        return false;
                    }
                    #endregion
                    #region 取得不需要讀入的欄位
                    if (excelReader.Read())
                    {
                        if (CheckEndOfRow(excelReader))
                        {
                            Common.DebugMsgFormat("{0} 太早遇到列結尾符號 {1}", excelAndTypeData.FileName, END_OF_ROW);
                            return false;
                        }
                        for (int col = 0; col < _columnCount; ++col)
                        {
                            if (excelReader.GetString(col).Equals("N")) // TODO: 之後要改這段
                            {
                                _dontNeedColumnIndexes.Add(col);
                                Common.DebugMsg(string.Format("ignore column {0}", col));
                            }
                        }
                    }
                    else
                    {
                        Common.DebugMsg(string.Format("{0} 內沒有指示各欄位是否需要的內容", excelAndTypeData.FileName));
                        return false;
                    }
                    #endregion
                    #region 確認各欄位和要被寫入的物件欄位Type有對應
                    int currentColumnIndex = 0;
                    bool isConform = CheckExcelAndObjectType(excelAndTypeData.DataType, ref currentColumnIndex);
                    if (!isConform)
                    {
                        Common.DebugMsg(string.Format("{0} 檔案和資料結構比對失敗", excelAndTypeData.FileName));
                        return false;
                    }
                    else
                    {
                        Common.DebugMsgFormat(" 檔案（{0}）和資料結構（{1}）比對成功", excelAndTypeData.FileName, excelAndTypeData.DataType);
                    }
                    #endregion
                    #region 抓取資料
                    
                    while (excelReader.Read())
                    {
                        if (CheckEndOfRow(excelReader))
                        {
                            break;
                        }
                        if (CheckEmptyRow(excelReader))
                        {
                            Common.DebugMsgFormat("檔案( {0}) 資料有空行 跳走", excelAndTypeData.FileName);
                            allData.Clear();
                            return false;
                        }
                        currentColumnIndex = 0;
                        allData.Add(GetObjectFromExcel(excelAndTypeData.DataType, excelReader, ref currentColumnIndex));
                    }
                    #endregion
                    foreach (object data in allData)
                    {
                        Common.DebugMsg(data.ToString());
                    }
                }
            }
                
            #endregion
        }
        return true;
    }

    /// <summary>
    /// 確認是否為空Row
    /// </summary>
    /// <param name="excelReader"></param>
    /// <returns></returns>
    bool CheckEmptyRow(Excel.ExcelOpenXmlReader excelReader)
    {
        int col = 0;
        while (col < _columnCount)
        {
            if (string.IsNullOrEmpty(excelReader.GetString(col)))
            {
                col = GetNextColumnIndex(col);
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 確認是否為表格結尾
    /// </summary>
    bool CheckEndOfRow(Excel.ExcelOpenXmlReader excelReader)
    {
        string firstRowString = excelReader.GetString(0);
        if (string.IsNullOrEmpty(firstRowString))
        {
            return false;
        }
        else
        {
            return firstRowString.ToUpper().Equals(END_OF_ROW);
        }
    }


    /// <summary>
    /// 取得下一筆要取出的column index
    /// </summary>
    /// <param name="curColumnIndex">現在的column index</param>
    /// <returns>下一筆column index</returns>
    int GetNextColumnIndex(int curColumnIndex)
    {
        int nextColumnIndex = curColumnIndex + 1;
        while (_dontNeedColumnIndexes.Contains(nextColumnIndex))
        {
            ++nextColumnIndex;
        }
        return nextColumnIndex;
    }

    /// <summary>
    /// 確認excel表格內定義的Type是否和定義的資料結構有對應
    /// </summary>
    /// <param name="type">資料結構Type</param>
    /// <param name="currentIndex">現在讀到第幾筆</param>
    /// <returns>回傳True表示有對應</returns>
    bool CheckExcelAndObjectType(Type type, ref int excelCurrentIndex)
    {
        object tempObject = Activator.CreateInstance(type);
        bool isConform = true;

        FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        for (int fieldInfoIndex = 0; fieldInfoIndex < fieldInfos.Length; ++fieldInfoIndex)
        {
            int curFieldNum; // 現在Field的個數：非array是1，array則是該array長
            Type curFieldType;

            #region 取出此field的Type(array則是項目的type，不考慮泛型) &個數
            if (fieldInfos[fieldInfoIndex].FieldType.IsArray)
            {
                var array = fieldInfos[fieldInfoIndex].GetValue(tempObject) as Array;
                curFieldNum = array.Length;
                curFieldType = fieldInfos[fieldInfoIndex].FieldType.GetElementType();
            }
            else
            {
                curFieldNum = 1;
                curFieldType = fieldInfos[fieldInfoIndex].FieldType;
            }
            #endregion

            for (int elementCount = 0; elementCount < curFieldNum; ++elementCount)
            {
                if (excelCurrentIndex >= _columnCount) //先檢查現在是否已經超出表格邊界
                {
                    isConform = false;
                    break;
                }
                string compareStr;
                if (_baseTypeString.TryGetValue(curFieldType, out compareStr)) // 是基本四型態(byte,ushort, uint, string)之一
                {
                    if (_allType[excelCurrentIndex].ToUpper().Equals(compareStr.ToUpper()))
                    {
                        excelCurrentIndex = GetNextColumnIndex(excelCurrentIndex);
                    }
                    else
                    {
                        isConform = false;
                        break;
                    }
                }
                else
                {
                    isConform = CheckExcelAndObjectType(curFieldType, ref excelCurrentIndex);
                }
                if (!isConform)
                {
                    break;
                }
            }
            if (!isConform)
            {
                break;
            }
        }
        return isConform;
    }

    object GetObjectFromExcel(Type type, Excel.ExcelOpenXmlReader excelReader, ref int excelCurrentIndex)
    {
        object returnObj = Activator.CreateInstance(type, true);
        bool isNull = true;

        FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        for (int fieldInfoIndex = 0; fieldInfoIndex < fieldInfos.Length; ++fieldInfoIndex)
        {
            Type curFieldType;
            if (fieldInfos[fieldInfoIndex].FieldType.IsArray)
            {
                Array array = fieldInfos[fieldInfoIndex].GetValue(returnObj) as Array;
                curFieldType = fieldInfos[fieldInfoIndex].FieldType.GetElementType();

                for (int elementCount = 0; elementCount < array.Length; ++elementCount)
                {
                    string value = excelReader.GetString(excelCurrentIndex);
                    if (!string.IsNullOrEmpty(value))
                    {
                        isNull = false;
                    }

                    object tempObj;
                    if (curFieldType.IsClass && curFieldType != typeof(string))
                    {
                        tempObj = GetObjectFromExcel(curFieldType, excelReader, ref excelCurrentIndex);
                    }
                    else
                    {
                        bool isSuccess = GetBaseTypeFromExcel(curFieldType, value, out tempObj);
                        if (isSuccess)
                        {
                            excelCurrentIndex = GetNextColumnIndex(excelCurrentIndex);
                        }
                        else
                        {
                            Common.DebugMsgFormat("{0} 欄位轉換失敗", fieldInfos[fieldInfoIndex].Name);
                        }
                    }
                    array.SetValue(tempObj, elementCount);

                }
                fieldInfos[fieldInfoIndex].SetValue(returnObj, array);
            }
            else
            {
                curFieldType = fieldInfos[fieldInfoIndex].FieldType;

                string value = excelReader.GetString(excelCurrentIndex);

                if (!string.IsNullOrEmpty(value))
                {
                    isNull = false;
                }
                object tempObj;
                if (curFieldType.IsClass && curFieldType != typeof(string))
                {
                    tempObj = GetObjectFromExcel(curFieldType, excelReader, ref excelCurrentIndex); // excelCurrentIndex已經重算好
                }
                else
                {
                    bool isSuccess = GetBaseTypeFromExcel(curFieldType, value, out tempObj);
                    if (isSuccess)
                    {
                        excelCurrentIndex = GetNextColumnIndex(excelCurrentIndex);
                    }
                    else
                    {
                        Common.DebugMsgFormat("{0} 欄位轉換失敗", fieldInfos[fieldInfoIndex].Name);
                    }
                }
                fieldInfos[fieldInfoIndex].SetValue(returnObj, tempObj);
            }
        }
        if (isNull)
        {
            returnObj = null;
        }
        return returnObj;
    }

    bool GetBaseTypeFromExcel(Type dataType, string excelStr, out object getData)
    {
        if (dataType == typeof(string)) // 如果是字串，不做處理直接丟出
        {
            getData = excelStr;
            return true;
        }
        else
        {
            bool isNullableType = (dataType.IsGenericType && dataType.GetGenericTypeDefinition() == typeof(Nullable<>));
            if (string.IsNullOrEmpty(excelStr))
            {
                getData = null;
                return isNullableType;
            }
            else
            {
                string[] para = new string[1] { excelStr };
                Type[] transferType = new Type[1] { typeof(string) };
                Type realType = (isNullableType) ? Nullable.GetUnderlyingType(dataType) : dataType;
                try
                {
                    getData = realType.GetMethod("Parse", transferType).Invoke(null, para);
                    return true;
                }
                catch(Exception e)
                {
                    Common.DebugMsgFormat("取得基本型別時出錯\n{0}\n{1}",e.Message, e.StackTrace);
                    getData = null;
                    return false;
                }
            }
        }
    }
}
