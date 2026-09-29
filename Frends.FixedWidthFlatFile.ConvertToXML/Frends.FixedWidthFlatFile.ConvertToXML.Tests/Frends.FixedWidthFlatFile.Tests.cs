using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace Frends.FixedWidthFlatFile.ConvertToXML.Tests;

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

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
    public void testParseXML()
    {
        var result = FixedWidthFlatFile.ConvertToXML(new Definitions.Input { FileContent = _testCases }, new Definitions.Options(), new System.Threading.CancellationToken());
        Assert.That(result.Success);
        Assert.That(!string.IsNullOrEmpty(result.Data));
        Assert.That(_testXML, Is.EqualTo(result.Data));
    }

    [Test]
    public void testParseXML_content()
    {
        var result = FixedWidthFlatFile.ConvertToXML(new Definitions.Input { FileContent = _testCases }, new Definitions.Options(), new System.Threading.CancellationToken());

        XmlDocument xmlResult = new XmlDocument();
        XmlDocument xmlTest = new XmlDocument();

        xmlResult.LoadXml(result.Data);
        xmlTest.LoadXml(_testXML);

        Assert.That(xmlResult, Is.Not.Null);
        Assert.That(xmlTest.GetElementsByTagName("Name"), Is.EqualTo(xmlResult.GetElementsByTagName("Name")));
        Assert.That(xmlResult.GetElementsByTagName("Name"), Is.Not.Null);
        Assert.That(xmlTest.GetElementsByTagName("Content"), Is.EqualTo(xmlResult.GetElementsByTagName("Content")));
        Assert.That(xmlTest.GetElementsByTagName("Timestamp"), Is.EqualTo(xmlResult.GetElementsByTagName("Timestamp")));
        Assert.That(xmlResult.GetElementsByTagName("Timestamp"), Is.Not.Null);
    }
}

#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
