using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace StringToExpression
{
    /// <summary>
    /// 表达式解析器
    /// </summary>
    internal class ExpressionParser
    {
        #region Fields
        /// <summary>
        /// 字符分析结果
        /// </summary>
        private SymbolParseResult spResult = null;
        /// <summary>
        /// 类型分析器
        /// </summary>
        private TypeParser typeParser = null;
        /// <summary>
        /// 委托参数类型
        /// </summary>
        private Type[] parameterTypes = null;
        /// <summary>
        /// 表达式树结果参数列表
        /// </summary>
        private List<ParameterExpression> expParams = new List<ParameterExpression>();
        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionParser"/> class.
        /// </summary>
        /// <param name="spResult">The sp result.</param>
        /// <param name="delegateType">Type of the delegate.</param>
        /// <param name="namespaces">The namespaces.</param>
        /// <param name="namespaces">The assemblies.</param>
        public ExpressionParser(SymbolParseResult spResult, Type delegateType, IEnumerable<string> namespaces = null, IEnumerable<Assembly> assemblies = null)
        {
            this.spResult = spResult;

            this.typeParser = new TypeParser(spResult);
            typeParser.SetNamespaces(namespaces);
            typeParser.SetAssemblies(assemblies);

            var method = delegateType.GetMethod("Invoke");
            parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
        }
        #endregion

        #region Lambda Perser
        public Expression<T> ToLambdaExpression<T>()
        {
            ProcessLambdaPrefix();
            var exp = ReadExpression();

            var returnType = typeof(T).GenericTypeArguments.LastOrDefault();
            if (returnType != exp.Expression.Type)
            {
                // 能隐式转换为返回类型时才转换
                if (returnType.IsStandardImplicitFrom(exp.Expression.Type))
                {
                    exp.Expression = Expression.Convert(exp.Expression, returnType);
                }
            }

            return Expression.Lambda<T>(exp.Expression, expParams);
        }
        #endregion

        #region Read Expressions
        /// <summary>
        /// 处理表达式前缀.
        /// </summary>
        private void ProcessLambdaPrefix()
        {
            // 检查是否有 Lambda 前置符(如: m => )
            if (spResult.Any(p => p.Is(OperatorWord.LambdaPrefix)))
            {
                Token token = spResult.Next();
                if (token.Is(OperatorWord.LeftBracket))
                {
                    // 有括号时，可以有多个参数
                    var bracketContent = spResult.SkipUntil(p => p.Is(OperatorWord.RightBracket));
                    bracketContent.RemoveAt(bracketContent.Count - 1);

                    // 检测下一个Token是否为LambdaPrefix
                    if (!spResult.Next().Is(OperatorWord.LambdaPrefix))
                    {
                        spResult.ReturnToIndex();
                        return;
                    }

                    // 解析参数
                    IEnumerable<Token> parameters = ResolveParameters(bracketContent);
                    int index = 0;
                    foreach (var item in parameters)
                    {
                        expParams.Add(Expression.Parameter(parameterTypes[index++], item.Text));
                    }
                }
                else if (token.Type == TokenType.Identifier && (char.IsLetter(token.Text[0]) || token.Text[0] == MarkChar.Underline) && !OperatorWord.Contains(token.Text))
                {
                    // 无括号时，最多只有一个参数
                    // 检测下一个Token是否为LambdaPrefix
                    if (!spResult.Next().Is(OperatorWord.LambdaPrefix))
                    {
                        spResult.ReturnToIndex();
                        return;
                    }

                    expParams.Add(Expression.Parameter(parameterTypes[0], token.Text));
                }

                // 参数表达式个数和传入委托参数个数不匹配判断
                if (expParams.Count != parameterTypes.Length)
                {
                    throw new ArgumentOutOfRangeException("The count of parameters is not equal.");
                }
            }
        }

        /// <summary>
        /// 读取表达式
        /// </summary>
        /// <param name="level">优先级别</param>
        /// <returns>ReadResult</returns>
        private ReadResult ReadExpression(int level = 0)
        {
            var exp = ReadFirstExpression();

            int nextLevel = 0;
            var next = spResult.PeekNext();
            while (!exp.IsClosedWrap && (nextLevel = OperatorPriority.GetOperatorLevel(next.Text)) > level)
            {
                exp = ReadNextExpression(nextLevel, exp);
                next = spResult.PeekNext();
            }

            return exp;
        }

        /// <summary>
        /// 读取第一个表达式
        /// </summary>
        /// <returns>ReadResult</returns>
        private ReadResult ReadFirstExpression()
        {
            ReadResult result = ReadResult.Empty;

            var token = spResult.Next();
            // 只列出可以出现在最左边的
            switch (token.Type)
            {
                case TokenType.Operator:
                    if (OperatorWord.UnaryExpressions.ContainsKey(token.Text))
                    {
                        result.Expression = OperatorWord.UnaryExpressions[token.Text](ReadExpression(OperatorPriority.GetOperatorLevel(token.Text)).Expression);
                    }
                    else if (token.Is(OperatorWord.LeftBracket))
                    {
                        result = ParseConvertType();
                    }
                    else if (token.Is(OperatorWord.Comma))
                    {
                        result = ReadExpression();
                    }
                    else
                    {
                        throw new ArgumentException(string.Format("Invalid Left Operator {0}", token.Text));
                    }
                    break;
                case TokenType.Identifier:
                    result = ParseIdentifier(token);
                    break;
                case TokenType.DigitValue:
                    result.Expression = Expression.Constant(DigitValue.Parse(token.Text), DigitValue.GetType(token.Text));
                    break;
                case TokenType.PackageValue:
                    // 去掉首尾引号
                    if (token.Text.StartsWith(PackageValue.String))
                    {
                        result.Expression = Expression.Constant(token.Text.Substring(1, token.Text.Length - 2), PackageValue.All[PackageValue.String]);
                    }
                    else if (token.Text.StartsWith(PackageValue.Char))
                    {
                        result.Expression = Expression.Constant(token.Text[1], PackageValue.All[PackageValue.Char]);
                    }
                    break;
                default:
                    throw new ArgumentException(string.Format("Unsupported TokenType {0}", token.Type));
            }

            return result;
        }

        /// <summary>
        /// 读取下一个表达式
        /// </summary>
        /// <param name="level">优先级别</param>
        /// <param name="previousResult">已读取的表达式</param>
        /// <returns>ReadResult</returns>
        private ReadResult ReadNextExpression(int level, ReadResult previousResult)
        {
            var result = previousResult;
            Token token = spResult.Next();

            switch (token.Type)
            {
                case TokenType.End:
                    result.IsClosedWrap = true;
                    break;
                case TokenType.Identifier:
                    if (OperatorWord.TypeExpressions.ContainsKey(token.Text))
                    {
                        result.Expression = OperatorWord.TypeExpressions[token.Text](result.Expression, typeParser.ReadType());
                    }
                    else
                    {
                        throw new ArgumentException(string.Format("Unsupported Identifier Operator {0}", token.Text));
                    }
                    break;
                case TokenType.Operator:
                    if (token.Is(OperatorWord.RightBracket) || token.Is(OperatorWord.RightSquareBracket) || token.Is(OperatorWord.RightBigBracket))
                    {
                        result.IsClosedWrap = true;
                    }
                    else if (token.Is(OperatorWord.LeftSquareBracket))
                    {
                        if (result.Expression.Type.IsArray)
                        {
                            result.Expression = Expression.ArrayIndex(result.Expression, ReadExpression(level).Expression);
                        }
                        else
                        {
                            string indexerName = "Item";

                            var indexerNameAtt = result.Expression.Type.GetCustomAttributes(typeof(DefaultMemberAttribute), true).Cast<DefaultMemberAttribute>().SingleOrDefault();
                            if (indexerNameAtt != null)
                            {
                                indexerName = indexerNameAtt.MemberName;
                            }

                            var methodInfo = result.Expression.Type.GetProperty(indexerName).GetGetMethod();
                            var @params = GetCollectionInits(true).ToArray();

                            result.Expression = Expression.Call(result.Expression, methodInfo, @params);
                        }
                    }
                    else if (token.Is(OperatorWord.Dot))
                    {
                        token = spResult.Next();
                        if (token.Type != TokenType.Identifier)
                        {
                            throw new ArgumentException("Behind the dot should be Identifier");
                        }

                        // 获取尖括号 <> 中的类型数据
                        Type[] types = Type.EmptyTypes;
                        if (spResult.IsGenericType())
                        {
                            types = GetTypes();
                        }

                        if (spResult.PeekNext().Is(OperatorWord.LeftBracket))
                        {
                            var @params = GetCollectionInits().ToArray();
                            var method = FindBestMethod(result.Expression.Type, token.Text, @params, false, types);
                            result.Expression = Expression.Call(result.Expression, method, @params);
                        }
                        else
                        {
                            var member = result.Expression.Type.GetMember(token.Text)[0];
                            if (member.MemberType == MemberTypes.Property)
                            {
                                result.Expression = Expression.Property(result.Expression, (PropertyInfo)member);
                            }
                            else
                            {
                                result.Expression = Expression.Field(result.Expression, (FieldInfo)member);
                            }
                        }
                    }
                    else
                    {
                        var right = ReadExpression(level);
                        // 字符串拼接
                        if (token.Is(OperatorWord.Plus) && (result.Expression.Type == typeof(string) || right.Expression.Type == typeof(string)))
                        {
                            var type = typeof(object);
                            result.Expression = Expression.Convert(result.Expression, type);
                            right.Expression = Expression.Convert(right.Expression, type);
                            result.Expression = Expression.Call(typeof(string).GetMethod("Concat", new Type[] { type, type }), result.Expression, right.Expression);
                        }
                        else if (OperatorWord.BinaryExpressions.ContainsKey(token.Text))
                        {
                            ImplicitConvert(ref result, ref right);
                            result.Expression = OperatorWord.BinaryExpressions[token.Text](result.Expression, right.Expression);
                        }
                        else if (token.Is(OperatorWord.Question))
                        {
                            var next = spResult.Next();
                            if (next.Is(OperatorWord.Colon))
                            {
                                var third = ReadExpression(level);
                                result.Expression = Expression.Condition(result.Expression, right.Expression, third.Expression);
                                result.IsClosedWrap = result.IsClosedWrap || right.IsClosedWrap || third.IsClosedWrap;
                            }
                            else
                            {
                                throw new ArgumentException(string.Format("{0} does not match {1}", OperatorWord.Question, OperatorWord.Colon));
                            }
                        }
                        else if (token.Is(OperatorWord.DoubleQuestion))
                        {
                            var test = Expression.Equal(result.Expression, Expression.Constant(null, result.Expression.Type));
                            result.Expression = Expression.Condition(test, right.Expression, result.Expression);
                        }
                        else
                        {
                            throw new ArgumentException(string.Format("Unsupported Operator {0}", token.Text));
                        }

                        result.IsClosedWrap = result.IsClosedWrap || right.IsClosedWrap;
                    }
                    break;
                default:
                    throw new ArgumentException(string.Format("Unsupported TokenType {0}", token.Type));
            }

            return result;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="left">左表达式结果</param>
        /// <param name="right">右表达式结果</param>
        private void ImplicitConvert(ref ReadResult left, ref ReadResult right)
        {
            if (left.Expression.Type != right.Expression.Type)
            {
                var type = TypeImplicitConvert.StandardImplicit(left.Expression.Type, right.Expression.Type);
                if (type != null)
                {
                    if (left.Expression.Type != type)
                    {
                        left.Expression = Expression.Convert(left.Expression, type);
                    }

                    if (right.Expression.Type != type)
                    {
                        right.Expression = Expression.Convert(right.Expression, type);
                    }
                }
            }
        }

        /// <summary>
        /// 解析参数并返回参数单元列表
        /// </summary>
        /// <param name="tokens">待解析的字符单元列表</param>
        /// <returns>参数单元列表</returns>
        private IEnumerable<Token> ResolveParameters(IEnumerable<Token> tokens)
        {
            IEnumerable<Token> parameters = null;
            if (tokens != null && tokens.Count() > 0)
            {
                // 参数总是以逗号分隔的形式出现，如：a,b,c
                parameters = tokens.Where(t => !(t.Is(OperatorWord.Comma)));
                // 检测是否都是Identifier
                if (parameters.Any(t => t.Type != TokenType.Identifier))
                {
                    throw new ArgumentException("Lambda Parameters Error");
                }

                // 检测数量
                if (tokens.Count() != parameters.Count() * 2 - 1)
                {
                    throw new ArgumentException("Lambda Parameters Error");
                }
            }

            return parameters ?? new List<Token>();
        }

        private ReadResult ParseIdentifier(Token token)
        {
            ReadResult result = ReadResult.Empty;
            switch (token.Text)
            {
                case MarkWord.True:
                    result.Expression = Expression.Constant(true);
                    break;
                case MarkWord.False:
                    result.Expression = Expression.Constant(false);
                    break;
                case MarkWord.Null:
                    result.Expression = Expression.Constant(null);
                    break;
                case OperatorWord.Sizeof:
                    if (spResult.Next().Is(OperatorWord.LeftBracket, true))
                    {
                        result.Expression = Expression.Constant(Marshal.SizeOf(typeParser.ReadType()));
                        spResult.Next().Is(OperatorWord.RightBracket);
                    }
                    break;
                case OperatorWord.Typeof:
                    if (spResult.Next().Is(OperatorWord.LeftBracket, true))
                    {
                        result.Expression = Expression.Constant(typeParser.ReadType(), typeof(Type));
                        spResult.Next().Is(OperatorWord.RightBracket);

                    }
                    break;
                case OperatorWord.New:
                    ParseNew(ref result);
                    break;
                default:
                    // 参数
                    if (expParams.Any(p => p.Name == token.Text))
                    {
                        result.Expression = expParams.First(p => p.Name == token.Text);
                    }
                    // 类型
                    else
                    {
                        var type = typeParser.ReadType(token.Text);
                        spResult.Next().Is(OperatorWord.Dot, true);
                        var next = spResult.Next();

                        // 获取尖括号 <> 中的类型数据
                        Type[] types = Type.EmptyTypes;
                        if (spResult.IsGenericType())
                        {
                            types = GetTypes();
                        }

                        if (spResult.PeekNext().Is(OperatorWord.LeftBracket))
                        {
                            var @params = GetCollectionInits().ToArray();
                            var method = FindBestMethod(type, next.Text, @params, true, types);
                            result.Expression = Expression.Call(method, @params);
                        }
                        else
                        {
                            var member = type.GetMember(next.Text)[0];
                            if (member.MemberType == MemberTypes.Property)
                            {
                                result.Expression = Expression.Property(null, (PropertyInfo)member);
                            }
                            else
                            {
                                result.Expression = Expression.Field(null, (FieldInfo)member);
                            }
                        }
                    }
                    break;
            }

            return result;
        }

        private void ParseNew(ref ReadResult result)
        {
            var type = typeParser.ReadType();

            // 判断初始化的类型
            Token token = spResult.Next();

            // 构造函数成员初始化/集合项初始化
            if (token.Is(OperatorWord.LeftBracket) || token.Is(OperatorWord.LeftBigBracket))
            {
                // 构建构造函数 new 的部分
                if (token.Is(OperatorWord.LeftBracket))
                {
                    // 获取参数
                    var listParam = GetCollectionInits(true).ToArray();
                    var paramTypes = listParam.Select(m => m.Type).ToArray();

                    // 获取构造函数
                    var constructor = type.GetConstructors()
                        .Where(p => p.GetParameters().Length == paramTypes.Length)
                        .Select(p => new
                        {
                            Parameters = p.GetParameters().Select(r => r.ParameterType).ToArray(),
                            Constructor = p,
                        })
                        .First(p => p.Parameters.Select(r => r.GetNoneNullableType()).SequenceEqual(paramTypes.Select(r => r.GetNoneNullableType()))).Constructor;
                    // 获取匹配的构造函数参数
                    var constructorParamTypes = constructor.GetParameters()
                                   .Select(p => p.ParameterType)
                                   .Zip(listParam, (x, y) => new { Left = x, Right = y })
                                   .Select((p, i) =>
                                   {
                                       return (p.Left.IsNullable() && p.Left != p.Right.Type) ? Expression.Convert(p.Right, p.Left.GetNullableType()) : p.Right;
                                   }).ToArray();

                    // 构造函数调用
                    result.Expression = Expression.New(constructor, constructorParamTypes);
                }
                else
                {
                    result.Expression = Expression.New(type.GetConstructor(Type.EmptyTypes));
                }

                // 构建构造函数属性成员初始化或者集合初始化
                if (spResult.PeekNext().Is(OperatorWord.LeftBigBracket) || token.Is(OperatorWord.LeftBigBracket))
                {
                    if (token.Is(OperatorWord.LeftBracket))
                    {
                        spResult.Next();
                    }

                    // 测试是否属性成员初始化                            
                    bool isMemberInit = spResult.PeekNext(2).Is(OperatorWord.Equal);

                    if (isMemberInit)
                    {
                        result.Expression = Expression.MemberInit((NewExpression)result.Expression, GetObjectMembers(type).ToArray());
                    }
                    else
                    {
                        var parameters = GetCollectionInits();
                        if (parameters.Count()>0)
                        {
                            result.Expression = Expression.ListInit((NewExpression)result.Expression, parameters.ToArray());
                        }
                    }
                }
            }
            else if (token.Is(OperatorWord.LeftSquareBracket))
            {
                Expression[] @params = null;
                if (spResult.PeekNext().Is(OperatorWord.RightSquareBracket))
                {
                    spResult.Next();
                }
                else
                {
                    @params = GetCollectionInits().ToArray();
                }

                if (spResult.PeekNext().Is(OperatorWord.LeftBigBracket))
                {
                    var parameters = GetCollectionInits();
                    if (parameters.Count() > 0)
                    {
                        result.Expression = Expression.NewArrayInit(type, parameters.ToArray());
                    }
                }
                else
                {
                    result.Expression = Expression.NewArrayBounds(type, @params);
                }
            }
            else
            {
                throw new ArgumentException(string.Format("not match to new, the token is {0}", token.Text));
            }
        }

        private ReadResult ParseConvertType()
        {
            var originPos = spResult.Index;
            var type = typeParser.ReadType(ignoreException: true);
            if (type != null && spResult.Next().Is(OperatorWord.RightBracket))
            {
                var inner = ReadExpression();
                return new ReadResult
                {
                    Expression = Expression.Convert(inner.Expression, type),
                    IsClosedWrap = inner.IsClosedWrap,
                };
            }
            else
            {
                spResult.ReturnToIndex(originPos);
                var result = ReadExpression();
                if (spResult.PeekNext().Type != TokenType.End)
                {
                    result.IsClosedWrap = false;
                }
                return result;
            }
        }

        private IEnumerable<MemberBinding> GetObjectMembers(Type type)
        {
            Token token = Token.Empty;
            while (!token.Is(OperatorWord.RightBigBracket) && !(token = spResult.Next()).Is(OperatorWord.RightBigBracket))
            {
                var member = type.GetProperty(token.Text);

                // 读取等号
                spResult.Next().Is(OperatorWord.Equal, true);

                var result = ReadExpression();
                if (result.Expression != null)
                {
                    yield return BindProperty(member, result.Expression);
                }

                // 读取逗号
                token = spResult.Next();
            }
        }

        private MemberAssignment BindProperty(PropertyInfo prop, Expression exp)
        {
            var targetType = prop.PropertyType;
            if (targetType.GetNoneNullableType().IsValueType)
            {
                var type = TypeImplicitConvert.StandardImplicit(targetType, exp.Type);
                if (type != null && targetType != type)
                {
                    exp = Expression.Convert(exp, type);
                }
            }
            return Expression.Bind(prop, exp);
        }

        private IEnumerable<Expression> GetCollectionInits(bool isReadPrefix = false)
        {
            ReadResult result = ReadResult.Empty;
            var token = spResult.PeekNext();
            if (!isReadPrefix)
            {
                if (token.Is(OperatorWord.LeftBracket) || token.Is(OperatorWord.LeftSquareBracket) || token.Is(OperatorWord.LeftBigBracket))
                {
                    spResult.Next();
                }
            }

            // 支持无参数场景，如无参构造函数、空集合，
            if (token.Is(OperatorWord.RightBracket) || token.Is(OperatorWord.RightSquareBracket) || token.Is(OperatorWord.RightBigBracket))
            {
                spResult.Next();
                result.IsClosedWrap=true;
                yield break;
            }

            do
            {
                result = ReadExpression();
                if (result.Expression != null)
                {
                    yield return result.Expression;
                }
            } while (!result.IsClosedWrap);
        }

        private MethodInfo FindBestMethod(Type type, string name, Expression[] @params, bool isStatic, Type[] genericParameterTypes)
        {
            BindingFlags flags = BindingFlags.Public;
            flags |= isStatic ? BindingFlags.Static : BindingFlags.Instance;
            var methods = type.GetMethods(flags).Where(p => p.Name == name).ToArray();
            if (methods.Length == 0)
            {
                return null;
            }

            if (genericParameterTypes != null && genericParameterTypes.Any())
            {
                methods = methods.Where(p => p.IsGenericMethod && p.GetGenericArguments().Length == genericParameterTypes.Length)
                                 .Select(p => p.MakeGenericMethod(genericParameterTypes))
                                 .Concat(methods.Where(p => !(p.IsGenericMethod && p.GetGenericArguments().Length == genericParameterTypes.Length)))
                                 .ToArray();
            }

            // 查询参数类型完全匹配的
            var paramTypes = @params == null ? Type.EmptyTypes : @params.Select(p => p.Type).ToArray();
            var find = methods.FirstOrDefault(p => p.GetParameters().Select(r => r.ParameterType).SequenceEqual(paramTypes));
            if (find != null)
            {
                return find;
            }

            // 查询可隐式转换参数的
            find = methods.Where(p => !p.IsGenericMethod && p.GetParameters().Length == paramTypes.Length)
                        .FirstOrDefault(p =>
                        {
                            for (int i = 0; i < paramTypes.Length; i++)
                            {
                                var left = p.GetParameters()[i].ParameterType;
                                if (left.IsStandardImplicitFrom(paramTypes[i]))
                                {
                                    continue;
                                }
                                else
                                {
                                    return false;
                                }
                            }

                            return true;
                        });

            if (find != null)
            {
                int i = 0;
                foreach (var item in find.GetParameters())
                {
                    @params[i] = Expression.Convert(@params[i], item.ParameterType);
                    i++;
                }
                return find;
            }

            // 查找泛型方法
            var finds = methods.Where(p => p.IsGenericMethod && p.GetParameters().Length == paramTypes.Length)
                              .Select((p, i) => new
                              {
                                  Prameters = p.GetParameters(),
                                  Method = p,
                                  Order = string.Join(string.Empty, p.GetParameters().Select(r => r.ParameterType.FullName == null ? "1" : "0"))
                              })
                              .Where(p =>
                              {
                                  for (int i = 0; i < paramTypes.Length; i++)
                                  {
                                      var left = p.Prameters[i].ParameterType;
                                      if (left.FullName == null)
                                      {
                                          continue;
                                      }

                                      var right = paramTypes[i];
                                      if (left == right)
                                      {
                                          continue;
                                      }
                                      else
                                      {
                                          return false;
                                      }
                                  }
                                  return true;
                              })
                              .OrderBy(p => p.Order)
                              .ToArray();

            if (finds.Length == 0)
            {
                return null;
            }

            // 只返回第一个符合条件的方法（已定类型的参数符合的优先）
            var first = finds.First();
            var genericParamTypes = first.Order.Select((x, y) => x == '1' ? paramTypes[y] : null).Where(p => p != null).ToArray();
            var genericTypes = first.Prameters.Where(p => p.ParameterType.FullName == null).ToList();
            var actualTypes = first.Method.GetGenericArguments().Select(r => genericParamTypes[genericTypes.FindIndex(p => p.ParameterType.Name == r.Name)]).ToArray();

            return first.Method.MakeGenericMethod(actualTypes);
        }

        private Type[] GetTypes()
        {
            List<Type> types = new List<Type>();
            if (spResult.IsGenericType())
            {
                spResult.Next();
            }

            do
            {
                types.Add(typeParser.ReadType());
                if (spResult.PeekNext().Is(OperatorWord.Comma))
                {
                    spResult.Next();
                }
            } while (!spResult.PeekNext().Is(OperatorWord.RightAngleBracket) && spResult.PeekNext().Type != TokenType.End);

            spResult.Next().Is(OperatorWord.RightAngleBracket, true);

            return types.ToArray();
        }
        #endregion
    }
}
