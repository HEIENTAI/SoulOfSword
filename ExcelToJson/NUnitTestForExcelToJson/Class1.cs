using System;
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
            TableToJsonString temp = new TableToJsonString();
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
            TableToJsonString temp = new TableToJsonString();
            string jsonStr = temp.ObjectToJsonString(ecd);
            string realJsonStr = "{\"CheckType\":0,\"CheckCondition1\":2,\"CheckCondition2\":null}";

            Assert.AreEqual(realJsonStr, jsonStr);
        }
    }

    [TestFixture]
    public class TestForExcelToJson
    {
        private ExcelToTable ett;
        private List<string> allType;
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
            ReadExcelError ree = ett.OpenExcelFile("EventData");
            Assert.AreEqual(ReadExcelError.NONE, ree);
            List<string> ans = new List<string>()
            {
                "#", null, null, null, null,
                null, null, null, null, null,
                null, null, null, null, null,
                null, null, null, null, null,
                null, null, null, null, null,
                null, null, null, null, null,
                null, null, null, null, null,
                null, null, null, null, null
            };
            Assert.AreEqual(ans, ett.GetNextRow());
            ans = new List<string>()
            {
                "TestStr", "TstByte", "TestUINT", "testnon",  "事件ID",
                "子事件ID","檢查1類型","檢查1條件1","檢查1條件2","檢查2類型",
                "檢查2條件1","檢查2條件2","檢查3類型","檢查3條件1","檢查3條件2",
                "正效果1類型","正效果1欄位1","正效果1欄位2","正效果1欄位3","正效果2類型",
                "正效果2欄位1","正效果2欄位2","正效果2欄位3","正效果3類型","正效果3欄位1",
                "正效果3欄位2","正效果3欄位3","反效果1類型","反效果1欄位1","反效果1欄位2",
                "反效果1欄位3","反效果2類型","反效果2欄位1","反效果2欄位2","反效果2欄位3",
                "反效果3類型","反效果3欄位1","反效果3欄位2","反效果3欄位3","EOC"
            };
            Assert.AreEqual(ans, ett.GetNextRow());
        }

        [Test]
        [Category("Test Read Excel File")]
        public void TestReadRealFile()
        {
            ReadExcelError ree = ett.OpenExcelFile("EventData");
            Assert.AreEqual(ReadExcelError.NONE, ree);
            ree = ett.CheckAndReadTableHeader(NeedReadSite.CLIENT, out allType);
            Assert.AreEqual(ReadExcelError.NONE, ree);
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
            ReadExcelError ree = ett.OpenExcelFile("EventData1");
            Assert.AreEqual(ReadExcelError.NONE, ree);
            ree = ett.CheckAndReadTableHeader(NeedReadSite.CLIENT, out allType);
            Assert.AreEqual(ReadExcelError.CANT_FIND_START_TOKEN, ree);
        }

        [Test]
        [Category("Test Read Excel File")]
        public void TestReadNotStartTokenFile()
        {
            // 檢測無start token
            ReadExcelError ree = ett.OpenExcelFile("EventData2");
            Assert.AreEqual(ReadExcelError.NONE, ree);
            ree = ett.CheckAndReadTableHeader(NeedReadSite.CLIENT, out allType);
            Assert.AreEqual(ReadExcelError.CANT_FIND_START_TOKEN, ree);
        }
        [Test]
        [Category("Test Read Excel File")]
        public void TestReadNotEndOfColTokenFile()
        {
            // 檢測無End of Column token
            ReadExcelError ree = ett.OpenExcelFile("EventData3");
            Assert.AreEqual(ReadExcelError.NONE, ree);
            ree = ett.CheckAndReadTableHeader(NeedReadSite.CLIENT, out allType);
            Assert.AreEqual(ReadExcelError.CANT_FIND_END_OF_COL_TOKEN, ree);
        }

        [Test]
        [Category("Test Read Excel File")]
        public void TestReadColNumIsZeroFile()
        {
            ReadExcelError ree = ett.OpenExcelFile("EventData_TABEL_COL_NUM_IS_ZERO");
            Assert.AreEqual(ReadExcelError.NONE, ree);
            ree = ett.CheckAndReadTableHeader(NeedReadSite.CLIENT, out allType);
            Assert.AreEqual(ReadExcelError.TABEL_COL_NUM_IS_ZERO, ree);
        }

        [Test]
        [Category("Test Read Excel File")]
        public void TestReadEndOfRowIsEarlyInColumnCountFile()
        {
            ReadExcelError ree = ett.OpenExcelFile("EventData_TABEL_END_OF_ROW_IS_EARLY_IN_COLUMN_COUNT");
            Assert.AreEqual(ReadExcelError.NONE, ree);
            ree = ett.CheckAndReadTableHeader(NeedReadSite.CLIENT, out allType);
            Assert.AreEqual(ReadExcelError.END_OF_ROW_TOKEN_TO_EARLY, ree);
        }
    }
}
