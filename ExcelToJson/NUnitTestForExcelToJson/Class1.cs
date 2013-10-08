using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ExcelToJson;
namespace NUnitTestForExcelToJson
{
    [TestFixture]
    public class TestExample
    {
        [Test]
        public void PureTest()
        {
            int a = 1;
            int b = 2;
            int sum = a + b;
            Assert.AreEqual(sum, 3);
        }
    }

    [TestFixture]
    public class TestJsonString
    {
        [Test]
        [Category("Test Object To Json String")]
        public void TestJsonStringNotEmptyField()
        {
            EventConditionData ecd = new EventConditionData();
            ecd.CheckCondition1 = 2;
            ecd.CheckCondition2 = 1;
            ecd.CheckType = 0;
            ExcelToJsonString temp = new ExcelToJsonString();
            string jsonStr = temp.ObjectToJsonString(ecd);
            string realJsonStr = "{\"CheckType\":0,\"CheckCondition1\":2,\"CheckCondition2\":1}";

            Assert.AreEqual(realJsonStr, jsonStr);
        }

        [Test]
        [Category("Test Object To Json String")]
        public void TestJsonStringHasEmptyField()
        {
            EventConditionData ecd = new EventConditionData();
            ecd.CheckCondition1 = 2;
            ecd.CheckType = 0;
            ExcelToJsonString temp = new ExcelToJsonString();
            string jsonStr = temp.ObjectToJsonString(ecd);
            string realJsonStr = "{\"CheckType\":0,\"CheckCondition1\":2,\"CheckCondition2\":null}";

            Assert.AreEqual(realJsonStr, jsonStr);
        }
    }

    [TestFixture]
    public class TestForExcelToTable
    {
        private ExcelToTable ett;
        private List<string> allType;
        private static readonly string EXCEL_DIRECTORY = "EXCEL";
        private static readonly string exceDirectorylPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + EXCEL_DIRECTORY;
        [SetUp]
        public void SetUp()
        {
            ett = new ExcelToTable();
        }

        [TearDown]
        public void TearDown()
        {
            ett.Close();
            ett = null;
            allType = null;
        }

        [Test]
        public void TestEndOfTableTest()
        {
            bool isEndOfTable = ett.CheckEndOfTable(new List<string>() { "EOR", "", "" });
            Assert.AreEqual(true, isEndOfTable);
            isEndOfTable = ett.CheckEndOfTable(new List<string>(){"dsfd", "werjwepo", "122", "", null});
            Assert.AreEqual(false, isEndOfTable);
        }

        [Test]
        [Category("Test Read Excel File")]
        public void TestReadExcelFile1()
        {
            ReadExcelToJsonStringError ree = ett.OpenExcelFile(exceDirectorylPath, "EventData");
            Assert.AreEqual(ReadExcelToJsonStringError.NONE, ree);
            List<string> ans = new List<string>()
            {
                "#",  null, null, null, null,
                null, null, null, null, null,
                null, null, null, null, null,
                null, null, null, null, null,
                null, null, null, null, null,
                null, null, null, null, null,
                null, null, null, null, null,
                null, null, null, null, "EOC"
            };
            Assert.AreEqual(ans, ett.GetNextRow());
        }

        [Test]
        [Category("Test Read Excel File")]
        public void TestReadRealFile()
        {
            ReadExcelToJsonStringError ree = ett.OpenExcelFile(exceDirectorylPath, "EventData");
            Assert.AreEqual(ReadExcelToJsonStringError.NONE, ree);
            ree = ett.CheckAndReadTableHeader(NeedReadSite.CLIENT, out allType);
            Assert.AreEqual(ReadExcelToJsonStringError.NONE, ree);
            List<string> realAllType = new List<string>()
            {
                "string", "byte", "uint", "ushort",	"ushort",
                "ushort", "ushort", "ushort", "ushort", "ushort",
                "ushort", "ushort", "ushort", "ushort", "ushort",
                "ushort", "ushort", "ushort", "ushort", "ushort",
                "ushort", "ushort", "ushort", "ushort", "ushort",
                "ushort", "ushort", "ushort", "ushort", "ushort",
                "ushort", "ushort", "ushort", "ushort", "ushort",
                "ushort", "ushort", "ushort"
            };
            Assert.AreEqual(realAllType, allType);
        }

