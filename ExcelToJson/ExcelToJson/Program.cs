using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ExcelToJson
{
    class Program
    {
        readonly static string JSON_DIRECTORY = "JSON";
        readonly static string EXCEL_DIRECTORY = "EXCEL"; // excel檔案所在的資料夾
        readonly static string JSON_EXT = ".json";       // json檔案副檔名
        static string fileListMessage = string.Empty;
        static string debugMessage = string.Empty;

        static void Main(string[] args)
        {
            //string path = Directory.GetCurrentDirectory();
            //var xlsxFiles = Directory.EnumerateFiles(path, "*.xlsx");

            TransferFilesFromExcelToJson();

            Common.DebugMsg(debugMessage);
            Common.DebugMsg(fileListMessage);
            Console.Write("按下Enter鈕結束。");
            Console.Read();
        }

        static void TransferFilesFromExcelToJson()
        {
            string exceDirectorylPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + EXCEL_DIRECTORY;
            string jsonDirectoryPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + JSON_DIRECTORY;
            if (!Directory.Exists(jsonDirectoryPath)) // 如果資料夾不存在
            {
                Directory.CreateDirectory(jsonDirectoryPath); // 建立目錄
            }

            ExcelToJsonString tableToJson = new ExcelToJsonString();
            int successFileCount = 0;

            foreach (DataConvertInfomation dci in GlobalConst.DataConvertList)
            {
                string dataJsonString;
                ReadExcelToJsonStringError error = tableToJson.ReadExcelFile(exceDirectorylPath, dci, NeedReadSite.CLIENT, out dataJsonString, out debugMessage);
                if (error == ReadExcelToJsonStringError.NONE)
                {
                    #region JsonString To File
                    string filePath = jsonDirectoryPath + Path.DirectorySeparatorChar + dci.FileName + JSON_EXT;
                    using (StreamWriter sw = new StreamWriter(filePath))
                    {
                        sw.Write(dataJsonString);
                    }
                    #endregion
                    debugMessage = string.Format("{0}將 {1} 資料轉換成json成功\n", debugMessage, filePath);
                    fileListMessage = string.Format("{0}{1}：O\n", fileListMessage, dci.FileName);
                    ++successFileCount;
                }
                else
                {
                    debugMessage = string.Format("{0}取得{1}內資料失敗\n", debugMessage, dci.FileName);
                    fileListMessage = string.Format("{0}{1}：X\n", fileListMessage, dci.FileName);
                }
            }
            debugMessage = string.Format("{0}共轉換 {1}個檔案成功，{2}個檔案失敗\n", debugMessage, successFileCount, GlobalConst.DataConvertList.Length - successFileCount);
        }
    }
}
