using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.Json;

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.


namespace Frends.FixedWidthFlatFile.ConvertToJSON.Tests;

[TestFixture]
public class Tests
{
    private List<Dictionary<string, dynamic?>> _testCases;
    private List<Json> _testJsons;

    #region Test helper classes
    private class Json
    {
        public string Name { get; set; }
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
    #endregion

    [SetUp]
    public void SetUp()
    {
        _testCases = new List<Dictionary<string, dynamic?>>();
        _testJsons = new List<Json>();
        Dictionary<string, dynamic?> testCase = new Dictionary<string, dynamic?>();
        var timestamp = DateTime.Now;

        testCase.Add("Name", "Test");
        testCase.Add("Content", "This is a test data");
        testCase.Add("Timestamp", timestamp);
        _testCases.Add(testCase);

        Json testJson = new()
        {
            Name = "Test",
            Content = "This is a test data",
            Timestamp = timestamp,
        };

        _testJsons.Add(testJson);
    }

    /// <summary>
    /// Test ParseJSON() -method from FixedWidthFlatFile -class.
    /// </summary>
    [Test]
    public void testParseJSON()
    {
        var result = FixedWidthFlatFile.ConvertToJSON(new Definitions.Input { FileContent = _testCases, Culture = null }, DefaultOptions(), new System.Threading.CancellationToken());
        Assert.That(result.Success);
        Assert.That(!string.IsNullOrEmpty(result.Data));
        Assert.That(JsonSerializer.Serialize(_testJsons), Is.EqualTo(result.Data));
    }

    [Test]
    public void testConvertToJSONWithCulture()
    {
        var result = FixedWidthFlatFile.ConvertToJSON(new Definitions.Input { FileContent = _testCases, Culture = "fi-EN" }, DefaultOptions(), new System.Threading.CancellationToken());
        Assert.That(result.Success);
        Assert.That(!string.IsNullOrEmpty(result.Data));
        Assert.That(JsonSerializer.Serialize(_testJsons), Is.EqualTo(result.Data));
    }

    /// <summary>
    /// Test ParseJSON() -method from FixedWidthFlatFile -class. Deserializables the object and compares values are right.
    /// </summary>
    [Test]
    public void testParseJSON_deserialize()
    {
        var result = FixedWidthFlatFile.ConvertToJSON(new Definitions.Input { FileContent = _testCases, Culture = null }, DefaultOptions(), new System.Threading.CancellationToken());
        var deserialized = JsonSerializer.Deserialize<List<Json>>(result.Data);
        Assert.That(deserialized != null);
        Assert.That(_testJsons[0].Name, Is.EqualTo(deserialized[0].Name));
        Assert.That(_testJsons[0].Content, Is.EqualTo(deserialized[0].Content));
        Assert.That(_testJsons[0].Timestamp, Is.EqualTo(deserialized[0].Timestamp));
    }

    private static Definitions.Options DefaultOptions()
    {
        return new Definitions.Options();
    }
}

#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