        [Test]
        [Category("Test Read Excel File")]
        public void TestReadEmptyFile()
        {
            // 檢測空白檔案
            ReadExcelToJsonStringError ree = ett.OpenExcelFile(exceDirectorylPath, "EventData1");
            Assert.AreEqual(ReadExcelToJsonStringError.NONE, ree);
            ree = ett.CheckAndReadTableHeader(NeedReadSite.CLIENT, out allType);
            Assert.AreEqual(ReadExcelToJsonStringError.CANT_FIND_START_TOKEN, ree);
        }

        [Test]
        [Category("Test Read Excel File")]
        public void TestReadNotStartTokenFile()
        {
            // 檢測無start token
            ReadExcelToJsonStringError ree = ett.OpenExcelFile(exceDirectorylPath, "EventData2");
            Assert.AreEqual(ReadExcelToJsonStringError.NONE, ree);
            ree = ett.CheckAndReadTableHeader(NeedReadSite.CLIENT, out allType);
            Assert.AreEqual(ReadExcelToJsonStringError.CANT_FIND_START_TOKEN, ree);
        }
        [Test]
        [Category("Test Read Excel File")]
        public void TestReadNotEndOfColTokenFile()
        {
            // 檢測無End of Column token
            ReadExcelToJsonStringError ree = ett.OpenExcelFile(exceDirectorylPath, "EventData3");
            Assert.AreEqual(ReadExcelToJsonStringError.NONE, ree);
            ree = ett.CheckAndReadTableHeader(NeedReadSite.CLIENT, out allType);
            Assert.AreEqual(ReadExcelToJsonStringError.CANT_FIND_END_OF_COL_TOKEN, ree);
        }

        [Test]
        [Category("Test Read Excel File")]
        public void TestReadColNumIsZeroFile()
        {
            ReadExcelToJsonStringError ree = ett.OpenExcelFile(exceDirectorylPath, "EventData_TABEL_COL_NUM_IS_ZERO");
            Assert.AreEqual(ReadExcelToJsonStringError.NONE, ree);
            ree = ett.CheckAndReadTableHeader(NeedReadSite.CLIENT, out allType);
            Assert.AreEqual(ReadExcelToJsonStringError.CANT_FIND_END_OF_COL_TOKEN, ree);
        }

        [Test]
        [Category("Test Read Excel File")]
        public void TestReadEndOfRowIsEarlyInColumnCountFile()
        {
            ReadExcelToJsonStringError ree = ett.OpenExcelFile(exceDirectorylPath, "EventData_TABEL_END_OF_ROW_IS_EARLY_IN_COLUMN_COUNT");
            Assert.AreEqual(ReadExcelToJsonStringError.NONE, ree);
            ree = ett.CheckAndReadTableHeader(NeedReadSite.CLIENT, out allType);
            Assert.AreEqual(ReadExcelToJsonStringError.CANT_FIND_END_OF_COL_TOKEN, ree);
        }

        [Test]
        [Category("Test Read Excel File")]
        public void TestReadStartTokenNotFirstRow()
        {
            ReadExcelToJsonStringError ree = ett.OpenExcelFile(exceDirectorylPath, "EventData_StartTokenNotFirstRow");
            Assert.AreEqual(ReadExcelToJsonStringError.NONE, ree);
            ree = ett.CheckAndReadTableHeader(NeedReadSite.CLIENT, out allType);
            Assert.AreEqual(ReadExcelToJsonStringError.NONE, ree);
        }
    }

    [TestFixture]
    public class TestForExcelToJsonString
    {
        private ExcelToJsonString _excelToJsonString;
        private static readonly string EXCEL_DIRECTORY = "EXCEL";
        private static readonly string exceDirectorylPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + EXCEL_DIRECTORY;

        [SetUp]
        public void SetUp()
        {
            _excelToJsonString = new ExcelToJsonString();
        }
        [TearDown]
        public void TearDown()
        {
            _excelToJsonString = null;
        }

