using System.Linq.Expressions;
using System.Reflection;

namespace StringToExpression
{
    /// <summary>
    /// Lambda 表达式分析器，不支持+=、++等赋值运算符，与Expression表达式树保持一致
    /// </summary>
    public static class LambdaParser
    {
        /// <summary>
        /// 将源字符串分析为 Lambda 表达式树
        /// 由于传入的字符串可能不正确或者不支持，需要捕获异常
        /// </summary>
        /// <typeparam name="T">委托类型</typeparam>
        /// <param name="source">用于分析的源字符串.</param>
        /// <param name="namespaces">分析过程中可能用到的命名空间列表.</param>
        /// <returns>分析完成的 Lambda 表达式树.</returns>
        public static Expression<T> Parse<T>(string source, params string[] namespaces)
        {
            return Parse<T>(source, null, namespaces);
        }

        /// <summary>
        /// 将源字符串分析为 Lambda 表达式树
        /// 由于传入的字符串可能不正确或者不支持，需要捕获异常
        /// </summary>
        /// <typeparam name="T">委托类型</typeparam>
        /// <param name="source">用于分析的源字符串.</param>
        /// <param name="assemblies">可能用到的程序集列表.</param>
        /// <param name="namespaces">分析过程中可能用到的命名空间列表.</param>
        /// <returns>分析完成的 Lambda 表达式树.</returns>
        public static Expression<T> Parse<T>(string source, Assembly[] assemblies, params string[] namespaces)
        {
            var parseResult = SymbolParser.Parse(source);
            var parser = new ExpressionParser(parseResult, typeof(T), namespaces, assemblies);

            return parser.ToLambdaExpression<T>();
        }

        /// <summary>
        /// 将源字符串分析为委托
        /// 由于传入的字符串可能不正确或者不支持，需要捕获异常
        /// </summary>
        /// <typeparam name="T">委托类型</typeparam>
        /// <param name="source">用于分析的源字符串.</param>
        /// <param name="namespaces">分析过程中可能用到的命名空间列表.</param>
        /// <returns>分析委托.</returns>
        public static T Compile<T>(string source, params string[] namespaces)
        {
            return Compile<T>(source, null, namespaces);
        }

        /// <summary>
        /// 将源字符串分析为委托
        /// 由于传入的字符串可能不正确或者不支持，需要捕获异常
        /// </summary>
        /// <typeparam name="T">委托类型</typeparam>
        /// <param name="source">用于分析的源字符串.</param>
        /// <param name="assemblies">可能用到的程序集列表.</param>
        /// <param name="namespaces">分析过程中可能用到的命名空间列表.</param>
        /// <returns>
        /// 分析委托.
        /// </returns>
        public static T Compile<T>(string source, Assembly[] assemblies, params string[] namespaces)
        {
            return Parse<T>(source, assemblies, namespaces).Compile();
        }
    }
}
