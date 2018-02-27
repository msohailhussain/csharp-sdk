using Moq;
using NUnit.Framework;
using OptimizelySDK.Logger;
using OptimizelySDK.Utils;
using System.Collections.Generic;

namespace OptimizelySDK.Tests.UtilsTests
{
    [TestFixture]
    class EventTagUtilsTest
    {
        private Mock<ILogger> LoggerMock;
        private ILogger Logger;

        [SetUp]
        public void Setup()
        {
            LoggerMock = new Mock<ILogger>();
            LoggerMock.Setup(i => i.Log(It.IsAny<LogLevel>(), It.IsAny<string>()));

            Logger = LoggerMock.Object;
        }

        [Test]
        public void TestGetRevenueValue()
        {
            var logger = LoggerMock.Object;
            var expectedValue = 42;
            var expectedValue2 = 100;
            var expectedValueString = 123;

            var validTag = new Dictionary<string, object>() {
                { "revenue", 42 }
            };
            var validTag2 = new Dictionary<string, object>() {
                { "revenue", 100 }
            };
            var validTagStringValue = new Dictionary<string, object>() {
                { "revenue", "123" }
            };

            var invalidTag = new Dictionary<string, object>() {
                { "abc", 42 }
            };
            var nullValue = new Dictionary<string, object>() {
                { "revenue", null }
            };
            var invalidValue = new Dictionary<string, object>() {
                { "revenue", 42.5 }
            };
            var invalidTagNonRevenue = new Dictionary<string, object>()
            {
                {"non-revenue", 123 }
            };

            // Invalid data.
            Assert.Null(EventTagUtils.GetRevenueValue(null, Logger));
            Assert.Null(EventTagUtils.GetRevenueValue(invalidTag, Logger));
            Assert.Null(EventTagUtils.GetRevenueValue(invalidTagNonRevenue, Logger));
            Assert.Null(EventTagUtils.GetRevenueValue(nullValue, Logger));
            Assert.Null(EventTagUtils.GetRevenueValue(invalidValue, Logger));

            LoggerMock.Verify(l => l.Log(LogLevel.DEBUG, "Event tags is undefined."), Times.Once);
            LoggerMock.Verify(l => l.Log(LogLevel.DEBUG, "The revenue key is not defined in the event tags."), Times.Exactly(2));
            LoggerMock.Verify(l => l.Log(LogLevel.ERROR, "The revenue key value is not defined in event tags."), Times.Once);
            LoggerMock.Verify(l => l.Log(LogLevel.ERROR, "Revenue value is not an integer or couldn't be parsed as an integer."), Times.Once);

            // Valid data.
            Assert.AreEqual(EventTagUtils.GetRevenueValue(validTag, Logger), expectedValue);
            Assert.AreEqual(EventTagUtils.GetRevenueValue(validTag2, Logger), expectedValue2);
            Assert.AreEqual(EventTagUtils.GetRevenueValue(validTagStringValue, Logger), expectedValueString);

            LoggerMock.Verify(l => l.Log(LogLevel.INFO, $"The numeric metric value {expectedValue} will be sent to results."), Times.Once);
            LoggerMock.Verify(l => l.Log(LogLevel.INFO, $"The numeric metric value {expectedValue2} will be sent to results."), Times.Once);
            LoggerMock.Verify(l => l.Log(LogLevel.INFO, $"The numeric metric value {expectedValueString} will be sent to results."), Times.Once);
        }

        [Test]
        public void TestGetNumericValue()
        {
            var logger = LoggerMock.Object;
            int expectedValue = 42;
            float expectedValue2 = 42.5F;
            double expectedValue3 = 42.52;
            
            var validTag = new Dictionary<string, object>() {
                { "value", expectedValue }
            };

            var validTag2 = new Dictionary<string, object>() {
                { "value", expectedValue2 }
            };

            var validTag3 = new Dictionary<string, object>() {
                { "value", expectedValue3 }
            };

            var invalidTag = new Dictionary<string, object>() {
                { "abc", 42 }
            };
            var nullValue = new Dictionary<string, object>() {
                { "value", null }
            };
            var validTagStr = new Dictionary<string, object>() {
                { "value", "43" }
            };
            var validTagStr1 = new Dictionary<string, object>() {
                { "value", "42.3" }
            };

            // Invalid data.
            Assert.Null(EventTagUtils.GetNumericValue(null, Logger));
            Assert.Null(EventTagUtils.GetNumericValue(invalidTag, Logger));
            Assert.Null(EventTagUtils.GetNumericValue(nullValue, Logger));

            LoggerMock.Verify(l => l.Log(LogLevel.DEBUG, "Event tags is undefined."), Times.Once);
            LoggerMock.Verify(l => l.Log(LogLevel.DEBUG, "The numeric metric key is not in event tags."), Times.Once);
            LoggerMock.Verify(l => l.Log(LogLevel.ERROR, "The numeric metric key value is not defined in event tags."), Times.Once);
            
            // Valid data.
            Assert.AreEqual(43, EventTagUtils.GetNumericValue(validTagStr, Logger));
            Assert.AreEqual("42.3", EventTagUtils.GetNumericValue(validTagStr1, Logger).ToString());
            Assert.AreEqual(EventTagUtils.GetNumericValue(validTag, Logger), expectedValue);
            Assert.AreEqual(EventTagUtils.GetNumericValue(validTag2, Logger), expectedValue2);
            Assert.AreEqual(EventTagUtils.GetNumericValue(validTag3, Logger).ToString(), expectedValue3.ToString());

            LoggerMock.Verify(l => l.Log(LogLevel.INFO, "The numeric metric value 43 will be sent to results."), Times.Once);
            LoggerMock.Verify(l => l.Log(LogLevel.INFO, "The numeric metric value 42.3 will be sent to results."), Times.Once);
            LoggerMock.Verify(l => l.Log(LogLevel.INFO, $"The numeric metric value {expectedValue} will be sent to results."), Times.Once);
            LoggerMock.Verify(l => l.Log(LogLevel.INFO, $"The numeric metric value {expectedValue2} will be sent to results."), Times.Once);
            LoggerMock.Verify(l => l.Log(LogLevel.INFO, $"The numeric metric value {expectedValue3} will be sent to results."), Times.Once);
        }
        
