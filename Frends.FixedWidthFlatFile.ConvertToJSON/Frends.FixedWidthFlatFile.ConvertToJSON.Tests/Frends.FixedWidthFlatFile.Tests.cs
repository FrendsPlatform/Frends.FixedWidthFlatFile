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
        var result = FixedWidthFlatFile.ConvertToJSON(new Definitions.Input { FileContent = _testCases, culture = null }, DefaultOptions(), new System.Threading.CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.IsTrue(!string.IsNullOrEmpty(result.Data));
        Assert.AreEqual(JsonSerializer.Serialize(_testJsons), result.Data);
    }

    [Test]
    public void testConvertToJSONWithCulture()
    {
        var result = FixedWidthFlatFile.ConvertToJSON(new Definitions.Input { FileContent = _testCases, culture = "fi-EN" }, DefaultOptions(), new System.Threading.CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.IsTrue(!string.IsNullOrEmpty(result.Data));
        Assert.AreEqual(JsonSerializer.Serialize(_testJsons), result.Data);
    }

    /// <summary>
    /// Test ParseJSON() -method from FixedWidthFlatFile -class. Deserializables the object and compares values are right.
    /// </summary>
    [Test]
    public void testParseJSON_deserialize()
    {
        var result = FixedWidthFlatFile.ConvertToJSON(new Definitions.Input { FileContent = _testCases, culture = null }, DefaultOptions(), new System.Threading.CancellationToken());
        var deserialized = JsonSerializer.Deserialize<List<Json>>(result.Data);
        Assert.IsTrue(deserialized != null);
        Assert.AreEqual(_testJsons[0].Name, deserialized[0].Name);
        Assert.AreEqual(_testJsons[0].Content, deserialized[0].Content);
        Assert.AreEqual(_testJsons[0].Timestamp, deserialized[0].Timestamp);
    }

    /// <summary>
    /// Test ParseJSON() -method from FixedWidthFlatFile -class. Throws will be thrown if fileContnet -parameter is empty.
    /// </summary>
    [Test]
    public void testParseJSON_throws_emptyParameter()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            var result = FixedWidthFlatFile.ConvertToJSON(new Definitions.Input { FileContent = null, culture = null }, DefaultOptions(), new System.Threading.CancellationToken());
        });
    }

    /// <summary>
    /// Test that a failed Result is returned instead of throwing when ThrowErrorOnFailure is false.
    /// </summary>
    [Test]
    public void testParseJSON_returnsFailedResult_whenThrowErrorOnFailureIsFalse()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        var result = FixedWidthFlatFile.ConvertToJSON(new Definitions.Input { FileContent = null, culture = null }, options, new System.Threading.CancellationToken());
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
    }

    /// <summary>
    /// Test that the custom ErrorMessageOnFailure is used when the Task throws.
    /// </summary>
    [Test]
    public void testParseJSON_usesCustomErrorMessageOnFailure()
    {
        const string customErrorMessage = "CustomErrorMessage";
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = customErrorMessage;
        var ex = Assert.Throws<Exception>(() =>
        {
            FixedWidthFlatFile.ConvertToJSON(new Definitions.Input { FileContent = null, culture = null }, options, new System.Threading.CancellationToken());
        });
        Assert.IsNotNull(ex);
        Assert.That(ex.Message, Contains.Substring(customErrorMessage));
    }

    private static Definitions.Options DefaultOptions()
    {
        return new Definitions.Options();
    }
}

#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

