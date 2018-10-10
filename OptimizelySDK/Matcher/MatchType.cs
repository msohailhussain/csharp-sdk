/* 
 * Copyright 2018, Optimizely
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use file except in compliance with the License.
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

using System;

namespace OptimizelySDK.Matcher
{
    public static class AttributeMatchTypes
    {
        public const string EXACT = "exact";
        public const string EXIST = "exists";
        public const string GREATER_THAN = "gt";
        public const string LESS_THAN = "lt";
        public const string SUBSTRING = "substring";
    }

    public static class MatchType
    {
        public static IMatcher GetMatcher(string matchType, object conditionValue)
        {
            switch (matchType)
            {
                case AttributeMatchTypes.EXACT:
                    if (conditionValue is string)
                        return new ExactMatcher<string>((string)conditionValue);
                    else if (conditionValue is float || conditionValue is double)
                            return new ExactMatcher<double>((double)conditionValue);
                    else if (conditionValue is bool)
                        return new ExactMatcher<bool>((bool)conditionValue);
                    else if (conditionValue is int || conditionValue is long)
                    {
                        // Fitering int and long types, as Json.NET by default reads int values as long.
                        // Otherwise, it fails ExactMatcher type checking.
                        long value = (long)conditionValue;
                        if (value < int.MinValue || value > int.MaxValue)
                            return new ExactMatcher<long>(value);
                        else
                            return new ExactMatcher<int>((int)value);
                    }
                    break;
                case AttributeMatchTypes.EXIST:
                    return new ExistsMatcher(conditionValue);
                case AttributeMatchTypes.GREATER_THAN:
                    if (conditionValue is float || conditionValue is double)
                        return new GTMatcher<double>((double)conditionValue);
                    else if (conditionValue is int || conditionValue is long)
                    {
                        // Fitering int and long types, as Json.NET by default reads int values as long.
                        // Otherwise, it fails type conversion.
                        long value = (long)conditionValue;
                        if (value < int.MinValue || value > int.MaxValue)
                            return new GTMatcher<long>(value);
                        else
                            return new GTMatcher<int>((int)value);
                    }
                    break;
                case AttributeMatchTypes.LESS_THAN:
                    if (conditionValue is float || conditionValue is double)
                        return new LTMatcher<double>((double)conditionValue);
                    else if (conditionValue is int || conditionValue is long)
                    {
                        // Fitering int and long types, as Json.NET by default reads int values as long.
                        // Otherwise, it fails type conversion.
                        long value = (long)conditionValue;
                        if (value < int.MinValue || value > int.MaxValue)
                            return new LTMatcher<long>(value);
                        else
                            return new LTMatcher<int>((int)value);
                    }
                    break;
                case AttributeMatchTypes.SUBSTRING:
                    if (conditionValue is string)
                        return new SubstringMatcher((string)conditionValue);
                    break;
                case null:
                    if (conditionValue is string)
                        return new DefaultMatcherForLegacyAttributes((string)conditionValue);
                    break;
            }

            return null;
        }
    }
}
