using System.Collections.Generic;
using OptimizelySDK.Logger;

namespace OptimizelySDK.Utils
{
    public class EventTagUtils
    {
        public const string REVENUE_EVENT_METRIC_NAME = "revenue";
        public const string VALUE_EVENT_METRIC_NAME = "value";

        public static object GetRevenueValue(Dictionary<string, object> eventTags, ILogger logger)
        {
            int result = 0;
            bool isCasted = false;
            string logMessage = string.Empty;
            LogLevel logLevel = LogLevel.INFO;

            if (eventTags == null)
            {
                logMessage = "Event tags is undefined.";
                logLevel = LogLevel.DEBUG;
            }
            else if (!eventTags.ContainsKey(REVENUE_EVENT_METRIC_NAME))
            {
                logMessage = "The revenue key is not defined in the event tags.";
                logLevel = LogLevel.DEBUG;
            }
            else if (eventTags[REVENUE_EVENT_METRIC_NAME] == null)
            {
                logMessage = "The revenue key value is not defined in event tags.";
                logLevel = LogLevel.ERROR;
            }
            else if (!int.TryParse(eventTags[REVENUE_EVENT_METRIC_NAME].ToString(), out result))
            {
                logMessage = "Revenue value is not an integer or couldn't be parsed as an integer.";
                logLevel = LogLevel.ERROR;
            }
            else
            {
                isCasted = true;
                logMessage = $"The numeric metric value {result} will be sent to results.";
            }

            if (logger != null)
                logger.Log(logLevel, logMessage);

            if (isCasted)
                return result;

            return null;
        }

        public static object GetNumericValue(Dictionary<string, object> eventTags, ILogger logger)
        {
            float refVar = 0;
            bool isCasted = false;
            string logMessage = string.Empty;
            LogLevel logLevel = LogLevel.INFO;

            if (eventTags == null)
            {
                logMessage = "Event tags is undefined.";
                logLevel = LogLevel.DEBUG;
            }
            else if (!eventTags.ContainsKey(VALUE_EVENT_METRIC_NAME))
            {
                logMessage = "The numeric metric key is not in event tags.";
                logLevel = LogLevel.DEBUG;
            }
            else if (eventTags[VALUE_EVENT_METRIC_NAME] == null)
            {
                logMessage = "The numeric metric key value is not defined in event tags.";
                logLevel = LogLevel.ERROR;
            }
            else if (eventTags[VALUE_EVENT_METRIC_NAME] is bool)
            {
                logMessage = "Provided numeric value is boolean which is an invalid format.";
                logLevel = LogLevel.ERROR;
            }
            else if (!(eventTags[VALUE_EVENT_METRIC_NAME] is int)
                && !(eventTags[VALUE_EVENT_METRIC_NAME] is string)
                && !(eventTags[VALUE_EVENT_METRIC_NAME] is float)
                && !(eventTags[VALUE_EVENT_METRIC_NAME] is decimal)
                && !(eventTags[VALUE_EVENT_METRIC_NAME] is double))
            {
                logMessage = "Numeric metric value is not in integer, float, or string form.";
                logLevel = LogLevel.ERROR;
            }
            else if (!float.TryParse(eventTags[VALUE_EVENT_METRIC_NAME].ToString(), out refVar) || float.IsInfinity(refVar))
            {
                logMessage = $"Provided numeric value {eventTags[VALUE_EVENT_METRIC_NAME]} is in an invalid format.";
                logLevel = LogLevel.ERROR;
            }
            else
            {
                isCasted = true;
                logMessage = $"The numeric metric value {refVar} will be sent to results.";
            }

            if (logger != null)
                logger.Log(logLevel, logMessage);

            // Special case, maximum value when passed and gone through tryparse, it loses precision.
            object objVal = refVar;
            if (isCasted && eventTags[VALUE_EVENT_METRIC_NAME] is float)
                objVal = eventTags[VALUE_EVENT_METRIC_NAME];

            return isCasted ? objVal : null;
        }
    }
}
 