        [Test]
        [Category("Test Read Excel File And Transfer Data")]
        public void TestReadStartTokenNotFirstRowTableToJsonString()
        {
            string jsonString = string.Empty;
            string debugString = string.Empty;
            Console.WriteLine(string.Format("path = {0}", exceDirectorylPath));
            EnumClassValue test = new EnumClassValue(typeof(EventData), "EventData_StartTokenNotFirstRow");
            ReadExcelToJsonStringError error = _excelToJsonString.ReadExcelFile(exceDirectorylPath, test, NeedReadSite.CLIENT, out jsonString, out debugString);
            Assert.AreEqual(ReadExcelToJsonStringError.NONE, error);
        }

        //[Test]
        //[Category("Test Read Excel File And Transfer Data")]
        //public void TestReadFileUseTableToJsonString()
        //{
        //    string jsonString = string.Empty;
        //    string debugString = string.Empty;
        //    Console.WriteLine(string.Format("path = {0}", exceDirectorylPath));
        //    ReadExcelToJsonStringError error = _excelToJsonString.ReadExcelFile(exceDirectorylPath, GlobalConst.DataLoadTag.Event, NeedReadSite.CLIENT, out jsonString, out debugString);
        //    Assert.AreEqual(ReadExcelToJsonStringError.NONE, error);
        //    List<EventData> ans = new List<EventData>(2);

