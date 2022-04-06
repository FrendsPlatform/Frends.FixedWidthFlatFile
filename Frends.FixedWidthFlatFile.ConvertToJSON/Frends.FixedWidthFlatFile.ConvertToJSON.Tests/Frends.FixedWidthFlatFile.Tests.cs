using Frends.FixedWidthFlatFile.ConvertToJSON.Definitions;
using NUnit.Framework;
using System;
using System.Linq;
using static Frends.FixedWidthFlatFile.ConvertToJSON.Definitions.Enums;

namespace Frends.FixedWidthFlatFile.ConvertToJSON.Tests
{

    [TestFixture]
    public class Tests
    {/*
        #region Test data creation helper methods

        private Result _withEmptyAndDateValues;
        
        /// <summary>
        /// Creation of values to use.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            string fileContent =
                @"First;Second;Third;Date
firstValue    ThirdValue190315";


            var parseInput = new Input
            {
                ColumnSpecifications = columnSpec,
                FlatFileContent = fileContent,
                HeaderDelimiter = ";",
                HeaderRow = HeaderRowType.Delimited
            };
            var options = new Options { SkipRows = false };

            _withEmptyAndDateValues = FixedWidthFlatFile.Parse(parseInput, options);
            Console.WriteLine(_withEmptyAndDateValues.Data);
        }

        #endregion

        /// <summary>
        /// Tests that given json with empty values do not throw exceptions.
        /// </summary>
        [Test]
        public void ToJson_WithEmptyValues_DoesNotThrowException()
        {
            Assert.AreEqual(null, _withEmptyAndDateValues.Data[0]["Second"]);
        }

        /// <summary>
        /// Tests that method converts date type right.
        /// </summary>
        [Test]
        public void ToJson_NullCheckWithDateTimeValue_DoesNotThrowException()
        {
            Assert.IsTrue(_withEmptyAndDateValues.Data[0]["Date"].GetType() == typeof(DateTime));
        }*/

    }
}
 