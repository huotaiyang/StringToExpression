
namespace StringToExpression
{
    /// <summary>
    /// 字符单元类型
    /// </summary>
    internal enum TokenType
    {
        /// <summary>
        /// 结束标志
        /// </summary>
        End,
        /// <summary>
        /// 成员、变量标识
        /// </summary>
        Identifier,

        /// <summary>
        /// 操作符，与OperatorWord对应
        /// </summary>
        Operator,

        /// <summary>
        /// 包裹值，与PackageValue对应
        /// </summary>
        PackageValue,

        /// <summary>
        /// 数值，与DigitValue对应
        /// </summary>
        DigitValue,
    }
}