        //    EventData temp1 = new EventData();
        //    temp1.testStr = "123";
        //    temp1.testByte = 1;
        //    temp1.testUINT = 5;
        //    temp1.EventMainID = 10;
        //    temp1.EventSubID = null;
        //    EventConditionData tempCD = new EventConditionData();
        //    tempCD.CheckType = 14;
        //    tempCD.CheckCondition1 = 16;
        //    tempCD.CheckCondition2 = 18;
        //    temp1.EventCheckCondition[0] = tempCD;
        //    tempCD = new EventConditionData();
        //    tempCD.CheckType = 20;
        //    tempCD.CheckCondition1 = 22;
        //    tempCD.CheckCondition2 = 24;
        //    temp1.EventCheckCondition[1] = tempCD;
        //    tempCD = new EventConditionData();
        //    tempCD.CheckType = 26;
        //    tempCD.CheckCondition1 = 28;
        //    tempCD.CheckCondition2 = 30;
        //    temp1.EventCheckCondition[2] = tempCD;
        //    EventEffectData tempED = new EventEffectData();
        //    tempED.EffectType = 32;
        //    tempED.EffectParameter[0] = 34;
        //    tempED.EffectParameter[1] = 36;
        //    tempED.EffectParameter[2] = 38;
        //    temp1.TrueEffect[0] = tempED;
        //    tempED = new EventEffectData();
        //    tempED.EffectType = 40;
        //    tempED.EffectParameter[0] = 42;
        //    tempED.EffectParameter[1] = 44;
        //    tempED.EffectParameter[2] = 46;
        //    temp1.TrueEffect[1] = tempED;
        //    tempED = new EventEffectData();
        //    tempED.EffectType = 48;
        //    tempED.EffectParameter[0] = 50;
        //    tempED.EffectParameter[1] = 52;
        //    tempED.EffectParameter[2] = 54;
        //    temp1.TrueEffect[2] = tempED;
        //    tempED = new EventEffectData();
        //    tempED.EffectType = 56;
        //    tempED.EffectParameter[0] = 58;
        //    tempED.EffectParameter[1] = 60;
        //    tempED.EffectParameter[2] = 62;
        //    temp1.FalseEffect[0] = tempED;
        //    tempED = new EventEffectData();
        //    tempED.EffectType = 64;
        //    tempED.EffectParameter[0] = 66;
        //    tempED.EffectParameter[1] = 68;
        //    tempED.EffectParameter[2] = 70;
        //    temp1.FalseEffect[1] = tempED;
        //    tempED = new EventEffectData();
        //    tempED.EffectType = 72;
        //    tempED.EffectParameter[0] = 74;
        //    tempED.EffectParameter[1] = 76;
        //    tempED.EffectParameter[2] = 78;
        //    temp1.FalseEffect[2] = tempED;
        //    ans.Add(temp1);
        //    temp1 = new EventData();
        //    temp1.testStr = "a23";
        //    temp1.testByte = 1;
        //    temp1.testUINT = null;
        //    temp1.EventMainID = 8;
        //    temp1.EventSubID = 9;
        //    tempCD = new EventConditionData();
        //    tempCD.CheckType = null;
        //    tempCD.CheckCondition1 = null;
        //    tempCD.CheckCondition2 = 123;
        //    temp1.EventCheckCondition[0] = tempCD;
        //    tempCD = new EventConditionData();
        //    tempCD.CheckType = null;
        //    tempCD.CheckCondition1 = null;
        //    tempCD.CheckCondition2 = 50;
        //    temp1.EventCheckCondition[1] = tempCD;
        //    tempCD = new EventConditionData();
        //    tempCD.CheckType = null;
        //    tempCD.CheckCondition1 = null;
        //    tempCD.CheckCondition2 = 98;
        //    temp1.EventCheckCondition[2] = tempCD;
        //    tempED = new EventEffectData();
        //    tempED.EffectType = null;
        //    tempED.EffectParameter[0] = null;
        //    tempED.EffectParameter[1] = 31;
        //    tempED.EffectParameter[2] = null;
        //    temp1.TrueEffect[0] = tempED;
        //    tempED = new EventEffectData();
        //    tempED.EffectType = null;
        //    tempED.EffectParameter[0] = 56465;
        //    tempED.EffectParameter[1] = null;
        //    tempED.EffectParameter[2] = null;
        //    temp1.TrueEffect[1] = tempED;
        //    tempED = new EventEffectData();
        //    tempED.EffectType = 12;
        //    tempED.EffectParameter[0] = null;
        //    tempED.EffectParameter[1] = null;
        //    tempED.EffectParameter[2] = 50;
        //    temp1.TrueEffect[2] = tempED;
        //    tempED = new EventEffectData();
        //    tempED.EffectType = null;
        //    tempED.EffectParameter[0] = null;
        //    tempED.EffectParameter[1] = 1254;
        //    tempED.EffectParameter[2] = null;
        //    temp1.FalseEffect[0] = tempED;
        //    //tempED = new EventEffectData();
        //    //tempED.EffectType = null;
        //    //tempED.EffectParameter[0] = null;
        //    //tempED.EffectParameter[1] = null;
        //    //tempED.EffectParameter[2] = null;
        //    temp1.FalseEffect[1] = null;
        //    tempED = new EventEffectData();
        //    tempED.EffectType = 66;
        //    tempED.EffectParameter[0] = null;
        //    tempED.EffectParameter[1] = null;
        //    tempED.EffectParameter[2] = 77;
        //    temp1.FalseEffect[2] = tempED;
        //    ans.Add(temp1);
        //    Assert.AreEqual(_excelToJsonString.ObjectToJsonString(ans), jsonString);
        //    DataConvertInfomation dci = new DataConvertInfomation(typeof(EventData), "EventData_TypeError");            
        //    error = _excelToJsonString.ReadExcelFile(exceDirectorylPath, dci, NeedReadSite.CLIENT, out jsonString, out debugString);
        //    Assert.AreEqual(ReadExcelToJsonStringError.TABLE_TYPE_IS_NOT_CONFORM, error);
        //}

        // 要測試讀取string array
        [Test]
        [Category("Test Read Excel File And Transfer Data")]
        public void TestReadStringArray()
        {
            string jsonString = string.Empty;
            string debugString = string.Empty;
            EnumClassValue test = new EnumClassValue(typeof(TestStringArray), "EventData_TestStringArray");
            ReadExcelToJsonStringError error = _excelToJsonString.ReadExcelFile(exceDirectorylPath, test, NeedReadSite.CLIENT, out jsonString, out debugString);
            Assert.AreEqual(ReadExcelToJsonStringError.NONE, error);
        }
    }
}
