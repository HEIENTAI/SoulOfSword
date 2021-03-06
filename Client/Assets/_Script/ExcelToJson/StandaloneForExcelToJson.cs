﻿using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 此 scirpt 會被編譯為 pc standalone 版本執行檔
/// 將Excel檔案轉換成json檔案的工具
/// </summary>
public class StandaloneForExcelToJson : MonoBehaviour 
{
    readonly string EXCEL_DIRECTORY = "EXCEL"; // excel檔案所在的資料夾
    readonly string JSON_DIRECTORY = "JSON";   // json檔案存放資料夾
    readonly string JSON_EXT = ".json";       // json檔案副檔名

    GUIStyle _uiStyle;
    Rect _fileListWindowRect;
    private Vector2 _fileListWindowScrollPosition;
    Rect _debugMessageWindowRect;
    private Vector2 _debugMessageWindowScrollPosition;
    string _debugMessage = string.Empty;
    string _fileListMessage = string.Empty;

    bool _currentlyTransfering = false; // 是否正在轉換中

    ExcelToJsonString excelToJsonString;
    
    void Awake()
    {
        _uiStyle = new GUIStyle();
        _uiStyle.fontSize = 14;
        _uiStyle.normal.textColor = Color.white;
        if (Camera.main != null)
            Camera.main.backgroundColor = Color.black;
    }


	// Use this for initialization
	void Start () 
    {
        excelToJsonString = new ExcelToJsonString();
        _fileListWindowRect = new Rect(0, 70, 150, Screen.height - 70);
        _debugMessageWindowRect = new Rect(170, 70, Screen.width - 170, Screen.height - 70);	
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
                if (GUI.Button(new Rect(10, 40, 100, 20), "Excel -> Json"))
                {
                    _currentlyTransfering = true;
                    _debugMessage = "";
                    _fileListMessage = "";
                    TransferFilesFromExcelToJson();
                }
            }
            catch (Exception e)
            {
                _currentlyTransfering = false;
                _debugMessage = _debugMessage + e.StackTrace + "\n" + e.Message;
            }
        }
        _fileListWindowRect = GUI.Window(0, _fileListWindowRect, FileListMessageWindow, "File List");
        _debugMessageWindowRect = GUI.Window(1, _debugMessageWindowRect, TransferMessageWindow, "Debug Window");
    }
    #region 訊息視窗
    /// <summary>
    /// 除錯訊息視窗
    /// </summary>
    /// <param name="windowID"></param>
    public void FileListMessageWindow(int windowID)
    {
        _fileListWindowScrollPosition = GUILayout.BeginScrollView(_fileListWindowScrollPosition); // 加入捲軸
        GUILayout.TextArea(_fileListMessage, _uiStyle, GUILayout.ExpandHeight(true)); // 自動伸縮捲軸
        GUILayout.EndScrollView();
        GUI.DragWindow();
    }

    /// <summary>
    /// 轉換訊息視窗
    /// </summary>
    public void TransferMessageWindow(int windowID)
    {
        _debugMessageWindowScrollPosition = GUILayout.BeginScrollView(_debugMessageWindowScrollPosition); // 加入捲軸
        GUILayout.TextArea(_debugMessage, _uiStyle, GUILayout.ExpandHeight(true)); // 自動伸縮捲軸
        GUILayout.EndScrollView();
        GUI.DragWindow();
    }
    #endregion

    void TransferFilesFromExcelToJson()
    {
        string jsonDirectoryPath = Application.dataPath + Path.DirectorySeparatorChar + JSON_DIRECTORY;
        string excelDirectoryPath = Application.dataPath + Path.DirectorySeparatorChar + EXCEL_DIRECTORY;
        if (!Directory.Exists(jsonDirectoryPath)) // 如果資料夾不存在
        {
            Directory.CreateDirectory(jsonDirectoryPath); // 建立目錄
        }
        int successFileCount = 0;

        Array dataLoadTags = Enum.GetValues(typeof(GlobalConst.DataLoadTag));
        foreach (GlobalConst.DataLoadTag dataLoadTag in dataLoadTags)
        {
            string dataJsonString;
            string tempDebugMsg;
            EnumClassValue dataConvertInfo;
            bool isSuccessGetAttr = CommonFunction.GetAttribute<EnumClassValue>(dataLoadTag, out dataConvertInfo);
            if (!isSuccessGetAttr) { continue; }
            string fileName = dataConvertInfo.FileName;
            System.Type dataType = dataConvertInfo.DataType;

            ReadExcelToJsonStringError error = excelToJsonString.ReadExcelFile(excelDirectoryPath, dataConvertInfo, NeedReadSite.CLIENT, out dataJsonString, out tempDebugMsg);
            _debugMessage += tempDebugMsg;
            if (error == ReadExcelToJsonStringError.NONE)
            {
                string filePath = jsonDirectoryPath + Path.DirectorySeparatorChar + fileName + JSON_EXT;
                WriteJsonStringToFile(dataJsonString, filePath);

                _debugMessage = string.Format("{0}將 {1} 資料轉換成json成功\n", _debugMessage, excelDirectoryPath + Path.DirectorySeparatorChar + fileName + ".xlsx");
                _fileListMessage = string.Format("{0}{1}：O\n", _fileListMessage, fileName);
                ++successFileCount;
            }
            else
            {
                string excelFilePath = excelDirectoryPath + Path.DirectorySeparatorChar + fileName + ".xlsx";
                _debugMessage = string.Format("{0}取得{1}內資料(型別為{2})失敗：失敗原因：{3}\n", _debugMessage, excelFilePath, dataType, error);
                _fileListMessage = string.Format("{0}{1}：X\n", _fileListMessage, fileName);
            }            
        }
        _debugMessage = string.Format("{0}共轉換 {1}個檔案成功，{2}個檔案失敗\n", _debugMessage, successFileCount, dataLoadTags.Length - successFileCount);
        _currentlyTransfering = false;
    }

    void WriteJsonStringToFile(string jsonString, string filePath)
    {
        using (StreamWriter sw = new StreamWriter(filePath))
        {
            sw.Write(jsonString);
        }
    }
}
