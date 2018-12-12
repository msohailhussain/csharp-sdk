/* 
 * Copyright 2017-2018, Optimizely
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OptimizelySDK.Entity;
using OptimizelySDK.Matcher;
using System;
using System.Linq;

namespace OptimizelySDK.Utils
{
    public class ConditionEvaluator
    {
        /// <summary>
        /// const string Representing AND operator.
        /// </summary>
        const string AND_OPERATOR = "and";

        /// <summary>
        /// const string Representing OR operator.
        /// </summary>
        const string OR_OPERATOR = "or";

        /// <summary>
        /// const string Representing NOT operator.
        /// </summary>
        const string NOT_OPERATOR = "not";

        /// <summary>
        /// String constant representing custome attribute condition type.
        /// </summary>
        const string CUSTOM_ATTRIBUTE_CONDITION_TYPE = "custom_attribute";

        /// <summary>
        /// Evaluates an array of conditions as if the evaluator had been applied
        /// to each entry and the results AND-ed together.
        /// </summary>
        /// <param name="conditions">Array of conditions ex: [operand_1, operand_2]</param>
        /// <param name="userAttributes">Hash representing user attributes</param>
        /// <returns>true/false if the user attributes match/don't match the given conditions,
        /// null if the user attributes and conditions can't be evaluated</returns>
        private bool? AndEvaluator(JArray conditions, UserAttributes userAttributes)
        {
            // According to the matrix:
            // false and true is false
            // false and null is false
            // true and null is null.
            // true and false is false
            // true and true is true
            // null and null is null
            var foundNull = false;
            foreach(var condition in conditions)
            {
                var result = Evaluate(condition, userAttributes);
                if (result == null)
                    foundNull = true;
                else if (result == false)
                    return false;
            }

            if (foundNull)
                return null;

            return true;
        }

        /// <summary>
        /// Evaluates an array of conditions as if the evaluator had been applied
        /// to each entry and the results OR-ed together.
        /// </summary>
        /// <param name="conditions">Array of conditions ex: [operand_1, operand_2]</param>
        /// <param name="userAttributes">Hash representing user attributes</param>
        /// <returns>true/false if the user attributes match/don't match the given conditions,
        /// null if the user attributes and conditions can't be evaluated</returns>
        private bool? OrEvaluator(JArray conditions, UserAttributes userAttributes)
        {
            // According to the matrix:
            // true returns true
            // false or null is null
            // false or false is false
            // null or null is null
            var foundNull = false;
            foreach (var condition in conditions)
            {
                var result = Evaluate(condition, userAttributes);
                if (result == null)
                    foundNull = true;
                else if (result == true)
                    return true;
            }

            if (foundNull)
                return null;

            return false;
        }

        /// <summary>
        /// Evaluates an array of conditions as if the evaluator had been applied
        /// to a single entry and NOT was applied to the result.
        /// </summary>
        /// <param name="conditions">Array of conditions ex: [operand_1, operand_2]</param>
        /// <param name="userAttributes">Hash representing user attributes</param>
        /// <returns>true/false if the user attributes match/don't match the given conditions,
        /// null if the user attributes and conditions can't be evaluated</returns>
        private bool? NotEvaluator(JArray conditions, UserAttributes userAttributes)
        {
            if (conditions.Count == 1)
            {
                var result = Evaluate(conditions[0], userAttributes);
                return result == null ? null : !result;
            }

            return false;
        }

        public bool? Evaluate(JToken conditions, UserAttributes userAttributes)
        {
            //Cloning is because it is reference type
            var conditionsArray = conditions.DeepClone() as JArray;
            if (conditionsArray != null)
            {
                switch (conditions[0].ToString())
                {
                    case AND_OPERATOR: conditionsArray.RemoveAt(0); return AndEvaluator(conditionsArray, userAttributes);
                    case OR_OPERATOR:  conditionsArray.RemoveAt(0); return OrEvaluator(conditionsArray, userAttributes);
                    case NOT_OPERATOR: conditionsArray.RemoveAt(0); return NotEvaluator(conditionsArray, userAttributes);
                    default:
                        return false;
                }
            }

            if (conditions["type"] != null && conditions["type"].ToString() != CUSTOM_ATTRIBUTE_CONDITION_TYPE)
                return null;

            string matchType = conditions["match"]?.ToString();
            var conditionValue = conditions["value"]?.ToObject<object>();
            
            object attributeValue = null;
            if (userAttributes != null && userAttributes.ContainsKey(conditions["name"].ToString()))
            {
                attributeValue = userAttributes[conditions["name"].ToString()];
            }

            // Check infinity or NaN for numeric attribute and condition values.
            if (!ValidateNumericValue(attributeValue) || !ValidateNumericValue(conditionValue))
                return null;

            return MatchType.GetMatcher(matchType, conditionValue)?.Eval(attributeValue);
        }

        public bool? Evaluate(object[] conditions, UserAttributes userAttributes)
        {
            return Evaluate(ConvertObjectArrayToJToken(conditions), userAttributes);
        }

        private JToken ConvertObjectArrayToJToken(object[] conditions)
        {
            var serializeConditions = JsonConvert.SerializeObject(conditions);

            return JToken.Parse(serializeConditions);
        }

        public static JToken DecodeConditions(string conditions)
        {
            return JToken.Parse(conditions);
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
                var convertedValue = Convert.ToInt64(value);
                return convertedValue > long.MinValue && convertedValue < long.MaxValue;
            }

            if (value is float || value is double)
            {
                var convertedValue = Convert.ToDouble(value);
                return !(double.IsInfinity(convertedValue) || double.IsNaN(convertedValue));
            }

            // Do not validate and return true when the provided value is not of a numeric type.
            return true;
        }
    }
}