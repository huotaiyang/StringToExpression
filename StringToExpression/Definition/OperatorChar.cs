using System;
using System.Collections.Generic;

namespace StringToExpression
{
    /// <summary>
    ///  操作符 Char 
    /// </summary>
    public class OperatorChar
    {
        #region fields
        /// <summary>
        /// (
        /// </summary>
        [Mark]
        public const char LeftBracket = '(';

        /// <summary>
        /// )
        /// </summary>
        [Mark]
        public const char RightBracket = ')';

        /// <summary>
        /// [
        /// </summary>
        [Mark]
        public const char LeftSquareBracket = '[';

        /// <summary>
        /// ]
        /// </summary>
        [Mark]
        public const char RightSquareBracket = ']';

        /// <summary>
        /// {
        /// </summary>
        [Mark]
        public const char LeftBigBracket = '{';

        /// <summary>
        /// }
        /// </summary>
        [Mark]
        public const char RightBigBracket = '}';

        /// <summary>
        /// &lt;
        /// </summary>
        [Mark]
        public const char LeftAngleBracket = '<';

        /// <summary>
        /// &gt;
        /// </summary>
        [Mark]
        public const char RightAngleBracket = '>';

        /// <summary>
        /// !
        /// </summary>
        [Mark]
        public const char Exclamation = '!';

        /// <summary>
        /// =
        /// </summary>
        [Mark]
        public const char Equal = '=';

        /// <summary>
        /// *
        /// </summary>
        [Mark]
        public const char Asterisk = '*';

        /// <summary>
        /// /
        /// </summary>
        [Mark]
        public const char Slash = '/';

        /// <summary>
        /// %
        /// </summary>
        [Mark]
        public const char Percent = '%';

        /// <summary>
        /// +
        /// </summary>
        [Mark]
        public const char Plus = '+';

        /// <summary>
        /// -
        /// </summary>
        [Mark]
        public const char Subtract = '-';

        /// <summary>
        /// &amp;
        /// </summary>
        [Mark]
        public const char Amphersand = '&';

        /// <summary>
        /// |
        /// </summary>
        [Mark]
        public const char Bar = '|';

        /// <summary>
        /// .
        /// </summary>
        [Mark]
        public const char Dot = '.';

        /// <summary>
        /// ,
        /// </summary>
        [Mark]
        public const char Comma = ',';

        /// <summary>
        /// :
        /// </summary>
        [Mark]
        public const char Colon = ':';

        /// <summary>
        /// ?
        /// </summary>
        [Mark]
        public const char Question = '?';

        /// <summary>
        /// ~
        /// </summary>
        [Mark]
        public const char Tilde = '~';

        /// <summary>
        /// ^
        /// </summary>
        [Mark]
        public const char Caret = '^';
        #endregion

        private static Lazy<HashSet<char>> lazy = new Lazy<HashSet<char>>(() => ReflectionHelper.ReadStaticFields<char, MarkAttribute>(typeof(OperatorChar)));

        /// <summary>
        /// 所有Char
        /// </summary>
        internal static HashSet<char> AllChars
        {
            get
            {
                return lazy.Value;
            }
        }

        /// <summary>
        /// 是否包含
        /// </summary>
        /// <param name="c">char</param>
        /// <returns>包含指定的元素，则为 true；否则为 false。</returns>
        internal static bool Contains(char c)
        {
            return AllChars.Contains(c);
        }
    }
}
