using System.Diagnostics;
using System.Linq.Expressions;

namespace StringToExpression
{
    /// <summary>
    /// 表达式读取结果
    /// </summary>
    [DebuggerDisplay("IsClosedWrap = {IsClosedWrap}, Expression = {Expression}")]
    internal struct ReadResult
    {
        /// <summary>
        /// 获取一个空的读取结果.
        /// </summary>
        public static ReadResult Empty
        {
            get { return new ReadResult(); }
        }

        /// <summary>
        /// 获取或设置读取到的表达式.
        /// </summary>
        public Expression Expression { get; set; }

        /// <summary>
        /// 获取或设置一个值, 通过该值指示是否已经读取了关闭符号.
        /// </summary>
        public bool IsClosedWrap { get; set; }
    }
}
