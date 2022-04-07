using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Frends.FixedWidthFlatFile.ConvertToXML.Tests
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

        
    }
}
 