using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ExcelToJson
{
    /// <summary>
    /// 負責將table內容(一連串的string)轉換成json格式。
    /// </summary>
    public class TableToJsonString
    {
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

        // 讀取table用
        int _columnCount; // table的column數
        List<string> _allType; // table中所有欄位的type字串
        List<int> _dontNeedColumnIndexes; // table不需要的欄位Index

        string _debugMessage = string.Empty;
        string _fileListMessage = string.Empty;


        ExcelToTable _excelToTable;
        

        public TableToJsonString()
        {
            _excelToTable = new ExcelToTable();
        }
        ~TableToJsonString()
        {
            _excelToTable = null;
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
        /// 將object資料轉成Json字串
        /// </summary>
        /// <param name="ob">待轉換的object</param>
        /// <returns>對應的Json字串</returns>
        public string ObjectToJsonString(object ob)
        {
            Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings();
            settings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            settings.CheckAdditionalContent = false;
            return Newtonsoft.Json.JsonConvert.SerializeObject(ob, settings);
        }

        public ReadExcelError ReadExcelFile(DataConvertInfomation dci, NeedReadSite needReadSite)
        {
            ReadExcelError readExcelError = _excelToTable.OpenExcelFile(dci.FileName);
            if (readExcelError != ReadExcelError.NONE) { return readExcelError; }
            List<string> allType;
            readExcelError = _excelToTable.CheckAndReadTableHeader(needReadSite, out allType);
            if (readExcelError != ReadExcelError.NONE) { return readExcelError; }
            #region 確認各欄位和要被寫入的物件欄位Type有對應

            #endregion
            return ReadExcelError.NONE;
        }




        /// <summary>
        /// 讀取一個excel檔案，讀到的資料存在allData，回傳是否成功
        /// </summary>
        /// <param name="dci"></param>
        /// <param name="allData">讀到轉換後的結果</param>
        /// <param name="debugMessage">中間產生的針錯訊息</param>
        /// <returns>是否成功</returns>
        public bool ReadExcelFile(DataConvertInfomation dci, out List<object> allData, out string debugMessage)
        {
            InitTableVariable();
            allData = new List<object>();
            string filePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + EXCEL_DIRECTORY + Path.DirectorySeparatorChar + dci.FileName + ".xlsx";
            if (!File.Exists(filePath))
            {
                _debugMessage = string.Format("{0} {1}檔案不存在\n", _debugMessage, filePath);
                debugMessage = _debugMessage;
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
                        _debugMessage = string.Format("{0}讀取 {1} 資料...\n", _debugMessage, dci.FileName);
                        #region 確認此Excel是否有所需Table
                        bool hasContent = false;
                        int line = 0;
                        while (excelReader.Read())
                        {
                            ++line;

                            if (!string.IsNullOrEmpty(excelReader.GetString(0)) && (excelReader.GetString(0)).Equals(START_OF_TABLE))
                            {
                                hasContent = true;
                                break;
                            }
                            else
                            {
                                _debugMessage = string.Format("{0}{1} 開頭為：{2}\n", _debugMessage, dci.FileName, excelReader.GetString(0));
                                _debugMessage = string.Format("{0} Test :[1] = {1}\n", _debugMessage, excelReader.GetString(1));
                            }
                        }
                        _debugMessage = string.Format("{0}{1} 已經讀了 {2} 行\n", _debugMessage, dci.FileName, line);
                        _debugMessage = string.Format("{0}{1}\n", _debugMessage, excelReader.ExceptionMessage);

                        if (!hasContent)
                        {
                            _debugMessage = string.Format("{0}{1} 轉換失敗：找不到開始符號[{2}]\n", _debugMessage, dci.FileName, START_OF_TABLE);
                            debugMessage = _debugMessage;
                            return false;
                        }
                        #endregion
                        #region 計算Table中的Column數
                        if (excelReader.Read())
                        {
                            if (CheckEndOfRow(excelReader))
                            {
                                _debugMessage = string.Format("{0}{1} 轉換失敗：太早遇到列結尾符號[{2}]", _debugMessage, dci.FileName, END_OF_ROW);
                                debugMessage = _debugMessage;
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
                                _debugMessage = string.Format("{0}{1} 轉換失敗：找不到行結尾符號[{2}]\n", _debugMessage, dci.FileName, END_OF_COLUMN);
                                Common.DebugMsgFormat("{0} 內找不到行結尾符號 {1}", dci.FileName, END_OF_COLUMN);
                                Common.DebugMsgFormat("{0}\n{1}", e.Message, e.StackTrace);
                                debugMessage = _debugMessage;
                                return false;
                            }
                        }
                        if (_columnCount == 0)
                        {
                            _debugMessage = string.Format("{0}{1} 轉換失敗：表格欄位數為0\n", _debugMessage, dci.FileName);
                            debugMessage = _debugMessage;
                            return false;
                        }
                        Common.DebugMsg(string.Format("{0} 內的表格欄位數是{1}", dci.FileName, _columnCount));
                        #endregion
                        #region 取得各欄位的類型
                        if (excelReader.Read())
                        {
                            if (CheckEndOfRow(excelReader))
                            {
                                _debugMessage = string.Format("{0}{1} 轉換失敗：太早遇到列結尾符號[{2}]\n", _debugMessage, dci.FileName, END_OF_ROW);
                                debugMessage = _debugMessage;
                                return false;
                            }
                            for (int col = 0; col < _columnCount; ++col)
                            {
                                _allType.Add(excelReader.GetString(col));
                            }
                        }
                        if (_allType.Count != _columnCount)
                        {
                            _debugMessage = string.Format("{0}{1} 轉換失敗：欄位型別資料不足\n", _debugMessage, dci.FileName);
                            debugMessage = _debugMessage;
                            return false;
                        }
                        #endregion
                        #region 取得不需要讀入的欄位
                        if (excelReader.Read())
                        {
                            if (CheckEndOfRow(excelReader))
                            {
                                _debugMessage = string.Format("{0}{1} 轉換失敗：太早遇到列結尾符號[{2}]\n", _debugMessage, dci.FileName, END_OF_ROW);
                                debugMessage = _debugMessage;
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
                            _debugMessage = string.Format("{0}{1} 轉換失敗：沒有指示各欄位是誰所需要\n", _debugMessage, dci.FileName);
                            debugMessage = _debugMessage;
                            return false;
                        }
                        #endregion
                        #region 確認各欄位和要被寫入的物件欄位Type有對應
                        int currentColumnIndex = 0;
                        bool isConform = CheckExcelAndObjectType(dci.DataType, ref currentColumnIndex);
                        if (!isConform)
                        {
                            _debugMessage = string.Format("{0}{1} 轉換失敗：表格與資料結構({2})內容不符\n", _debugMessage, dci.FileName, dci.DataType);
                            debugMessage = _debugMessage;
                            return false;
                        }
                        else
                        {
                            _debugMessage = string.Format("{0}檔案({1})內的表格與資料結構({2})相符\n", _debugMessage, dci.FileName, dci.DataType);
                        }
                        #endregion
                        #region 抓取資料
                        bool hasEOR = false;
                        while (excelReader.Read())
                        {
                            if (CheckEndOfRow(excelReader))
                            {
                                hasEOR = true;
                                break;
                            }
                            if (CheckEmptyRow(excelReader))
                            {
                                _debugMessage = string.Format("{0}{1} 轉換失敗：表格有空行", _debugMessage, dci.FileName);
                                allData.Clear();
                                debugMessage = _debugMessage;
                                return false;
                            }
                            currentColumnIndex = 0;
                            allData.Add(GetObjectFromExcel(dci.DataType, excelReader, ref currentColumnIndex));
                        }
                        if (!hasEOR)
                        {
                            _debugMessage = string.Format("{0}{1} 轉換失敗：找不到列結尾符號[{2}]\n", _debugMessage, dci.FileName, END_OF_ROW);
                            debugMessage = _debugMessage;
                            return false;
                        }
                        #endregion
                        foreach (object data in allData)
                        {
                            Common.DebugMsg(data.ToString());
                        }
                    }
                    else
                    {
                        _debugMessage = string.Format("{0}{1}打開有問題\n", _debugMessage, dci.FileName);
                        debugMessage = _debugMessage;
                        return false;
                    }
                }
                #endregion
            }
            debugMessage = _debugMessage;
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
        /// <param name="excelCurrentIndex">現在讀到第幾筆</param>
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
                    GetArrayTypeFromExcel(fieldInfos[fieldInfoIndex].FieldType, excelReader, ref excelCurrentIndex, ref array);
                    if (array != null)
                    {
                        isNull = false;
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
                            _debugMessage = string.Format("{0}{1} 欄位轉換失敗\n", _debugMessage, fieldInfos[fieldInfoIndex].Name);
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

        bool GetArrayTypeFromExcel(Type dataType, Excel.ExcelOpenXmlReader excelReader, ref int excelCurrentIndex, ref Array returnArray)
        {
            bool success = true;
            if (!dataType.IsArray)
            {
                _debugMessage = string.Format("{0} 轉檔錯誤：非陣列的類型({1})想解成陣列\n", _debugMessage, dataType.Name);
                return false;
            }

            Type elementType = dataType.GetElementType();
            bool isNull = true;
            for (int elementCount = 0; elementCount < returnArray.Length; ++elementCount)
            {
                if (elementType.IsArray)
                {
                    Array array = dataType.GetFields()[0].GetValue(returnArray) as Array;
                    success = GetArrayTypeFromExcel(elementType, excelReader, ref excelCurrentIndex, ref array);
                    if (array != null)
                    {
                        isNull = false;
                    }
                    returnArray.SetValue(array, elementCount);
                }
                else if (elementType.IsClass && elementType != typeof(string))
                {
                    object tempObj = GetObjectFromExcel(elementType, excelReader, ref excelCurrentIndex);
                    returnArray.SetValue(tempObj, elementCount);
                    if (tempObj != null)
                    {
                        isNull = false;
                    }
                }
                else
                {
                    object tempObj;
                    success = GetBaseTypeFromExcel(elementType, excelReader.GetString(excelCurrentIndex), out tempObj);
                    if (success)
                    {
                        excelCurrentIndex = GetNextColumnIndex(excelCurrentIndex);
                        returnArray.SetValue(tempObj, elementCount);
                        if (tempObj != null)
                        {
                            isNull = false;
                        }
                    }
                    else
                    {
                        _debugMessage = string.Format("{0}{1} 欄位轉換失敗\n", _debugMessage, dataType.GetFields()[0].Name);
                        return false;
                    }
                }
            }
            if (isNull)
            {
                returnArray = null;
            }
            return true;
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
                    catch (Exception e)
                    {
                        Common.DebugMsgFormat("取得基本型別時出錯\n{0}\n{1}", e.Message, e.StackTrace);
                        getData = null;
                        return false;
                    }
                }
            }
        }
    }
}
