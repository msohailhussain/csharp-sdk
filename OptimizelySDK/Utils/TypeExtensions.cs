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

using System;
using System.Reflection;

namespace OptimizelySDK.Utils
{
    static class TypeExtensions
    {
        /// <summary>
        /// Extension method of Type class to determine whether the specified object is an instance 
        /// of the current type.
        /// </summary>
        /// <param name="type">Type class</param>
        /// <param name="obj">The object to compare with the current type</param>
        /// <returns>true if the current Type is in the inheritance hierarchy of the object represented
        /// by obj, or if the current Type is an interface that obj implements. Otherwise false.</returns>
        public static bool IsInstance(this Type type, object obj)
        {
#if NETSTANDARD1_6
            return type.GetTypeInfo().IsAssignableFrom(obj.GetType());
#else
            return obj != null && type.IsInstanceOfType(obj);
#endif
        }
    }
}
