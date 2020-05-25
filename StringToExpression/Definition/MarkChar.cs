namespace StringToExpression
{
    /// <summary>
    /// 标识 Char
    /// </summary>
    public class MarkChar
    {
        /// <summary>
        /// '\0' 结束符
        /// </summary>
        public const char End = char.MinValue;

        /// <summary>
        /// _ 下画线
        /// </summary>
        public const char Underline = '_';

        /// <summary>
        /// @参数符
        /// </summary>
        public const char Parameter = '@';

        /// <summary>
        /// . 点
        /// </summary>
        public const char Dot = '.';

        /// <summary>
        /// + 正
        /// </summary>
        public const char Positive = '+';

        /// <summary>
        /// - 负
        /// </summary>
        public const char Negative = '-';

        /// <summary>
        /// 指数
        /// </summary>
        public static readonly char[] Exponent = new char[] { 'e', 'E' };
    }
}
