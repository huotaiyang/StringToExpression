using Microsoft.VisualStudio.TestTools.UnitTesting;
using StringToExpression;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UnitTestProject
{
    /// <summary>
    /// 字符串转表达式测试
    /// 目前只做了基本的正常测试
    /// </summary>
    [TestClass]
    public class StringToExpressionTest
    {
        /// <summary>
        /// 算术运算测试
        /// </summary>
        [TestMethod]
        public void ArithmeticTest()
        {
            TryAction(() =>
            {
                Expression<Func<int>> srcExp = () => 3 + 2 * 5 + ((5 - 1) + 10) / 4;
                Expression<Func<int>> targetExp = LambdaParser.Parse<Func<int>>("() => 3 + 2 * 5 + ((5 - 1) + 10) / 4");
                IsTrue(srcExp, targetExp);
            });
        }

        /// <summary>
        /// 强制转换测试
        /// </summary>
        [TestMethod]
        public void StrongConvertTest()
        {
            TryAction(() =>
            {
                Expression<Func<int>> srcExp = () => (int)3.2;
                Expression<Func<int>> targetExp = LambdaParser.Parse<Func<int>>("() => (int)3.2");
                IsTrue(srcExp, targetExp);
            });
        }

        /// <summary>
        /// 隐式转换测试
        /// </summary>
        [TestMethod]
        public void ImplicitConvertTest()
        {
            TryAction(() =>
            {
                Expression<Func<double>> srcExp = () => 1.2f + 3.2;
                Expression<Func<double>> targetExp = LambdaParser.Parse<Func<double>>("() => 1.2f + 3.2");
                IsTrue(srcExp, targetExp);
            });
        }

        /// <summary>
        /// 位移运算测试
        /// </summary>
        [TestMethod]
        public void ShiftTest()
        {
            TryAction(() =>
            {
                Expression<Func<int>> srcExp = () => 6 << 2 + 5 >> 1;
                Expression<Func<int>> targetExp = LambdaParser.Parse<Func<int>>("() => 6 << 2 + 5 >> 1");
                IsTrue(srcExp, targetExp);
            });
        }

        /// <summary>
        /// 一元运算测试
        /// </summary>
        [TestMethod]
        public void UnitaryTest()
        {
            TryAction(() =>
            {
                Expression<Func<int>> srcExp = () => -1 + ~(5 + 1);
                Expression<Func<int>> targetExp = LambdaParser.Parse<Func<int>>("() => -1 + ~(5 + 1)");
                IsTrue(srcExp, targetExp);
            });
        }

        /// <summary>
        /// ?:条件运算测试
        /// </summary>
        [TestMethod]
        public void ConditionTest()
        {
            TryAction(() =>
            {
                Expression<Func<int, int, int>> srcExp = (x, y) => x > y ? x : y;
                Expression<Func<int, int, int>> targetExp = LambdaParser.Parse<Func<int, int, int>>("(x, y) => x > y ? x : y");
                IsTrue(srcExp, targetExp, 1, 2);
                IsTrue(srcExp, targetExp, 2, 1);
            });
        }

        /// <summary>
        /// 指数运算测试
        /// </summary>
        [TestMethod]
        public void ExponenTest()
        {
            TryAction(() =>
            {
                Expression<Func<double>> srcExp = () => 2 * 3.40E+30F;
                Expression<Func<double>> targetExp = LambdaParser.Parse<Func<double>>("() => 2 * 3.40E+30F");
                IsTrue(srcExp, targetExp);
            });
        }

        /// <summary>
        /// Sizeof运算测试
        /// </summary>
        [TestMethod]
        public void SizeofTest()
        {
            TryAction(() =>
            {
                Expression<Func<int>> srcExp = () => sizeof(double);
                Expression<Func<int>> targetExp = LambdaParser.Parse<Func<int>>("() => sizeof(double)");
                IsTrue(srcExp, targetExp);
            });
        }

        /// <summary>
        /// Math函数调用测试
        /// </summary>
        [TestMethod]
        public void MathTest()
        {
            TryAction(() =>
            {
                Expression<Func<int>> srcExp = () => (int)(Math.Pow(2, 3) + Math.Sqrt(4) + Math.PI);
                Expression<Func<int>> targetExp = LambdaParser.Parse<Func<int>>("() => (int)(Math.Pow(2, 3) + Math.Sqrt(4)+ Math.PI)", "System");
                IsTrue(srcExp, targetExp);
            });
        }

        /// <summary>
        /// New运算测试
        /// </summary>
        [TestMethod]
        public void NewTest()
        {
            TryAction(() =>
            {
                Expression<Func<object>> targetExp = LambdaParser.Parse<Func<object>>("() => new object(){}");
                Assert.IsTrue(targetExp.Compile().Invoke() != null);
            });
        }

        /// <summary>
        /// Is运算测试
        /// </summary>
        [TestMethod]
        public void IsTest()
        {
            TryAction(() =>
            {
                object obj = new object();
                Expression<Func<object, bool>> targetExp = LambdaParser.Parse<Func<object, bool>>("(t) => t != null && t is object == true");
                Assert.IsTrue(targetExp.Compile().Invoke(obj));
            });
        }

        /// <summary>
        /// 索引运算测试
        /// </summary>
        [TestMethod]
        public void IndexTest()
        {
            TryAction(() =>
            {
                List<int> list = new List<int> { 1, 2, 3, 4 };
                Expression<Func<List<int>, int>> srcExp = (l) => l[1] + l[2];
                Expression<Func<List<int>, int>> targetExp = LambdaParser.Parse<Func<List<int>, int>>("(l) => l[1] + l[2]");
                Assert.IsTrue(srcExp.Compile().Invoke(list) == targetExp.Compile().Invoke(list));
            });
        }

        /// <summary>
        /// 自定义对象测试
        /// </summary>
        [TestMethod]
        public void CustomObjTest()
        {
            TryAction(() =>
            {
                Expression<Func<TestObj, long>> srcExp = (t) => t.Count + 8L;
                Expression<Func<TestObj, long>> targetExp = LambdaParser.Parse<Func<TestObj, long>>("(t) => t.Count + 8L");

                var test = new TestObj { Count = 15 };
                Assert.IsTrue(srcExp.Compile().Invoke(test) == targetExp.Compile().Invoke(test));
            });
        }

        /// <summary>
        /// 比较运算测试
        /// </summary>
        [TestMethod]
        public void CompareTest()
        {
            TryAction(() =>
            {
                Expression<Func<TestObj, bool>> srcExp = (t) => !(t.Count > 5) || (t.Count < 100 && t.Count > 10);
                Expression<Func<TestObj, bool>> targetExp = LambdaParser.Parse<Func<TestObj, bool>>("(t) => !(t.Count > 5) || (t.Count < 100 && t.Count > 10)");

                var test = new TestObj { Count = 15 };
                Assert.IsTrue(srcExp.Compile().Invoke(test) == targetExp.Compile().Invoke(test));
            });
        }

        /// <summary>
        /// 泛型运算测试
        /// </summary>
        [TestMethod]
        public void GenericTest()
        {
            TryAction(() =>
            {
                Expression<Func<TestObj, string>> srcExp = (t) => t.Func<int, string>(4);
                Expression<Func<TestObj, string>> targetExp = LambdaParser.Parse<Func<TestObj, string>>("(t) => t.Func<int, string>(4)");

                var test = new TestObj { Count = 15 };
                Assert.IsTrue(srcExp.Compile().Invoke(test) == targetExp.Compile().Invoke(test));
            });
        }

        private static void TryAction(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        private static void IsTrue(Expression<Func<int>> srcExp, Expression<Func<int>> targetExp)
        {
            Assert.IsTrue(srcExp.Compile().Invoke() == targetExp.Compile().Invoke());
        }

        private static void IsTrue(Expression<Func<double>> srcExp, Expression<Func<double>> targetExp)
        {
            var fmt = "F5";
            Assert.IsTrue(srcExp.Compile().Invoke().ToString(fmt) == targetExp.Compile().Invoke().ToString(fmt));
        }

        private static void IsTrue(Expression<Func<int, int, int>> srcExp, Expression<Func<int, int, int>> targetExp, int x, int y)
        {
            Assert.IsTrue(srcExp.Compile().Invoke(x, y) == targetExp.Compile().Invoke(x, y));
        }
    }

    /// <summary>
    /// 临时测试对象类
    /// </summary>
    public class TestObj
    {
        public int Count { get; set; }

        public TOut Func<TIn, TOut>(TIn a)
        {
            return default(TOut);
        }
    }
}
