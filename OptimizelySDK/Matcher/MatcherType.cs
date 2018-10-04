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
    public static class MatcherType
    {
        public static IMatcher GetMatcher(string matchType, object conditionValue)
        {
            switch (matchType)
            {
                case "exact":
                    if (conditionValue is string)
                        return new ExactMatcher<string>((string)conditionValue);
                    else if (conditionValue is int)
                        return new ExactMatcher<int>((int)conditionValue);
                    else if (conditionValue is double)
                        return new ExactMatcher<double>((double)conditionValue);
                    else if (conditionValue is bool)
                        return new ExactMatcher<bool>((bool)conditionValue);
                    break;
                case "exists":
                    return new ExistsMatcher(conditionValue);
                case "gt":
                    if (conditionValue is int)
                        return new GTMatcher<int>((int)conditionValue);
                    else if (conditionValue is double)
                        return new GTMatcher<double>((double)conditionValue);
                    break;
                case "lt":
                    if (conditionValue is int)
                        return new LTMatcher<int>((int)conditionValue);
                    else if (conditionValue is double)
                        return new LTMatcher<double>((double)conditionValue);
                    break;
                case "substring":
                    if (conditionValue is string)
                        return new SubstringMatcher((string)conditionValue);
                    break;
                default:
                    if (conditionValue is string)
                        return new DefaultMatcherForLegacyAttributes((string)conditionValue);
                    break;
            }

            return null;
        }
    }
}
