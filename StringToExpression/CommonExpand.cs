using System;
using System.Collections.Generic;

namespace StringToExpression
{
    /// <summary>
    /// 公用扩展类
    /// </summary>
    internal static class CommonExpand
    {
        /// <summary>
        /// 获取去掉可空类型（Nullable&lt;&gt;）的类型.
        /// </summary>
        /// <param name="type">当前类型的实例对象.</param>
        /// <returns>去掉可空类型（Nullable&lt;&gt;）的类型.</returns>
        public static Type GetNoneNullableType(this Type type)
        {
            if (IsNullable(type))
            {
                return Nullable.GetUnderlyingType(type);
            }

            return type;
        }

        /// <summary>
        /// 获取可空类型（Nullable&lt;&gt;）的类型.
        /// </summary>
        /// <param name="type">当前类型的实例对象.</param>
        /// <returns>可空类型（Nullable&lt;&gt;）的类型.</returns>
        public static Type GetNullableType(this Type type)
        {
            if (!IsNullable(type) && type.IsValueType)
            {
                return typeof(Nullable<>).MakeGenericType(type);
            }

            return type;
        }

        /// <summary>
        /// 获取一个值, 通过该值指示当前类型是否为可空类型（Nullable&lt;&gt;）.
        /// </summary>
        /// <param name="type">当前类型的实例对象.</param>
        /// <returns>
        ///   <c>true</c> 表示为可空类型（Nullable&lt;&gt;）; 否则返回 <c>false</c>.
        /// </returns>
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// 是否为指定的值
        /// </summary>
        /// <param name="token">当前类型的实例对象</param>
        /// <param name="value">值</param>
        /// <param name="throwIfNot">如果设置为 <c>true</c> 表示抛出异常. 默认为 <c>false</c> 表示不抛出异常.</param>
        /// <returns>是返回true，否则返回false</returns>
        public static bool Is(this Token token, string value, bool throwIfNot = false)
        {
            var result = token.Text == value;
            if (!result && throwIfNot)
            {
                throw new ArgumentException(string.Format("token is not {0}", value));
            }

            return result;
        }

        /// <summary>
        /// 是否为指定的值
        /// </summary>
        /// <param name="token">当前类型的实例对象</param>
        /// <param name="value">值</param>
        /// <param name="throwIfNot">如果设置为 <c>true</c> 表示抛出异常. 默认为 <c>false</c> 表示不抛出异常.</param>
        /// <returns>是返回true，否则返回false</returns>
        public static bool Is(this Token token, char value, bool throwIfNot = false)
        {
            return Is(token, value.ToString(), throwIfNot);
        }

        /// <summary>
        /// 将指定的键和值添加到字典中
        /// </summary>
        /// <param name="dic">字典</param>
        /// <param name="opt">操作符</param>
        /// <param name="value">值</param>
        public static void Add<T>(this Dictionary<string, T> dic, char opt, T value)
        {
            dic.Add(opt.ToString(), value);
        }
    }
}
