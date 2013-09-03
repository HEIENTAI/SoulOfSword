using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Excel;

public enum ReadExcelError
{
    NONE = 0, // 沒有問題
    FILE_NOT_EXIST = 1, // 檔案不存在
    FILE_OPEN_ERROR = 2, // 檔案開啟有問題

    CANT_FIND_START_TOKEN = 10, // 找不到開始符號

    CANT_FIND_END_OF_COL_TOKEN = 20, // 找不到行結尾符號
    TABEL_COL_NUM_IS_ZERO = 21, // 欄位數為0
    TABLE_COL_NUM_NOT_ENOUGH = 22, // 欄位不足

    END_OF_ROW_TOKEN_TO_EARLY = 30, // 太早遇到列結尾符號
    DONT_HAVE_TYPE_ROW = 31, // 沒有指示型別的列
    DONT_INSTRUCT_NEED_ROW = 32, // 沒指示需要欄位
};

/// <summary>
/// 需要讀取此資料的是server還client
/// </summary>
public enum NeedReadSite
{
    CLIENT,
    SERVER,
};

/// <summary>
/// 將excel讀入取得所需的table
/// </summary>
public class ExcelToTable
{
    readonly string EXCEL_DIRECTORY = "EXCEL"; // excel檔案所在的資料夾
    readonly string EXCEL_EXT = ".xlsx"; // excel檔案副檔名

    readonly string START_OF_TABLE = "#"; // 表示表格開始的識別字
    readonly string END_OF_COLUMN = "EOC";// 表示為最後column（不包含此column）的識別字
    readonly string END_OF_ROW = "EOR";   // 表示為最後row（不包含此row）的識別字
    readonly string NEED_READ_SITE_IS_ALL = "A";    // 「都需要讀」的欄位識別字
    readonly string NEED_READ_SITE_IS_SERVER = "S"; // 「只有Server需要讀」的欄位識別字
    readonly string NEED_READ_SITE_IS_CLIENT = "C"; // 「只有Client需要讀」的欄位識別字
    readonly string NEED_READ_SITE_IS_NONE = "N";   // 「都不需要讀」的欄位識別字

    ExcelOpenXmlReader _excelReader = null;

    int _columnCount;
    List<int> _dontNeedColumnIndexes; // table不需要的欄位Index

    public ExcelToTable()
    {
        _dontNeedColumnIndexes = new List<int>();
    }

    ~ExcelToTable()
    {
        _dontNeedColumnIndexes = null;
        if (_excelReader != null)
        {
            _excelReader.Close(); // Free Resources
            _excelReader = null;
        }
    }

    #region 確認table header正確性
    /// <summary>
    /// 確認excel檔案有正確的table header 且取得相關資訊
    /// </summary>
    /// <returns>可能有的錯誤類型</returns>
    public ReadExcelError CheckAndReadTableHeader(NeedReadSite nrs, out List<string> allType)
    {
        allType = null;
        if (!HasTable()) { return ReadExcelError.CANT_FIND_START_TOKEN; }
        ReadExcelError ree = GetTableColumnCount();
        if (ree != ReadExcelError.NONE) { return ree; }
        ree = GetTableAllColumnType(out allType);
        if (ree != ReadExcelError.NONE) { return ree; }
        ree = GetTableIgnoreColumn(nrs);
        if (ree != ReadExcelError.NONE) { return ree; }
        // 由於先讀type，再讀忽略欄位index，所以得再此才能依據忽略的欄位index調整allType
        _dontNeedColumnIndexes.Sort();
        for (int index = _dontNeedColumnIndexes.Count - 1; index >= 0; --index) // 由大往小刪除，避免刪錯
        {
            allType.RemoveAt(_dontNeedColumnIndexes[index]);
        }
        return ReadExcelError.NONE;
    }

    /// <summary>
    /// 確認此Excel是否有所需Table，
    /// </summary>
    bool HasTable()
    {
        bool hasContent = false;
        while (!hasContent) // 如果沒找到content則要一直尋找
        {
            List<string> getData = GetNextRow();
            if (getData == null) { break; } // 表示已經讀到excel檔案結尾依舊沒東西 or 根本沒讀取file
            if (!string.IsNullOrEmpty(getData[0]) && getData[0].Equals(START_OF_TABLE)) { hasContent = true; }
        }
        return hasContent;
    }

    /// <summary>
    /// 取得excel中table的column數，結果存在_columnCount
    /// </summary>
    /// <returns>可能有的錯誤訊息</returns>
    ReadExcelError GetTableColumnCount()
    {
        List<string> countColumnData = GetNextRow();
        if (countColumnData == null || countColumnData.Count == 0) { return ReadExcelError.TABEL_COL_NUM_IS_ZERO; } // 沒有計算到欄位數
        if (!string.IsNullOrEmpty(countColumnData[0]) && countColumnData[0].Equals(END_OF_ROW)) { return ReadExcelError.END_OF_ROW_TOKEN_TO_EARLY; } // 太早遇到END_OF_ROW
        for (_columnCount = 0; _columnCount < countColumnData.Count; ++_columnCount)
        {
            if (!string.IsNullOrEmpty(countColumnData[_columnCount]) && countColumnData[_columnCount].Equals(END_OF_COLUMN)) { break; } // 遇到END_OF_COLUMN跳離，此時_columnCount即欄位數
        }
        if (_columnCount == countColumnData.Count) // 表示中途都未跳離 
        {
            _columnCount = 0; // 將column數量重設回0
            return ReadExcelError.CANT_FIND_END_OF_COL_TOKEN;
        }
        return ReadExcelError.NONE;
    }

