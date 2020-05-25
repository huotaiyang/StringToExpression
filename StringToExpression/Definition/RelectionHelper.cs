using System;
using System.Collections.Generic;
using System.Reflection;

namespace StringToExpression
{
    /// <summary>
    /// 反射辅助类
    /// </summary>
    internal class ReflectionHelper
    {
        /// <summary>
        /// 读取静态字段信息（不包含继承成员），并转换为HashSet返回
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static HashSet<TKey> ReadStaticFields<TKey, TAttribute>(Type type) where TAttribute : Attribute
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            HashSet<TKey> ret = new HashSet<TKey>();
            FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);
            foreach (FieldInfo f in fields)
            {
                if (f.GetCustomAttribute(typeof(TAttribute), false) != null)
                {
                    ret.Add((TKey)f.GetValue(null));
                }
            }

            return ret;
        }
    }
}
