using System;
using System.Collections.Generic;
using System.Linq;

namespace StringToExpression
{
    /// <summary>
    /// 数值类
    /// </summary>
    internal class DigitValue
    {
        /// <summary>
        /// int 
        /// </summary>
        public static readonly char[] Int = new char[] { };

        /// <summary>
        /// long
        /// </summary>
        public static readonly char[] Long = new char[] { 'l', 'L' };

        /// <summary>
        /// float
        /// </summary>
        public static readonly char[] Float = new char[] { 'f', 'F' };

        /// <summary>
        /// double
        /// </summary>
        public static readonly char[] Double = new char[] { 'd', 'D' };

        /// <summary>
        /// decimal
        /// </summary>
        public static readonly char[] Decimal = new char[] { 'm', 'M' };

        private static Lazy<Dictionary<char[], Type>> lazy = new Lazy<Dictionary<char[], Type>>(() =>
        {
            Dictionary<char[], Type> dic = new Dictionary<char[], Type>();
            dic.Add(Int, typeof(int));
            dic.Add(Long, typeof(long));
            dic.Add(Float, typeof(float));
            dic.Add(Double, typeof(double));
            dic.Add(Decimal, typeof(decimal));

            return dic;
        });

        /// <summary>
        /// 所有
        /// </summary>
        private static Dictionary<char[], Type> All
        {
            get
            {
                return lazy.Value;
            }
        }

        /// <summary>
        /// 获取类型
        /// </summary>
        /// <param name="vaule">字符串</param>
        /// <returns>字符串对应的数值类型</returns>
        public static Type GetType(string vaule)
        {
            char mark = vaule.Last();
            if (char.IsLetter(mark))
            {
                var find = All.Keys.FirstOrDefault(k => k.Contains(mark));
                if (find == null)
                {
                    throw new ArgumentException(string.Format("Unsupported DigitValue Mark {0}", mark));
                }

                return All[find];
            }

            // 有小数点时，是Double
            if (vaule.Contains(MarkChar.Dot))
            {
                return All[Double];
            }

            return All[Int];
        }

        /// <summary>
        /// 解析为数值
        /// </summary>
        /// <param name="vaule">字符串</param>
        /// <returns>数值</returns>
        public static object Parse(string vaule)
        {
            Type type = GetType(vaule);
            // 去除如F、L等类型标识符
            if (char.IsLetter(vaule.Last()))
            {
                vaule = vaule.Substring(0, vaule.Length - 1);
            }

            return type.GetMethod("Parse", new Type[] { typeof(string) }).Invoke(null, new string[] { vaule });
        }

        /// <summary>
        /// 是否包含
        /// </summary>
        /// <param name="mark">标识</param>
        /// <returns>包含指定的元素，则为 true；否则为 false。</returns>
        public static bool Contains(char mark)
        {
            return All.Keys.Any(k => k.Contains(mark));
        }
    }
}