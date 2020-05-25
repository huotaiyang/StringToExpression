using System;
using System.Collections.Generic;

namespace StringToExpression
{
    /// <summary>
    /// 包裹值类型，比如'a'需要成对的'进行包裹
    /// </summary>
    internal class PackageValue
    {
        /// <summary>
        /// string 
        /// </summary>
        public static readonly string String = "\"";

        /// <summary>
        /// Char
        /// </summary>
        public static readonly string Char = "'";

        private static Lazy<Dictionary<string, Type>> lazy = new Lazy<Dictionary<string, Type>>(() =>
        {
            Dictionary<string, Type> dic = new Dictionary<string, Type>();
            dic.Add(String, typeof(string));
            dic.Add(Char, typeof(char));

            return dic;
        });

        /// <summary>
        /// 所有
        /// </summary>
        public static Dictionary<string, Type> All
        {
            get
            {
                return lazy.Value;
            }
        }

        /// <summary>
        /// 是否包含
        /// </summary>
        /// <param name="mark">标识</param>
        /// <returns>包含指定的元素，则为 true；否则为 false。</returns>
        public static bool Contains(char mark)
        {
            return All.ContainsKey(mark.ToString());
        }
    }
}
