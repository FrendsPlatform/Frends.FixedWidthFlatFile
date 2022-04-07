using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Frends.FixedWidthFlatFile.ConvertToJSON.Tests
{

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
        public void SetUp() { 
            _testCases = new List<Dictionary<string, dynamic?>>();
            _testJsons = new List<Json>();
            Dictionary<string, dynamic?> testCase = new Dictionary<string, dynamic?>();
            var timestamp = DateTime.Now;

            testCase.Add("Name", "Test");
            testCase.Add("Content", "This is a test data");
            testCase.Add("Timestamp", timestamp);
            _testCases.Add(testCase);

            Json testJson = new Json
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
        public void testParseJSON() {
            var result = FixedWidthFlatFile.ConvertToJSON(new Definitions.Input { FileContent = _testCases, culture = null }, new System.Threading.CancellationToken());
            Assert.IsTrue(!string.IsNullOrEmpty(result.Data));
            Assert.AreEqual(JsonSerializer.Serialize(_testJsons), result.Data);
        }

        /// <summary>
        /// Test ParseJSON() -method from FixedWidthFlatFile -class. Deserializables the object and compares values are right.
        /// </summary>
        [Test]
        public void testParseJSON_deserialize() {
            var result = FixedWidthFlatFile.ConvertToJSON(new Definitions.Input { FileContent = _testCases, culture = null }, new System.Threading.CancellationToken());
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
        public void testParseJSON_throws_emptyParameter() {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var result = FixedWidthFlatFile.ConvertToJSON(new Definitions.Input { FileContent = null, culture = null }, new System.Threading.CancellationToken());
            });
        }
    }
}
 