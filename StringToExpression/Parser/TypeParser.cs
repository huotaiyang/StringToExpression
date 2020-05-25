using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace StringToExpression
{
    /// <summary>
    /// 类型分析器
    /// </summary>
    internal class TypeParser
    {
        #region fields
        /// <summary>
        /// 原始字符串分析结果
        /// </summary>
        private SymbolParseResult spResult = null;
        /// <summary>
        /// 获得待分析的类型可能用到的命名空间列表
        /// </summary>
        private IEnumerable<string> namespaces = Enumerable.Empty<string>();
        /// <summary>
        /// 获得额外的程序集信息列表
        /// </summary>
        private IEnumerable<Assembly> extensionAssemblys = Enumerable.Empty<Assembly>();

        private static Assembly[] currentAssemblies = null;
        private static Assembly[] CurrentAssemblies
        {
            get
            {
                if (currentAssemblies == null)
                {
                    currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                }

                return currentAssemblies;
            }
        }

        /// <summary>
        /// 系统类型字典
        /// </summary>
        private static readonly Dictionary<string, Type> systemTypeDic = new Dictionary<string, Type>()
        {
            {"bool", typeof(bool) },
            {"byte", typeof(byte) },
            {"sbyte", typeof(sbyte) },
            {"char", typeof(char) },
            {"short", typeof(short) },
            {"ushort", typeof(ushort) },
            {"int", typeof(int) },
            {"uint", typeof(uint) },
            {"long", typeof(long) },
            {"ulong", typeof(ulong) },
            {"float", typeof(float) },
            {"double", typeof(double) },
            {"decimal", typeof(decimal) },
            {"string", typeof(string) },
            {"object", typeof(object) },
        };
        #endregion

        internal TypeParser(SymbolParseResult spResult)
        {
            this.spResult = spResult;
        }

        #region public Methods
        /// <summary>
        /// 添加可能遇到的命名空间字符串列表
        /// </summary>
        /// <param name="namespaces">新的命名空间字符串列表</param>
        /// <returns>修改后的自身</returns>
        public TypeParser SetNamespaces(IEnumerable<string> namespaces)
        {
            this.namespaces = namespaces ?? Enumerable.Empty<string>();

            return this;
        }

        /// <summary>
        /// 添加可能遇到的程序集信息列表
        /// </summary>
        /// <param name="assemblies">附加的程序集信息列表</param>
        /// <returns>修改后的自身</returns>
        public TypeParser SetAssemblies(IEnumerable<Assembly> assemblies)
        {
            extensionAssemblys = assemblies ?? Enumerable.Empty<Assembly>();

            return this;
        }
        #endregion

        #region Private Methods
        internal Type ReadType(string typeName = null, bool ignoreException = false)
        {
            Type type = null;
            StringBuilder sbValue = new StringBuilder(string.IsNullOrEmpty(typeName) ? spResult.Next().Text : typeName);
            do
            {
                // 读取参数
                if (spResult.IsGenericType())
                {
                    spResult.Skip();
                    List<Type> types = new List<Type>();
                    while (true)
                    {
                        types.Add(ReadType());
                        if (spResult.PeekNext().Is(OperatorWord.Comma))
                        {
                            spResult.Skip();
                        }
                        else
                        {
                            break;
                        }
                    }
                    spResult.Next().Is(OperatorWord.RightAngleBracket, true);

                    sbValue.AppendFormat("`{0}[{1}]", types.Count, string.Join(",", types.Select(p => "[" + p.AssemblyQualifiedName + "]").ToArray()));
                }

                type = GetType(sbValue.ToString());
                if (type == null)
                {
                    var token = spResult.Next();
                    if (!token.Is(OperatorWord.Dot))
                    {
                        if (ignoreException)
                        {
                            break;
                        }

                        throw new ArgumentException(string.Format("Unsupported {0}", token.Text));
                    }
                    sbValue.Append(".");
                    sbValue.Append(spResult.Next().Text);
                }
            } while (type == null);

            return type;
        }

        private Type GetType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            // 可空类型
            bool isNullable = false;
            if (typeName.EndsWith("?"))
            {
                isNullable = true;
                typeName = typeName.Substring(0, typeName.Length - 1);
            }

            Type type = null;
            if (systemTypeDic.ContainsKey(typeName))
            {
                type = systemTypeDic[typeName];
            }
            else
            {
                type = GetTypeFromAssembly(typeName);
                if (type == null)
                {
                    // 加上指定的命名空间再获取
                    foreach (string theNamespace in namespaces)
                    {
                        type = GetTypeFromAssembly(string.Concat(theNamespace, ".", typeName));
                        if (type != null)
                        {
                            break;
                        }
                    }
                }
            }

            if (isNullable && type != null)
            {
                type = type.GetNullableType();
            }

            return type;
        }

        private Type GetTypeFromAssembly(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                type = GetTypeFromAssembly(typeName, CurrentAssemblies);
                if (type == null && extensionAssemblys != null && extensionAssemblys.Any())
                {
                    type = GetTypeFromAssembly(typeName, extensionAssemblys);
                }
            }

            return type;
        }

        private Type GetTypeFromAssembly(string typeName, IEnumerable<Assembly> assemblys)
        {
            Type type = null;
            foreach (Assembly assembly in assemblys)
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
        #endregion
    }
}
