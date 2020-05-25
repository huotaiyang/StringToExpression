using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StringToExpression
{
    /// <summary>
    /// 符号解析结果
    /// </summary>
    internal class SymbolParseResult : ReadOnlyCollection<Token>
    {
        #region Constuction
        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolParseResult"/> class.
        /// </summary>
        /// <param name="list">The list.</param>
        internal SymbolParseResult(IList<Token> list)
            : base(list)
        {

        }
        #endregion

        #region Properties
        /// <summary>
        /// 获取或设置当前读取索引
        /// </summary>
        public int Index { get; private set; } = -1;
        #endregion

        #region Methods
        /// <summary>
        /// 读取下一个字符单元, 同时读取索引前进.
        /// </summary>
        /// <returns>读取得到的字符单元</returns>
        public Token Next()
        {
            Token token;
            if (TryGetElement(out token, Index + 1))
            {
                return token;
            }

            return Token.Empty;
        }

        /// <summary>
        /// 尝试读取下一个字符单元, 但并不前进.
        /// </summary>
        /// <param name="count">尝试读取的当前字符单元的后面第几个单元, 默认为后面第一个单元.</param>
        /// <returns>读取得到的字符单元.</returns>
        public Token PeekNext(int count = 1)
        {
            Token token;
            if (TryPeekGetElement(out token, Index + count))
            {
                return token;
            }

            return Token.Empty;
        }

        /// <summary>
        /// 前进跳过指定的字符单元.
        /// </summary>
        /// <param name="count">The count.</param>
        public void Skip(int count = 1)
        {
            count = Index + count;
            if (count < 0 || count > this.Count - 1)
            {
                throw new IndexOutOfRangeException();
            }

            Index = count;
        }

        /// <summary>
        /// 读取直到符合 predicate 的条件时停止.
        /// </summary>
        /// <param name="predicate">比较当前 Token 是否符合条件的方法.</param>
        /// <returns>读取停止时的 Token 列表.</returns>
        public IList<Token> SkipUntil(Func<Token, bool> predicate)
        {
            List<Token> tokens = new List<Token>();
            Token token = Token.Empty;
            do
            {
                token = Next();
                tokens.Add(token);
            } while (!(predicate(token) || token.Type == TokenType.End));

            return tokens;
        }

        /// <summary>
        /// 是否泛型（检测是否有匹配的&lt;&gt;）
        /// </summary>
        /// <returns>是否泛型</returns>
        public bool IsGenericType()
        {
            int count = 1;
            if (PeekNext(count++).Is(OperatorWord.LeftAngleBracket))
            {
                var next = PeekNext(count);
                while (next.Type != TokenType.End)
                {
                    if (next.Is(OperatorWord.RightAngleBracket))
                    {
                        return true;
                    }

                    if (next.Type == TokenType.Identifier || next.Is(OperatorWord.Comma))
                    {
                        count++;
                        next = PeekNext(count);
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 返回到指定的读取索引.
        /// </summary>
        /// <param name="index">目标读取索引.</param>
        public void ReturnToIndex(int index = -1)
        {
            if (index < -1 || index > this.Count - 1)
            {
                throw new IndexOutOfRangeException();
            }

            Index = index;
        }
        #endregion

        #region Private Methods
        private bool TryGetElement(out Token token, int index)
        {
            bool result = TryPeekGetElement(out token, index);
            if (result)
            {
                Index = index;
            }

            return result;
        }

        private bool TryPeekGetElement(out Token token, int index)
        {
            if (index < 0 || index > this.Count - 1)
            {
                token = Token.Empty;
                return false;
            }
            else
            {
                token = this[index];
                return true;
            }
        }

        #endregion
    }
}
