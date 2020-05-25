using System.Collections.Generic;
using System.Diagnostics;

namespace StringToExpression
{
    /// <summary>
    /// 运算符优先级
    /// </summary>
    [DebuggerStepThrough]
    internal static class OperatorPriority
    {
        private static readonly Dictionary<string, int> operators = null;

        static OperatorPriority()
        {
            operators = new Dictionary<string, int>();

            // 括号
            int priority = 100;
            operators.Add(OperatorWord.LeftBracket, priority);
            operators.Add(OperatorWord.RightBracket, priority);
            operators.Add(OperatorWord.LeftSquareBracket, priority);
            operators.Add(OperatorWord.RightSquareBracket, priority);

            // 成员运算符
            priority--;
            operators.Add(OperatorWord.Dot, priority);
            operators.Add(OperatorWord.New, priority);
            operators.Add(OperatorWord.Typeof, priority);
            operators.Add(OperatorWord.Sizeof, priority);

            // 乘法运算符
            priority--;
            operators.Add(OperatorWord.Asterisk, priority);
            operators.Add(OperatorWord.Slash, priority);
            operators.Add(OperatorWord.Percent, priority);

            // 一元运算符
            priority--;
            operators.Add(OperatorWord.Tilde, priority);
            operators.Add(OperatorWord.Exclamation, priority);

            // 相加运算符
            priority--;
            operators.Add(OperatorWord.Plus, priority);
            operators.Add(OperatorWord.Subtract, priority);

            // 移位运算符
            priority--;
            operators.Add(OperatorWord.LeftShift, priority);
            operators.Add(OperatorWord.RightShift, priority);

            // 关系和类型测试运算符
            priority--;
            operators.Add(OperatorWord.LeftAngleBracket, priority);
            operators.Add(OperatorWord.RightAngleBracket, priority);
            operators.Add(OperatorWord.LessThanEqual, priority);
            operators.Add(OperatorWord.GreaterThanEqual, priority);
            operators.Add(OperatorWord.Is, priority);
            operators.Add(OperatorWord.As, priority);

            // 相等运算符
            priority--;
            operators.Add(OperatorWord.DoubleEqual, priority);
            operators.Add(OperatorWord.ExclamationEqual, priority);

            // 逻辑运算符
            priority--;
            operators.Add(OperatorWord.Amphersand, --priority);
            operators.Add(OperatorWord.Caret, --priority);
            operators.Add(OperatorWord.Bar, --priority);

            // 条件运算符
            priority--;
            operators.Add(OperatorWord.DoubleAmphersand, --priority);
            operators.Add(OperatorWord.DoubleBar, --priority);
            operators.Add(OperatorWord.DoubleQuestion, --priority);
            operators.Add(OperatorWord.Question, --priority);
        }

        /// <summary>
        /// 获取操作符优先级
        /// </summary>
        /// <param name="operatorWord">操作符</param>
        /// <returns>优先级别，越大优先级越高</returns>
        public static int GetOperatorLevel(string operatorWord)
        {
            int level = 0;
            if (!string.IsNullOrEmpty(operatorWord) && operators.TryGetValue(operatorWord, out level))
            {
                return level;
            }

            return -1;
        }
    }
}
