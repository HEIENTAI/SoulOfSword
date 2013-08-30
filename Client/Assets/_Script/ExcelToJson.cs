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

    const string EXCEL_DIRECTORY = "EXCEL"; // excel檔案所在的資料夾
    const string JSON_DIRECTORY = "JSON";   // json檔案存放資料夾
    const string START_OF_TABLE = "#"; // 表示表格開始的識別字
    const string END_OF_COLUMN = "EOC";// 表示為最後column（不包含此column）的識別字
    const string END_OF_ROW = "EOR";   // 表示為最後row（不包含此row）的識別字


    readonly Dictionary<Type, string> _baseTypeString = new Dictionary<Type, string>()
    {
        {typeof(byte), "BYTE"},
        {typeof(ushort), "USHORT"},
        {typeof(uint), "UINT"},
        {typeof(string), "STRING"},
    };

    GUIStyle _uiStyle;
    Rect _fileListWindowRect;
    private Vector2 _windowScrollPosition;
    string _windowMessage = string.Empty;

    bool _currentlyTransfering = false; // 是否正在轉換中

    // 讀取table用
    int _columnCount; // table的column數
    List<string> _allType; // table中所有欄位的type字串
    List<int> _dontNeedColumnIndexes; // table不需要的欄位Index

    void Awake()
    {
        _uiStyle = new GUIStyle();
        _uiStyle.fontSize = 14;
        _uiStyle.normal.textColor = Color.yellow;
        if (Camera.main != null)
            Camera.main.backgroundColor = Color.black;
    }


	// Use this for initialization
	void Start () 
    {
        _fileListWindowRect = new Rect(0, 70, Screen.width - 40, Screen.height - 70);

	}
	
	// Update is called once per frame
	void Update () 
    {
	}

    void OnGUI()
    {
        if (!_currentlyTransfering)
        {
            try
            {
                if (GUI.Button(new Rect(100, 40, 100, 20), "Excel -> Json"))
                {
                    _currentlyTransfering = true;
                    _windowMessage = "";
                    TransferFilesFromExcelToJson();
                }
            }
            catch (Exception e)
            {
                _currentlyTransfering = false;
                _windowMessage = _windowMessage + e.StackTrace + "\n" + e.Message;
            }
        }

        _fileListWindowRect = GUI.Window(0, _fileListWindowRect, TransferMessageWindow, "Debug Window");
    }

    void OnDestroy()
    {
        _allType = null;
        _dontNeedColumnIndexes = null;
    }

    /// <summary>
    /// 轉換訊息視窗
    /// </summary>
    public void TransferMessageWindow(int windowID)
    {
        _windowScrollPosition = GUILayout.BeginScrollView(_windowScrollPosition); // 加入捲軸
        GUILayout.TextArea(_windowMessage, _uiStyle, GUILayout.ExpandHeight(true)); // 自動伸縮捲軸
        GUILayout.EndScrollView();
        GUI.DragWindow();
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
    /// 轉換檔案(Excel -> Json)
    /// </summary>
    void TransferFilesFromExcelToJson()
    {
        bool isSuccess;
        List<object> allData;
        int successFileCount = 0;
        foreach (DataConvertAttribute oneData in DataConvertList)
        {
            InitTableVariable();
            isSuccess = ReadOneExcelFile(oneData, out allData);
            if (isSuccess)
            {
                WriteToJsonFile(allData, oneData.FileName);
                ++successFileCount;
            }
            else
            {
                _windowMessage = string.Format("{0}取得{1}內資料失敗\n", oneData.FileName);
            }
        }
        _windowMessage = string.Format("{0}共轉換 {1}個檔案成功，{2}個檔案失敗\n", _windowMessage, successFileCount, DataConvertList.Length - successFileCount);
        _currentlyTransfering = false;
    }

    /// <summary>
    /// 將ob資料寫成json檔
    /// </summary>
    void WriteToJsonFile(object ob, string fileName)
    {
        Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings();
        settings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        settings.CheckAdditionalContent = false;
        string encodeToJson = Newtonsoft.Json.JsonConvert.SerializeObject(ob, settings);
        string directoryPath = Application.dataPath + Path.DirectorySeparatorChar + JSON_DIRECTORY;

        if (!Directory.Exists(directoryPath)) // 如果資料夾不存在
        {
            Directory.CreateDirectory(directoryPath); // 建立目錄
        }
        string filePath = directoryPath + Path.DirectorySeparatorChar + fileName + ".json";
        using (StreamWriter sw = new StreamWriter(filePath))
        {
            sw.Write(encodeToJson);
        }
        _windowMessage = string.Format("{0}將 {1} 資料轉換成json成功\n", _windowMessage, filePath);
    }

    /// <summary>
    /// 讀取一個excel檔案，讀到的資料存在allData，回傳是否成功
    /// </summary>
    bool ReadOneExcelFile(DataConvertAttribute excelAndTypeData, out List<object> allData)
    {
        allData = new List<object>();
        string filePath = Application.dataPath + Path.DirectorySeparatorChar + EXCEL_DIRECTORY + Path.DirectorySeparatorChar + excelAndTypeData.FileName + ".xlsx";
        if (!File.Exists(filePath))
        {
            _windowMessage = string.Format("{0} {1}檔案不存在\n", _windowMessage, filePath);
            return false;
        }
        using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            #region ExcelDataReader
            // 1. Reading from a OpenXml Excel file (2007 format; *.xlsx)
            // 下面會在結束時自動dispose
            using (Excel.ExcelOpenXmlReader excelReader = Excel.ExcelReaderFactory.CreateOpenXmlReader(fs) as Excel.ExcelOpenXmlReader) 
            {
                if (excelReader != null)
                {
                    // 2. Data Reader methods
                    _windowMessage = string.Format("{0}讀取 {1} 資料...\n", _windowMessage, excelAndTypeData.FileName);
                    #region 確認此Excel是否有所需Table
                    bool hasContent = false;
                    while (excelReader.Read())
                    {
                        if (!string.IsNullOrEmpty(excelReader.GetString(0)) && excelReader.GetString(0).Equals(START_OF_TABLE)) 
                        { 
                            hasContent = true;
                            break;
                        }
                    }
                    if (!hasContent)
                    {
                        _windowMessage = string.Format("{0}{1} 轉換失敗：找不到開始符號[{2}]\n", _windowMessage, excelAndTypeData.FileName, START_OF_TABLE);
                        return false;
                    }
                    #endregion
                    #region 計算Table中的Column數
                    if (excelReader.Read())
                    {
                        if (CheckEndOfRow(excelReader))
                        {
                            _windowMessage = string.Format("{0}{1} 轉換失敗：太早遇到列結尾符號[{2}]", _windowMessage, excelAndTypeData.FileName, END_OF_ROW);
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
                            _windowMessage = string.Format("{0}{1} 轉換失敗：找不到行結尾符號[{2}]\n", _windowMessage, excelAndTypeData.FileName, END_OF_COLUMN);
                            Common.DebugMsgFormat("{0} 內找不到行結尾符號 {1}", excelAndTypeData.FileName, END_OF_COLUMN);
                            Common.DebugMsgFormat("{0}\n{1}", e.Message, e.StackTrace);
                            return false;    
                        }
                    }
                    if (_columnCount == 0)
                    {
                        _windowMessage = string.Format("{0}{1} 轉換失敗：表格欄位數為0\n", _windowMessage, excelAndTypeData.FileName);
                        return false;
                    }
                    Common.DebugMsg(string.Format("{0} 內的表格欄位數是{1}", excelAndTypeData.FileName, _columnCount));
                    #endregion
                    #region 取得各欄位的類型
                    if (excelReader.Read())
                    {
                        if (CheckEndOfRow(excelReader))
                        {
                            _windowMessage = string.Format("{0}{1} 轉換失敗：太早遇到列結尾符號[{2}]\n", _windowMessage, excelAndTypeData.FileName, END_OF_ROW);
                            return false;
                        }
                        for (int col = 0; col < _columnCount; ++col)
                        {
                            _allType.Add(excelReader.GetString(col));
                        }
                    }
                    if (_allType.Count != _columnCount)
                    {
                        _windowMessage = string.Format("{0}{1} 轉換失敗：欄位型別資料不足\n", _windowMessage, excelAndTypeData.FileName); 
                        return false;
                    }
                    #endregion
                    #region 取得不需要讀入的欄位
                    if (excelReader.Read())
                    {
                        if (CheckEndOfRow(excelReader))
                        {
                            _windowMessage = string.Format("{0}{1} 轉換失敗：太早遇到列結尾符號[{2}]\n", _windowMessage, excelAndTypeData.FileName, END_OF_ROW);
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
                        _windowMessage = string.Format("{0}{1} 轉換失敗：沒有指示各欄位是誰所需要\n", _windowMessage, excelAndTypeData.FileName);
                        return false;
                    }
                    #endregion
                    #region 確認各欄位和要被寫入的物件欄位Type有對應
                    int currentColumnIndex = 0;
                    bool isConform = CheckExcelAndObjectType(excelAndTypeData.DataType, ref currentColumnIndex);
                    if (!isConform)
                    {
                        _windowMessage = string.Format("{0}{1} 轉換失敗：表格與資料結構({2})內容不符\n", _windowMessage, excelAndTypeData.FileName, excelAndTypeData.DataType); 
                        return false;
                    }
                    else
                    {
                        _windowMessage = string.Format("{0}檔案({1})內的表格與資料結構({2})相符\n", _windowMessage, excelAndTypeData.FileName, excelAndTypeData.DataType);
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
                            _windowMessage = string.Format("{0}{1} 轉換失敗：表格有空行", _windowMessage, excelAndTypeData.FileName);
                            allData.Clear();
                            return false;
                        }
                        currentColumnIndex = 0;
                        allData.Add(GetObjectFromExcel(excelAndTypeData.DataType, excelReader, ref currentColumnIndex));
                    }
                    if (!excelReader.Read())
                    {
                        _windowMessage = string.Format("{0}{1} 轉換失敗：找不到列結尾符號[{2}]\n", _windowMessage, excelAndTypeData.FileName, END_OF_ROW);
                    }
                    else
                    {
                        if (!CheckEndOfRow(excelReader))
                        {
                            _windowMessage = string.Format("{0}{1} 轉換失敗：找不到列結尾符號[{2}]\n", _windowMessage, excelAndTypeData.FileName, END_OF_ROW);
                        }
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
                            _windowMessage = string.Format("{0}{1} 欄位轉換失敗\n", _windowMessage, fieldInfos[fieldInfoIndex].Name);
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
                        _windowMessage = string.Format("{0}{1} 欄位轉換失敗\n", _windowMessage, fieldInfos[fieldInfoIndex].Name);
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
