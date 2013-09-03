using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Excel;


namespace ExcelToJson
{
    class Program
    {
        readonly static string JSON_DIRECTORY = "JSON";
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
            string directoryPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + JSON_DIRECTORY;
            if (!Directory.Exists(directoryPath)) // 如果資料夾不存在
            {
                Directory.CreateDirectory(directoryPath); // 建立目錄
            }

            TableToJsonString tableToJson = new TableToJsonString();
            List<object> allData;
            int successFileCount = 0;

            foreach (DataConvertInfomation dci in GlobalConst.DataConvertList)
            {
                bool isSuccess = tableToJson.ReadExcelFile(dci, out allData, out debugMessage);
                if (isSuccess)
                {
                    string dataJsonString = tableToJson.ObjectToJsonString(allData);
                    #region JsonString To File
                    string filePath = directoryPath + Path.DirectorySeparatorChar + dci.FileName + ".json";
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