    /// <summary>
    /// 取得excel中table內所有column對應的type，結果存在_allType
    /// </summary>
    /// <returns>可能有的錯誤訊息</returns>
    ReadExcelError GetTableAllColumnType(out List<string> typeColumnData)
    {
        typeColumnData = GetNextRow();
        if (typeColumnData == null || typeColumnData.Count == 0) { return ReadExcelError.DONT_HAVE_TYPE_ROW; } // 沒有型別row
        if (!string.IsNullOrEmpty(typeColumnData[0]) && typeColumnData[0].Equals(END_OF_ROW)) { return ReadExcelError.END_OF_ROW_TOKEN_TO_EARLY; } // 太早遇到END_OF_ROW
        if (typeColumnData.Count != _columnCount) { return ReadExcelError.TABLE_COL_NUM_NOT_ENOUGH; } // column數量不正確

        return ReadExcelError.NONE;
    }

    /// <summary>
    /// 取得excel中table內所有忽略的column index，結果存在ignoreColumnData
    /// </summary>
    /// <returns>可能有的錯誤訊息</returns>
    ReadExcelError GetTableIgnoreColumn(NeedReadSite nrs)
    {
        _dontNeedColumnIndexes.Clear();
        List<string> ignoreColumnData = GetNextRow();
        if (ignoreColumnData == null || ignoreColumnData.Count == 0) { return ReadExcelError.DONT_INSTRUCT_NEED_ROW; } // 沒指示不需讀入欄位
        if (!string.IsNullOrEmpty(ignoreColumnData[0]) && ignoreColumnData[0].Equals(END_OF_ROW)) { return ReadExcelError.END_OF_ROW_TOKEN_TO_EARLY; } // 太早遇到END_OF_ROW
        if (ignoreColumnData.Count < _columnCount) { return ReadExcelError.TABLE_COL_NUM_NOT_ENOUGH; } //column數量不正確
        for (int col = 0; col < _columnCount; ++col)
        {
            if (string.IsNullOrEmpty(ignoreColumnData[col])) { return ReadExcelError.TABLE_COL_NUM_NOT_ENOUGH; } // column數量不正確
            if (!(ignoreColumnData[col].Equals(NEED_READ_SITE_IS_ALL) ||
                (ignoreColumnData[col].Equals(NEED_READ_SITE_IS_SERVER) && nrs == NeedReadSite.SERVER) ||
                (ignoreColumnData[col].Equals(NEED_READ_SITE_IS_CLIENT) && nrs == NeedReadSite.CLIENT))) // 
            { _dontNeedColumnIndexes.Add(col); }
        }
        return ReadExcelError.NONE;
    }
    #endregion
    #region 確認table結尾
    /// <summary>
    /// 確定是否為table結尾
    /// </summary>
    /// <param name="rowData">row資料，不處理本身為null，或count = 0的狀況</param>
    /// <returns>是否為table結尾</returns>
    public bool CheckEndOfTable(List<string> rowData)
    {
        return !string.IsNullOrEmpty(rowData[0]) && rowData[0].Equals(END_OF_ROW);
    }
    #endregion
    #region 開關檔、讀一行資料
    /// <summary>
    /// 開啟一excel檔案，開啟成功（回傳值為ReadExcelError.NONE）則_excelReader可讀取資料
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public ReadExcelError OpenExcelFile(string fileName)
    {
        string filePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + EXCEL_DIRECTORY + Path.DirectorySeparatorChar + fileName + EXCEL_EXT;
        if (!File.Exists(filePath)) { return ReadExcelError.FILE_NOT_EXIST; }

        try
        {
            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                _excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs) as ExcelOpenXmlReader;
                if (_excelReader == null) { return ReadExcelError.FILE_OPEN_ERROR; }
            }
        }
        catch { return ReadExcelError.FILE_OPEN_ERROR; }
        return ReadExcelError.NONE;
    }

    /// <summary>
    /// 關閉開啟的excel資源
    /// </summary>
    public void Close()
    {

        if (_excelReader != null)
        {
            _excelReader.Close();
            _excelReader = null;
        }
    }

    /// <summary>
    /// 取得下一行資料
    /// </summary>
    public List<string> GetNextRow()
    {
        if (_excelReader == null) { return null; }
        if (!_excelReader.Read()) { return null; }

        List<string> retStrList = new List<string>();

        int colCount = 0;
        int readColumnCount = (_columnCount == 0) ? int.MaxValue : _columnCount; // 如果_columnCount有值，取其值，否則取最大值
        try
        {
            for (colCount = 0; colCount < readColumnCount; ++colCount)
            {
                if (!_dontNeedColumnIndexes.Contains(colCount)) { retStrList.Add(_excelReader.GetString(colCount)); } // 如果有在忽略列，就不管
            }
        }
        catch (Exception e) { Common.DebugMsgFormat("colCount = {0} retStrList.Count = {1} error = {2}\n", colCount, retStrList.Count, e.Message); }
        return retStrList;
    }
    #endregion
}
