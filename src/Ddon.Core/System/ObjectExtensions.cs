﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace System
{
    /// <summary>
    /// Extension methods for all objects.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Used to simplify and beautify casting an object to a type.
        /// </summary>
        /// <typeparam name="T">Type to be casted</typeparam>
        /// <param name="obj">Object to cast</param>
        /// <returns>Casted object</returns>
        public static T As<T>(this object obj)
            where T : class
        {
            return (T)obj;
        }

        /// <summary>
        /// Converts given object to a value type using <see cref="Convert.ChangeType(object,System.Type)"/> method.
        /// </summary>
        /// <param name="obj">Object to be converted</param>
        /// <typeparam name="T">Type of the target object</typeparam>
        /// <returns>Converted object</returns>
        public static T? To<T>(this object obj)
            where T : struct
        {
            if (typeof(T) == typeof(Guid))
            {
                return (T?)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(obj.ToString()!);
            }

            return (T)Convert.ChangeType(obj, typeof(T), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Check if an item is in a list.
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <param name="list">List of items</param>
        /// <typeparam name="T">Type of the items</typeparam>
        public static bool IsIn<T>(this T item, params T[] list)
        {
            return list.Contains(item);
        }

        /// <summary>
        /// Check if an item is in the given enumerable.
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <param name="items">Items</param>
        /// <typeparam name="T">Type of the items</typeparam>
        public static bool IsIn<T>(this T item, IEnumerable<T> items)
        {
            return items.Contains(item);
        }

        /// <summary>
        /// Can be used to conditionally perform a function
        /// on an object and return the modified or the original object.
        /// It is useful for chained calls.
        /// </summary>
        /// <param name="obj">An object</param>
        /// <param name="condition">A condition</param>
        /// <param name="func">A function that is executed only if the condition is <code>true</code></param>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <returns>
        /// Returns the modified object (by the <paramref name="func"/> if the <paramref name="condition"/> is <code>true</code>)
        /// or the original object if the <paramref name="condition"/> is <code>false</code>
        /// </returns>
        public static T If<T>(this T obj, bool condition, Func<T, T> func)
        {
            if (condition)
            {
                return func(obj);
            }

            return obj;
        }

        /// <summary>
        /// Can be used to conditionally perform an action
        /// on an object and return the original object.
        /// It is useful for chained calls on the object.
        /// </summary>
        /// <param name="obj">An object</param>
        /// <param name="condition">A condition</param>
        /// <param name="action">An action that is executed only if the condition is <code>true</code></param>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <returns>
        /// Returns the original object.
        /// </returns>
        public static T If<T>(this T obj, bool condition, Action<T> action)
        {
            if (condition)
            {
                action(obj);
            }

            return obj;
        }

        /// <summary>
        /// 格式化为 Key-Value 格式
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ICollection<KeyValuePair<string, string>> ToKeyValuePair(this object obj)
        {
            var keyValues = new Collection<KeyValuePair<string, string>>();

            if (obj == null) return keyValues;

            if (obj.GetType() == typeof(string) || (obj?.GetType().IsValueType ?? false))
            {
                keyValues.Add(new KeyValuePair<string, string>(obj.ToString() ?? string.Empty, obj.ToString() ?? string.Empty));
                return keyValues;
            }

            var properties = obj?.GetType().GetProperties().ToList();
            if (properties?.Count > 0)
            {
                foreach (var pro in properties)
                {
                    var value = pro.GetValue(obj);
                    if (value != null && pro.PropertyType == typeof(DateTime))
                    {
                        try
                        {
                            // FIX: 这样会丢失时区信息
                            value = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        catch
                        {
                            value = string.Empty;
                        }
                    }

                    keyValues.Add(new KeyValuePair<string, string>(pro.Name, value?.ToString() ?? string.Empty));
                }
            }
            return keyValues;
        }
    }
}