        [Test]
        public void TestGetNumericMetricNoValueTag()
        {
            // Test that numeric value is not returned when there's no numeric event tag.
            Assert.IsNull(EventTagUtils.GetNumericValue(new Dictionary<string, object> { }, Logger));
            Assert.IsNull(EventTagUtils.GetNumericValue(new Dictionary<string, object> { { "non-value", 42 } }, Logger));

            LoggerMock.Verify(l => l.Log(LogLevel.DEBUG, "The numeric metric key is not in event tags."), Times.Exactly(2));

            //Errors for all, because it accepts only dictionary// 
            //Assert.IsNull(EventTagUtils.GetEventValue(new object[] { }));
        }

        [Test]
        public void TestGetNumericMetricValueTagWithTypeCastingAndMinMaxValues()
        {
            // An integer should be cast to a float
            Assert.AreEqual(12345.0, EventTagUtils.GetNumericValue(new Dictionary<string, object> { { "value", 12345 } }, Logger));

            // A string should be cast to a float
            Assert.AreEqual(12345.0, EventTagUtils.GetNumericValue(new Dictionary<string, object> { { "value", "12345" } }, Logger));

            LoggerMock.Verify(l => l.Log(LogLevel.INFO, "The numeric metric value 12345 will be sent to results."), Times.Exactly(2));

            // Min Max float value tests.
            float maxFloat = float.MaxValue;
            Assert.AreEqual(maxFloat, EventTagUtils.GetNumericValue(new Dictionary<string, object> { { "value", maxFloat } }, Logger));
            float minFloat = float.MinValue;
            Assert.AreEqual(minFloat, EventTagUtils.GetNumericValue(new Dictionary<string, object> { { "value", minFloat } }, Logger));

            LoggerMock.Verify(l => l.Log(LogLevel.INFO, $"The numeric metric value {maxFloat} will be sent to results."), Times.Once);
            LoggerMock.Verify(l => l.Log(LogLevel.INFO, $"The numeric metric value {minFloat} will be sent to results."), Times.Once);

            var numericValueOverflow = EventTagUtils.GetNumericValue(new Dictionary<string, object> { { "value", float.MaxValue * 10 } }, Logger);
            Assert.IsNull(numericValueOverflow, string.Format("Max numeric value is {0}", float.MaxValue * 10 ));
            LoggerMock.Verify(l => l.Log(LogLevel.ERROR, $"Provided numeric value {float.PositiveInfinity} is in an invalid format."), Times.Once);

            var zeroValue = 0.0;
            Assert.AreEqual(zeroValue, EventTagUtils.GetNumericValue(new Dictionary<string, object> { { "value", zeroValue } }, Logger));
            LoggerMock.Verify(l => l.Log(LogLevel.INFO, $"The numeric metric value {zeroValue} will be sent to results."), Times.Once);

            /* Value is converted into 1234F */
            //var numericValueInvalidLiteral = EventTagUtils.GetEventValue(new Dictionary<string, object> { { "value", "1,234" } });
            //Assert.IsNull(numericValueInvalidLiteral, string.Format("Invalid string literal value is {0}", numericValueInvalidLiteral));

            /* Not valid use cases in C# */
            // float - inf is not possible.
            // float -inf is not possible.
        }

        [Test]
        public void TestGetRevenueValueWithInvalidTypes()
        {
            Assert.IsNull(EventTagUtils.GetNumericValue(new Dictionary<string, object> { { "value", false } }, Logger));
            Assert.IsNull(EventTagUtils.GetNumericValue(new Dictionary<string, object> { { "value", true } }, Logger));

            Assert.IsNull(EventTagUtils.GetNumericValue(new Dictionary<string, object> { { "value", new int[] { } } }, Logger));
            Assert.IsNull(EventTagUtils.GetNumericValue(new Dictionary<string, object> { { "value", new object[] { } } }, Logger));

            LoggerMock.Verify(l => l.Log(LogLevel.ERROR, "Provided numeric value is boolean which is an invalid format."), Times.Exactly(2));
            LoggerMock.Verify(l => l.Log(LogLevel.ERROR, "Numeric metric value is not in integer, float, or string form."), Times.Exactly(2));
        }
    }
}
