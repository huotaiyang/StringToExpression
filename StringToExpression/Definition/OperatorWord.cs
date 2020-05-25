using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace StringToExpression
{
    /// <summary>
    /// 操作符 Word
    /// </summary>
    public class OperatorWord : OperatorChar
    {
        #region fields
        /// <summary>
        /// &lt;&lt;
        /// </summary>
        [Mark]
        public static readonly string LeftShift = Splice(LeftAngleBracket, LeftAngleBracket);

        /// <summary>
        /// &gt;&gt;
        /// </summary>
        [Mark]
        public static readonly string RightShift = Splice(RightAngleBracket, RightAngleBracket);

        /// <summary>
        /// &gt;=
        /// </summary>
        [Mark]
        public static readonly string GreaterThanEqual = Splice(RightAngleBracket, Equal);

        /// <summary>
        /// &lt;=
        /// </summary>
        [Mark]
        public static readonly string LessThanEqual = Splice(LeftAngleBracket, Equal);

        /// <summary>
        /// ==
        /// </summary>
        [Mark]
        public static readonly string DoubleEqual = Splice(Equal, Equal);

        /// <summary>
        /// !=
        /// </summary>
        [Mark]
        public static readonly string ExclamationEqual = Splice(Exclamation, Equal);

        /// <summary>
        /// &amp;&amp;
        /// </summary>
        [Mark]
        public static readonly string DoubleAmphersand = Splice(Amphersand, Amphersand);

        /// <summary>
        /// ||
        /// </summary>
        [Mark]
        public static readonly string DoubleBar = Splice(Bar, Bar);

        /// <summary>
        /// ??
        /// </summary>
        [Mark]
        public static readonly string DoubleQuestion = Splice(Question, Question);

        /// <summary>
        /// Lambda =>
        /// </summary>
        [Mark]
        public static readonly string LambdaPrefix = Splice(Equal, RightAngleBracket);

        /// <summary>
        /// new 
        /// </summary>
        [Mark]
        public const string New = "new";

        /// <summary>
        /// typeof
        /// </summary>
        [Mark]
        public const string Typeof = "typeof";

        /// <summary>
        /// sizeof
        /// </summary>
        [Mark]
        public const string Sizeof = "sizeof";

        /// <summary>
        /// is
        /// </summary>
        [Mark]
        public const string Is = "is";

        /// <summary>
        /// as
        /// </summary>
        [Mark]
        public const string As = "as";
        #endregion

        private static Lazy<HashSet<string>> lazySet = new Lazy<HashSet<string>>(() =>
        {
            var set = ReflectionHelper.ReadStaticFields<string, MarkAttribute>(typeof(OperatorWord));
            set.UnionWith(AllChars.Select(c => c.ToString()));

            return set;
        });

        private static Lazy<Dictionary<string, Func<Expression, UnaryExpression>>> lazyUnaryDic = new Lazy<Dictionary<string, Func<Expression, UnaryExpression>>>(() =>
        {
            var dic = new Dictionary<string, Func<Expression, UnaryExpression>>();

            dic.Add(Exclamation, Expression.Not);
            dic.Add(Tilde, Expression.Not);
            dic.Add(Subtract, Expression.Negate);
            dic.Add(Plus, Expression.UnaryPlus);

            return dic;
        });

        private static Lazy<Dictionary<string, Func<Expression, Expression, BinaryExpression>>> lazyBinaryDic = new Lazy<Dictionary<string, Func<Expression, Expression, BinaryExpression>>>(() =>
        {
            var dic = new Dictionary<string, Func<Expression, Expression, BinaryExpression>>();

            // 乘法运算符
            dic.Add(Asterisk, Expression.Multiply);
            dic.Add(Slash, Expression.Divide);
            dic.Add(Percent, Expression.Modulo);

            // 相加运算符
            dic.Add(Plus, Expression.Add);
            dic.Add(Subtract, Expression.Subtract);

            // 移位运算符
            dic.Add(LeftShift, Expression.LeftShift);
            dic.Add(RightShift, Expression.RightShift);

            // 关系运算符
            dic.Add(LeftAngleBracket, Expression.LessThan);
            dic.Add(LessThanEqual, Expression.LessThanOrEqual);
            dic.Add(RightAngleBracket, Expression.GreaterThan);
            dic.Add(GreaterThanEqual, Expression.GreaterThanOrEqual);
            dic.Add(DoubleEqual, Expression.Equal);
            dic.Add(ExclamationEqual, Expression.NotEqual);

            // 条件运算符
            dic.Add(DoubleAmphersand, Expression.AndAlso);
            dic.Add(DoubleBar, Expression.OrElse);

            return dic;
        });

        private static Lazy<Dictionary<string, Func<Expression, Type, Expression>>> lazyTypeDic = new Lazy<Dictionary<string, Func<Expression, Type, Expression>>>(() =>
         {
             var dic = new Dictionary<string, Func<Expression, Type, Expression>>();

             dic.Add(Is, Expression.TypeIs);
             dic.Add(As, Expression.TypeAs);

             return dic;
         });

        /// <summary>
        /// 所有
        /// </summary>
        public static HashSet<string> All
        {
            get
            {
                return lazySet.Value;
            }
        }

        /// <summary>
        /// 一元表达式字典
        /// </summary>
        internal static Dictionary<string, Func<Expression, UnaryExpression>> UnaryExpressions
        {
            get
            {
                return lazyUnaryDic.Value;
            }
        }

        /// <summary>
        /// 二元表达式字典
        /// </summary>
        internal static Dictionary<string, Func<Expression, Expression, BinaryExpression>> BinaryExpressions
        {
            get
            {
                return lazyBinaryDic.Value;
            }
        }

        /// <summary>
        /// 类型表达式字典
        /// </summary>
        internal static Dictionary<string, Func<Expression, Type, Expression>> TypeExpressions
        {
            get
            {
                return lazyTypeDic.Value;
            }
        }

        /// <summary>
        /// 是否包含
        /// </summary>
        /// <param name="word">word</param>
        /// <returns>包含指定的元素，则为 true；否则为 false。</returns>
        internal static bool Contains(string word)
        {
            return All.Contains(word);
        }

        /// <summary>
        /// 拼接
        /// </summary>
        /// <param name="chars">char集合</param>
        /// <returns>拼接后字符串</returns>
        private static string Splice(params char[] chars)
        {
            return string.Join("", chars);
        }
    }
}
