using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.FixedWidthFlatFile.ConvertToJSON.Tests
{
    [TestFixture]
    internal class ErrorHandlerTest
    {
        private const string CustomErrorMessage = "CustomErrorMessage";

        private static Definitions.Options DefaultOptions()
        {
            return new Definitions.Options();
        }

        private static Definitions.Input DefaultInput()
        {
            return new Definitions.Input();
        }

        [Test]
        public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
        {
            var ex = Assert.Catch<Exception>((Action)(() =>
               FixedWidthFlatFile.ConvertToJSON(DefaultInput(), DefaultOptions(), CancellationToken.None)));
            Assert.That(ex, Is.Not.Null);
        }

        [Test]
        public void Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
        {
            var options = DefaultOptions();
            options.ThrowErrorOnFailure = false;
            var result = FixedWidthFlatFile.ConvertToJSON(DefaultInput(), options, CancellationToken.None);
            Assert.That(result.Success, Is.False);
        }

        [Test]
        public void Should_Use_Custom_ErrorMessageOnFailure()
        {
            var options = DefaultOptions();
            options.ErrorMessageOnFailure = CustomErrorMessage;
            var ex = Assert.Throws<Exception>((Action)(() =>
                FixedWidthFlatFile.ConvertToJSON(DefaultInput(), options, CancellationToken.None)));
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Contains.Substring(CustomErrorMessage));
        }
    }
}
