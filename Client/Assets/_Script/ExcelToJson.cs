using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

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
    const string END_OF_COLUMN = "EOF";// 表示為最後column（不包含此column）的識別字
    const string END_OF_ROW = "EOF";   // 表示為最後row（不包含此row）的識別字

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
        ReadOneExcelFile();
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    void ReadOneExcelFile()
    {
        foreach (DataConvertAttribute oneData in DataConvertList)
        {
            using (FileStream fs = File.Open(oneData + ".xlsx", FileMode.Open, FileAccess.Read))
            {
                #region ExcelDataReader
                // 1. Reading from a OpenXml Excel file (2007 format; *.xlsx)
                // 下面會在結束時自動dispose
                using (Excel.ExcelOpenXmlReader excelReader = Excel.ExcelReaderFactory.CreateOpenXmlReader(fs) as Excel.ExcelOpenXmlReader) 
                {
                    if (excelReader != null)
                    {
                        bool isStartTable = false; // 是否已經開始讀取表格
                        int columnCount = 0; // column數
                        List<string> allType = new List<string>();  // 所有欄位的type字串
                        List<int> dontNeedColumn = new List<int>(); // 不需要的欄位
                        // 2. Data Reader methods
                        while (excelReader.Read())
                        {
                            if (!isStartTable && excelReader.GetString(0).Equals(START_OF_TABLE))
                            {
                                isStartTable = true;
                            }
                            if (isStartTable)
                            {
                                if (columnCount == 0) // 還不知道有幾個column
                                {
                                    columnCount = 1;
                                    while (!excelReader.GetString(columnCount).Equals(END_OF_COLUMN))
                                    {
                                        ++columnCount;
                                    }
                                }
                                else // 已經知道Column數量
                                {
                                    if (allType.Count == 0) // 還不知道各項目類型
                                    {
                                        for (int col = 1; col < columnCount; ++col)
                                        {
                                            allType.Add(excelReader.GetString(col));
                                        }
                                    }
                                    else // 已經知道各項目類型
                                    {
                                        if (dontNeedColumn.Count == 0) //
                                        {
                                            for (int col = 1; col < columnCount; ++col)
                                            {
                                                if (excelReader.GetString(col).Equals("N")) // TODO: 之後要改這段
                                                {
                                                    dontNeedColumn.Add(col);
                                                }
                                            }
                                            // TODO：確認格式
                                        }
                                        else
                                        {
                                            object data = Activator.CreateInstance(oneData.DataType);
                                            
                                        }
                                    }
                                }
                            }
                            Common.DebugMsg(excelReader.GetString(0));
                        }
                    }
                }
                
                #endregion
            }
        }
    }

}
