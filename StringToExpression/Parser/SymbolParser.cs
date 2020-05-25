using System;
using System.Collections.Generic;
using System.Linq;

namespace StringToExpression
{
    /// <summary>
    /// 符号解析器
    /// </summary>
    internal sealed class SymbolParser
    {
        #region Fields and Propertys
        /// <summary>
        /// 源字符串
        /// </summary>
        private string source;

        /// <summary>
        /// 源字符串长度
        /// </summary>
        private int sourceLength;

        /// <summary>
        /// 当前位置
        /// </summary>
        private int currentPosition = -1;

        /// <summary>
        /// 当前位置字符
        /// </summary>
        private char currentChar;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolParser"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public SymbolParser(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentNullException("source");
            }

            this.source = source;
            this.sourceLength = source.Length;
            NextChar();
        }
        #endregion

        #region public Methods
        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <returns>SymbolParseResult</returns>
        public static SymbolParseResult Parse(string source)
        {
            var parser = new SymbolParser(source);
            List<Token> tokens = new List<Token>();
            while (true)
            {
                var token = parser.CurrentToken();
                tokens.Add(token);
                if (token.Type == TokenType.End)
                {
                    break;
                }
            }

            return new SymbolParseResult(tokens);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 移动到下一个字符
        /// </summary>
        private void NextChar()
        {
            if (currentPosition < sourceLength)
            {
                currentPosition++;
            }

            currentChar = currentPosition < sourceLength ? source[currentPosition] : MarkChar.End;
        }

        /// <summary>
        /// 获取当前Token
        /// </summary>
        /// <returns>当前Token</returns>
        private Token CurrentToken()
        {
            while (Char.IsWhiteSpace(currentChar))
            {
                NextChar();
            }

            TokenType type;
            int tokenPos = currentPosition;
            if (OperatorChar.Contains(currentChar))
            {
                type = TokenType.Operator;
                string mark = currentChar.ToString();
                // 处理双操作符
                NextChar();
                if (OperatorChar.Contains(currentChar))
                {
                    mark = mark + currentChar.ToString();
                    if (OperatorWord.Contains(mark))
                    {
                        NextChar();
                    }
                }
            }
            else if (Char.IsLetter(currentChar) || currentChar == MarkChar.Parameter || currentChar == MarkChar.Underline)
            {
                type = TokenType.Identifier;
                do
                {
                    NextChar();
                } while (Char.IsLetterOrDigit(currentChar) || currentChar == MarkChar.Underline || currentChar == OperatorChar.Question);
            }
            else if (PackageValue.Contains(currentChar))
            {
                type = TokenType.PackageValue;
                char mark = currentChar;
                do
                {
                    NextChar();
                } while (currentChar != mark && currentPosition < sourceLength);

                if (currentPosition == sourceLength)
                {
                    throw new ArgumentException(string.Format("PackageValue not matched to paired {0}", mark));
                }

                NextChar();
            }
            else if (Char.IsDigit(currentChar))
            {
                type = TokenType.DigitValue;
                do
                {
                    NextChar();
                } while (Char.IsDigit(currentChar));

                // 数值尾标
                if (DigitValue.Contains(currentChar))
                {
                    NextChar();
                }
                else if (currentChar == MarkChar.Dot)
                {
                    NextChar();
                    ValidateDigit();
                    do
                    {
                        NextChar();
                    } while (Char.IsDigit(currentChar));

                    // 指数
                    if (MarkChar.Exponent.Contains(currentChar))
                    {
                        NextChar();
                        if (currentChar == MarkChar.Positive || currentChar == MarkChar.Negative)
                        {
                            NextChar();
                        }

                        ValidateDigit();

                        while (Char.IsDigit(currentChar))
                        {
                            NextChar();
                        }
                    }

                    // 数值尾标
                    if (DigitValue.Contains(currentChar))
                    {
                        NextChar();
                    }
                }
            }
            else if (currentChar == MarkChar.End)
            {
                type = TokenType.End;
            }
            else
            {
                throw new ArgumentException(string.Format("Unsupported Char {0}", currentChar));
            }

            var text = source.Substring(tokenPos, currentPosition - tokenPos);

            return new Token { Type = type, Text = text, Index = tokenPos };
        }

        /// <summary>
        /// 检测是否有数字
        /// </summary>
        private void ValidateDigit()
        {
            if (!Char.IsDigit(currentChar))
            {
                throw new ArgumentException(string.Format("Expected position {0} is a digit, but it is {1}", currentPosition, currentChar));
            }
        }
        #endregion
    }
}
