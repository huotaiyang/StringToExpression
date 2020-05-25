using System.Diagnostics;

namespace StringToExpression
{
    /// <summary>
    /// 字符单元
    /// </summary>
    [DebuggerDisplay("Text = {Text}, Type = {Type}, Index = {Index}")]
    internal struct Token
    {
        /// <summary>
        /// 空的字符单元
        /// </summary>
        public static readonly Token Empty = new Token();

        /// <summary>
        /// 字符类型
        /// </summary>
        public TokenType Type { get; set; }
        /// <summary>
        /// 文本表示
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 索引
        /// </summary>
        public int Index { get; set; }
    }
}
