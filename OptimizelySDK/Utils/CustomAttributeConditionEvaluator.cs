/* 
 * Copyright 2018, Optimizely
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Newtonsoft.Json.Linq;
using OptimizelySDK.Entity;
using OptimizelySDK.Matcher;
using System;

namespace OptimizelySDK.Utils
{
    public static class CustomAttributeConditionEvaluator
    {
        /// <summary>
        /// String constant representing custome attribute condition type.
        /// </summary>
        public const string CUSTOM_ATTRIBUTE_CONDITION_TYPE = "custom_attribute";

        public static bool? Evaluate(JToken condition, UserAttributes userAttributes)
        {
            if (condition["type"] != null && condition["type"].ToString() != CUSTOM_ATTRIBUTE_CONDITION_TYPE)
                return null;

            string matchType = condition["match"]?.ToString();
            var conditionValue = condition["value"]?.ToObject<object>();

            object attributeValue = null;
            if (userAttributes != null && userAttributes.ContainsKey(condition["name"].ToString()))
            {
                attributeValue = userAttributes[condition["name"].ToString()];

                // Newtonsoft.JSON converts int value to long. And exact matcher expect types to be the same.
                // This conversion enable us to pass that scenario.
                attributeValue = attributeValue is int ? Convert.ToInt64(attributeValue) : attributeValue;
            }

            // Check infinity or NaN for numeric attribute and condition values.
            if (!ValidateNumericValue(attributeValue) || !ValidateNumericValue(conditionValue))
                return null;

            return MatchType.GetMatcher(matchType, conditionValue)?.Eval(attributeValue);
        }

        /// <summary>
        /// Determine if the value is a valid numeric value.
        /// </summary>
        /// <param name="value">Value to be validated</param>
        /// <returns>true for numeric types if the value is valid numeric value, false otherwise.
        /// Returns true for non-numeric typed value.</returns>
        private static bool ValidateNumericValue(object value)
        {
            if (value is int || value is long)
            {
                var convertedValue = (long)value;
                return convertedValue > long.MinValue && convertedValue < long.MaxValue;
            }

            if (value is float || value is double)
            {
                var convertedValue = (double)value;
                return !(double.IsInfinity(convertedValue) || double.IsNaN(convertedValue));
            }

            // Do not validate and return true when the provided value is not of a numeric type.
            return true;
        }
    }
}
