using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace Frends.FixedWidthFlatFile.ConvertToXML.Tests
{

    [TestFixture]
    public class Tests
    {
        private List<Dictionary<string, dynamic?>> _testCases;
        private string _testXML;

        [SetUp]
        public void SetUp()
        {
            _testCases = new List<Dictionary<string, dynamic?>>();
            Dictionary<string, dynamic?> testCase = new Dictionary<string, dynamic?>();
            var timestamp = DateTime.Now;

            testCase.Add("Name", "");
            testCase.Add("Content", "This is a test data");
            testCase.Add("Timestamp", null);
            _testCases.Add(testCase);

            using (var ms = new MemoryStream())
            {
                using (var writer = new XmlTextWriter(ms, new UTF8Encoding(false)) { Formatting = Formatting.Indented })
                {
                    writer.WriteStartDocument(); // start doc
                    writer.WriteStartElement("Root");
                    writer.WriteStartElement("Rows");
                    writer.WriteStartElement("Row");
                    writer.WriteElementString("Name", "");
                    writer.WriteElementString("Content", "This is a test data");
                    writer.WriteElementString("Timestamp", null);
                    writer.WriteEndElement(); // end Row
                    writer.WriteEndElement(); // end Rows
                    writer.WriteEndElement(); // end Root
                    writer.WriteEndDocument(); // end doc
                }
                _testXML = Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        [Test]
        public void testParseXML() {
            var result = FixedWidthFlatFile.ConvertToXML(new Definitions.Input { FileContent = _testCases }, new System.Threading.CancellationToken());
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(_testXML, result.Data);
        }

        [Test]
        public void testParseXML_content()
        {
            var result = FixedWidthFlatFile.ConvertToXML(new Definitions.Input { FileContent = _testCases }, new System.Threading.CancellationToken());

            XmlDocument xmlResult = new XmlDocument();
            XmlDocument xmlTest = new XmlDocument();

            xmlResult.LoadXml(result.Data);
            xmlTest.LoadXml(_testXML);

            Assert.IsNotNull(xmlResult);
            Assert.AreEqual(xmlTest.GetElementsByTagName("Name"), xmlResult.GetElementsByTagName("Name"));
            Assert.IsNotNull(xmlResult.GetElementsByTagName("Name"));
            Assert.AreEqual(xmlTest.GetElementsByTagName("Content"), xmlResult.GetElementsByTagName("Content"));
            Assert.AreEqual(xmlTest.GetElementsByTagName("Timestamp"), xmlResult.GetElementsByTagName("Timestamp"));
            Assert.IsNotNull(xmlResult.GetElementsByTagName("Timestamp"));
        }

        [Test]
        public void testParseXML_throws_emptyParameter()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var result = FixedWidthFlatFile.ConvertToXML(new Definitions.Input { FileContent = null }, new System.Threading.CancellationToken());
            });
        }
    }
}
 