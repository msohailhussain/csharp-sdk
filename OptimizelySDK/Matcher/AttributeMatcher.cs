﻿/* 
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

using OptimizelySDK.Utils;
using System;

namespace OptimizelySDK.Matcher
{
    public abstract class AttributeMatcher<T> : IMatcher
    {
        public abstract bool? Eval(object attributeValue);

        public bool ConvertValue(T conditionValue, object attributeValue, out T convertedValue)
        {
            try
            {
                if (conditionValue.GetType().IsInstance(attributeValue))
                {
                    convertedValue = (T)attributeValue;
                    return true;
                }

                convertedValue = default(T);
                return false;
            }
            catch (Exception)
            {
                convertedValue = default(T);
                return false;
            }
        }
    }
}
